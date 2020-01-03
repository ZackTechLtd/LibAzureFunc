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

namespace LibAzureFunc.LibraryBook
{
    public class GetLibraryBookHttpFunction
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly ILibraryBookWebApiManager _libraryBookWebApiManager;

        public GetLibraryBookHttpFunction(IAccessTokenProvider tokenProvider, ILibraryBookWebApiManager libraryBookWebApiManager)
        {
            _tokenProvider = tokenProvider;
            _libraryBookWebApiManager = libraryBookWebApiManager;
        }

        [FunctionName("GetLibraryBookHttpFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getlibrarybook")] HttpRequest req, ILogger log)
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

            var librarybook = _libraryBookWebApiManager.GetLibraryBookByLibraryBookCode(Id);

            if (librarybook == null)
            {
                return new BadRequestObjectResult("Book not found");
            }

            return (ActionResult)new OkObjectResult(librarybook);

        }
    }
}
