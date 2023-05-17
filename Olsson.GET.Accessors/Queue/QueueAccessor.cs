using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Olsson.GET.Common.Utilities;
using System;
using System.Net;

namespace Olsson.GET.Accessors.Queue
{
    class QueueAccessor : BaseTableAccessor, IQueueAccessor
    {
        static QueueAccessor()
        {
            //need this to pass validation on the self signed cert
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
        }

        public void CreateGenerateInputsMessage(int runId, TimeSpan? delay)
        {
            var queue = GetQueue(ConfigurationHelper.AppSettings.GenerateInputsQueueName);
            var message = new CloudQueueMessage(runId.ToString());
            queue.AddMessage(message, initialVisibilityDelay: delay);
        }

        public void CreateRunAnalysisMessage(int runId, TimeSpan? delay)
        {
            var queue = GetQueue(ConfigurationHelper.AppSettings.RunAnalysisQueueName);
            var message = new CloudQueueMessage(runId.ToString());
            queue.AddMessage(message, initialVisibilityDelay: delay);
        }

        public void CreateGenerateOutputsMessage(int runId, TimeSpan? delay)
        {
            var queue = GetQueue(ConfigurationHelper.AppSettings.GenerateOutputsQueueName);
            var message = new CloudQueueMessage(runId.ToString());
            queue.AddMessage(message, initialVisibilityDelay: delay);
        }

        private static CloudQueue GetQueue(string queueName)
        {
            var storageAccount = CloudStorageAccount.Parse(ConfigurationHelper.ConnectionStrings.AzureStorageAccount);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(queueName);
            queue.CreateIfNotExists();

            return queue;
        }
    }
}
