
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

    public class DeleteLibraryBookHttpFunction
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly ILibraryBookWebApiManager _libraryBookWebApiManager;

        public DeleteLibraryBookHttpFunction(IAccessTokenProvider tokenProvider, ILibraryBookWebApiManager libraryBookWebApiManager)
        {
            _tokenProvider = tokenProvider;
            _libraryBookWebApiManager = libraryBookWebApiManager;
        }

        [FunctionName("DeleteLibraryBookHttpFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "deletelibrarybook")] HttpRequest req, ILogger log)
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

            string Id = req.Query["Id"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            Id = Id ?? data?.name;

            if (string.IsNullOrEmpty(Id))
            {
                return new BadRequestObjectResult("Please pass id on the query string or in the request body");
            }

            int retVal = _libraryBookWebApiManager.DeleteLibraryBook(Id);

            if (retVal < 1)
            {
                return new BadRequestObjectResult("Unable to Delete book");
            }

            return new OkResult();

        }
    }
}
