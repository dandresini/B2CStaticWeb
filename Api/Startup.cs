using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;


[assembly: FunctionsStartup(typeof(BlazorApp.Api.Startup))]


namespace BlazorApp.Api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            {
                builder.Services.AddScoped(implementationFactory =>
                {
                    TokenCredentialOptions options = new TokenCredentialOptions
                    {
                        AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                    };

                    //
                    var clientSecretCredential = new ClientSecretCredential(
                    Environment.GetEnvironmentVariable("B2C_TENANTID"),
                    Environment.GetEnvironmentVariable("B2C_CLIENT_ID"),
                    Environment.GetEnvironmentVariable("B2C_CLIENT_SECRET"),
                    options);

                    return new Microsoft.Graph.GraphServiceClient(clientSecretCredential, new[] {"https://graph.microsoft.com/.default" });
                });


            }
        }
    }
}
