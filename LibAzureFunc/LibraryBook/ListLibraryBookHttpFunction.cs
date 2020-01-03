
namespace LibAzureFunc.LibraryBook
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using DataAccess.WebApiManager.Interfaces;
    using LibAzureFunc.AccessTokens;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class ListLibraryBookHttpFunction
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly ILibraryBookWebApiManager _libraryBookWebApiManager;

        public ListLibraryBookHttpFunction(IAccessTokenProvider tokenProvider, ILibraryBookWebApiManager libraryBookWebApiManager)
        {
            _tokenProvider = tokenProvider;
            _libraryBookWebApiManager = libraryBookWebApiManager;
        }

        [FunctionName("ListLibraryBookHttpFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "listlibrarybook")] HttpRequest req, ILogger log)
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

                string search = req.Query["search"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                search = search ?? data?.name;

                if (string.IsNullOrEmpty(search))
                {
                    return new BadRequestObjectResult("Please pass search on the query string or in the request body");
                }

                var booksApiModel = _libraryBookWebApiManager.GetBooks(search);

                if (booksApiModel == null)
                {
                    return new BadRequestObjectResult("Nothing found");
                }

                return (ActionResult)new OkObjectResult(booksApiModel);
            }
            catch (Exception ex)
            {
                log.LogError($"Caught exception: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }

        }
    }
}
