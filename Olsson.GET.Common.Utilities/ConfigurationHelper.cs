using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace Olsson.GET.Common.Utilities
{
    public static class ConfigurationHelper
    {

        public static ConnectionStrings ConnectionStrings;
        private static readonly ILogger _logger = Logging.GetLogger(nameof(ConfigurationHelper));
        public static AppSettings AppSettings;
        static ConfigurationHelper()
        {
            
        }

        public static void Build(string executionDirectory = null)
        {
            _logger.LogInformation($"setting with {executionDirectory}");
            var configBuilder = new ConfigurationBuilder();

            string basePath = !string.IsNullOrEmpty(executionDirectory) ? $"{executionDirectory}\\" : "";
            
            configBuilder.AddJsonFile($"{basePath}appsettings.json", false); // app settings shared between all environments
            configBuilder.AddJsonFile($"{basePath}appsettings.{GetEnvironment()}.json", true); // app settings for the hosted environment
            configBuilder.AddJsonFile($"{basePath}appsecrets.json", true); // secrets if they exist
            configBuilder.AddEnvironmentVariables();
            var configRoot = configBuilder.Build();

            AppSettings = configRoot.Get<AppSettings>();
            ConnectionStrings = configRoot.Get<ConnectionStrings>();
        }

        public static string GetEnvironment()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
            {
                return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            } else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")))
            {
                return Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            } else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPSETTING_Environment")))
            {
                return Environment.GetEnvironmentVariable("APPSETTING_Environment");
            }

            throw new Exception("Could not find \"Environment\". Tried checking \"ASPNETCORE_ENVIRONMENT\", \"DOTNET_ENVIRONMENT\", and \"APPSETTING_Environment\" ");
            
        }

    }

    public class ConnectionStrings
    {
        public string GetPrimaryDatabase { get; set; }
        public string AzureStorageAccount { get; set; }
        public string AzureWebJobsDashboard { get; set; }
        public string AzureWebJobsStorage { get; set; }
    }

    public class AppSettings
    {
        public string SendGridApiKey { get; set; }
        public string ImageServerUri { get; set; }
        public string FromEmail { get; set; }
        public string NewPasswordTemplateId { get; set; }
        public string RunCompletedTemplateId { get; set; }
        public string RunErroredTemplateId { get; set; }
        public string PortalGoogleAnalyticsTrackingCode { get; set; }
        public string PortalApplicationInsightsKey { get; set; }
        public string BlobStorageModelDataFolder { get; set; }
        public string BlobStorageModelOutputsFolder { get; set; }
        public string ModflowDataFolder { get; set; }
        public string DockerAgentContainerPath { get; set; }
        public string ApplicationBaseUrl { get; set; }
        public string GETSupportEmailAddress { get; set; }
        public string GoogleMapsAPIKey { get; set; }
        public TimeSpan LoginTimeout { get; set; } = TimeSpan.FromDays(60);
        public int TrialRunLimit { get; set; } = 10;
        public int MaxRunProcessingTimeInHours { get; set; } = 24;
        public int ContainerRetentionPeriodInDays { get; set; } = 1;
        public int MaxContainerCount { get; set; } = 90;
        public string APIFunctionCode { get; set; }
        public string RunAnalysisUrl { get; set; }
        public string GenerateOutputsUrl { get; set; }
        public string SendRunCompletedNotificationUrl { get; set; }
        public string GenerateInputsQueueName { get; set; }
        public string RunAnalysisQueueName { get; set; }
        public string GenerateOutputsQueueName { get; set; }
        public string AzureResourceGroup { get; set; }
        public string AzureRegistryServer { get; set; }
        public string AzureRegistryUsername { get; set; }
        public string AzureRegistryPassword { get; set; }
        public string AzureContainerTcpPort { get; set; }
        public string AzureContainerVolumeName { get; set; }
        public string AzureStorageAccountName { get; set; }
        public string AzureStorageAccountKey { get; set; }
        public string FunctionClientId { get; set; }
        public string FunctionSecret { get; set; }
        public string FunctionTenantId { get; set; }
        public int DashboardPageRecordCount { get; set; } = 20;
        public int MaxNumberOfDataSeriesToDisplay { get; set; } = 20;
        public int MaxNumberOfActionsInBucket { get; set; } = 4;
        public TimeSpan CacheStaticContentTimeSpan { get; set; } = new TimeSpan();

    }
}
