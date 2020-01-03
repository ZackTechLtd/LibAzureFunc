
namespace LibAzureFunc.LibraryBookStatus
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using LibAzureFunc.AccessTokens;
    using DataAccess.WebApiManager.Interfaces;
    using Common.Models.Api;

    public class InsertLibraryBookHttpFunction
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly ILibraryBookStatusWebApiManager _libraryBookStatusWebApiManager;
        private readonly ILibraryBookWebApiManager _libraryBookWebApiManager;

        public InsertLibraryBookHttpFunction(IAccessTokenProvider tokenProvider, ILibraryBookStatusWebApiManager libraryBookStatusWebApiManager, ILibraryBookWebApiManager libraryBookWebApiManager)
        {
            _tokenProvider = tokenProvider;
            _libraryBookStatusWebApiManager = libraryBookStatusWebApiManager;
            _libraryBookWebApiManager = libraryBookWebApiManager;
        }

        [FunctionName("InsertLibraryBookStatusHttpFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "insertlibrarybookstatus")] HttpRequest req, ILogger log)
        {
            try
            {
                var result = _tokenProvider.ValidateToken(req);

                if (result.Status == AccessTokenStatus.Valid)
                {
                    log.LogInformation($"Request received for {result.Principal.Identity.Name}.");
                }
                else
                {
                    return new UnauthorizedResult();
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                LibraryBookStatusApiModel model = JsonConvert.DeserializeObject<LibraryBookStatusApiModel>(requestBody);

                if (model == null)
                {
                    return new BadRequestObjectResult("Please pass LibraryBookApiModel in the request body");
                }

                int retVal = 0;
                string libraryBookCode = string.Empty;
                if (model != null)
                {
                    int? returnVal = _libraryBookStatusWebApiManager.GetCountOfBookCurrentLent(model?.LibraryUser.LibraryUserCode);
                    if (returnVal != null && returnVal.Value > 4)
                    {
                        return new BadRequestObjectResult("Maximum number of books exceeded");
                    }

                    LibraryBookApiModel libraryBookApiModel = _libraryBookWebApiManager.GetLibraryBookByLibraryBookCode(model.LibraryBook?.LibraryBookCode);
                    if (libraryBookApiModel == null)
                    {
                        return new BadRequestObjectResult("Library book not found");
                    }

                    if (_libraryBookStatusWebApiManager.HasMoreThanOneBookWithSameISBN(libraryBookApiModel.ISBN, model?.LibraryUser.LibraryUserCode))
                    {
                        return new BadRequestObjectResult("Library User already has that book");
                    }

                    model.CreatedBy = _tokenProvider.User;
                    model.DateCreated = DateTime.Now;
                    model.ModifiedBy = _tokenProvider.User;
                    model.DateModified = DateTime.Now;

                    retVal = _libraryBookStatusWebApiManager.InsertLibraryBookStatus(model, out libraryBookCode);
                }

                if (retVal < 1)
                {
                    return new BadRequestObjectResult("Failed to insert record");
                }

                return (ActionResult)new OkObjectResult(new ContentResult
                {
                    Content = libraryBookCode,
                    ContentType = "text/plain",
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                log.LogError($"Caught exception: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }

        }
    }

}
