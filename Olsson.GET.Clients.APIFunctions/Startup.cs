
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Customers;
using Olsson.GET.Managers.Runs;

[assembly: FunctionsStartup(typeof(Startup))]


public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton<ManagerFactory>();
        builder.Services.AddSingleton<ICustomerManager>();
        builder.Services.AddSingleton<IRunManager>();
    }
}
