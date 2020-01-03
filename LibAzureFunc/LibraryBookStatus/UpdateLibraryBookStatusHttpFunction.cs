using System;
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


    public class UpdateLibraryBookStatusHttpFunction
    {
   
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly ILibraryBookStatusWebApiManager _libraryBookStatusWebApiManager;

        public UpdateLibraryBookStatusHttpFunction(IAccessTokenProvider tokenProvider, ILibraryBookStatusWebApiManager libraryBookStatusWebApiManager)
        {
            _tokenProvider = tokenProvider;
            _libraryBookStatusWebApiManager = libraryBookStatusWebApiManager;
        }

        [FunctionName("UpdateLibraryBookStatusHttpFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "updatelibrarybookstatus")] HttpRequest req, ILogger log)
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
                if (model != null)
                {
                    model.ModifiedBy = _tokenProvider.User;
                    model.DateModified = DateTime.Now;

                    retVal = _libraryBookStatusWebApiManager.UpdateLibraryBookStatus(model);
                }

                if (retVal < 1)
                {
                    return new BadRequestObjectResult("Failed to insert record");
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError($"Caught exception: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
