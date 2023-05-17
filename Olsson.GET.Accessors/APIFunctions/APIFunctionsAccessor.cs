using Newtonsoft.Json;
using Olsson.GET.Common.DataContracts.APIFunctionModels;
using Olsson.GET.Common.Utilities;
using System;
using System.Net.Http;
using System.Text;

namespace Olsson.GET.Accessors.APIFunctions
{
    class APIFunctionsAccessor : BaseTableAccessor, IAPIFunctionsAccessor
    {
        public void MakeFunctionCall(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = client.GetAsync(url).Result)
                using (HttpContent respContent = response.Content)
                {
                    var tr = respContent.ReadAsStringAsync().Result;
                }
            }
        }

        public void NotificationFunctionCall(int runId, bool isSystemFailure, Exception ex)
        {
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
                    }
                }
            }
        }
    }
}
