
namespace LibAzureFunc.AccessTokens
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// Validates a incoming request and extracts any <see cref="ClaimsPrincipal"/> contained within the bearer token.
    /// </summary>
    public class AccessTokenProvider : IAccessTokenProvider
    {
        private const string AUTH_HEADER_NAME = "Authorization";
        private const string BEARER_PREFIX = "Bearer ";
        private readonly string _issuerToken;
        private readonly string _audience;
        private readonly string _issuer;

        public string User { get; private set; }

        public AccessTokenProvider(string issuerToken, string audience, string issuer)
        {
            _issuerToken = issuerToken;
            _audience = audience;
            _issuer = issuer;
        }

        public AccessTokenResult ValidateToken(HttpRequest request)
        {
            try
            {
                // Get the token from the header
                if (request != null &&
                    request.Headers.ContainsKey(AUTH_HEADER_NAME) &&
                    request.Headers[AUTH_HEADER_NAME].ToString().StartsWith(BEARER_PREFIX, StringComparison.OrdinalIgnoreCase))
                {
                    var token = request.Headers[AUTH_HEADER_NAME].ToString().Substring(BEARER_PREFIX.Length);

                    if (!string.IsNullOrEmpty(token))
                    {
                        JwtSecurityToken sectoken = new JwtSecurityToken(jwtEncodedString: token);
                        if (token != null)
                        {
                            User = sectoken.Subject;
                        }
                    }
                    
                    // Create the parameters
                    //var tokenParams = new TokenValidationParameters()
                    //{
                    //    RequireSignedTokens = true,
                    //    ValidAudience = _audience,
                    //    ValidateAudience = true,
                    //    ValidIssuer = _issuer,
                    //    ValidateIssuer = true,
                    //    ValidateIssuerSigningKey = true,
                    //    ValidateLifetime = true,
                    //    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_issuerToken))
                    //    //IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(_issuerToken))
                    //};

                    var tokenParams = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,

                        ValidIssuer = "ZackTechSecurityBearer",
                        ValidAudience = "ZackTechSecurityBearer",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_issuerToken))
                    };

                    // Validate the token
                    var handler = new JwtSecurityTokenHandler();

                    //var tokenS = handler.ReadToken(token) as JwtSecurityToken;

                    var result = handler.ValidateToken(token, tokenParams, out var securityToken);
                    return AccessTokenResult.Success(result);
                }
                else
                {
                    return AccessTokenResult.NoToken();
                }
            }
            catch (SecurityTokenExpiredException)
            {
                return AccessTokenResult.Expired();
            }
            catch (Exception ex)
            {
                return AccessTokenResult.Error(ex);
            }
        }

        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
