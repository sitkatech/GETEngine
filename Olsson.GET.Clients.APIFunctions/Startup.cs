
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Olsson.GET.Common.Utilities;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Customers;
using Olsson.GET.Managers.Runs;
using System;
using System.Collections.Generic;

[assembly: FunctionsStartup(typeof(Startup))]


public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        string appRootPath = builder.GetContext().ApplicationRootPath;
        ConfigurationHelper.Build(appRootPath);
        builder.Services.AddSingleton<ManagerFactory>();
        builder.Services.AddSingleton<ICustomerManager, CustomerManager>();
        builder.Services.AddSingleton<IRunManager, RunManager>();
        
        builder.Services.AddMvcCore().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new DefaultContractResolver();
        });


        builder.Services.AddSingleton<IOpenApiConfigurationOptions>(_ =>
        {
            var azureFunctionsEnvironment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
            List<OpenApiServer> servers = new List<OpenApiServer>();
            if (string.IsNullOrEmpty(azureFunctionsEnvironment) || (!string.IsNullOrEmpty(azureFunctionsEnvironment) && azureFunctionsEnvironment != "Development"))
            {
                servers.Add(new OpenApiServer()
                {
                    Url = ConfigurationHelper.AppSettings.CustomerAzureFunctionsBaseUrl,
                });
            }

            var options = new OpenApiConfigurationOptions()
            {
                Info = new OpenApiInfo()
                {
                    Version = DefaultOpenApiConfigurationOptions.GetOpenApiDocVersion(),
                    Title = $"OpenAPI Documentation for GET",
                    Description = $"",
                },
                Servers = servers,
                OpenApiVersion = DefaultOpenApiConfigurationOptions.GetOpenApiVersion(),
                IncludeRequestingHostName = false,
                
            };

            return options;
        });
        
    }
}
