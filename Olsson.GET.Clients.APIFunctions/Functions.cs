using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Common.DataContracts.APIFunctionModels;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Customers;
using Olsson.GET.Managers.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;

namespace Olsson.GET.Clients.APIFunctions
{
    public class Functions
    {
        private readonly ILogger _logger;
        private readonly ManagerFactory _managerFactory;

        public Functions(ILogger<Functions> logger, ManagerFactory managerFactory)
        {
            _logger = logger;
            _managerFactory = managerFactory;
        }

        private const string WaterLevelChangeFileName = "Water Level Change";

        //MP 11/17/21 This function should be built out to be a little bit more informative. But for now just use it to see if the API is responsive
        [FunctionName("Health")]
        public HttpResponseMessage Health([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request: Health.");

            return req.CreateResponse(HttpStatusCode.OK, "API is responsive.");
        }

        [FunctionName("RetrieveResult")]
        public HttpResponseMessage RetrieveResult([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, ILogger logger)
        {
            logger.LogInformation("C# HTTP trigger function processed a request: Retrieve Result.");

            string requestBody = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<RetrieveResultModel>(requestBody);

            if (data == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass run details in the request body");
            }

            if (!data.RunId.HasValue)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid run id in the request body");
            }

            if (!data.CustomerId.HasValue)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid customer id in the request body");
            }

            if (string.IsNullOrEmpty(data.FileName))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a file name to be downloaded in the request body");
            }

            var subType = string.IsNullOrWhiteSpace(data.SubType) ? data.FileDate : data.SubType;

            var runManager = _managerFactory.CreateManager<IRunManager>();

            var runResult = runManager.GetRunResult(data.RunId.Value, data.CustomerId.Value, data.FileName, subType, data.FileExtension);

            if (runResult == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, new
                {
                    RunId = data.RunId.Value,
                    Message = "There is no run associated with the run id provided, the run has not completed, or you do not have access to view the status of the run"
                });
            }

