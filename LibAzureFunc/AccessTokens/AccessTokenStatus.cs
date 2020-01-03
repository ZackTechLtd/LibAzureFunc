using System;
namespace LibAzureFunc.AccessTokens
{
    public enum AccessTokenStatus
    {
        Valid,
        Expired,
        Error,
        NoToken
    }
}
