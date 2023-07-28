using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Utilities;
using Olsson.GET.Accessors.Runs;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.Shared;
using Olsson.GET.Engines.RunDataParse;
using Olsson.GET.Accessors.Models;
using Olsson.GET.Accessors.Containers;
using Olsson.GET.Accessors.Customers;
using Olsson.GET.Common.Shared.Extensions;
using Olsson.GET.Engines.ModelInputOutputEngines;
using Olsson.GET.Managers.Notification;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.Shared.Enums;
using Olsson.GET.Accessors.Queue;
using Olsson.GET.Accessors.APIFunctions;
using System.IO;
using Olsson.GET.Common.DataContracts.APIFunctionModels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Common.DataContracts.Scenarios;
using Olsson.GET.Engines;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;
using RunBucket = Olsson.GET.Common.DataContracts.Runs.RunBucket;
using User = Olsson.GET.Common.DataContracts.Users.User;
using RunStatus = Olsson.GET.Accessors.EntityFramework.RunStatus;

namespace Olsson.GET.Managers.Runs
{
    public class RunManager : BaseManager, IRunManager
    {
        private static readonly ILogger Logger = Logging.GetLogger<RunManager>();
        private static readonly Regex FileNameParseRegEx = new Regex(@"(?<hidden>!?)(?<id>\d+)\-(?<name>[^\\]+)((?=(?<extension>\.json))|(?=(?<extension>\.kml)))", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private const string WaterLevelChangeFileName = "Water Level Change";
        private const string WaterLevelFileName = "Water Level";
        private const string DrawdownFileName = "Drawdown";
        private const string WaterBudgetByZoneFileName = "Water Budget By Zone";
        private const string WaterBudgetByBudgetItemFileName = "Water Budget By Budget Item";
        private static readonly List<int> RunStatusesToDelete = new List<int> { RunStatus.Complete.RunStatusID, RunStatus.InvalidInput.RunStatusID, RunStatus.InvalidOutput.RunStatusID, RunStatus.SystemError.RunStatusID };
        private static readonly List<int> FinishedStatuses = new List<int> { RunStatus.Complete.RunStatusID, RunStatus.InvalidOutput.RunStatusID, RunStatus.SystemError.RunStatusID, RunStatus.InvalidInput.RunStatusID, RunStatus.HasDryCells.RunStatusID };
        private static readonly int MaxNumRestarts = 1;
        private static readonly decimal ContainerDefaultMemory = 5.0m;
        public Run CreateOrUpdateRun(Run run)
        {
            Logger.LogInformation("Creating or Updating run");

            if (!AccessorFactory.CreateAccessor<ICustomerAccessor>().FindAllModelsForCustomer(run.CustomerID)
                .Select(m => m.ModelID).Contains(run.ModelID))
            {
                throw new Exception($"Customer {run.CustomerID} can't create run for model {run.ModelID}");
            }

            return AccessorFactory.CreateAccessor<IRunAccessor>().CreateOrUpdateRun(run);
        }

        public Run FindRun(int runId, int customerId, bool includeHiddenFiles = false)
        {
            Logger.LogInformation($"Finding run {runId} for customer {customerId}");

            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            var run = AccessorFactory.CreateAccessor<IRunAccessor>().FindRun(runId, customerId);

            if (FinishedStatuses.Contains(run.RunStatusID))
            {
                var files = blobFileAccessor.GetFilesInDirectory(OutputDirectoryPathForRun(run), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
                run.AvailableRunResults = files.Select(a => FileNameParseRegEx.Match(a))
                                               .Where(a => a.Success)
                                               .Where(a => includeHiddenFiles ? true : string.IsNullOrWhiteSpace(a.Groups["hidden"].Value))
                                               .Select(a => new RunResultListItem
                                               {
                                                   RunResultId = int.Parse(a.Groups["id"].Value),
                                                   RunResultName = a.Groups["name"].Value,
                                                   RunResultFileExtension = a.Groups["extension"].Value
                                               }).ToList();
            }

            if (run.Scenario.ScenarioFiles != null && run.Scenario.ScenarioFiles.Length > 0)
            {
                var files = blobFileAccessor.GetFilesInDirectory(StorageLocations.InputFolderPathForRun(run.FileStorageLocator),
                    ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;

                foreach (var scenarioFile in run.Scenario.ScenarioFiles)
                {
                    scenarioFile.Uploaded = files.Any(x => x.Equals(scenarioFile.ScenarioFileName, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            var runInputData = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedCanalInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            if (runInputData != null)
            {
                run.CanalRunInputs =
                    JsonConvert.DeserializeObject<List<RunCanalInput>>(Encoding.UTF8.GetString(runInputData));
            }
            else if (run.Scenario.InputControlType == InputControlType.CanalTable && !IsCustomInput(run))
            {
                //we can build a template of inputs from the model specs
                run.CanalRunInputs = BuildCanalInputsForRun(run);

                //save them
                UpdateInputCanalData(run, run.CanalRunInputs.ToArray());
            }

            var wellMapInputData = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedWellInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            if (wellMapInputData != null)
            {
                run.WellMapInputs = JsonConvert.DeserializeObject<RunWellInput[]>(Encoding.UTF8.GetString(wellMapInputData)).ToList();

                run.PivotedWellMapInputs = BuildWellPivotedInputData(run.WellMapInputs.ToArray(), run).ToList();
            }

            var runZoneInputData = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedZoneInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            if (runZoneInputData != null)
            {
                run.RunZoneInputs = JsonConvert.DeserializeObject<RunZoneInput[]>(Encoding.UTF8.GetString(runZoneInputData)).ToList();
            }

            var wellParticleMapInputData = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedWellParticleInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            if (wellParticleMapInputData != null)
            {
                run.RunWellParticleInputs = JsonConvert.DeserializeObject<RunWellParticleInput[]>(Encoding.UTF8.GetString(wellParticleMapInputData)).ToList();
            }

            return run;
        }

        public Run FindRun(int runId, bool includeHiddenFiles = false)
        {
            Logger.LogInformation($"Finding run {runId}");

            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            var run = AccessorFactory.CreateAccessor<IRunAccessor>().FindRun(runId);

            if (FinishedStatuses.Contains(run.RunStatusID))
            {
                var files = blobFileAccessor.GetFilesInDirectory(OutputDirectoryPathForRun(run), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
                run.AvailableRunResults = files.Select(a => FileNameParseRegEx.Match(a))
                                               .Where(a => a.Success)
                                               .Where(a => includeHiddenFiles || string.IsNullOrWhiteSpace(a.Groups["hidden"].Value))
                                               .Select(a => new RunResultListItem
                                               {
                                                   RunResultId = int.Parse(a.Groups["id"].Value),
                                                   RunResultName = a.Groups["name"].Value,
                                                   RunResultFileExtension = a.Groups["extension"].Value
                                               }).ToList();
            }

            if (run.Scenario.ScenarioFiles != null && run.Scenario.ScenarioFiles.Length > 0)
            {
                var files = blobFileAccessor.GetFilesInDirectory(StorageLocations.InputFolderPathForRun(run.FileStorageLocator),
                    ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;

                foreach (var scenarioFile in run.Scenario.ScenarioFiles)
                {
                    scenarioFile.Uploaded = files.Any(x => x.Equals(scenarioFile.ScenarioFileName, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            var runInputData = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedCanalInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            if (runInputData != null)
            {
                run.CanalRunInputs =
                    JsonConvert.DeserializeObject<List<RunCanalInput>>(Encoding.UTF8.GetString(runInputData));
            }
            else if (run.Scenario.InputControlType == InputControlType.CanalTable && !IsCustomInput(run))
            {
                //we can build a template of inputs from the model specs
                run.CanalRunInputs = BuildCanalInputsForRun(run);

                //save them
                UpdateInputCanalData(run, run.CanalRunInputs.ToArray());
            }

            var wellMapInputData = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedWellInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            if (wellMapInputData != null)
            {
                run.WellMapInputs = (JsonConvert.DeserializeObject<RunWellInput[]>(Encoding.UTF8.GetString(wellMapInputData))).ToList();

                run.PivotedWellMapInputs = BuildWellPivotedInputData(run.WellMapInputs.ToArray(), run).ToList();
            }

            var runZoneInputData = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedZoneInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            if (runZoneInputData != null)
            {
                run.RunZoneInputs = (JsonConvert.DeserializeObject<RunZoneInput[]>(Encoding.UTF8.GetString(runZoneInputData))).ToList();
            }

            var wellParticleMapInputData = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedWellParticleInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            if (wellParticleMapInputData != null)
            {
                run.RunWellParticleInputs = (JsonConvert.DeserializeObject<RunWellParticleInput[]>(Encoding.UTF8.GetString(wellParticleMapInputData))).ToList();
            }

            return run;
        }


        public List<AvailableRunResult> FindAvailableRunResults(int runId, int customerId)
        {
            Logger.LogInformation($"Finding run {runId} for customer {customerId}");

            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            var run = AccessorFactory.CreateAccessor<IRunAccessor>().FindRun(runId, customerId);

            if (run == null || !FinishedStatuses.Contains(run.RunStatusID))
            {
                return null;
            }

            var result = new List<AvailableRunResult>();

            var files = blobFileAccessor.GetFilesInDirectory(OutputDirectoryPathForRun(run), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result
                .Select(a => new { Match = FileNameParseRegEx.Match(a), FullName = a })
                .Where(a => a.Match.Success)
                .Select(a => new
                {
                    IsHidden = !string.IsNullOrWhiteSpace(a.Match.Groups["hidden"].Value),
                    FileName = a.Match.Groups["name"].Value,
                    Extension = a.Match.Groups["extension"].Value,
                    a.FullName
                })
                .GroupBy(a => a.FileName)
                .ToList();
            foreach (var file in files)
            {
                if (!file.First().IsHidden)
                {
                    var availableRunResult = new AvailableRunResult
                    {
                        FileName = file.Key,
                        AvailableFileTypes = file.Select(a => a.Extension).Distinct().ToList()
                    };
                    if (IsWaterBudgetZoneFile(file.Key) || IsWaterBudgetItemFile(file.Key))
                    {
                        var fileData = blobFileAccessor.GetFile(OutputFilePathForRun(run.FileStorageLocator, file.First().FullName), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
                        var fileDetails = JsonConvert.DeserializeObject<WaterBudgetResultHelper>(Encoding.UTF8.GetString(fileData));

                        availableRunResult.AvailableSubTypes = fileDetails.RelatedResultOptions.Select(a => a.Label).ToList();

                        availableRunResult.AvailableFileTypes = files
                            .Where(a => availableRunResult.AvailableSubTypes.Any(b => string.Equals(a.Key, b, StringComparison.OrdinalIgnoreCase)))
                            .SelectMany(a => a)
                            .Select(a => a.Extension)
                            .Distinct()
                            .ToList();
                    }
                    else if (IsWaterLevelChangeFile(file.Key) || IsWaterLevelFile(file.Key) || IsDrawdownFile(file.Key))
                    {
                        var fileData = blobFileAccessor.GetFile(OutputFilePathForRun(run.FileStorageLocator, file.First().FullName), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
                        var fileDetails = JsonConvert.DeserializeObject<WaterChangeResultHelper>(Encoding.UTF8.GetString(fileData));

                        availableRunResult.AvailableSubTypes = fileDetails.ResultSets.First().MapData.AvailableStressPeriods.Select(a => a.Label).ToList();

                        availableRunResult.AvailableFileTypes = files
                            .Where(a => availableRunResult.AvailableSubTypes.Any(b => string.Equals(a.Key, b, StringComparison.OrdinalIgnoreCase)))
                            .SelectMany(a => a)
                            .Select(a => a.Extension)
                            .Distinct()
                            .ToList();
                    }
                    result.Add(availableRunResult);
                }
            }

            return result;
        }

        private class FileDetailsHelper
        {
            public bool IsHidden { get; set; }
            public string FileName { get; set; }
            public string Extension { get; set; }
            public string FullName { get; set; }
        }

        private static bool IsWaterBudgetZoneFile(string fileName)
        {
            return string.Equals(fileName, WaterBudgetByZoneFileName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsWaterBudgetItemFile(string fileName)
        {
            return string.Equals(fileName, WaterBudgetByBudgetItemFileName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsWaterLevelChangeFile(string fileName)
        {
            return string.Equals(fileName, WaterLevelChangeFileName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsWaterLevelFile(string fileName)
        {
            return string.Equals(fileName, WaterLevelFileName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDrawdownFile(string fileName)
        {
            return string.Equals(fileName, DrawdownFileName, StringComparison.OrdinalIgnoreCase);
        }

        public RunResultResponseModel GetRunResult(int runId, int customerId, string fileName, string subType, string fileType)
        {
            Logger.LogInformation($"Finding run results {fileName}/{subType}{fileType} for run {runId} for customer {customerId}");

            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            var run = AccessorFactory.CreateAccessor<IRunAccessor>().FindRun(runId, customerId);

            if (run == null || !FinishedStatuses.Contains(run.RunStatusID))
            {
                return null;
            }

            var allFileData = GetFileDetails(blobFileAccessor, run);
            var fileData = allFileData.SingleOrDefault(a => string.Equals(a.Key, fileName));

            if (fileData == null)
            {
                return null;
            }

            fileType = string.IsNullOrWhiteSpace(fileType) ? ".json" : fileType.ToLower();

            if (IsWaterLevelChangeFile(fileName) || IsWaterLevelFile(fileName) || IsDrawdownFile(fileName))
            {
                return FindHeatMapData(runId, subType, fileType, allFileData, blobFileAccessor, run, fileData);
            }

            if ((IsWaterBudgetItemFile(fileName) || IsWaterBudgetZoneFile(fileName)) && !string.IsNullOrWhiteSpace(subType))
            {
                fileData = allFileData.SingleOrDefault(a => string.Equals(a.Key, subType));

                if (fileData == null)
                {
                    return null;
                }
            }

            var matchingType = fileData.FirstOrDefault(a => a.Extension == fileType);
            if (matchingType == null)
            {
                return null;
            }

            return new RunResultResponseModel
            {
                RunId = runId,
                FileDetails = Encoding.UTF8.GetString(blobFileAccessor.GetFile(
                    OutputFilePathForRun(run.FileStorageLocator, matchingType.FullName),
                    ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result)
            };
        }

        public List<Run> FindRunsByModelId(int modelId)
        {
            var runs = AccessorFactory.CreateAccessor<IRunAccessor>().FindRunsByModelId(modelId);
            return runs;
        }

        public List<Run> List()
        {
            var runs = AccessorFactory.CreateAccessor<IRunAccessor>().List();
            return runs;
        }

        public List<Run> FindRunsById(List<int> runIDs)
        {
            Logger.LogInformation("Finding runs for report generator");
            return AccessorFactory.CreateAccessor<IRunAccessor>().FindRunsById(runIDs);
        }

        public Run FindRunById(int selectedModelID)
        {
            Logger.LogInformation("Finding runs for report generator");
            return AccessorFactory.CreateAccessor<IRunAccessor>().FindRunById(selectedModelID);
        }

        private RunResultResponseModel FindHeatMapData(int runId, string subType, string fileType, List<IGrouping<string, FileDetailsHelper>> allFileData, IBlobFileAccessor blobFileAccessor, Run run, IGrouping<string, FileDetailsHelper> fileData)
        {
            if (string.IsNullOrWhiteSpace(subType))
            {
                //the subType was not specified so use the first period
                return FindWaterChangeDataForFirstPeriod(runId, subType, fileType, blobFileAccessor, run, fileData);
            }

            var subFile = allFileData.FirstOrDefault(a => string.Equals(a.Key, subType, StringComparison.OrdinalIgnoreCase));
            if (subFile != null)
            {
                var matchingType = subFile.FirstOrDefault(a => a.Extension == fileType);
                if (matchingType == null)
                {
                    return null;
                }

                return new RunResultResponseModel
                {
                    RunId = runId,
                    FileDetails = System.Text.Encoding.UTF8.GetString(blobFileAccessor.GetFile(
                        OutputFilePathForRun(run.FileStorageLocator, matchingType.FullName),
                        ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result)
                };
            }

            //it may be the first period
            return FindWaterChangeDataForFirstPeriod(runId, subType, fileType, blobFileAccessor, run, fileData);
        }

        private RunResultResponseModel FindWaterChangeDataForFirstPeriod(int runId, string subType, string fileType, IBlobFileAccessor blobFileAccessor, Run run, IGrouping<string, FileDetailsHelper> fileData)
        {
            if (fileType != ".json" && fileType != ".kml")
            {
                return null;
            }

            var mainFileData = blobFileAccessor.GetFile(OutputFilePathForRun(run.FileStorageLocator, fileData.First().FullName), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            var mainFileString = Encoding.UTF8.GetString(mainFileData);
            var fileDetails = JsonConvert.DeserializeObject<WaterChangeResultHelper>(mainFileString);

            if (!string.IsNullOrWhiteSpace(subType) && !string.Equals(fileDetails.RunResultName, subType, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (fileType == ".json")
            {
                return new RunResultResponseModel
                {
                    RunId = runId,
                    FileDetails = mainFileString
                };
            }

            //must be fileType == ".kml"
            if (string.IsNullOrWhiteSpace(fileDetails.ResultSets.First().MapData.KmlString))
            {
                return null;
            }
            return new RunResultResponseModel
            {
                RunId = runId,
                FileDetails = fileDetails.ResultSets.First().MapData.KmlString
            };
        }

        private List<IGrouping<string, FileDetailsHelper>> GetFileDetails(IBlobFileAccessor blobFileAccessor, Run run)
        {
            return blobFileAccessor.GetFilesInDirectory(OutputDirectoryPathForRun(run), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result
                .Select(a => new { Match = FileNameParseRegEx.Match(a), FullName = a })
                .Where(a => a.Match.Success)
                .Select(a => new FileDetailsHelper
                {
                    IsHidden = !string.IsNullOrWhiteSpace(a.Match.Groups["hidden"].Value),
                    FileName = a.Match.Groups["name"].Value,
                    Extension = a.Match.Groups["extension"].Value,
                    FullName = a.FullName
                })
                .GroupBy(a => a.FileName)
                .ToList();
        }

        private class WaterBudgetResultHelper
        {
            public List<RelatedResultOption> RelatedResultOptions { get; set; }
        }

        private class WaterChangeResultHelper
        {
            public List<WaterChangeResultSetHelper> ResultSets { get; set; }
            public string RunResultName { get; set; }
        }

        private class WaterChangeResultSetHelper
        {
            public WaterChangeMapDataHelper MapData { get; set; }
        }

        private class WaterChangeMapDataHelper
        {
            public string KmlString { get; set; }
            public List<WaterChangeAvailableStressPeriodHelper> AvailableStressPeriods { get; set; }
        }

        private class WaterChangeAvailableStressPeriodHelper
        {
            public string Label { get; set; }
        }

        private class RelatedResultOption
        {
            public string Label { get; set; }
        }

        public List<Run> FindRuns(int userId, int customerId, RunFilter filter, int pageNum = 0)
        {
            Logger.LogInformation($"Finding runs for user {userId} customer {customerId} for page #{pageNum}");

            var recordCount = ConfigurationHelper.AppSettings.DashboardPageRecordCount;

            var skip = pageNum * recordCount;

            return AccessorFactory.CreateAccessor<IRunAccessor>().FindRuns(userId, customerId, filter, skip, recordCount);
        }

        public List<RunSummaryReponseModel> GetRuns(int customerId)
        {
            var runs = AccessorFactory.CreateAccessor<IRunAccessor>().GetRuns(customerId);

            return runs.Select(x => new RunSummaryReponseModel
            {
                CreatedDate = x.CreatedDate,
                RunId = x.RunID,
                RunName = x.RunName,
                Status = RunStatus.AllLookupDictionary[x.RunStatusID].RunStatusDisplayName,
                UserId = x.UserID
            }).ToList();
        }

        public int FindRunsCount(int customerId, RunFilter filter)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().FindRunsCount(customerId, filter);
        }

        public bool DeleteRun(int runId, int customerId)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().DeleteRun(runId, customerId);
        }

        public Run DuplicateRun(int runId, int customerId, int userId)
        {
            var accessor = AccessorFactory.CreateAccessor<IRunAccessor>();
            var run = accessor.FindRun(runId, customerId);

            if (run == null)
            {
                throw new Exception($"Invalid run id: {runId}");
            }

            if (!AccessorFactory.CreateAccessor<ICustomerAccessor>().FindAllModelsForCustomer(run.CustomerID)
                .Select(m => m.ModelID).Contains(run.ModelID))
            {
                throw new Exception($"Customer {run.CustomerID} can't create run for model {run.ModelID}");
            }

            var originalLocator = run.FileStorageLocator;

            run.RunID = 0; //id set to default will force a create instead of update
            run.RunStatusID = RunStatus.Created.RunStatusID;

            var newName = run.RunName;

            if (newName.Contains("- Copy"))
            {
                newName = newName.Substring(0, newName.IndexOf("- Copy"));
            }

            newName = $"{newName} - Copy {DateTime.UtcNow:MM/dd/yy H:mm}";

            run.RunName = newName;
            run.UserID = userId;
            run.CreatedDate = DateTime.UtcNow;
            run.FileStorageLocator = Guid.NewGuid().ToString();
            run.ImageID = null;
            run.Image = null;
            run.ProcessingStartDate = null;
            run.ProcessingEndDate = null;
            run.InputVolumeUnitID = run.InputVolumeUnitID;
            run.OutputVolumeUnitID = run.OutputVolumeUnitID;

            run = accessor.CreateOrUpdateRun(run);

            var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            //Copy canal file
            var parsedInputFileContent = fileAccessor.GetFile(ParsedCanalInputFilePathForRun(originalLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            if (parsedInputFileContent != null)
            {
                fileAccessor.SaveFile(ParsedCanalInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, parsedInputFileContent).Wait();
            }

            //copy map input file
            var parsedWellInputFileContent = fileAccessor.GetFile(ParsedWellInputFilePathForRun(originalLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            if (parsedWellInputFileContent != null)
            {
                fileAccessor.SaveFile(ParsedWellInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, parsedWellInputFileContent).Wait();
            }

            //copy zone input file
            var parsedZoneInputFileContent = fileAccessor.GetFile(ParsedZoneInputFilePathForRun(originalLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            if (parsedZoneInputFileContent != null)
            {
                fileAccessor.SaveFile(ParsedZoneInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, parsedZoneInputFileContent).Wait();
            }

            //copy map particle input file
            var parsedWellParticleInputFileContent = fileAccessor.GetFile(ParsedWellParticleInputFilePathForRun(originalLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            if (parsedWellParticleInputFileContent != null)
            {
                fileAccessor.SaveFile(ParsedWellParticleInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, parsedWellParticleInputFileContent).Wait();
            }

            return run;
        }

        public bool RenameRun(int runId, int customerId, string newName)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().RenameRun(runId, customerId, newName);
        }

        public bool ChangeRunDescription(int runId, int customerId, string newDescription)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().ChangeRunDescription(runId, customerId, newDescription);
        }

        public RunBucket FindRunBucket(int bucketId, int customerId)
        {
            Logger.LogInformation($"Finding action bucket with ID {bucketId} for customer {customerId}");
            var runBucket = AccessorFactory.CreateAccessor<IRunAccessor>().FindRunBucket(bucketId, customerId);
            runBucket.AvailableRunResults = new List<RunResultListItem>();

            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            foreach (var run in runBucket.Runs)
            {
                if (FinishedStatuses.Contains(run.RunStatusID))
                {
                    var files = blobFileAccessor.GetFilesInDirectory(OutputDirectoryPathForRun(run), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
                    run.AvailableRunResults = files.Select(a => FileNameParseRegEx.Match(a))
                               .Where(a => a.Success)
                               .Where(a => string.IsNullOrWhiteSpace(a.Groups["hidden"].Value))
                               .Select(a => new RunResultListItem
                               {
                                   RunResultId = int.Parse(a.Groups["id"].Value),
                                   RunResultName = a.Groups["name"].Value,
                                   RunResultFileExtension = a.Groups["extension"].Value
                               }).ToList();

                    foreach (var runResult in run.AvailableRunResults)
                    {
                        if (runBucket.AvailableRunResults.FindIndex(x => x.RunResultName == runResult.RunResultName) < 0)
                        {
                            runBucket.AvailableRunResults.Add(runResult);
                        }
                    }
                }
            }

            runBucket.AvailableRunResults = runBucket.AvailableRunResults.Where(x => x.RunResultName != "List File Output" &&
                x.RunResultName != "Water Level Change" &&
                x.RunResultName != "Water Level" &&
                x.RunResultName != "Drawdown").ToList();

            return runBucket;
        }

        public List<RunBucket> GetRunBuckets(int userId, int customerId)
        {
            Logger.LogInformation($"Finding action buckets for user {userId} customer {customerId}");
            return AccessorFactory.CreateAccessor<IRunAccessor>().GetRunBuckets(userId, customerId); ;
        }


        public RunBucket CreateOrUpdateRunBucket(RunBucket runBucket)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().CreateOrUpdateRunBucket(runBucket); ;
        }

        public bool RenameRunBucket(int bucketId, int customerId, string newName)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().RenameRunBucket(bucketId, customerId, newName);
        }

        public bool ChangeRunBucketDescription(int bucketId, int customerId, string newDescription)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().ChangeRunBucketDescription(bucketId, customerId, newDescription);
        }

        public bool DeleteRunBucket(int bucketId, int customerId)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().DeleteRunBucket(bucketId, customerId); ;
        }

        public bool AddRunToRunBucket(int runId, int customerId, int bucketId)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().AddRunToRunBucket(runId, customerId, bucketId); ;
        }

        public bool RemoveRunFromRunBucket(int runId, int customerId, int bucketId)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().RemoveRunFromRunBucket(runId, customerId, bucketId); ;
        }

        public bool DuplicateRunBucket(int bucketId, int customerId, int userId)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().DuplicateRunBucket(bucketId, customerId, userId);
        }

        public RunCanalInputParseResult ProcessRunInputFile(Run run, byte[] fileContent)
        {
            //parse and preview
            var parseResult = EngineFactory.CreateEngine<IRunDataParseEngine>().ParseCanalRunDataFromFile(fileContent, run.Model);

            //save serialized and parsed result
            if (parseResult.Success)
            {
                //update run with new file name
                AccessorFactory.CreateAccessor<IRunAccessor>().CreateOrUpdateRun(run);

                var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

                //save parsed file                
                fileAccessor.SaveFile(ParsedCanalInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parseResult.RunInputs))).Wait();
            }

            return parseResult;
        }

        public RunWellInputParseResult ProcessWellRunInputFile(Run run, byte[] fileContent)
        {
            //parse and preview
            var parseResult = EngineFactory.CreateEngine<IRunDataParseEngine>().ParseWellRunDataFromFile(fileContent, run.Model);

            //save serialized and parsed result
            if (parseResult.Success)
            {
                //update run with new file name
                AccessorFactory.CreateAccessor<IRunAccessor>().CreateOrUpdateRun(run);

                var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

                //save parsed file                
                fileAccessor.SaveFile(ParsedWellInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parseResult.RunInputs))).Wait();
            }

            return parseResult;
        }

        public RunWellParticleInputParseResult ProcessWellParticleRunInputFile(Run run, byte[] fileContent)
        {
            //parse and preview
            var parseResult = EngineFactory.CreateEngine<IRunDataParseEngine>().ParseWellParticleRunDataFromFile(fileContent, run.Model);

            //save serialized and parsed result
            if (parseResult.Success)
            {
                //update run with new file name
                AccessorFactory.CreateAccessor<IRunAccessor>().CreateOrUpdateRun(run);

                var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

                //save parsed file                
                fileAccessor.SaveFile(ParsedWellParticleInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parseResult.RunInputs))).Wait();
            }

            return parseResult;
        }

        public async Task<bool> UploadInputFile(Run run, string name, byte[] fileContent)
        {
            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            if (run.Scenario.ScenarioFiles.Any(x => x.ScenarioFileName.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            {
                await blobFileAccessor.SaveFile(StorageLocations.InputFilePathForRun(run.FileStorageLocator, name), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, fileContent);

                return true;
            }

            return false;
        }

        public bool DeleteInputFile(string fileLocator, string filename)
        {
            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            blobFileAccessor.DeleteFile(StorageLocations.InputFilePathForRun(fileLocator, filename), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Wait();

            return true;
        }

        public bool UpdateInputCanalData(Run run, RunCanalInput[] data)
        {
            var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            //save parsed inputs                
            fileAccessor.SaveFile(ParsedCanalInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data))).Wait();

            return true;
        }

        public byte[] FindCanalRunInputFile(Run run)
        {
            Logger.LogInformation($"Finding input file for run {run.RunID}");

            var data = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedCanalInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;

            if (data == null)
            {
                return EngineFactory.CreateEngine<IRunDataParseEngine>().CanalRunDataToCsv(BuildCanalInputsForRun(run));
            }

            var csvData = JsonConvert.DeserializeObject<List<RunCanalInput>>(Encoding.UTF8.GetString(data));

            return EngineFactory.CreateEngine<IRunDataParseEngine>().CanalRunDataToCsv(csvData);
        }

        public byte[] FindWellRunInputFile(Run run)
        {
            Logger.LogInformation($"Finding input file for run {run.RunID}");

            var data = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedWellInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;

            var csvData = JsonConvert.DeserializeObject<List<RunWellInput>>(Encoding.UTF8.GetString(data));

            return EngineFactory.CreateEngine<IRunDataParseEngine>().WellRunDataToCsv(csvData);
        }

        public byte[] FindWellParticleRunInputFile(Run run)
        {
            Logger.LogInformation($"Finding input file for run {run.RunID}");

            var data = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedWellParticleInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;

            var csvData = JsonConvert.DeserializeObject<List<RunWellParticleInput>>(Encoding.UTF8.GetString(data));

            return EngineFactory.CreateEngine<IRunDataParseEngine>().WellParticleRunDataToCsv(csvData);
        }

        public byte[] DownloadKmlFile(string fileStorageLocator, string filename, int runResultId)
        {
            var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
            return fileAccessor.GetFile(StorageLocations.OutputFilePathForRun(fileStorageLocator, $"!{runResultId.ToString().PadLeft(3, '0')}-{filename}.kml"), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
        }

        public RunResultDetails FindRunResultDetails(string fileStorageLocator, int runResultId)
        {
            var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
            var files = fileAccessor.GetFilesInDirectory(OutputDirectoryPathForRun(fileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            var file = files.Select(a => new { Name = a, Match = FileNameParseRegEx.Match(a) })
                .FirstOrDefault(a => a.Match.Success &&
                int.Parse(a.Match.Groups["id"].Value) == runResultId &&
                a.Match.Groups["extension"].Value.Equals(".json", StringComparison.InvariantCultureIgnoreCase));

            if (file == null)
            {
                return null;
            }

            var fileData = fileAccessor.GetFile(OutputFilePathForRun(fileStorageLocator, file.Name), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;

            var runResultDetails = JsonConvert.DeserializeObject<RunResultDetails>(System.Text.Encoding.UTF8.GetString(fileData));

            if (runResultDetails.ResultSets != null && runResultDetails.ResultSets.Count > 0 && runResultDetails.ResultSets[0].MapData != null)
            {
                var containsKmlFile = files.Select(a => new { Name = a, Match = FileNameParseRegEx.Match(a) })
                    .Any(a => a.Match.Success &&
                    int.Parse(a.Match.Groups["id"].Value) == runResultId &&
                    a.Match.Groups["extension"].Value.Equals(".kml", StringComparison.InvariantCultureIgnoreCase));

                runResultDetails.ResultSets[0].MapData.ContainsKmlFile = containsKmlFile;
            }

            return runResultDetails;
        }

        public ActionBucketResultDetails FindAggregateRunResultDetails(List<RunResultDisplay> runResultsToDisplay)
        {
            var resultDetails = new ActionBucketResultDetails();
            resultDetails.ResultSets = new List<RunResultSet>();

            var allResults = new List<RunResultSet>();

            foreach (var runResult in runResultsToDisplay)
            {
                var resultSet = new RunResultSet();

                var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
                var files = fileAccessor.GetFilesInDirectory(OutputDirectoryPathForRun(runResult.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
                var file = files.Select(a => new { Name = a, Match = FileNameParseRegEx.Match(a) })
                    .FirstOrDefault(a => a.Match.Success &&
                    int.Parse(a.Match.Groups["id"].Value) == runResult.RunResultId &&
                    a.Match.Groups["extension"].Value.Equals(".json", StringComparison.InvariantCultureIgnoreCase));

                if (file == null)
                {
                    return null;
                }

                var fileData = fileAccessor.GetFile(OutputFilePathForRun(runResult.FileStorageLocator, file.Name), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
                var runResultDetails = JsonConvert.DeserializeObject<RunResultDetails>(System.Text.Encoding.UTF8.GetString(fileData));

                foreach (var resultSetDetails in runResultDetails.ResultSets)
                {
                    var matchingResultSet = resultDetails.ResultSets.SingleOrDefault(x => x.Name == resultSetDetails.Name);

                    foreach (var dataSeries in resultSetDetails.DataSeries)
                    {
                        dataSeries.Name = $"{{\"RunName\":\"{runResult.Name}\",\"DataSeriesName\":\"{dataSeries.Name}\"}}";
                    }

                    if (matchingResultSet == null)
                    {
                        resultDetails.ResultSets.Add(resultSetDetails);
                    }
                    else
                    {
                        matchingResultSet.DataSeries.AddRange(resultSetDetails.DataSeries);
                    }
                }

                if (runResultDetails.RelatedResultOptions != null)
                {
                    resultDetails.RelatedResultOptions = resultDetails.RelatedResultOptions ?? new List<ActionBucketRelatedResultOption>();
                    foreach (var relatedResultOption in runResultDetails.RelatedResultOptions)
                    {
                        resultDetails.RelatedResultOptions.Add(new ActionBucketRelatedResultOption
                        {
                            ResultId = relatedResultOption.Id,
                            RelatedResultName = relatedResultOption.Label,
                            FileStorageLocator = runResult.FileStorageLocator,
                        });
                    }
                }

            }

            foreach (var resultSet in resultDetails.ResultSets)
            {
                resultSet.DataSeries = resultSet.DataSeries.OrderBy(x => JsonConvert.DeserializeObject<ResultSetDataSeries>(x.Name).DataSeriesName).ToList();
            }

            return resultDetails;
        }

        public class ResultSetDataSeries
        {
            public string RunName { get; set; }
            public string DataSeriesName { get; set; }
        }

        public string GetRunResultData(string fileStorageLocator, int runResultId, string fileExtension = ".json")
        {
            var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
            var files = fileAccessor.GetFilesInDirectory(OutputDirectoryPathForRun(fileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            var file = files.Select(a => new { Name = a, Match = FileNameParseRegEx.Match(a) })
                .FirstOrDefault(a => a.Match.Success &&
                int.Parse(a.Match.Groups["id"].Value) == runResultId &&
                a.Match.Groups["extension"].Value.Equals(fileExtension, StringComparison.InvariantCultureIgnoreCase));

            if (file == null)
            {
                return null;
            }

            var fileData = fileAccessor.GetFile(OutputFilePathForRun(fileStorageLocator, file.Name), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;

            return System.Text.Encoding.UTF8.GetString(fileData);
        }

        public bool QueueRun(int runId, int customerId, bool shouldCreateMaps)
        {
            Logger.LogInformation($"Queuing run {runId} for customer {customerId}");

            var accessor = AccessorFactory.CreateAccessor<IRunAccessor>();

            var run = accessor.FindRun(runId, customerId);

            //set which image will execute this run
            var image = AccessorFactory.CreateAccessor<IModelAccessor>().FindImageForModel(run.ModelID);
            run.ImageID = image.ImageID;

            //update status
            run.RunStatusID = RunStatus.Queued.RunStatusID;
            run.ShouldCreateMaps = shouldCreateMaps;

            var result = accessor.CreateOrUpdateRun(run) != null;

            var queueAccessor = AccessorFactory.CreateAccessor<IQueueAccessor>();
            queueAccessor.CreateGenerateInputsMessage(runId, null);

            return result;
        }

        public bool QueueGenerateOutput(int runId)
        {
            var queueAccessor = AccessorFactory.CreateAccessor<IQueueAccessor>();
            queueAccessor.CreateGenerateOutputsMessage(runId, null);

            return true;
        }

        public bool QueueRunAnalysis(int runId)
        {
            var queueAccessor = AccessorFactory.CreateAccessor<IQueueAccessor>();
            queueAccessor.CreateRunAnalysisMessage(runId, null);

            return true;
        }

        public async Task StartContainer(int runId, AgentProcessType processType)
        {
            var containerAccessor = AccessorFactory.CreateAccessor<IContainerAccessor>();
            var queueAccessor = AccessorFactory.CreateAccessor<IQueueAccessor>();

            if (!await containerAccessor.CanQueueNewContainer())
            {
                switch (processType)
                {
                    case AgentProcessType.Input:
                        queueAccessor.CreateGenerateInputsMessage(runId, TimeSpan.FromMinutes(5));
                        break;
                    case AgentProcessType.Analysis:
                        queueAccessor.CreateRunAnalysisMessage(runId, TimeSpan.FromMinutes(5));
                        break;
                    case AgentProcessType.Output:
                        queueAccessor.CreateGenerateOutputsMessage(runId, TimeSpan.FromMinutes(5));
                        break;
                }
                return;
            }

            var runAccessor = AccessorFactory.CreateAccessor<IRunAccessor>();
            var apiFunctionsAccessor = AccessorFactory.CreateAccessor<IAPIFunctionsAccessor>();
            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
            var run = runAccessor.FindRun(runId);

            Exception startException = null;
            var containerStarted = false;

            var sasToken = blobFileAccessor.GetAgentFileShareSASToken();

            // run custom image
            if (processType == AgentProcessType.Input && run.Scenario.InputImage != null)
            {
                blobFileAccessor.CreateFileShare(run.FileStorageLocator).Wait();
                

                //move input files into file storage
                var files = blobFileAccessor.GetFilesInDirectory(StorageLocations.InputFolderPathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;

                foreach (var file in files)
                {
                    blobFileAccessor.CopyFromBlobStorageToFileShare(StorageLocations.InputFilePathForRun(run.FileStorageLocator, file),
                        ConfigurationHelper.AppSettings.BlobStorageModelDataFolder,
                        file,
                        run.FileStorageLocator).Wait();
                }

                try
                {
                    Dictionary<string, string> envVars = new Dictionary<string, string>
                    {
                        { "SOURCE_FOLDER", ConfigurationHelper.AppSettings.AzureContainerVolumeName },
                        { "ANALYSIS_URL", GetAnalysisUrl(runId) }, // this one is named "ANALYSIS_URL" because I don't have control of the linux containers at all
                        { "MODEL_ID", run.ModelID.ToString() },
                        { "DOTNET_ENVIRONMENT", ConfigurationHelper.GetEnvironment() }, // i'm guessing this isn't being used at all because the linux containers are small and simple
                        { "STORAGE_ACCOUNT", ConfigurationHelper.AppSettings.AzureStorageAccountName},
                        { "SAS_TOKEN", sasToken}
                    };

                    containerAccessor.StartAzureContainer(run.FileStorageLocator,
                        run.Scenario.InputImage.ImageName,
                        run.Scenario.InputImage.CpuCoreCount ?? 1,
                        decimal.ToDouble(run.Scenario.InputImage.Memory ?? ContainerDefaultMemory),
                        envVars,
                        processType,
                        run.Scenario.InputImage.IsLinux);

                    containerStarted = true;
                }
                catch (Exception ex)
                {
                    startException = ex;
                    Logger.LogError($"Error while starting container: {ex.AllExceptionMessages()}");
                }
            }
            // start regular docker container
            else
            {
                Dictionary<string, string> envVars = new Dictionary<string, string>
                    {
                        { "RUNANALYSISURL", ConfigurationHelper.AppSettings.RunAnalysisUrl },
                        { "APIFUNCTIONCODE", ConfigurationHelper.AppSettings.APIFunctionCode },
                        { "AZURESTORAGEACCOUNT", ConfigurationHelper.ConnectionStrings.AzureStorageAccount },
                        { "BLOBSTORAGEMODELDATAFOLDER", ConfigurationHelper.AppSettings.BlobStorageModelDataFolder },
                        { "GETPRIMARYDATABASE", ConfigurationHelper.ConnectionStrings.GetPrimaryDatabase },
                        { "MODFLOWDATAFOLDER", ConfigurationHelper.AppSettings.ModflowDataFolder },
                        { "SENDRUNCOMPLETEDNOTIFICATIONURL", ConfigurationHelper.AppSettings.SendRunCompletedNotificationUrl },
                        { "GENERATEOUTPUTSURL", ConfigurationHelper.AppSettings.GenerateOutputsUrl },
                        { "PROCESSTYPE", ((int)processType).ToString()  },
                        { "RUN_ID", runId.ToString() },
                        { "MODEL_ID", run.ModelID.ToString() },
                        { "DOTNET_ENVIRONMENT", ConfigurationHelper.GetEnvironment() },
                        { "STORAGE_ACCOUNT", ConfigurationHelper.AppSettings.AzureStorageAccountName },
                        { "SAS_TOKEN", sasToken}
                    };

                try
                {
                    containerAccessor.StartAzureContainer(run.FileStorageLocator,
                       run.Image.ImageName,
                       run.Image.CpuCoreCount ?? 1,
                       decimal.ToDouble(run.Image.Memory ?? ContainerDefaultMemory),
                       envVars,
                       processType,
                       run.Image.IsLinux);

                    containerStarted = true;
                }
                catch (Exception ex)
                {
                    startException = ex;
                    Logger.LogError($"Error while starting container: {ex.AllExceptionMessages()}");
                }
            }

            if (processType == AgentProcessType.Input)
            {
                run.ProcessingStartDate = DateTime.UtcNow;
            }

            if (containerStarted)
            {
                if (processType == AgentProcessType.Input)
                {
                    run.RunStatusID = RunStatus.ProcesingInputs.RunStatusID;
                }
                else if (processType == AgentProcessType.Analysis)
                {
                    run.RunStatusID = RunStatus.RunningAnalysis.RunStatusID;
                }
            }
            else
            {
                run.RunStatusID = RunStatus.SystemError.RunStatusID;
                run.ProcessingEndDate = DateTime.UtcNow;
            }

            runAccessor.CreateOrUpdateRun(run);
            if (!containerStarted)
            {
                apiFunctionsAccessor.NotificationFunctionCall(run.RunID, true, startException ?? new Exception("Unable to start container."));
            }
        }

        public bool GenerateInputFiles(int runId)
        {
            Logger.LogInformation($"Generating input files for run {runId}:");
            var fileAccessor = AccessorFactory.CreateAccessor<IFileAccessor>();
            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
            var initialFiles = fileAccessor.GetFilesInModflowDataFolder();

            var runAccessor = AccessorFactory.CreateAccessor<IRunAccessor>();
            var apiFunctionsAccessor = AccessorFactory.CreateAccessor<IAPIFunctionsAccessor>();
            var run = runAccessor.FindRun(runId);

            var wellParticleMapInputData = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedWellParticleInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            if (wellParticleMapInputData != null)
            {
                run.RunWellParticleInputs = (JsonConvert.DeserializeObject<RunWellParticleInput[]>(Encoding.UTF8.GetString(wellParticleMapInputData))).ToList();
            }

            var modelInputOutputEngine = new ModelInputOutputEngineFactory().CreateModelInputOutputEngine(run);

            try
            {
                //Parse Inputs
                modelInputOutputEngine.GenerateInputFiles(run);
            }
            catch (InputDataInvalidException diex)
            {
                Logger.LogError(diex.Message);
                runAccessor.UpdateRunStatus(run.RunID, run.CustomerID, RunStatus.InvalidInput.RunStatusID);
                apiFunctionsAccessor.NotificationFunctionCall(run.RunID, false, diex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                runAccessor.UpdateRunStatus(run.RunID, run.CustomerID, RunStatus.SystemError.RunStatusID);
                apiFunctionsAccessor.NotificationFunctionCall(run.RunID, true, ex);
                return false;
            }

            var updatedFiles = fileAccessor.GetFilesInModflowDataFolder();

            // upload any new or updated files from analysis into blob storage
            foreach (var file in updatedFiles)
            {
                if (!initialFiles.Any(x => x.Path.Equals(file.Path)) || initialFiles.Any(x => x.Path.Equals(file.Path) && x.ModDate != file.ModDate))
                {
                    Logger.LogInformation($"saving file because it is new");
                    blobFileAccessor.SaveFile(StorageLocations.GenerateInputOutputFilePath(run.FileStorageLocator, file.Name), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, file.Path).Wait();
                }
            }

            apiFunctionsAccessor.MakeFunctionCall(GetAnalysisUrl(runId));

            return true;
        }

        // planned to upload analysis output files to blob storage to be downloaded by generate outputs container
        // but encountered large files output (6.5gb)
        // MemoryStream upload is limited to 2gb
        // a library Microsoft.Azure.Storage.DataMovement supports transferring large files but timed out at 5.7gb after running for ~30 minutes
        // generate outputs will now be performed in the analysis container for now
        public bool RunAnalysis(int runId)
        {
            var fileAccessor = AccessorFactory.CreateAccessor<IFileAccessor>();
            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            var runAccessor = AccessorFactory.CreateAccessor<IRunAccessor>();
            var run = runAccessor.FindRun(runId);
            Logger.LogInformation($"Found run \"{run.RunName}\"");
            var storageFiles = new List<string>();
            var storageFilesCopied = new List<string>();
            var usesFileStorage = run.Scenario.InputImage != null && run.Scenario.InputImage.IsLinux;

            // RL 12/22/22: there seems to be an expectation that GenerateInputFiles will generate at lease one file
            // For IWFM, we are actually not doing anything so we need to skip the transferring of input files to the container and just run Analysis
            if (run.Model.ModelEngineTypeID != (int)ModelEngineTypeEnum.IWFM)
            {
                // get files from generate input container
                if (usesFileStorage)
                {
                    var modelFiles = fileAccessor.GetFilesInModflowDataFolder();

                    storageFiles = blobFileAccessor.GetFilesInShareDirectory(run.FileStorageLocator).Result;

                    foreach (var file in storageFiles)
                    {
                        if (modelFiles.Any(x => x.Name.Equals(file, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            var destPath = Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, file);
                            fileAccessor.DeleteFile(destPath);
                            blobFileAccessor.GetSharedFile(file, run.FileStorageLocator, destPath).Wait();
                            storageFilesCopied.Add(file);
                        }
                    }
                }
                else
                {
                    storageFiles = blobFileAccessor.GetFilesInDirectory(
                        StorageLocations.GenerateInputOutputFolderPath(run.FileStorageLocator),
                        ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;

                    foreach (var blobFile in storageFiles)
                    {
                        var destPath = Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, blobFile);
                        fileAccessor.DeleteFile(destPath);
                        blobFileAccessor.GetFile(
                            StorageLocations.GenerateInputOutputFilePath(run.FileStorageLocator, blobFile),
                            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, destPath).Wait();
                    }
                }
            }

            //Run Modflow
            var analysisEngine = new AnalysisEngine();
            var analysisEngineSuccess = false;
            foreach (var modelExecutable in run.Model.ModelExecutables.OrderBy(x => x.RunOrder))
            {
                var runResult = analysisEngine.RunAnalysis(modelExecutable);
                analysisEngineSuccess = runResult.Success;
                runAccessor.UpdateRunOutput(run.RunID, run.CustomerID, runResult.ConsoleOutput);
                if (!analysisEngineSuccess)
                {
                    Logger.LogError("Analysis failed to complete successfully.  Still will attempt to generate outputs.");
                    break;
                }
            }

            runAccessor.UpdateRunStatus(run.RunID, run.CustomerID, analysisEngineSuccess ? RunStatus.AnalysisSuccess.RunStatusID : RunStatus.AnalysisFailed.RunStatusID);

            if (run.Model.ModelEngineTypeID != (int)ModelEngineTypeEnum.IWFM)
            {
                if (usesFileStorage)
                {
                    // move copied files into model outputs
                    foreach (var file in storageFilesCopied)
                    {
                        blobFileAccessor.CopyFromFileShareToBlobStorage(file, run.FileStorageLocator,
                            StorageLocations.ModelOutputFolderPath(run.Image.ImageName, file),
                            ConfigurationHelper.AppSettings.BlobStorageModelOutputsFolder).Wait();
                    }

                    // delete files from generate input
                    blobFileAccessor.DeleteCloudFileShare(run.FileStorageLocator).Wait();
                }
                else
                {
                    foreach (var blobFile in storageFiles)
                    {
                        blobFileAccessor.DeleteFile(
                            StorageLocations.GenerateInputOutputFilePath(run.FileStorageLocator, blobFile),
                            ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Wait();
                    }
                }
            }

            return analysisEngineSuccess;
        }

        public bool GenerateOutputFiles(int runId)
        {
            var runAccessor = AccessorFactory.CreateAccessor<IRunAccessor>();
            var apiFunctionsAccessor = AccessorFactory.CreateAccessor<IAPIFunctionsAccessor>();
            var run = runAccessor.FindRun(runId);

            var modelInputOutputEngine = new ModelInputOutputEngineFactory().CreateModelInputOutputEngine(run);

            var wellParticleMapInputData = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedWellParticleInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder).Result;
            if (wellParticleMapInputData != null)
            {
                run.RunWellParticleInputs = (JsonConvert.DeserializeObject<RunWellParticleInput[]>(Encoding.UTF8.GetString(wellParticleMapInputData))).ToList();
            }

            try
            {
                //Parse Outputs
                modelInputOutputEngine.GenerateOutputFiles(run);
            }
            catch (OutputDataInvalidException diex)
            {
                Logger.LogError(diex.Message);
                runAccessor.UpdateRunStatus(run.RunID, run.CustomerID, diex.Status);
                apiFunctionsAccessor.NotificationFunctionCall(run.RunID, false, diex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                runAccessor.UpdateRunStatus(run.RunID, run.CustomerID, RunStatus.SystemError.RunStatusID);
                apiFunctionsAccessor.NotificationFunctionCall(run.RunID, true, ex);
                return false;
            }
            finally
            {
                var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
                blobFileAccessor.DeleteCloudFileShare(run.FileStorageLocator).Wait();
            }

            if (run.RunStatusID != RunStatus.AnalysisFailed.RunStatusID)
            {
                // preserve whatever message
                runAccessor.UpdateRunStatus(run.RunID, run.CustomerID, RunStatus.Complete.RunStatusID);
                apiFunctionsAccessor.NotificationFunctionCall(run.RunID, false, null);
            }
            else
            {
                runAccessor.UpdateRunStatus(run.RunID, run.CustomerID, RunStatus.SystemError.RunStatusID);
                apiFunctionsAccessor.NotificationFunctionCall(run.RunID, true, new Exception("Modflow failed to run successfully."));
            }

            return true;
        }

        public void SendNotification(int runId, bool isSystemFailure, Exception exception)
        {
            if (exception == null)
            {
                SendSuccessfulRunCompletedNotification(runId);
            }
            else
            {
                SendFailureRunCompletedNotification(runId, isSystemFailure, exception);
            }
        }

        public async Task CleanCompletedRuns()
        {
            var containerAccessor = AccessorFactory.CreateAccessor<IContainerAccessor>();
            var runAccessor = AccessorFactory.CreateAccessor<IRunAccessor>();
            var apiFunctionsAccessor = AccessorFactory.CreateAccessor<IAPIFunctionsAccessor>();
            var containers = await containerAccessor.GetAzureContainers();

            var fileStorageLocator = GetFileStorageLocators(containers.Select(x => x.GroupName).ToList());

            var runs = runAccessor.FindRunsByFileStorageLocators(fileStorageLocator);

            var cutOffDate = DateTime.UtcNow.AddDays(-ConfigurationHelper.AppSettings.ContainerRetentionPeriodInDays);

            var deleteContainerTasks = new List<Task>();
            foreach (var exitedContainer in containers)
            {
                var run = runs.SingleOrDefault(x => x.FileStorageLocator.Equals(GetFileStorageLocator(exitedContainer.GroupName)));

                //The run no longer exists, don't bother holding onto the container
                if (run == null)
                {
                    deleteContainerTasks.Add(containerAccessor.DeleteAzureContainer(exitedContainer.Id));
                    continue;
                }

                //This state is on the container instance, not the containers within the container instance
                if (exitedContainer.State.Equals("Succeeded") ||
                    exitedContainer.State.Equals("Stopped"))
                {
                    if (run.RunStatusID == RunStatus.Complete.RunStatusID || (exitedContainer.Events != null && exitedContainer.Events.Count > 0))
                    {
                        var timestamp =
                            exitedContainer.Events != null && exitedContainer.Events.Count > 0 ? exitedContainer.Events
                                .OrderByDescending(x => x.LastTimeStamp).First().LastTimeStamp :
                            run.ProcessingEndDate ?? (run.ProcessingStartDate ?? run.CreatedDate);

                        if (timestamp < cutOffDate &&
                            (run.IsDeleted || RunStatusesToDelete.Contains(run.RunStatusID)))
                        {
                            deleteContainerTasks.Add(containerAccessor.DeleteAzureContainer(exitedContainer.Id));
                        }
                        continue;
                    }

                    if (!RunStatusesToDelete.Contains(run.RunStatusID) && run.RestartCount <= MaxNumRestarts)
                    {
                        RestartContainer(run, exitedContainer.Id, runAccessor, containerAccessor, apiFunctionsAccessor);
                        continue;
                    }
                }

                if (exitedContainer.State.Equals("Failed") && !RunStatusesToDelete.Contains(run.RunStatusID) && run.RestartCount <= MaxNumRestarts)
                {
                    RestartContainer(run, exitedContainer.Id, runAccessor, containerAccessor, apiFunctionsAccessor);
                }
            }
            await Task.WhenAll(deleteContainerTasks);
        }

        private List<string> GetFileStorageLocators(List<string> containerGroupNames)
        {
            var names = new List<string>();

            foreach (var name in containerGroupNames)
            {
                names.Add(GetFileStorageLocator(name));
            }

            return names;
        }

        private string GetFileStorageLocator(string containerGroupName)
        {
            return containerGroupName.Replace("-input", "").Replace("-analysis", "");
        }

        private void RestartContainer(Run run, string containerId, IRunAccessor runAccessor, IContainerAccessor containerAccessor, IAPIFunctionsAccessor apiFunctionsAccessor)
        {
            Logger.LogWarning($"Entering RestartContainer flow for Run:[{run.RunID}] Container:[{containerId}]");
            if (run.RestartCount < MaxNumRestarts)
            {
                Logger.LogWarning($"Run:[{run.RunID}] has not yet been restarted. Attempting restart");
                containerAccessor.RestartContainerAsync(containerId);

                run.RestartCount = run.RestartCount + 1;

                runAccessor.CreateOrUpdateRun(run);
                Logger.LogWarning($"Run:[{run.RunID}] restart successful");
                return;
            }

            Logger.LogWarning($"Run:[{run.RunID}] has already been restarted. Will not attempt restart. Current Run Status:[{run.RunStatus.RunStatusDisplayName}]");
            if (!RunStatusesToDelete.Contains(run.RunStatusID))
            {
                Logger.LogWarning($"Run:[{run.RunID}] updating Run Status to:[{RunStatus.SystemError.RunStatusDisplayName}]");
                runAccessor.UpdateRunStatus(run.RunID, run.CustomerID, RunStatus.SystemError.RunStatusID);
            }

            Logger.LogWarning($"Run:[{run.RunID}] sending failure notification");
            apiFunctionsAccessor.NotificationFunctionCall(run.RunID, true, new Exception("Azure container instance could not be started. Please contact your system administrator for more information."));
        }

        public async Task FailLongProcessingRuns()
        {
            var containerAccessor = AccessorFactory.CreateAccessor<IContainerAccessor>();
            var runAccessor = AccessorFactory.CreateAccessor<IRunAccessor>();
            var containers = await containerAccessor.GetAzureContainers();

            var fileStorageLocator = GetFileStorageLocators(containers.Select(x => x.GroupName).ToList());

            var runs = runAccessor.FindRunsByFileStorageLocators(fileStorageLocator);

            var cutOffDate = DateTime.UtcNow.AddHours(-ConfigurationHelper.AppSettings.MaxRunProcessingTimeInHours);

            foreach (var container in containers)
            {
                var run = runs.SingleOrDefault(x => x.FileStorageLocator.Equals(GetFileStorageLocator(container.GroupName)));

                if (container.State.Equals("Running") &&
                    run != null &&
                    run.ProcessingStartDate < cutOffDate)
                {
                    Logger.LogWarning($"Stopping long running container [{container.Id}] [{run.RunID}]");
                    await containerAccessor.StopContainerAsync(container.Id);

                    //flag it as error
                    run.ProcessingEndDate = DateTime.UtcNow;
                    run.RunStatusID = RunStatus.SystemError.RunStatusID;

                    //save it
                    runAccessor.CreateOrUpdateRun(run);

                    //Notify 
                    SendFailureRunCompletedNotification(run.RunID, true, new Exception($"Action {run.RunName} reached maximum processing time of {ConfigurationHelper.AppSettings.MaxRunProcessingTimeInHours}"));
                }
            }
        }

        public bool UpdateInputWellData(PivotedRunWellInput[] wellData, int runId, int customerId)
        {
            var run = AccessorFactory.CreateAccessor<IRunAccessor>().FindRun(runId, customerId);
            //PivotedRunWellInput[] to RunWellInput[]
            var runWellInputs = BuildWellInputData(wellData, run);

            //save parsed inputs                
            AccessorFactory.CreateAccessor<IBlobFileAccessor>().SaveFile(ParsedWellInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(runWellInputs))).Wait();

            return true;
        }

        public bool UpdateInputWellParticleData(RunWellParticleInput[] wellData, int runId, int customerId)
        {
            var run = AccessorFactory.CreateAccessor<IRunAccessor>().FindRun(runId, customerId);

            //save parsed inputs                
            var serializeObject = JsonConvert.SerializeObject(wellData);
            AccessorFactory.CreateAccessor<IBlobFileAccessor>().SaveFile(ParsedWellParticleInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, Encoding.UTF8.GetBytes(serializeObject)).Wait();

            return true;
        }

        public bool UpdateInputZoneData(RunZoneInput[] zoneData, int runId, int customerId)
        {
            var run = AccessorFactory.CreateAccessor<IRunAccessor>().FindRun(runId, customerId);

            //save parsed inputs                
            AccessorFactory.CreateAccessor<IBlobFileAccessor>().SaveFile(ParsedZoneInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(zoneData))).Wait();

            return true;
        }

        public List<User> FindUsersFromRunsForCustomer(int customerId)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().FindUsersFromRunsForCustomer(customerId);
        }

        public List<ModelSimpleDto> FindModelsFromRunsForCustomer(int customerId)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().FindModelsFromRunsForCustomer(customerId);
        }

        public List<ScenarioSimpleDto> FindScenariosFromRunsForCustomer(int customerId)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().FindScenariosFromRunsForCustomer(customerId);
        }

        public Common.DataContracts.Runs.RunStatus GetRunStatus(int runId, int customerId)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().GetRunStatus(runId, customerId);
        }

        #region Private Methods
        private string ParsedCanalInputFilePathForRun(string fileLocator)
        {
            return StorageLocations.ParsedInputFilePathForRun(fileLocator);
        }

        private string ParsedWellInputFilePathForRun(string fileLocator)
        {
            return StorageLocations.ParsedWellInputFilePathForRun(fileLocator);
        }

        private string ParsedWellParticleInputFilePathForRun(string fileLocator)
        {
            return StorageLocations.ParsedWellParticleInputFilePathForRun(fileLocator);
        }

        private string ParsedZoneInputFilePathForRun(string fileLocator)
        {
            return StorageLocations.ParsedZoneInputFilePathForRun(fileLocator);
        }

        private string OutputFilePathForRun(string fileStorageLocator, string outputFileName)
        {
            return $"{OutputDirectoryPathForRun(fileStorageLocator)}/{outputFileName}";
        }

        private string OutputDirectoryPathForRun(Run run)
        {
            return StorageLocations.OutputFolderPathForRun(run.FileStorageLocator);
        }

        private string OutputDirectoryPathForRun(string fileStorageLocator)
        {
            return StorageLocations.OutputFolderPathForRun(fileStorageLocator);
        }

        private void SendSuccessfulRunCompletedNotification(int runId)
        {
            SendManagerToManagerCall<INotificationManager>(a => a.SendRunCompletedEmail(runId, ""));
        }

        private void SendFailureRunCompletedNotification(int runId, bool isSystemFailure, Exception ex)
        {
            var errorMessage = isSystemFailure ? $"System Error: \n {ex}" : $"Invalid Results: {ex.Message}";
            SendManagerToManagerCall<INotificationManager>(a => a.SendRunCompletedEmail(runId, errorMessage));
        }

        private List<RunCanalInput> BuildCanalInputsForRun(Run run)
        {
            var inputs = new List<RunCanalInput>();

            if (string.IsNullOrWhiteSpace(run.Model.CanalData))
            {
                throw new Exception("Trying to build canal inputs but no canal data in the database.");
            }

            var canals = (run.Model.CanalData ?? "").Split(',');

            for (var i = 0; i < run.Model.NumberOfStressPeriods; i++)
            {
                var stressPeriodDate = run.Model.ModelStressPeriodCustomStartDates != null && run.Model.ModelStressPeriodCustomStartDates.Any() ? run.Model.ModelStressPeriodCustomStartDates[i].StressPeriodStartDate : run.Model.StartDateTime.AddMonths(i);
                var input = new RunCanalInput()
                {
                    Month = stressPeriodDate.Month,
                    Year = stressPeriodDate.Year
                };

                input.Values = new List<FeatureValue>();

                foreach (var canal in canals)
                {
                    input.Values.Add(new FeatureValue()
                    {
                        Value = 0,
                        FeatureName = canal
                    });
                }

                inputs.Add(input);
            }

            return inputs;
        }

        private RunWellInput[] BuildWellInputData(PivotedRunWellInput[] data, Run run)
        {
            var result = new List<RunWellInput>();

            for (var i = 0; i < run.Model.NumberOfStressPeriods; i++)
            {
                var stressPeriodDate = run.Model.ModelStressPeriodCustomStartDates != null && run.Model.ModelStressPeriodCustomStartDates.Any() ? run.Model.ModelStressPeriodCustomStartDates[i].StressPeriodStartDate : run.Model.StartDateTime.AddMonths(i);

                var stressPeriodInput = new RunWellInput()
                {
                    Month = stressPeriodDate.Month,
                    Year = stressPeriodDate.Year,
                    ManuallyAdded = data.Any(d => d.ManuallyAdded),
                    Values = new List<FeatureWithLocationValue>()
                };

                foreach (var well in data)
                {
                    var value = well.StressPeriodValues?.Any() == true ? well.StressPeriodValues.FirstOrDefault(x => x.Month == stressPeriodDate.Month && x.Year == stressPeriodDate.Year)?.Value ?? 0.0 : well.AverageValue;
                    stressPeriodInput.Values.Add(new FeatureWithLocationValue()
                    {
                        Value = value,
                        Lng = well.Lng,
                        Lat = well.Lat,
                        FeatureName = well.Name
                    });
                }

                result.Add(stressPeriodInput);
            }

            return result.ToArray();
        }

        private PivotedRunWellInput[] BuildWellPivotedInputData(RunWellInput[] inputs, Run run)
        {
            var result = new List<PivotedRunWellInput>();
            foreach (var well in inputs.First().Values)
            {
                var pivotedData = new PivotedRunWellInput()
                {
                    Lat = well.Lat,
                    Lng = well.Lng,
                    ManuallyAdded = inputs.First().ManuallyAdded,
                    Name = well.FeatureName,
                };

                var values = new List<StressPeriodValue>();

                foreach (var input in inputs)
                {
                    values.Add(new StressPeriodValue()
                    {
                        Month = input.Month,
                        Year = input.Year,
                        Value = input.Values.First(v => v.FeatureName == well.FeatureName).Value
                    });
                }

                pivotedData.StressPeriodValues = values;
                pivotedData.AverageValue = pivotedData.StressPeriodValues.Average(spv => spv.Value);

                result.Add(pivotedData);
            }

            return result.ToArray();
        }

        private string GetAnalysisUrl(int runId)
        {
            return $"{ConfigurationHelper.AppSettings.RunAnalysisUrl}?code={ConfigurationHelper.AppSettings.APIFunctionCode}&RunId={runId.ToString()}";
        }

        private string GetGenerateOutputsUrl(int runId)
        {
            return $"{ConfigurationHelper.AppSettings.GenerateOutputsUrl}?code={ConfigurationHelper.AppSettings.APIFunctionCode}&RunId={runId.ToString()}";
        }

        private DateTime ParseDateTime(string str)
        {
            return new DateTime(int.Parse(str.Substring(0, 4)),
                int.Parse(str.Substring(4, 2)),
                int.Parse(str.Substring(6, 2)),
                int.Parse(str.Substring(8, 2)),
                int.Parse(str.Substring(10, 2)),
                int.Parse(str.Substring(12, 2)));
        }

        private bool IsCustomInput(Run run)
        {
            return run?.Scenario?.ScenarioFiles != null && run.Scenario.ScenarioFiles.Any();
        }

        #endregion

    }
}
