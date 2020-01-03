
namespace LibAzureFunc.LibraryBook
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
        private readonly ILibraryBookWebApiManager _libraryBookWebApiManager;

        public InsertLibraryBookHttpFunction(IAccessTokenProvider tokenProvider, ILibraryBookWebApiManager libraryBookWebApiManager)
        {
            _tokenProvider = tokenProvider;
            _libraryBookWebApiManager = libraryBookWebApiManager;
        }

        [FunctionName("InsertLibraryBookHttpFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "insertlibrarybook")] HttpRequest req, ILogger log)
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
                LibraryBookApiModel model = JsonConvert.DeserializeObject<LibraryBookApiModel>(requestBody);

                if (model == null)
                {
                    return new BadRequestObjectResult("Please pass LibraryBookApiModel in the request body");
                }

                int retVal = 0;
                string libraryBookCode = string.Empty;
                if (model != null)
                {

                    model.CreatedBy = _tokenProvider.User;
                    model.DateCreated = DateTime.Now;
                    model.ModifiedBy = _tokenProvider.User;
                    model.DateModified = DateTime.Now;

                    retVal = _libraryBookWebApiManager.InsertLibraryBook(model, out libraryBookCode);
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
