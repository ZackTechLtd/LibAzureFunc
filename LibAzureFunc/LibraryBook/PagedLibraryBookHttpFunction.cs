
namespace LibAzureFunc.LibraryBook
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

    public class PagedLibraryBookHttpFunction
    {
        
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly ILibraryBookWebApiManager _libraryBookWebApiManager;

        public PagedLibraryBookHttpFunction(IAccessTokenProvider tokenProvider, ILibraryBookWebApiManager libraryBookWebApiManager)
        {
            _tokenProvider = tokenProvider;
            _libraryBookWebApiManager = libraryBookWebApiManager;
        }

        [FunctionName("PagedLibraryBookHttpFunction")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pagedlibrarybook")] HttpRequest req, ILogger log)
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

                bool listLostAndStolen = Helper.GetBoolValue(dicItems["listLostAndStolen"]);


                var librarybookpaged = _libraryBookWebApiManager.GetLibraryBooksPaged(pagedBase, listLostAndStolen, out int searchResultCount);

                if (librarybookpaged == null)
                {
                    return new BadRequestObjectResult("Nothing found");
                }

                return (ActionResult)new OkObjectResult(librarybookpaged);
            }
            catch(Exception ex)
            {
                log.LogError($"Caught exception: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }

        }

    }
}
