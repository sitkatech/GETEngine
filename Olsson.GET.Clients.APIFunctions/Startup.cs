
using System;
using System.Diagnostics;
using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Olsson.GET.Common.Utilities;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Customers;
using Olsson.GET.Managers.Runs;
using Swashbuckle.AspNetCore.SwaggerGen;

[assembly: FunctionsStartup(typeof(Startup))]


public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton<ManagerFactory>();
        builder.Services.AddSingleton<ICustomerManager, CustomerManager>();
        builder.Services.AddSingleton<IRunManager, RunManager>();
        
        builder.Services.AddMvcCore().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new DefaultContractResolver();
        });
        builder.AddSwashBuckle(Assembly.GetExecutingAssembly(), opts => {
            opts.AddCodeParameter = true;
            opts.Documents = new[] {
                new SwaggerDocument {
                    Name = "v1",
                    Title = "Swagger Document",
                    Description = "Swagger UI for Azure Functions",
                    Version = "v1"
                }
            };
            opts.ConfigureSwaggerGen = x => {
                x.CustomOperationIds(apiDesc => {
                    return apiDesc.TryGetMethodInfo(out MethodInfo mInfo) ? mInfo.Name : default(Guid).ToString();
                });
            };
        });
        string appRootPath = builder.GetContext().ApplicationRootPath;
        ConfigurationHelper.Build(appRootPath);
    }
}
