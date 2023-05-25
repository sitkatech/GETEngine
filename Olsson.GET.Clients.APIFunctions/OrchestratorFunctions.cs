using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Runs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.WebUtilities;
using Olsson.GET.Common.DataContracts.APIFunctionModels;
using Newtonsoft.Json;

namespace Olsson.GET.Clients.APIFunctions
{
    public class OrchestratorFunctions
    {
        private readonly ILogger _logger;
        private readonly ManagerFactory _managerFactory;

        public OrchestratorFunctions(ILogger logger, ManagerFactory managerFactory)
        {
            _logger = logger;
            _managerFactory = managerFactory;
        }

        [FunctionName("RunAnalysis")]
        public HttpResponseMessage RunAnalysis([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req)
        {
            _logger.LogInformation("Test to see if changes are being propagated");
            _logger.LogInformation("C# HTTP trigger function processed a request: Run Analysis.");

            // parse query parameter
            var queryParams = QueryHelpers.ParseQuery(req.RequestUri?.Query);

            string runIdStr = queryParams
                .FirstOrDefault(q => String.Compare(q.Key, "RunId", StringComparison.OrdinalIgnoreCase) == 0)
                .Value;

            int runId;

            if (!int.TryParse(runIdStr, out runId))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a valid run id on the query string or in the request body");
            }

            var runManager = _managerFactory.CreateManager<IRunManager>();

            runManager.QueueRunAnalysis(runId);

            var response = new RunResponseModel
            {
                RunId = runId,
                Message = "Run is queued for analysis"
            };

            return req.CreateResponse(HttpStatusCode.OK, response);
        }

        [FunctionName("GenerateOutputs")]
        public HttpResponseMessage GenerateOutputs(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req)
        {

            _logger.LogInformation("C# HTTP trigger function processed a request: Generate Outputs.");

            // parse query parameter
            // parse query parameter
            var queryParams = QueryHelpers.ParseQuery(req.RequestUri?.Query);
            string runIdStr = queryParams
                .FirstOrDefault(q => String.Compare(q.Key, "RunId", StringComparison.OrdinalIgnoreCase) == 0)
                .Value;

            int runId;

            if (!int.TryParse(runIdStr, out runId))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a valid run id on the query string or in the request body");
            }

            var runManager = _managerFactory.CreateManager<IRunManager>();

            runManager.QueueGenerateOutput(runId);

            var response = new RunResponseModel
            {
                RunId = runId,
                Message = "Run is queued for output generation"
            };

            return req.CreateResponse(HttpStatusCode.OK, response);
        }

        [FunctionName("SendRunCompletedNotification")]
        public HttpResponseMessage SendRunCompletedNotification([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request: Send Run Completed Notification.");

            string requestBody = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<NotificationModel>(requestBody);

            if (data == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass run details in the request body");
            }

            if (!data.RunId.HasValue)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid run id in the request body");
            }

            var runManager = _managerFactory.CreateManager<IRunManager>();

            runManager.SendNotification(data.RunId.Value, data.IsSystemFailure, data.Exception);

            var response = new RunResponseModel
            {
                RunId = data.RunId.Value,
                Message = "Notification sent"
            };

            return req.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}
