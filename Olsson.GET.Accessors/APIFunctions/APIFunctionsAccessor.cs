using Newtonsoft.Json;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.APIFunctionModels;
using Olsson.GET.Common.Utilities;
using System;
using System.Net.Http;
using System.Text;
using Serilog;

namespace Olsson.GET.Accessors.APIFunctions
{
    class APIFunctionsAccessor : BaseTableAccessor, IAPIFunctionsAccessor
    {
        private static readonly ILogger Logger = Logging.GetLogger<APIFunctionsAccessor>();
        public void MakeFunctionCall(string url)
        {
            Logger.Information($"Begin: MakeFunctionCall to url: \"{url}\"");
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = client.GetAsync(url).Result)
                using (HttpContent respContent = response.Content)
                {
                    var tr = respContent.ReadAsStringAsync().Result;
                    Logger.Information($"Finished: MakeFunctionCall to url: \"{url}\" with result \"{tr}\"");
                }
            }
        }

        public void NotificationFunctionCall(int runId, bool isSystemFailure, Exception ex)
        {
            Logger.Information($"Begin: NotificationFunctionCall for RunID: \"{runId}\"");
            var notification = new NotificationModel
            {
                Exception = ex,
                IsSystemFailure = isSystemFailure,
                RunId = runId
            };

            var url = $"{ConfigurationHelper.AppSettings.SendRunCompletedNotificationUrl}?code={ConfigurationHelper.AppSettings.APIFunctionCode}";

            var content = JsonConvert.SerializeObject(notification);

            using (HttpClient client = new HttpClient())
            {
                HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                using (HttpResponseMessage response = client.PostAsync(url, httpContent).Result)
                {
                    using (HttpContent respContent = response.Content)
                    {
                        var tr = respContent.ReadAsStringAsync().Result;
                        Logger.Information($"Finished: NotificationFunctionCall for RunID: \"{runId}\" with response \"{tr}\"");
                    }
                }
            }
        }
    }
}
