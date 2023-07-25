
using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Olsson.GET.Common.Utilities;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Customers;
using Olsson.GET.Managers.Runs;
using Serilog;

[assembly: FunctionsStartup(typeof(Startup))]


public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton<ManagerFactory>();
        builder.Services.AddSingleton<ICustomerManager, CustomerManager>();
        builder.Services.AddSingleton<IRunManager, RunManager>();
        
        ConfigurationHelper.Build(Environment.CurrentDirectory);
    }
}
