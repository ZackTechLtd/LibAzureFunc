using System;
using Common.Util;
using DataAccess.WebApiManager.Interfaces;
using DataAccess.WebApiManager.Manager;
using DataAccess.WebApiRepository.Interfaces;
using DataAccess.WebApiRepository.Repository;
using LibAzureFunc.AccessTokens;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(LibAzureFunc.Startup))]
namespace LibAzureFunc
{
    /// <summary>
    /// Runs when the Azure Functions host starts.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Get the configuration files for the OAuth token issuer
            var issuerToken = Environment.GetEnvironmentVariable("IssuerToken");
            var audience = Environment.GetEnvironmentVariable("Audience");
            var issuer = Environment.GetEnvironmentVariable("Issuer");

            var connectionString = Environment.GetEnvironmentVariable("ConnectString");
            var dbType = Environment.GetEnvironmentVariable("DatabaseType");

            // Register the access token provider as a singleton
            builder.Services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>(s => new AccessTokenProvider(issuerToken, audience, issuer));
            // Register database services
            builder.Services.AddTransient<IRandomKeyGenerator, RandomKeyGenerator>();
            builder.Services.AddTransient<ILibraryBookRepository, LibraryBookRepository>(s => new LibraryBookRepository(dbType, dbType, new RandomKeyGenerator()));
            builder.Services.AddTransient<ILibraryBookStatusRepository, LibraryBookStatusRepository>(s => new LibraryBookStatusRepository(dbType, dbType, new RandomKeyGenerator()));
            builder.Services.AddTransient<ILibraryUserRepository, LibraryUserRepository>(s => new LibraryUserRepository(dbType, dbType, new RandomKeyGenerator()));
            builder.Services.AddTransient<ILibraryBookWebApiManager, LibraryBookWebApiManager>();
            builder.Services.AddTransient<ILibraryUserWebApiManager, LibraryUserWebApiManager>();
            builder.Services.AddTransient<ILibraryBookStatusWebApiManager, LibraryBookStatusWebApiManager>();
        }
    }
}

