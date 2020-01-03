
namespace LibAzureFunc.LibraryUser
{
    using System;
    using Common.Models;
    using DataAccess.WebApiManager.Interfaces;
    using LibAzureFunc.AccessTokens;
    using LibAzureFunc.Util;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;

    public class PagedLibraryUserHttpFunction
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly ILibraryUserWebApiManager _libraryUserWebApiManager;

        public PagedLibraryUserHttpFunction(IAccessTokenProvider tokenProvider, ILibraryUserWebApiManager libraryUserWebApiManager)
        {
            _tokenProvider = tokenProvider;
            _libraryUserWebApiManager = libraryUserWebApiManager;
        }

        [FunctionName("PagedLibraryUserHttpFunction")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pagedlibraryuser")] HttpRequest req, ILogger log)
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

                var dicItems = req.GetQueryParameterDictionary();
                PagedBase pagedBase = new PagedBase()
                {
                    PageNum = Helper.GetIntValue(dicItems["PageNum"]),
                    PageSize = Helper.GetIntValue(dicItems["PageSize"]),
                    OrderBy = Helper.GetIntValue(dicItems["OrderBy"]),
                    SortOrder = Helper.GetIntValue(dicItems["SortOrder"]),
                    SearchText = dicItems["SortOrder"]
                };

                var libraryUserpaged = _libraryUserWebApiManager.GetLibraryUsersPaged(pagedBase, out int searchResultCount);

                if (libraryUserpaged == null)
                {
                    return new BadRequestObjectResult("Nothing found");
                }

                return (ActionResult)new OkObjectResult(libraryUserpaged);
            }
            catch (Exception ex)
            {
                log.LogError($"Caught exception: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }

        }
    }
}
