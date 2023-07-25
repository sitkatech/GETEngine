using Serilog;
using Olsson.GET.Accessors.Customers;
using Olsson.GET.Accessors.Notification;
using Olsson.GET.Accessors.Runs;
using Olsson.GET.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olsson.GET.Managers.Notification
{
    public class NotificationManager : BaseManager, INotificationManager
    {
        private static readonly ILogger Logger = Logging.GetLogger<NotificationManager>();

        public Task SendPasswordResetEmail(string toAddress, string resetLink)
        {
            Logger.Information($"Sending password reset email to {toAddress}");

            return AccessorFactory.CreateAccessor<IEmailAccessor>().SendEmail(
                 new string[] { toAddress },
                 ConfigurationHelper.AppSettings.FromEmail,
                 ConfigurationHelper.AppSettings.NewPasswordTemplateId,
                 new Dictionary<string, string>() { { "%resetpasswordlink%", resetLink } }
                 );
        }

        public async Task SendRunCompletedEmail(int runId, string errorMessage)
        {
            var successful = string.IsNullOrWhiteSpace(errorMessage);

            Logger.Information($"Sending run completed email for RunId {runId} - {(successful ? "Success" : "Failure")}");
            
            var run = AccessorFactory.CreateAccessor<IRunAccessor>().FindRun(runId);
            if (run == null)
            {
                Logger.Warning($"Unable to find RunId {runId}");
                return;
            }

            var baseUri = new Uri(ConfigurationHelper.AppSettings.ApplicationBaseUrl);
            var runUri = new Uri(baseUri, $"Action/ActionDetails?actionID={run.RunID}");

            var emailAccessor = AccessorFactory.CreateAccessor<IEmailAccessor>();
            await emailAccessor.SendEmail(
                new[] { run.User.Email },
                ConfigurationHelper.AppSettings.FromEmail,
                ConfigurationHelper.AppSettings.RunCompletedTemplateId,
                new Dictionary<string, string>
                {
                    { "%runurl%", runUri.ToString() },
                    { "%subjectstatus%", successful ? "Success": "Failure" },
                    { "%bodystatus%", successful ? "successfully": "but encountered errors" },
                    { "%modelname%", run.Model.ModelName },
                    { "%runname%", run.RunName },
                    { "%scenarioname%", run.Scenario.ScenarioName }
                }
            );

            if (!successful && !string.IsNullOrWhiteSpace(ConfigurationHelper.AppSettings.GETSupportEmailAddress))
            {
                var customer = AccessorFactory.CreateAccessor<ICustomerAccessor>().FindCustomerById(run.CustomerID);
                var adminRunUri = new Uri(baseUri, $"Action/AdminActionDetails?actionID={run.RunID}&customerID={run.CustomerID}");
                await emailAccessor.SendEmail(
                    new[] { ConfigurationHelper.AppSettings.GETSupportEmailAddress },
                    ConfigurationHelper.AppSettings.FromEmail,
                    ConfigurationHelper.AppSettings.RunErroredTemplateId,
                    new Dictionary<string, string>
                    {
                        { "%runurl%", adminRunUri.ToString() },
                        { "%customername%", customer?.CustomerName },
                        { "%username%", run.User.FullName },
                        { "%modelname%", run.Model.ModelName },
                        { "%scenarioname%", run.Scenario.ScenarioName },
                        { "%runname%", run.RunName },
                        { "%error%", errorMessage }
                    }
                );
            }

        }
    }
}
