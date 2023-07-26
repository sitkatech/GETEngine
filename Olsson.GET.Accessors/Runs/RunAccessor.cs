using Microsoft.Extensions.Logging;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.DataContracts.Scenarios;
using Olsson.GET.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;
using RunBucket = Olsson.GET.Common.DataContracts.Runs.RunBucket;
using RunStatus = Olsson.GET.Accessors.EntityFramework.RunStatus;
using Scenario = Olsson.GET.Common.DataContracts.Scenarios.Scenario;

namespace Olsson.GET.Accessors.Runs
{
    internal class RunAccessor : BaseTableAccessor, IRunAccessor
    {
        private static readonly ILogger Logger = Logging.GetLogger<RunAccessor>();
        public Run CreateOrUpdateRun(Run run)
        {
            return base.CreateOrUpdate<Run, EntityFramework.Run, PrimaryDBContext>(run);
        }

        public bool DeleteRun(int runID, int customerID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var run = (from r in db.Runs
                           where r.RunID == runID && r.CustomerID == customerID
                           select r).SingleOrDefault();

                run.IsDeleted = true;

                return db.SaveChanges() == 1;
            }
        }

        private readonly int[] _processingStatuses = { (int)RunStatusEnum.Processing };

        public Run FindRun(int runID, int customerId)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var run = db.Runs
                    .Include("Model")
                    .Include("Model.ModelMapAreaBoundaries")
                    .Include("Model.ModelInputZoneDatas")
                    .Include("Model.ModelOutputZoneDatas")
                    .Include("Model.ModelStressPeriodCustomStartDates")
                    .Include("Scenario")
                    .Include("User")
                    .Include("Image")
                    .Include("Scenario.InputImage")
                    .Include("Scenario.ScenarioFiles")
                    .Include("Model.BaseflowTableProcessingConfiguration")
                    .FirstOrDefault(r => r.CustomerID == customerId && r.RunID == runID && !r.IsDeleted);

