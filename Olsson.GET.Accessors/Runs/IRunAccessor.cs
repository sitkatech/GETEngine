using Olsson.GET.Common.DataContracts.Runs;
using System.Collections.Generic;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.DataContracts.Scenarios;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;
using RunBucket = Olsson.GET.Common.DataContracts.Runs.RunBucket;
using RunStatus = Olsson.GET.Common.DataContracts.Runs.RunStatus;

namespace Olsson.GET.Accessors.Runs
{
    public interface IRunAccessor
    {
        List<Run> FindRuns(int userID, int customerID, RunFilter filter, int skip, int take);

        List<Run> GetRuns(int customerID);

        int FindRunsCount(int customerID, RunFilter filter);

        List<Common.DataContracts.Users.User> FindUsersFromRunsForCustomer(int customerID);

        List<ModelSimpleDto> FindModelsFromRunsForCustomer(int customerID);

        List<ScenarioSimpleDto> FindScenariosFromRunsForCustomer(int customerID);

        Run FindRun(int runID, int customerId);

        Run FindRun(int runID);

        Run CreateOrUpdateRun(Run run);

        bool DeleteRun(int runID, int customerID);

        bool RenameRun(int runID, int customerID, string newName);

        bool ChangeRunDescription(int runID, int customerID, string newDescription);

        bool UpdateRunStatus(int runID, int customerID, int runStatusID);

        bool UpdateRunOutput(int runID, int customerID, string output);

        RunStatus GetRunStatus(int runID, int customerID);

        List<Run> FindRunsByFileStorageLocators(List<string> fileStorageLocators);

        RunBucket FindRunBucket(int runBucketID, int customerID);

        List<RunBucket> GetRunBuckets(int userId, int customerID);

        RunBucket CreateOrUpdateRunBucket(RunBucket runBucket);

        bool RenameRunBucket(int runBucketID, int customerID, string newName);

        bool ChangeRunBucketDescription(int runBucketID, int customerID, string newDescription);

        bool DeleteRunBucket(int runBucketID, int customerID);

        bool DuplicateRunBucket(int runBucketID, int customerID, int userID);

        bool AddRunToRunBucket(int runID, int customerID, int runBucketID);

        bool RemoveRunFromRunBucket(int runID, int customerID, int runBucketID);

        List<Run> FindRunsByModelId(int modelID);
        List<Run> List();
        List<Run> FindRunsById(List<int> runIDs);
        Run FindRunById(int selectedModelID);
    }
}
