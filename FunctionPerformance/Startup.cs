using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FunctionPerformance.Startup))]
namespace FunctionPerformance
{
    public class Startup : FunctionsStartup
    {
        private const string EndpointUrl = "ENTER YOU COSMOS URL";
        private const string AuthorizationKey = "ENTER YOUR COSMOS KEY";

        public Startup()
        {
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            CosmosClient cosmosClient = new CosmosClient(EndpointUrl, AuthorizationKey, new CosmosClientOptions() { AllowBulkExecution = true });

            builder.Services.AddSingleton<CosmosClient>(cosmosClient);
        }
    }
}
