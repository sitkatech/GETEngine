using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace Olsson.GET.Common.Utilities
{
    public static class ConfigurationHelper
    {
        public static ConnectionStrings ConnectionStrings => new ConnectionStrings();

        public static AppSettings AppSettings => new AppSettings();
    }

    public class ConnectionStrings
    {
        public string GetPrimaryConnectionString => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GETPRIMARYDATABASE")) ?
            Environment.GetEnvironmentVariable("GETPRIMARYDATABASE") : ConfigurationManager.ConnectionStrings["GetPrimaryDatabase"]?.ConnectionString;
        public string AzureStorageAccount => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURESTORAGEACCOUNT")) ?
            Environment.GetEnvironmentVariable("AZURESTORAGEACCOUNT") : ConfigurationManager.ConnectionStrings["AzureStorageAccount"]?.ConnectionString;
    }

    public class AppSettings
    {
        public string SendGridApiKey => ConfigurationManager.AppSettings["SendGridAPIKey"] ?? string.Empty;
        public string FromEmail => ConfigurationManager.AppSettings["FromEmail"] ?? string.Empty;
        public string NewPasswordTemplateId => ConfigurationManager.AppSettings["NewPasswordTemplateId"] ?? string.Empty;
        public string RunCompletedTemplateId => ConfigurationManager.AppSettings["RunCompletedTemplateId"] ?? string.Empty;
        public string RunErroredTemplateId => ConfigurationManager.AppSettings["RunErroredTemplateId"] ?? string.Empty;
        public string PortalGoogleAnalyticsTrackingCode => ConfigurationManager.AppSettings["PortalGoogleAnalyticsTrackingCode"] ?? string.Empty;
        public string PortalApplicationInsightsKey => ConfigurationManager.AppSettings["PortalApplicationInsightsKey"] ?? string.Empty;
        public string BlobStorageModelDataFolder => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BLOBSTORAGEMODELDATAFOLDER")) ?
            Environment.GetEnvironmentVariable("BLOBSTORAGEMODELDATAFOLDER") : ConfigurationManager.AppSettings["BlobStorageModelDataFolder"] ?? string.Empty;
        public string BlobStorageModelOutputsFolder => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BLOBSTORAGEMODELOUTPUTSFOLDER")) ?
            Environment.GetEnvironmentVariable("BLOBSTORAGEMODELOUTPUTSFOLDER") : ConfigurationManager.AppSettings["BlobStorageModelOutputsFolder"] ?? string.Empty;
        public string ModflowDataFolder => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MODFLOWDATAFOLDER")) ?
            Environment.GetEnvironmentVariable("MODFLOWDATAFOLDER") : ConfigurationManager.AppSettings["ModflowDataFolder"] ?? string.Empty;
        public string DockerAgentContainerPath => ConfigurationManager.AppSettings["DockerAgentContainerPath"] ?? string.Empty;
        public string ApplicationBaseUrl => ConfigurationManager.AppSettings["ApplicationBaseUrl"] ?? string.Empty;
        public string GETSupportEmailAddress => ConfigurationManager.AppSettings["GETSupportEmailAddress"] ?? string.Empty;
        public string GoogleMapsAPIKey => ConfigurationManager.AppSettings["GoogleMapsAPIKey"] ?? string.Empty;
        public TimeSpan LoginTimeout => TimeSpan.TryParse(ConfigurationManager.AppSettings["LoginTimeout"], out var result) ? result : TimeSpan.FromDays(60);
        public int TrialRunLimit => int.TryParse(ConfigurationManager.AppSettings["TrialRunLimit"], out var result) ? result : 10;
        public int MaxRunProcessingTimeInHours => int.TryParse(ConfigurationManager.AppSettings["MaxRunProcessingTimeInHours"], out var result) ? result : 12;
        public int ContainerRetentionPeriodInDays => int.TryParse(ConfigurationManager.AppSettings["ContainerRetentionPeriodInDays"], out var result) ? result : 1;
        public int MaxContainerCount => int.TryParse(ConfigurationManager.AppSettings["MaxContainerCount"], out var result) ? result : 90;
        public string APIFunctionCode => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("API_FUNCTION_CODE")) ?
          Environment.GetEnvironmentVariable("API_FUNCTION_CODE") : ConfigurationManager.AppSettings["APIFunctionCode"] ?? string.Empty;
        public string RunAnalysisUrl => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ANALYSIS_URL")) ?
            Environment.GetEnvironmentVariable("ANALYSIS_URL") : ConfigurationManager.AppSettings["RunAnalysisUrl"] ?? string.Empty;
        public string GenerateOutputsUrl => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OUTPUTS_URL")) ?
            Environment.GetEnvironmentVariable("OUTPUTS_URL") : ConfigurationManager.AppSettings["GenerateOutputsUrl"] ?? string.Empty;
        public string SendRunCompletedNotificationUrl => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NOTIFICATION_URL")) ?
            Environment.GetEnvironmentVariable("NOTIFICATION_URL") : ConfigurationManager.AppSettings["SendRunCompletedNotificationUrl"] ?? string.Empty;
        public string GenerateInputsQueueName => ConfigurationManager.AppSettings["GenerateInputsQueueName"] ?? string.Empty;
        public string RunAnalysisQueueName => ConfigurationManager.AppSettings["RunAnalysisQueueName"] ?? string.Empty;
        public string GenerateOutputsQueueName => ConfigurationManager.AppSettings["GenerateOutputsQueueName"] ?? string.Empty;
        public string AzureResourceGroup => ConfigurationManager.AppSettings["AzureResourceGroup"] ?? string.Empty;
        public string AzureRegistryServer => ConfigurationManager.AppSettings["AzureRegistryServer"] ?? string.Empty;
        public string AzureRegistryUsername => ConfigurationManager.AppSettings["AzureRegistryUsername"] ?? string.Empty;
        public string AzureRegistryPassword => ConfigurationManager.AppSettings["AzureRegistryPassword"] ?? string.Empty;
        public string AzureContainerTcpPort => ConfigurationManager.AppSettings["AzureContainerTcpPort"] ?? string.Empty;
        public string AzureContainerVolumeName => ConfigurationManager.AppSettings["AzureContainerVolumeName"] ?? string.Empty;
        public string AzureStorageAccountName => ConfigurationManager.AppSettings["AzureStorageAccountName"] ?? string.Empty;
        public string AzureStorageAccountKey => ConfigurationManager.AppSettings["AzureStorageAccountKey"] ?? string.Empty;
        public string FunctionClientId => ConfigurationManager.AppSettings["FunctionClientId"] ?? string.Empty;
        public string FunctionSecret => ConfigurationManager.AppSettings["FunctionSecret"] ?? string.Empty;
        public string FunctionTenantId => ConfigurationManager.AppSettings["FunctionTenantId"] ?? string.Empty;
        public int DashboardPageRecordCount => int.TryParse(ConfigurationManager.AppSettings["DashboardPageRecordCount"], out var result) ? result : 20;
        public int MaxNumberOfDataSeriesToDisplay => int.TryParse(ConfigurationManager.AppSettings["MaxNumberOfDataSeriesToDisplay"], out var result) ? result : 20;
        public int MaxNumberOfActionsInBucket => int.TryParse(ConfigurationManager.AppSettings["MaxNumberOfActionsInBucket"], out var result) ? result : 4;
        public TimeSpan CacheStaticContentTimeSpan => TimeSpan.TryParse(ConfigurationManager.AppSettings["CacheStaticContentTimeSpan"], out var result) ? result : new TimeSpan();

    }
}