            return req.CreateResponse(HttpStatusCode.OK, runResult);
        }

        [FunctionName("StartRun")]
        public HttpResponseMessage StartRun(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request: Start Run.");

            NewRunModel data = null;
            var inputFiles = new List<InputFile>();

            var reqContentType = req.Content.Headers.ContentType.MediaType;

            if (reqContentType.Equals("multipart/form-data"))
            {
                var multipart = req.Content.ReadAsMultipartAsync().Result;

                foreach (var content in multipart.Contents)
                {
                    if (content.Headers.ContentDisposition.Name.Equals("\"files\"", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var file = new InputFile
                        {
                            FileContent = content.ReadAsByteArrayAsync().Result,
                            FileName = content.Headers.ContentDisposition.FileName.Replace("\"", "")
                        };

                        inputFiles.Add(file);
                    }
                    else if (content.Headers.ContentDisposition.Name.Equals("\"request\"", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var contentStr = content.ReadAsStringAsync().Result;
                        data = JsonConvert.DeserializeObject<NewRunModel>(contentStr);

                    }
                }
            }
            else
            {
                var contentStr = req.Content.ReadAsStringAsync().Result;
                data = JsonConvert.DeserializeObject<NewRunModel>(contentStr);
            }

            if (data == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass run details in the request body");
            }

            if (!data.CustomerId.HasValue)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid customer id in the request body");
            }

            if (!data.UserId.HasValue)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid user id in the request body");
            }

            if (!data.ModelId.HasValue)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid model id in the request body");
            }

            if (!data.ScenarioId.HasValue)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid scenario id in the request body");
            }

            if (string.IsNullOrEmpty(data.Name))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid name in the request body");
            }

            var customerManager = _managerFactory.CreateManager<ICustomerManager>();
            var runManager = _managerFactory.CreateManager<IRunManager>();

            // ensure customer has access to the model requested
            var customerModel = customerManager.FindModelForCustomer(data.CustomerId.Value, data.ModelId.Value, data.ScenarioId.Value);

            if (customerModel == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass model id and scenario id that you have access to in the request body");
            }

            var containsScenarioFiles = customerModel.Scenarios.First().ScenarioFiles.Length > 0;

            // ensure inputs are supplied
            if (containsScenarioFiles)
            {
                var requiredFiles = customerModel.Scenarios.First().ScenarioFiles.Where(x => x.IsRequired).ToList();

                foreach (var file in requiredFiles)
                {
                    if (!inputFiles.Any(x => x.FileName.Equals(file.ScenarioFileName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        return req.CreateErrorResponse(HttpStatusCode.BadRequest, $"Missing required input file \"{file.ScenarioFileName}\"");
                    }
                }
            }
            else
            {
                switch (customerModel.Scenarios.First().InputControlType)
                {
                    case InputControlType.CanalTable:
                        if (data.RunCanalInputs == null || data.RunCanalInputs.Count == 0)
                        {
                            return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass canal inputs in the request body");
                        }
                        break;
                    case InputControlType.WellMap:
                        if (data.PivotedRunWellInputs == null || data.PivotedRunWellInputs.Count == 0)
                        {
                            return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass well inputs in the request body");
                        }
                        break;
                    case InputControlType.ZoneMap:
                        if (data.RunZoneInputs == null || data.RunZoneInputs.Count == 0)
                        {
                            return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass zone inputs in the request body");
                        }
                        break;
                    case InputControlType.ParticleMap:
                        if (data.RunWellParticleInputs == null || data.RunWellParticleInputs.Count == 0)
                        {
                            return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass particle inputs in the request body");
                        }
                        break;
                }
            }

            // create the run
            var saveResult = runManager.CreateOrUpdateRun(new Run
            {
                CreatedDate = DateTime.UtcNow,
                CustomerID = data.CustomerId.Value,
                FileStorageLocator = Guid.NewGuid().ToString(),
                RunName = data.Name,
                ModelID = data.ModelId.Value,
                ScenarioID = data.ScenarioId.Value,
                UserID = data.UserId.Value,
                RunStatusID = RunStatus.Created.RunStatusID,
                InputVolumeUnitID = data.InputVolumeType,
                OutputVolumeUnitID = data.OutputVolumeType,
                IsDifferential = data.IsDifferential ?? true,
                RunDescription = data.Description,
            });

            // save the inputs
            if (containsScenarioFiles)
            {
                saveResult.Scenario = customerModel.Scenarios.First();

                foreach (var inputFile in inputFiles)
                {
                    runManager.UploadInputFile(saveResult, inputFile.FileName, inputFile.FileContent);
                }
            }
            else
            {
                switch (customerModel.Scenarios.First().InputControlType)
                {
                    case InputControlType.CanalTable:
                        runManager.UpdateInputCanalData(saveResult, data.RunCanalInputs.ToArray());
                        break;
                    case InputControlType.WellMap:
                        runManager.UpdateInputWellData(data.PivotedRunWellInputs.ToArray(), saveResult.RunID, data.CustomerId.Value);
                        break;
                    case InputControlType.ZoneMap:
                        runManager.UpdateInputZoneData(data.RunZoneInputs.ToArray(), saveResult.RunID, data.CustomerId.Value);
                        break;
                    case InputControlType.ParticleMap:
                        runManager.UpdateInputWellParticleData(data.RunWellParticleInputs.ToArray(), saveResult.RunID, data.CustomerId.Value);
                        break;
                }
            }

            // start the run
            var queueRunSuccess = runManager.QueueRun(saveResult.RunID, data.CustomerId.Value, data.CreateMaps);

            var response = new RunResponseModel
            {
                RunId = saveResult.RunID,
                RunStatus = saveResult.RunStatus,
                Message = queueRunSuccess ? "Run is successfully queued" : "An error is encountered when trying to start a run"
            };

            return req.CreateResponse(HttpStatusCode.OK, response);
        }

        private static int GetDefaultInputVolumeType(int scenarioID)
        {
            switch (scenarioID)
            {
                case 1:
                case 2:
                case 6:
                case 11:
                    return VolumeUnit.Gallon.VolumeUnitID;
                case 3:
                case 4:
                case 7:
                case 8:
                case 10:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                    return VolumeUnit.AcreFeet.VolumeUnitID;
                default:
                    return VolumeUnit.CubicFeet.VolumeUnitID;
            }
        }

        private static VolumeUnitEnum GetDefaultOutputVolumeType(int scenarioId)
        {
            return VolumeUnitEnum.AcreFeet;
        }

        [FunctionName("GetRunStatus")]
        public HttpResponseMessage GetRunStatus(
          [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request: Get Run Status.");

            string requestBody = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<RunDetailModel>(requestBody);

            if (data == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass run details in the request body");
            }

            if (!data.CustomerId.HasValue)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid customer id in the request body");
            }

            if (!data.RunId.HasValue)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid run id in the request body");
            }

            var runManager = _managerFactory.CreateManager<IRunManager>();

            var runStatus = runManager.GetRunStatus(data.RunId.Value, data.CustomerId.Value);

            if (runStatus == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, new RunResponseModel
                {
                    RunId = data.RunId.Value,
                    RunStatus = null,
                    Message = "There is no run associated with the run id provided or you do not have access to view the status of the run"
                });
            }

            var response = new RunResponseModel
            {
                RunId = data.RunId.Value,
                RunStatus = runStatus,
                Message = string.Empty
            };

            return req.CreateResponse(HttpStatusCode.OK, response);
        }

        [FunctionName("GetAvailableRunResults")]
        public HttpResponseMessage GetAvailableRunResults([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request: Get Available Run Results.");

            string requestBody = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<RunDetailModel>(requestBody);

            if (data == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass run details in the request body");
            }

            if (!data.CustomerId.HasValue)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid customer id in the request body");
            }

            if (!data.RunId.HasValue)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid run id in the request body");
            }

            var runManager = _managerFactory.CreateManager<IRunManager>();

            var availableRunResults = runManager.FindAvailableRunResults(data.RunId.Value, data.CustomerId.Value);

            if (availableRunResults == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, new
                {
                    RunId = data.RunId.Value,
                    Message = "There is no run associated with the run id provided, the run has not completed, or you do not have access to view the status of the run"
                });
            }

            return req.CreateResponse(HttpStatusCode.OK, availableRunResults);
        }

        private class AvailableRunResult
        {
            public string FileName { get; set; }
            public List<string> AvailableDates { get; set; }
            public string SomethingElse { get; set; }
        }

        private class AvailableRunResultHelper
        {
            public string FileName { get; set; }
            public List<string> AvailableDates { get; set; }
            public string SomethingElse { get; set; }
        }

        [FunctionName("GetRuns")]
        public HttpResponseMessage GetRuns([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request: Get Runs.");

            string requestBody = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<CustomerRunModel>(requestBody);

            var runManager = _managerFactory.CreateManager<IRunManager>();

            var runs = runManager.GetRuns(data.CustomerId);

            return req.CreateResponse(HttpStatusCode.OK, runs);
        }
    }
}
