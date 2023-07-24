using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Olsson.GET.Common.Utilities;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

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
                b.AddAzureStorageCoreServices();
                b.AddAzureStorageQueues();
                b.AddAzureStorageBlobs();
                b.AddTimers();

            });

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
        
    }
}
