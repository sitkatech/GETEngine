
using System;
using System.Diagnostics;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Olsson.GET.Common.Utilities;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Customers;
using Olsson.GET.Managers.Runs;

[assembly: FunctionsStartup(typeof(Startup))]


public class Startup : FunctionsStartup
{
    private static readonly ILogger _logger = Logging.GetLogger<Startup>();
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton<ManagerFactory>();
        builder.Services.AddSingleton<ICustomerManager, CustomerManager>();
        builder.Services.AddSingleton<IRunManager, RunManager>();
        
        ConfigurationHelper.Build(Environment.CurrentDirectory);
    }
}
