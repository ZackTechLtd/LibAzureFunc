using System;
namespace LibAzureFunc.AccessTokens
{
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Validates access tokes that have been submitted as part of a request.
    /// </summary>
    public interface IAccessTokenProvider
    {
        string User { get; }

        /// <summary>
        /// Validate the access token, returning the security principal in a result.
        /// </summary>
        /// <param name="request">The HTTP request containing the access token.</param>
        /// <returns>A result that contains the security principal.</returns>
        AccessTokenResult ValidateToken(HttpRequest request);
    }
}
