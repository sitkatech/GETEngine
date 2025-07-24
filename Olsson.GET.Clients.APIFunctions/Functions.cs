using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Olsson.GET.Accessors;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.APIFunctionModels;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.Shared;
using Olsson.GET.Common.Utilities;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Customers;
using Olsson.GET.Managers.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;

namespace Olsson.GET.Clients.APIFunctions
{
    public class Functions
    {
        private readonly ManagerFactory _managerFactory;
        private static readonly ILogger _logger = Logging.GetLogger<Functions>();
        public Functions(ManagerFactory managerFactory)
        {
            _managerFactory = managerFactory;
        }

        //MP 11/17/21 This function should be built out to be a little bit more informative. But for now just use it to see if the API is responsive
        [FunctionName("Health")]
        [OpenApiOperation(operationId: "Health")]
        [OpenApiSecurity("subscription_key_header", SecuritySchemeType.ApiKey, Name = "Ocp-Apim-Subscription-Key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("subscription_key_query_param", SecuritySchemeType.ApiKey, Name = "subscription-key", In = OpenApiSecurityLocationType.Query)]
        public IActionResult Health([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request: Health.");
            return new OkObjectResult("API is responsive.");
        }

        [FunctionName("RetrieveInput")]
        [OpenApiOperation(operationId: "RetrieveInput")]
        [OpenApiRequestBody("application/json", typeof(RetrieveResultModel), Required = true, Description = "Retrieve Input Model")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(RunResultResponseModel))]
        public IActionResult RetrieveInput(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req)

        {
            _logger.LogInformation("Processing request: Retrieve Input.");
            string requestBody = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<RetrieveResultModel>(requestBody);

            if (data == null || !data.RunId.HasValue || !data.CustomerId.HasValue || string.IsNullOrEmpty(data.FileName))
            {
                return new BadRequestObjectResult("Missing required fields: runId, customerId, or fileName.");
            }

            var runManager = _managerFactory.CreateManager<IRunManager>();
            var runInput = runManager.GetRunInput(data.RunId.Value, data.CustomerId.Value, data.FileName);

            if (runInput == null)
            {
                return new BadRequestObjectResult(new RunResponseModel
                {
                    RunId = data.RunId.Value,
                    Message = "Input file not found or access denied."
                });
            }

            return new OkObjectResult(runInput);
        }

        [FunctionName("RetrieveResult")]
        [OpenApiOperation(operationId: "RetrieveResult")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(RetrieveResultModel), Required = true, Description = "Retrieve Result Model")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(RunResultResponseModel))]
        [OpenApiSecurity("subscription_key_header", SecuritySchemeType.ApiKey, Name = "Ocp-Apim-Subscription-Key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("subscription_key_query_param", SecuritySchemeType.ApiKey, Name = "subscription-key", In = OpenApiSecurityLocationType.Query)]
        public IActionResult RetrieveResult([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, Microsoft.Azure.WebJobs.ExecutionContext context)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request: Retrieve Result.");

            string requestBody = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<RetrieveResultModel>(requestBody);

            if (data == null)
            {
                return new BadRequestObjectResult("Please pass run details in the request body");
            }

            if (!data.RunId.HasValue)
            {
                return new BadRequestObjectResult("Please pass a valid run id in the request body");
            }

            if (!data.CustomerId.HasValue)
            {
                return new BadRequestObjectResult("Please pass a valid customer id in the request body");
            }

            if (string.IsNullOrEmpty(data.FileName))
            {
                return new BadRequestObjectResult("Please pass a file name to be downloaded in the request body");
            }

            var subType = string.IsNullOrWhiteSpace(data.SubType) ? data.FileDate : data.SubType;

            var runManager = _managerFactory.CreateManager<IRunManager>();

            var runResult = runManager.GetRunResult(data.RunId.Value, data.CustomerId.Value, data.FileName, subType, data.FileExtension);

            if (runResult == null)
            {
                return new BadRequestObjectResult(new RunResponseModel()
                {
                    RunId = data.RunId.Value,
                    Message = "There is no run associated with the run id provided, the run has not completed, or you do not have access to view the status of the run"
                });
                
            }
            return new OkObjectResult(runResult);
        }

        [FunctionName("StartRun")]
        [OpenApiOperation(operationId: "StartRun")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(NewRunModel), Required = true, Description = "New Run Model")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(RunResponseModel))]
        [OpenApiSecurity("subscription_key_header", SecuritySchemeType.ApiKey, Name = "Ocp-Apim-Subscription-Key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("subscription_key_query_param", SecuritySchemeType.ApiKey, Name = "subscription-key", In = OpenApiSecurityLocationType.Query)]
        public IActionResult StartRun(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req)
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
                return new BadRequestObjectResult("Please pass run details in the request body");
            }

            if (!data.CustomerId.HasValue)
            {
                return new BadRequestObjectResult("Please pass a valid customer id in the request body");
            }

            if (!data.UserId.HasValue)
            {
                return new BadRequestObjectResult("Please pass a valid user id in the request body");
            }

            if (!data.ModelId.HasValue)
            {
                return new BadRequestObjectResult("Please pass a valid model id in the request body");
            }

            if (!data.ScenarioId.HasValue)
            {
                return new BadRequestObjectResult("Please pass a valid scenario id in the request body");
            }

            if (string.IsNullOrEmpty(data.Name))
            {
                return new BadRequestObjectResult("Please pass a valid name in the request body");
            }

            var customerManager = _managerFactory.CreateManager<ICustomerManager>();
            var runManager = _managerFactory.CreateManager<IRunManager>();

            // ensure customer has access to the model requested
            var customerModel = customerManager.FindModelForCustomer(data.CustomerId.Value, data.ModelId.Value, data.ScenarioId.Value);

            if (customerModel == null)
            {
                return new BadRequestObjectResult("Please pass model id and scenario id that you have access to in the request body");
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
                        return new BadRequestObjectResult($"Missing required input file \"{file.ScenarioFileName}\"");
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
                            return new BadRequestObjectResult("Please pass canal inputs in the request body");
                        }
                        break;
                    case InputControlType.WellMap:
                        if (data.PivotedRunWellInputs == null || data.PivotedRunWellInputs.Count == 0)
                        {
                            return new BadRequestObjectResult("Please pass well inputs in the request body");
                        }
                        break;
                    case InputControlType.ZoneMap:
                        if (data.RunZoneInputs == null || data.RunZoneInputs.Count == 0)
                        {
                            return new BadRequestObjectResult("Please pass zone inputs in the request body");
                        }
                        break;
                    case InputControlType.ParticleMap:
                        if (data.RunWellParticleInputs == null || data.RunWellParticleInputs.Count == 0)
                        {
                            return new BadRequestObjectResult("Please pass particle inputs in the request body");
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
            return new OkObjectResult(response);
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
        [OpenApiOperation(operationId: "GetRunStatus")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(RunDetailModel), Required = true, Description = "Run Detail Model")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(RunResponseModel))]
        [OpenApiSecurity("subscription_key_header", SecuritySchemeType.ApiKey, Name = "Ocp-Apim-Subscription-Key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("subscription_key_query_param", SecuritySchemeType.ApiKey, Name = "subscription-key", In = OpenApiSecurityLocationType.Query)]
        public IActionResult GetRunStatus(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request: Get Run Status.");

            string requestBody = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<RunDetailModel>(requestBody);

            if (data == null)
            {
                return new BadRequestObjectResult("Please pass run details in the request body");
            }

            if (!data.CustomerId.HasValue)
            {
                return new BadRequestObjectResult("Please pass a valid customer id in the request body");
            }

            if (!data.RunId.HasValue)
            {
                return new BadRequestObjectResult("Please pass a valid run id in the request body");
            }

            var runManager = _managerFactory.CreateManager<IRunManager>();

            var runStatus = runManager.GetRunStatus(data.RunId.Value, data.CustomerId.Value);

            if (runStatus == null)
            {
                return new BadRequestObjectResult(new RunResponseModel
                {
                    RunId = data.RunId.Value,
                    RunStatus = null,
                    Message =
                        "There is no run associated with the run id provided or you do not have access to view the status of the run"
                });
               
            }

            var response = new RunResponseModel
            {
                RunId = data.RunId.Value,
                RunStatus = runStatus,
                Message = string.Empty
            };
            return new OkObjectResult(response);
        }

        [FunctionName("GetAvailableRunInputs")]
        [OpenApiOperation(operationId: "GetAvailableRunInputs")]
        [OpenApiRequestBody("application/json", typeof(RunDetailModel), Required = true, Description = "Run Detail Model")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<AvailableRunInput>))]
        public IActionResult GetAvailableRunInputs([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req)
        {
            _logger.LogInformation("Processing request: Get Available Run Inputs.");
            string requestBody = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<RunDetailModel>(requestBody);

            if (data == null || !data.RunId.HasValue || !data.CustomerId.HasValue)
            {
                return new BadRequestObjectResult("Missing required fields: runId or customerId.");
            }

            var runManager = _managerFactory.CreateManager<IRunManager>();
            var availableInputs = runManager.FindAvailableRunInputs(data.RunId.Value, data.CustomerId.Value);

            if (availableInputs == null)
            {
                return new BadRequestObjectResult(new
                {
                    RunId = data.RunId.Value,
                    Message = "Run not found or no inputs available."
                });
            }

            return new OkObjectResult(availableInputs);
        }

        [FunctionName("GetAvailableRunResults")]
        [OpenApiOperation(operationId: "GetAvailableRunResults")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(RunDetailModel), Required = true, Description = "Run Detail Model")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<AvailableRunResult>))]
        [OpenApiSecurity("subscription_key_header", SecuritySchemeType.ApiKey, Name = "Ocp-Apim-Subscription-Key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("subscription_key_query_param", SecuritySchemeType.ApiKey, Name = "subscription-key", In = OpenApiSecurityLocationType.Query)]
        public IActionResult GetAvailableRunResults([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request: Get Available Run Results.");

            string requestBody = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<RunDetailModel>(requestBody);

            if (data == null)
            {
                return new BadRequestObjectResult("Please pass run details in the request body");
            }

            if (!data.CustomerId.HasValue)
            {
                return new BadRequestObjectResult("Please pass a valid customer id in the request body");
            }

            if (!data.RunId.HasValue)
            {
                return new BadRequestObjectResult("Please pass a valid run id in the request body");
            }

            var runManager = _managerFactory.CreateManager<IRunManager>();

            var availableRunResults = runManager.FindAvailableRunResults(data.RunId.Value, data.CustomerId.Value);

            if (availableRunResults == null)
            {
                return new BadRequestObjectResult(new
                {
                    RunId = data.RunId.Value,
                    Message =
                        "There is no run associated with the run id provided, the run has not completed, or you do not have access to view the status of the run"
                });
               
            }
            return new OkObjectResult(availableRunResults);
        }

        private class AvailableRunResult
        {
            public string FileName { get; set; }
            public List<string> AvailableSubTypes { get; set; }
            public List<string> AvailableFileTypes { get; set; }
        }

        [FunctionName("GetRuns")]
        [OpenApiOperation(operationId: "GetRuns")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CustomerRunModel), Required = true, Description = "Customer Run Model")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<RunSummaryReponseModel>))]
        [OpenApiSecurity("subscription_key_header", SecuritySchemeType.ApiKey, Name = "Ocp-Apim-Subscription-Key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("subscription_key_query_param", SecuritySchemeType.ApiKey, Name = "subscription-key", In = OpenApiSecurityLocationType.Query)]
        public IActionResult GetRuns([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request: Get Runs.");

            string requestBody = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<CustomerRunModel>(requestBody);

            var runManager = _managerFactory.CreateManager<IRunManager>();

            var runs = runManager.GetRuns(data.CustomerId);

            return new OkObjectResult(runs);
        }
    }
}
