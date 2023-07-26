
using System;
using System.Diagnostics;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Olsson.GET.Common.Utilities;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Customers;
using Olsson.GET.Managers.Runs;

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

        ConfigurationHelper.Build(Environment.CurrentDirectory);
    }
}