                return DTOMapper.Mapper.Map<Run>(run);
            }
        }

        public List<Run> FindRunsByModelId(int modelID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var source = db.Runs
                    .Include("RunBucketRuns")
                    .Include("RunBucketRuns.RunBucket")
                    .Include("Scenario")
                    .Include("User")
                    .Include("Model")
                    .Where(x => x.ModelID == modelID && !x.IsDeleted)
                    .ToList();
                return DTOMapper.Mapper.Map<List<Run>>(source);
            }
        }

        public List<Run> List()
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var source = db.Runs
                    .Include("RunBucketRuns")
                    .Include("RunBucketRuns.RunBucket")
                    .Include("Scenario")
                    .Include("User")
                    .Include("Model")
                    .Include("Scenario")
                    .ToList();
                return DTOMapper.Mapper.Map<List<Run>>(source);
            }
        }

        public List<Run> FindRunsById(List<int> runIDs)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var source = db.Runs
                    .Include("RunBucketRun")
                    .Include("RunBucketRun.RunBucket")
                    .Include("Scenario")
                    .Include("User")
                    .Include("Model")
                    .Include("Scenario")
                    .Where(x => runIDs.Contains(x.RunID))
                    .ToList();

                return DTOMapper.Mapper.Map<List<Run>>(source);

            }
        }

        public Run FindRunById(int selectedModelID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var source = db.Runs
                    .Include("RunBucketRun")
                    .Include("RunBucketRun.RunBucket")
                    .Include("Scenario")
                    .Include("User")
                    .Include("Model")
                    .Include("Scenario")
                    .Single(x => selectedModelID == x.ModelID);

                return DTOMapper.Mapper.Map<Run>(source);

            }
        }

        public Run FindRun(int runID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                return DTOMapper.Mapper.Map<Run>(
                    db.Runs
                        .Include("Model")
                        .Include("Model.ModelExecutables")
                        .Include("Model.ModelMapAreaBoundaries")
                        .Include("Model.ModelInputZoneDatas")
                        .Include("Model.ModelOutputZoneDatas")
                        .Include("Model.ModelStressPeriodCustomStartDates")
                        .Include("Scenario")
                        .Include("User")
                        .Include("Image")
                        .Include("Scenario.InputImage")
                        .Include("Scenario.ScenarioFiles")
                        .Include("Model.BaseflowTableProcessingConfiguration")
                        .FirstOrDefault(x => x.RunID == runID && !x.IsDeleted));
            }
        }

        public List<Run> FindRuns(int userID, int customerID, RunFilter filter, int skip, int take)
        {
            var hasStatusFilter = (filter.RunStatusIDs != null && filter.RunStatusIDs.Count > 0);

            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runs = (from r in db.Runs.Include("Model").Include("Scenario").Include("User").Include("Model.Image").Include("Scenario.InputImage")
                            where r.CustomerID == customerID
                            && (filter.NameSearch == null || filter.NameSearch.Trim() == string.Empty || r.RunName.Contains(filter.NameSearch))
                            && (!filter.ModelID.HasValue || filter.ModelID.Value == r.ModelID)
                            && (!filter.ScenarioID.HasValue || filter.ScenarioID.Value == r.ScenarioID)
                            && (!filter.UserID.HasValue || filter.UserID.Value == r.UserID)
                            && (!hasStatusFilter || (filter.RunStatusIDs.Select(s => (int)s).Contains(r.RunStatusID)))
                            && !r.IsDeleted
                            orderby r.CreatedDate descending
                            select new { r, m = r.Model, u = r.User, i = r.Model.Image, s = r.Scenario })
                            .Skip(skip)
                            .Take(take)
                            .ToArray();

                var result = DTOMapper.Mapper.Map<Run[]>(runs.Select(run => run.r)).ToList();

                return result;
            }
        }

        public List<Run> GetRuns(int customerID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runs = (from r in db.Runs
                            where r.CustomerID == customerID
                            select r).ToList();

                return DTOMapper.Mapper.Map<Run[]>(runs.Select(run => run)).ToList();
            }
        }

        public int FindRunsCount(int customerID, RunFilter filter)
        {
            var hasStatusFilter = (filter.RunStatusIDs != null && filter.RunStatusIDs.Count > 0);

            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runCount = (from r in db.Runs.Include("Model").Include("Scenario").Include("User").Include("Model.Image").Include("Scenario.InputImage")
                                where r.CustomerID == customerID
                                && (filter.NameSearch == null || filter.NameSearch.Trim() == string.Empty || r.RunName.Contains(filter.NameSearch))
                                && (!filter.ModelID.HasValue || filter.ModelID.Value == r.ModelID)
                                && (!filter.ScenarioID.HasValue || filter.ScenarioID.Value == r.ScenarioID)
                                && (!filter.UserID.HasValue || filter.UserID.Value == r.UserID)
                                && (!hasStatusFilter || (filter.RunStatusIDs.Select(s => (int)s).Contains(r.RunStatusID)))
                                && !r.IsDeleted
                                select r.RunID).Count();

                return runCount;
            }
        }

        public List<Run> FindRunsByFileStorageLocators(List<string> fileStorageLocators)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runs = (from r in db.Runs
                            where fileStorageLocators.Contains(r.FileStorageLocator)
                            select new { r }).ToArray();

                return DTOMapper.Mapper.Map<Run[]>(runs.Select(run => run.r)).ToList();
            }
        }

        public bool RenameRun(int runID, int customerID, string newName)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var run = GetRunByIDAndCustomer(db, runID, customerID);

                run.RunName = newName;

                return db.SaveChanges() == 1;
            }
        }

        public bool ChangeRunDescription(int runID, int customerID, string newDescription)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var run = GetRunByIDAndCustomer(db, runID, customerID);

                run.RunDescription = newDescription;

                return db.SaveChanges() == 1;
            }
        }

        private static EntityFramework.Run GetRunByIDAndCustomer(PrimaryDBContext db, int runID, int customerID)
        {
            return db.Runs.SingleOrDefault(r => r.RunID == runID && r.CustomerID == customerID);
        }

        private readonly List<int> _finishedStatuses = new List<int> { RunStatus.Complete.RunStatusID, RunStatus.InvalidOutput.RunStatusID, RunStatus.SystemError.RunStatusID, RunStatus.InvalidInput.RunStatusID, RunStatus.HasDryCells.RunStatusID };

        public bool UpdateRunStatus(int runID, int customerID, int runStatusID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var run = GetRunByIDAndCustomer(db, runID, customerID);
                if (run != null)
                {
                    run.RunStatusID = runStatusID;
                    if (_finishedStatuses.Contains(runStatusID))
                    {
                        run.ProcessingEndDate = DateTime.UtcNow;
                    }

                    return db.SaveChanges() == 1;
                }

                return false;
            }
        }

        public bool UpdateRunOutput(int runID, int customerID, string output)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var run = GetRunByIDAndCustomer(db, runID, customerID);
                if (run != null)
                {
                    run.Output += output;
                    return db.SaveChanges() == 1;
                }

                return false;
            }
        }

        public Common.DataContracts.Runs.RunStatus GetRunStatus(int runID, int customerID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var run = db.Runs.SingleOrDefault(r => r.RunID == runID && r.CustomerID == customerID);

                return DTOMapper.Mapper.Map<Common.DataContracts.Runs.RunStatus>(run?.RunStatus);
            }
        }

        public List<Common.DataContracts.Users.User> FindUsersFromRunsForCustomer(int customerID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var users = db.Runs.Include("User").Where(r => r.CustomerID == customerID).Select(r => r.User).Distinct();

                return DTOMapper.Mapper.Map<Common.DataContracts.Users.User[]>(users).ToList();
            }
        }

        public List<Common.DataContracts.Models.ModelSimpleDto> FindModelsFromRunsForCustomer(int customerID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var models = db.Runs.Include("Model").Where(r => r.CustomerID == customerID).Select(r => r.Model).Distinct();

                return DTOMapper.Mapper.Map<List<Common.DataContracts.Models.ModelSimpleDto>>(models).ToList();
            }
        }

        public List<ScenarioSimpleDto> FindScenariosFromRunsForCustomer(int customerID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var scenarios = db.Runs.Include("Scenario")
                    .Where(r => r.CustomerID == customerID)
                    .Select(r => r.Scenario).Distinct();

                return DTOMapper.Mapper.Map<List<ScenarioSimpleDto>>(scenarios).ToList();
            }
        }

        public RunBucket FindRunBucket(int runBucketID, int customerID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runBucket = GetRunBucketByIDAndCustomerID(runBucketID, customerID, db);
                if (runBucket == null)
                {
                    return null;
                }

                var bucket = new RunBucket
                {
                    RunBucketID = runBucket.RunBucketID,
                    RunBucketName = runBucket.RunBucketName,
                    CreatedDate = runBucket.CreatedDate,
                    CustomerID = runBucket.CustomerID,
                    UserID = runBucket.UserID,
                    RunBucketDescription = runBucket.RunBucketDescription,
                    Runs = runBucket.RunBucketRuns.Where(y => y.Run.IsDeleted == false)
                        .Select(y =>
                        {
                            var run = new Run
                            {
                                RunID = y.RunID,
                                RunName = y.Run.RunName,
                                RunStatusID = y.Run.RunStatusID,
                                FileStorageLocator = y.Run.FileStorageLocator,
                                Model = new Common.DataContracts.Models.Model(),
                                Scenario = new Scenario()
                            };
                            run.Model.ModelID = y.Run.ModelID;
                            run.Model.ModelName = y.Run.Model.ModelName;
                            run.Model.BuddyGroup = y.Run.Model.BuddyGroup;
                            run.Scenario.ScenarioID = y.Run.Scenario.ScenarioID;
                            run.Scenario.ScenarioName = y.Run.Scenario.ScenarioName;
                            run.Scenario.InputControlType = (Common.DataContracts.Models.InputControlType)y.Run.Scenario.InputControlType;
                            run.Scenario.ShouldSwitchSign = y.Run.Scenario.ShouldSwitchSign;
                            run.Scenario.InputImageID = y.Run.Scenario.InputImageID;
                            return run;
                        }).ToList()
                };
                return bucket;
            }
        }

        public List<RunBucket> GetRunBuckets(int userId, int customerID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runBuckets = db.RunBuckets
                    .Include("RunBucketRuns.Run.Model")
                    .Where(x => x.CustomerID == customerID)
                    .Select(x => new RunBucket()
                    {
                        RunBucketID = x.RunBucketID,
                        RunBucketName = x.RunBucketName,
                        CreatedDate = x.CreatedDate,
                        CustomerID = x.CustomerID,
                        UserID = x.UserID,
                        Runs = x.RunBucketRuns.Where(y => y.Run.IsDeleted == false)
                        .Select(y => new Run
                        {
                            RunID = y.RunID,
                            RunName = y.Run.RunName,
                            OutputVolumeUnitID = y.Run.OutputVolumeUnitID,
                            IsDifferential = y.Run.IsDifferential,
                            RunDescription = y.Run.RunDescription,
                            Model = new Common.DataContracts.Models.Model
                            {
                                ModelID = y.Run.ModelID,
                                ModelName = y.Run.Model.ModelName,
                                BuddyGroup = y.Run.Model.BuddyGroup
                            }
                        }).ToList(),
                        RunBucketDescription = x.RunBucketDescription
                    })
                    .OrderByDescending(x => x.CreatedDate)
                    .ToList();

                return runBuckets;
            }
        }

        public RunBucket CreateOrUpdateRunBucket(RunBucket runBucket)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                return base.CreateOrUpdate<RunBucket, EntityFramework.RunBucket, PrimaryDBContext>(runBucket);
            }
        }

        public bool RenameRunBucket(int runBucketID, int customerID, string newName)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runBucket = GetRunBucketByIDAndCustomerID(runBucketID, customerID, db);
                runBucket.RunBucketName = newName;
                return db.SaveChanges() == 1;
            }
        }

        private static EntityFramework.RunBucket GetRunBucketByIDAndCustomerID(int runBucketID, int customerID, PrimaryDBContext db)
        {
            var runBucket = db.RunBuckets.Include("RunBucketRuns.Run.Model").Include("RunBucketRuns.Run.Scenario").SingleOrDefault(r => r.RunBucketID == runBucketID && r.CustomerID == customerID);
            return runBucket;
        }

        public bool ChangeRunBucketDescription(int runBucketID, int customerID, string newDescription)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runBucket = GetRunBucketByIDAndCustomerID(runBucketID, customerID, db);
                runBucket.RunBucketDescription = newDescription;
                return db.SaveChanges() == 1;
            }
        }

        public bool DeleteRunBucket(int runBucketID, int customerID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                db.RunBucketRuns.RemoveRange(from r in db.RunBucketRuns
                                             where r.RunBucketID == runBucketID && r.Run.CustomerID == customerID
                                             select r);

                db.RunBuckets.RemoveRange(from r in db.RunBuckets
                                          where r.RunBucketID == runBucketID && r.CustomerID == customerID
                                          select r);

                return db.SaveChanges() == 1;
            }
        }

        public bool DuplicateRunBucket(int runBucketID, int customerID, int userID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runBucket = (from r in db.RunBuckets
                                 where r.RunBucketID == runBucketID
                                    && r.CustomerID == customerID
                                    && r.UserID == userID
                                 select r).SingleOrDefault();
                if (runBucket == null)
                {
                    // TODO: Maybe have a NotFound exception instead
                    throw new NullReferenceException($"Run Bucket {runBucketID} not found in DB!");
                }

                var newBucket = db.RunBuckets.Add(new EntityFramework.RunBucket()
                {
                    RunBucketName = $"{runBucket.RunBucketName} - Copy @{DateTime.Now}",
                    CreatedDate = DateTime.Now,
                    UserID = runBucket.UserID,
                    CustomerID = runBucket.CustomerID
                });

                db.SaveChanges();

                var runBucketRuns = (from r in db.RunBucketRuns
                                     where r.RunBucketID == runBucketID
                                     select r).ToList();

                foreach (var run in runBucketRuns)
                {
                    db.RunBucketRuns.Add(new EntityFramework.RunBucketRun()
                    {
                        RunID = run.RunID,
                        RunBucketID = newBucket.RunBucketID
                    });
                    db.SaveChanges();
                }

                return true;
            }
        }

        public bool AddRunToRunBucket(int runID, int customerID, int runBucketID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runBucket = GetRunBucketByIDAndCustomerID(runBucketID, customerID, db);
                if (runBucket != null)
                {
                    db.RunBucketRuns.Add(new EntityFramework.RunBucketRun()
                    {
                        RunID = runID,
                        RunBucketID = runBucketID
                    });

                    return db.SaveChanges() == 1;
                }

                return false;
            }
        }

        public bool RemoveRunFromRunBucket(int runID, int customerID, int runBucketID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runBucket = (from r in db.RunBucketRuns
                                 where r.RunID == runID && r.RunBucketID == runBucketID && r.RunBucket.CustomerID == customerID
                                 select r).FirstOrDefault();

                if (runBucket != null)
                {
                    db.RunBucketRuns.Remove(runBucket);

                    return db.SaveChanges() == 1;
                }

                return false;
            }
        }
    }
}
