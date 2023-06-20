using Olsson.GET.Common.DataContracts.APIFunctionModels;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.DataContracts.Scenarios;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;
using RunBucket = Olsson.GET.Common.DataContracts.Runs.RunBucket;
using User = Olsson.GET.Common.DataContracts.Users.User;

namespace Olsson.GET.Managers.Runs
{
    public interface IRunManager
    {
        List<Run> FindRuns(int userId, int customerId, RunFilter filter, int pageNum = 0);

        List<RunSummaryReponseModel> GetRuns(int customerId);

        int FindRunsCount(int customerId, RunFilter filter);

        Run FindRun(int runId, int customerId, bool includeHiddenFiles = false);
        Run FindRun(int runId, bool includeHiddenFiles = false);

        Run CreateOrUpdateRun(Run run);

        Run DuplicateRun(int runId, int customerId, int userId);

        bool DeleteRun(int runId, int customerId);

        bool RenameRun(int runId, int customerId, string newName);

        bool ChangeRunDescription(int runId, int customerId, string newDescription);

        RunBucket FindRunBucket(int bucketId, int customerId);

        List<RunBucket> GetRunBuckets(int userId, int customerId);

        RunBucket CreateOrUpdateRunBucket(RunBucket runBucket);

        bool RenameRunBucket(int bucketId, int customerId, string newName);

        bool ChangeRunBucketDescription(int bucketId, int customerId, string newDescription);

        bool AddRunToRunBucket(int runId, int customerId, int bucketId);

        bool RemoveRunFromRunBucket(int runId, int customerId, int bucketId);

        bool DuplicateRunBucket(int bucketId, int customerId, int userId);

        bool DeleteRunBucket(int bucketId, int customerId);

        RunCanalInputParseResult ProcessRunInputFile(Run run, byte[] fileContent);

        RunWellInputParseResult ProcessWellRunInputFile(Run run, byte[] fileContent);

        RunWellParticleInputParseResult ProcessWellParticleRunInputFile(Run run, byte[] fileContent);

        bool UpdateInputCanalData(Run run, RunCanalInput[] data);

        bool UpdateInputWellData(PivotedRunWellInput[] wellData, int runId, int customerId);

        bool UpdateInputWellParticleData(RunWellParticleInput[] wellData, int runId, int customerId);

        bool UpdateInputZoneData(RunZoneInput[] zoneData, int runId, int customerId);

        RunResultDetails FindRunResultDetails(string fileStorageLocator, int runResultId);

        ActionBucketResultDetails FindAggregateRunResultDetails(List<RunResultDisplay> runResultsToDisplay);

        string GetRunResultData(string fileStorageLocator, int runResultId, string fileExtension = ".json");

        bool QueueRun(int runId, int customerId, bool shouldCreateMaps);

        byte[] FindCanalRunInputFile(Run run);

        byte[] FindWellRunInputFile(Run run);

        byte[] FindWellParticleRunInputFile(Run run);

        bool GenerateInputFiles(int runId);

        bool RunAnalysis(int runId);

        bool GenerateOutputFiles(int runId);

        Task CleanCompletedRuns();

        Task FailLongProcessingRuns();

        List<User> FindUsersFromRunsForCustomer(int customerId);

        List<ModelSimpleDto> FindModelsFromRunsForCustomer(int customerId);

        List<ScenarioSimpleDto> FindScenariosFromRunsForCustomer(int customerId);

        Task StartContainer(int runId, AgentProcessType processType);

        bool QueueGenerateOutput(int runId);

        bool QueueRunAnalysis(int runId);

        void SendNotification(int runId, bool isSystemFailure, Exception exception);

        Common.DataContracts.Runs.RunStatus GetRunStatus(int runId, int customerId);

        byte[] DownloadKmlFile(string fileStorageLocator, string filename, int runResultId);

        Task<bool> UploadInputFile(Run run, string name, byte[] fileContent);

        bool DeleteInputFile(string fileLocator, string filename);

        List<AvailableRunResult> FindAvailableRunResults(int runId, int customerId);

        RunResultResponseModel GetRunResult(int runId, int customerId, string fileName, string subType, string fileType);

        List<Run> FindRunsByModelId(int modelId);
        List<Run> List();
        List<Run> FindRunsById(List<int> runIDs);
        Run FindRunById(int selectedModelID);
    }
}
