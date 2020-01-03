
namespace LibAzureFunc.LibraryBookStatus
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


    public class PagedLibraryBookStatusHttpFunction
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly ILibraryBookStatusWebApiManager _libraryBookStatusWebApiManager;

        public PagedLibraryBookStatusHttpFunction(IAccessTokenProvider tokenProvider, ILibraryBookStatusWebApiManager libraryBookStatusWebApiManager)
        {
            _tokenProvider = tokenProvider;
            _libraryBookStatusWebApiManager = libraryBookStatusWebApiManager;
        }

        [FunctionName("PagedLibraryBookStatusHttpFunction")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pagedlibrarybookstatus")] HttpRequest req, ILogger log)
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


                var librarybookstatuspaged = _libraryBookStatusWebApiManager.GetLibraryBookStatusPaged(pagedBase, out int searchResultCount);

                if (librarybookstatuspaged == null)
                {
                    return new BadRequestObjectResult("Nothing found");
                }

                return (ActionResult)new OkObjectResult(librarybookstatuspaged);
            }
            catch (Exception ex)
            {
                log.LogError($"Caught exception: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }

        }
    }
}
