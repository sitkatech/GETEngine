using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Olsson.GET.Common.Utilities;
using System;
using System.Threading.Tasks;

namespace Olsson.GET.Clients.Orchestrator
{

    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        private static readonly ILogger Logger = Logging.GetLogger<Program>();
        static async Task Main()
        {
            var builder = new HostBuilder();
            
            builder.UseEnvironment(ConfigurationHelper.GetEnvironment());

            builder.ConfigureWebJobs((context, b) =>
            {
                b.AddAzureStorageQueues();
            });
            
            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
        
    }
}
