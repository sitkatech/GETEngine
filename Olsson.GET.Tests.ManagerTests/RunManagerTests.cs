using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Olsson.GET.Accessors;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Accessors.Runs;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Managers.Runs;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using System;
using System.Configuration;
using System.Threading.Tasks;
using Olsson.GET.Accessors.Containers;
using Olsson.GET.Accessors.Customers;
using Olsson.GET.Common.DataContracts.Customers;
using Olsson.GET.Engines;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Notification;
using Olsson.GET.Engines.ModelInputOutputEngines;
using Olsson.GET.Accessors.APIFunctions;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Common.Shared.Enums;
using Olsson.GET.Accessors.Queue;
using Image = Olsson.GET.Common.DataContracts.Models.Image;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using ModelExecutable = Olsson.GET.Common.DataContracts.Models.ModelExecutable;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;
using RunBucket = Olsson.GET.Common.DataContracts.Runs.RunBucket;
using Scenario = Olsson.GET.Common.DataContracts.Scenarios.Scenario;
using RunStatus = Olsson.GET.Accessors.EntityFramework.RunStatus;

namespace Olsson.GET.Tests.ManagerTests
{
    [TestClass]
    public class RunManagerTests
    {
        private readonly IBlobFileAccessor _blobFileAccessorMock = Mock.Create<IBlobFileAccessor>(Behavior.Strict);
        private readonly ICustomerAccessor _customerAccessorMock = Mock.Create<ICustomerAccessor>(Behavior.Strict);
        private readonly IFileAccessor _fileAccessorMock = Mock.Create<IFileAccessor>(Behavior.Strict);
        private readonly IRunAccessor _runAccessorMock = Mock.Create<IRunAccessor>(Behavior.Strict);
        private readonly IContainerAccessor _containerAccessorMock = Mock.Create<IContainerAccessor>(Behavior.Strict);
        private readonly IAPIFunctionsAccessor _apiFunctionsAccessorMock = Mock.Create<IAPIFunctionsAccessor>(Behavior.Strict);
        private readonly INotificationManager _notificationManagerMock = Mock.Create<INotificationManager>(Behavior.Strict);
        private readonly IModelInputOutputEngineFactory _modelInputOutputEngineFactoryMock = Mock.Create<IModelInputOutputEngineFactory>(Behavior.Strict);
        private readonly IModelInputOutputEngine _modelInputOutputEngineMock = Mock.Create<IModelInputOutputEngine>(Behavior.Strict);
        private readonly IQueueAccessor _queueAccessorMock = Mock.Create<IQueueAccessor>(Behavior.Strict);


        [TestMethod]
        public void FindRunResultDetails_Exists()
        {
            const string fileStorageLocator = "FakeFileStorageLocator";
            _blobFileAccessorMock.Arrange(a => a.GetFilesInDirectory($"{fileStorageLocator}/outputs", Arg.AnyString))
                .ReturnsAsync(new List<string>
                {
                    "11-Fake file title.json"
                });
            _blobFileAccessorMock.Arrange(a =>
                    a.GetFile($"{fileStorageLocator}/outputs/11-Fake file title.json", Arg.AnyString))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new RunResultDetails
                {
                    RunResultId = 11,
                    RunResultName = "fake stuff"
                })));

            var sut = CreateRunManager();
            var result = sut.FindRunResultDetails(fileStorageLocator, 11);

            Assert.AreEqual(11, result.RunResultId);
            Assert.AreEqual("fake stuff", result.RunResultName);

            _blobFileAccessorMock.AssertAll();

        }

        [TestMethod]
        public void FindRunResultDetails_MultipleExist()
        {
            const string fileStorageLocator = "FakeFileStorageLocator";
            _blobFileAccessorMock.Arrange(a => a.GetFilesInDirectory($"{fileStorageLocator}/outputs", Arg.AnyString))
                .ReturnsAsync(new List<string>
                {
                    "10-Something else.json",
                    "11-Fake file title.json"
                });
            _blobFileAccessorMock.Arrange(a =>
                    a.GetFile($"{fileStorageLocator}/outputs/11-Fake file title.json", Arg.AnyString))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new RunResultDetails
                {
                    RunResultId = 11,
                    RunResultName = "fake stuff"
                })));

            var sut = CreateRunManager();
            var result = sut.FindRunResultDetails(fileStorageLocator, 11);

            Assert.AreEqual(11, result.RunResultId);
            Assert.AreEqual("fake stuff", result.RunResultName);

            _blobFileAccessorMock.AssertAll();

        }

        [TestMethod]
        public void FindRun_Completed_HasFiles()
        {
            var fakeMapData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new[] { new RunWellInput()
            {
                Values = new List<FeatureWithLocationValue>()
            } }));

            const string fileStorageLocator = "FakeFileStorageLocator";
            _blobFileAccessorMock.Arrange(a => a.GetFilesInDirectory($"{fileStorageLocator}/outputs", Arg.AnyString))
                .ReturnsAsync(new List<string>
                {
                    "10-Something else.json",
                    "11-Fake file title.json"
                });
            _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).ReturnsAsync(fakeMapData);
            
            _runAccessorMock.Arrange(a => a.FindRun(12, 5))
                .Returns(new Run
                {
                    RunStatusID = RunStatus.Complete.RunStatusID,
                    FileStorageLocator = fileStorageLocator,
                    Scenario = new Scenario()
                });

            var sut = CreateRunManager();
            var result = sut.FindRun(12, 5);

            Assert.AreEqual(RunStatus.Complete.RunStatusID, result.RunStatusID);
            Assert.IsNotNull(result.AvailableRunResults);
            Assert.AreEqual(2, result.AvailableRunResults.Count);
            var run10 = result.AvailableRunResults.Single(a => a.RunResultId == 10);
            Assert.AreEqual("Something else", run10.RunResultName);
            var run11 = result.AvailableRunResults.Single(a => a.RunResultId == 11);
            Assert.AreEqual("Fake file title", run11.RunResultName);
            Assert.AreEqual(1, result.WellMapInputs.Count);

            _runAccessorMock.AssertAll();
            _blobFileAccessorMock.AssertAll();

        }

        [TestMethod]
        public void FindRunBucket()
        {
            _runAccessorMock.Arrange(a => a.GetRunBuckets(1, 1))
                .Returns(new List<RunBucket>
                {
                    new RunBucket()
                    {
                        RunBucketID = 1,
                        RunBucketName = "Test 1",
                        Runs = new List<Run>()
                        {
                            new Run()
                            {
                                RunName = "Run 1"
                            },
                            new Run()
                            {
                                RunName = "Run 2"
                            }
                        }
                    },
                    new RunBucket()
                    {
                        RunBucketID = 2,
                        RunBucketName = "Test 2",
                        Runs = new List<Run>(),
                    }
                });

            var sut = CreateRunManager();
            var result = sut.GetRunBuckets(1, 1);

            Assert.AreEqual(result.Count(), 2);
            Assert.AreEqual(result[0].RunBucketID, 1);
            Assert.AreEqual(result[0].RunBucketName, "Test 1");
            Assert.AreEqual(result[0].Runs.Count(), 2);
            Assert.AreEqual(result[0].Runs[0].RunName, "Run 1");
            Assert.AreEqual(result[0].Runs[1].RunName, "Run 2");

            Assert.AreEqual(result[1].RunBucketID, 2);
            Assert.AreEqual(result[1].RunBucketName, "Test 2");
            Assert.IsNotNull(result[1].Runs);
            Assert.AreEqual(result[1].Runs.Count(), 0);

            _runAccessorMock.AssertAll();
        }

        [TestMethod]
        public void Duplicate()
        {
            var existingRun = new Run()
            {
                RunID = 1,
                RunName = "og",
                FileStorageLocator = Guid.NewGuid().ToString(),
                InputFileName = "test.csv",
                ModelID = 2
            };

            _runAccessorMock.Arrange(a => a.FindRun(Arg.AnyInt, Arg.AnyInt)).Returns(existingRun);
            _runAccessorMock.Arrange(a => a.CreateOrUpdateRun(Arg.IsAny<Run>())).Returns(existingRun);

            _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).ReturnsAsync(Encoding.UTF8.GetBytes("test"));
            _blobFileAccessorMock.Arrange(a => a.SaveFile(Arg.AnyString, Arg.AnyString, Arg.IsAny<byte[]>(), null)).Returns(Task.CompletedTask);

            _customerAccessorMock.Arrange(a => a.FindAllModelsForCustomer(Arg.AnyInt))
                .Returns(new[] { new CustomerModelWithScenariosDto() { ModelID = 2 } });

            var mgr = CreateRunManager();
            var result = mgr.DuplicateRun(1, 1, 1);

            _blobFileAccessorMock.Assert(a => a.GetFile(Arg.AnyString, Arg.AnyString), Occurs.Exactly(4));
            _runAccessorMock.AssertAll();
            _blobFileAccessorMock.AssertAll();
            _customerAccessorMock.AssertAll();
        }

        //[TestMethod]
        //[Timeout(5000)]
        //public void GenerateInputFiles_Successful()
        //{
        //    const int runId = 1234;
        //    const int customerId = 4321;
        //    _runAccessorMock.Arrange(a => a.FindRun(runId))
        //        .Returns(new Run
        //        {
        //            RunID = runId,
        //            CustomerID = customerId,
        //            Model = new Model
        //            {
        //                ModelExecutables = new List<ModelExecutable>
        //                {
        //                    new ModelExecutable
        //                    {
        //                        ExecutableName = "fake.exe",
        //                        Arguments = "fake.name"
        //                    }
        //                }
        //            }
        //        });
        //    _modelInputOutputEngineMock.Arrange(a => a.GenerateInputFiles(Arg.Matches<Run>(b => b.RunID == runId)));
        //    _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns(default(byte[]));
        //    _apiFunctionsAccessorMock.Arrange(a => a.MakeFunctionCall(Arg.AnyString));
        //    _fileAccessorMock.Arrange(a => a.GetFilesInModflowDataFolder()).Returns(new List<FileModel>());

        //    var sut = CreateRunManager();
        //    sut.GenerateInputFiles(runId);

        //    _modelInputOutputEngineMock.Assert(a => a.GenerateInputFiles(Arg.Matches<Run>(b => b.RunID == runId)), Occurs.Once());
        //    _apiFunctionsAccessorMock.Assert(a => a.MakeFunctionCall(Arg.AnyString), Occurs.Once());
        //    _fileAccessorMock.Assert(a => a.GetFilesInModflowDataFolder(), Occurs.Exactly(2));
        //}

        //[TestMethod]
        //[Timeout(5000)]
        //public void GenerateInputFiles_InputDataError()
        //{
        //    const int runId = 1234;
        //    const int customerId = 4321;
        //    _runAccessorMock.Arrange(a => a.FindRun(runId))
        //        .Returns(new Run
        //        {
        //            RunID = runId,
        //            CustomerID = customerId,
        //            Model = new Model
        //            {
        //                ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
        //                ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
        //                ModelExecutables = new List<ModelExecutable>
        //                {
        //                    new ModelExecutable
        //                    {
        //                        ExecutableName = "fake.exe",
        //                        Arguments = "fake.name"
        //                    }
        //                }
        //            }
        //        });
        //    var exception = new InputDataInvalidException("Fake message");
        //    _modelInputOutputEngineMock.Arrange(a => a.GenerateInputFiles(Arg.Matches<Run>(b => b.RunID == runId)))
        //        .Throws(exception);
        //    _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, customerId, RunStatus.InvalidInput.RunStatusID, Arg.AnyString))
        //        .Returns(true);
        //    _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns(default(byte[]));
        //    _apiFunctionsAccessorMock.Arrange(a => a.NotificationFunctionCall(runId, false, exception));
        //    _fileAccessorMock.Arrange(a => a.GetFilesInModflowDataFolder()).Returns(new List<FileModel>());

        //    var sut = CreateRunManager();
        //    sut.GenerateInputFiles(runId);

        //    _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, customerId, RunStatus.InvalidInput.RunStatusID, null), Occurs.Once());
        //    _modelInputOutputEngineMock.Assert(a => a.GenerateInputFiles(Arg.Matches<Run>(b => b.RunID == runId)), Occurs.Once());
        //    _apiFunctionsAccessorMock.Assert(a => a.NotificationFunctionCall(runId, false, exception), Occurs.Once());
        //    _apiFunctionsAccessorMock.Assert(a => a.MakeFunctionCall(Arg.AnyString), Occurs.Never());
        //    _fileAccessorMock.Assert(a => a.GetFilesInModflowDataFolder(), Occurs.Once());
        //}

        //[TestMethod]
        //[Timeout(5000)]
        //public void GenerateInputFiles_InputNonDataError()
        //{
        //    const int runId = 1234;
        //    const int customerId = 4321;
        //    _runAccessorMock.Arrange(a => a.FindRun(runId))
        //        .Returns(new Run
        //        {
        //            RunID = runId,
        //            CustomerID = customerId,
        //            Model = new Model
        //            {
        //                ModelExecutables = new List<ModelExecutable>
        //                {
        //                    new ModelExecutable
        //                    {
        //                        ExecutableName = "fake.exe",
        //                        Arguments = "fake.name"
        //                    }
        //                }
        //            }
        //        });
        //    var exception = new Exception("Fake message");
        //    _modelInputOutputEngineMock.Arrange(a => a.GenerateInputFiles(Arg.Matches<Run>(b => b.RunID == runId)))
        //        .Throws(exception);
        //    _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, customerId, RunStatus.SystemError.RunStatusID, Arg.AnyString))
        //        .Returns(true);
        //    _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns(default(byte[]));
        //    _apiFunctionsAccessorMock.Arrange(a => a.NotificationFunctionCall(runId, true, exception));
        //    _fileAccessorMock.Arrange(a => a.GetFilesInModflowDataFolder()).Returns(new List<FileModel>());

        //    var sut = CreateRunManager();
        //    sut.GenerateInputFiles(runId);

        //    _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, customerId, RunStatus.SystemError.RunStatusID, null), Occurs.Once());
        //    _modelInputOutputEngineMock.Assert(a => a.GenerateInputFiles(Arg.Matches<Run>(b => b.RunID == runId)), Occurs.Once());
        //    _apiFunctionsAccessorMock.Assert(a => a.NotificationFunctionCall(runId, true, exception), Occurs.Once());
        //    _apiFunctionsAccessorMock.Assert(a => a.MakeFunctionCall(Arg.AnyString), Occurs.Never());
        //    _fileAccessorMock.Assert(a => a.GetFilesInModflowDataFolder(), Occurs.Once());
        //}

        //[TestMethod]
        //[Timeout(5000)]
        //public void RunAnalysis_Classic_Successful()
        //{
        //    const int runId = 1234;
        //    const int customerId = 4321;
        //    _runAccessorMock.Arrange(a => a.FindRun(runId))
        //        .Returns(new Run
        //        {
        //            RunID = runId,
        //            CustomerID = customerId,
        //            Model = new Model
        //            {
        //                ModelExecutables = new List<ModelExecutable>
        //                {
        //                    new ModelExecutable
        //                    {
        //                        ExecutableName = "fake.exe",
        //                        Arguments = "fake.name"
        //                    }
        //                }
        //            },
        //            Scenario = new Scenario()
        //        });
        //    _analysisEngineMock.Arrange(a => a.RunAnalysis())
        //       .Returns(new AnalysisResult() { Success = true });
        //    _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, customerId, RunStatus.AnalysisSuccess.RunStatusID, Arg.AnyString))
        //     .Returns(true);
        //    _fileAccessorMock.Arrange(a => a.GetFilesInModflowDataFolder()).Returns(new List<FileModel>());
        //    _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns(default(byte[]));
        //    _blobFileAccessorMock.Arrange(a => a.GetFilesInDirectory(Arg.AnyString, Arg.AnyString)).Returns(new List<string>());

        //    var sut = CreateRunManager();
        //    sut.RunAnalysis(runId);

        //    _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, customerId, RunStatus.AnalysisSuccess.RunStatusID, null), Occurs.Once());
        //    _analysisEngineMock.Assert(a => a.RunAnalysis(), Occurs.Once());
        //    _fileAccessorMock.Assert(a => a.GetFilesInModflowDataFolder(), Occurs.Never());
        //    _blobFileAccessorMock.Assert(a => a.GetFilesInDirectory(Arg.AnyString, Arg.AnyString), Occurs.Once());
        //}

        //[TestMethod]
        //[Timeout(5000)]
        //public void RunAnalysis_Classic_Failure()
        //{
        //    const int runId = 1234;
        //    const int customerId = 4321;
        //    _runAccessorMock.Arrange(a => a.FindRun(runId))
        //        .Returns(new Run
        //        {
        //            RunID = runId,
        //            CustomerID = customerId,
        //            Model = new Model
        //            {
        //                ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
        //                ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
        //                ModelExecutables = new List<ModelExecutable>
        //                {
        //                    new ModelExecutable
        //                    {
        //                        ExecutableName = "fake.exe",
        //                        Arguments = "fake.name"
        //                    }
        //                }
        //            },
        //            Scenario = new Scenario()
        //        });
        //    _analysisEngineMock.Arrange(a => a.RunAnalysis())
        //       .Returns(new AnalysisResult() { Success = false });
        //    _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, customerId, RunStatus.AnalysisFailed.RunStatusID, Arg.AnyString))
        //     .Returns(true);
        //    _fileAccessorMock.Arrange(a => a.GetFilesInModflowDataFolder()).Returns(new List<FileModel>());
        //    _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns(default(byte[]));
        //    _blobFileAccessorMock.Arrange(a => a.GetFilesInDirectory(Arg.AnyString, Arg.AnyString)).Returns(new List<string>());

        //    var sut = CreateRunManager();
        //    sut.RunAnalysis(runId);

        //    _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, customerId, RunStatus.AnalysisFailed.RunStatusID, null), Occurs.Once());
        //    _analysisEngineMock.Assert(a => a.RunAnalysis(), Occurs.Once());
        //    _fileAccessorMock.Assert(a => a.GetFilesInModflowDataFolder(), Occurs.Never());
        //    _blobFileAccessorMock.Assert(a => a.GetFilesInDirectory(Arg.AnyString, Arg.AnyString), Occurs.Once());
        //}

        //[TestMethod]
        //[Timeout(5000)]
        //public void RunAnalysis_Custom_Successful()
        //{
        //    const int runId = 1234;
        //    const int customerId = 4321;
        //    _runAccessorMock.Arrange(a => a.FindRun(runId))
        //        .Returns(new Run
        //        {
        //            RunID = runId,
        //            CustomerID = customerId,
        //            Model = new Model
        //            {
        //                ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
        //                ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
        //                ModelExecutables = new List<ModelExecutable>
        //                {
        //                    new ModelExecutable
        //                    {
        //                        ExecutableName = "fake.exe",
        //                        Arguments = "fake.name"
        //                    }
        //                }
        //            },
        //            Scenario = new Scenario
        //            {
        //                InputImage = new Image { CpuCoreCount = 1, IsLinux = true, Memory = 4, ImageName = "some name" }
        //            }
        //        });
        //    _analysisEngineMock.Arrange(a => a.RunAnalysis())
        //       .Returns(new AnalysisResult() { Success = true });
        //    _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, customerId, RunStatus.AnalysisSuccess.RunStatusID, Arg.AnyString))
        //     .Returns(true);
        //    _fileAccessorMock.Arrange(a => a.GetFilesInModflowDataFolder()).Returns(new List<FileModel>());
        //    _blobFileAccessorMock.Arrange(a => a.GetFilesInShareDirectory(Arg.AnyString)).Returns(new List<string>());
        //    _blobFileAccessorMock.Arrange(a => a.DeleteCloudFileShare(Arg.AnyString));

        //    var sut = CreateRunManager();
        //    sut.RunAnalysis(runId);

        //    _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, customerId, RunStatus.AnalysisSuccess.RunStatusID, null), Occurs.Once());
        //    _analysisEngineMock.Assert(a => a.RunAnalysis(), Occurs.Once());
        //    _fileAccessorMock.Assert(a => a.GetFilesInModflowDataFolder(), Occurs.Once());
        //    _blobFileAccessorMock.Assert(a => a.GetFilesInShareDirectory(Arg.AnyString), Occurs.Once());
        //    _blobFileAccessorMock.Assert(a => a.DeleteCloudFileShare(Arg.AnyString), Occurs.Once());
        //}

        //[TestMethod]
        //[Timeout(5000)]
        //public void RunAnalysis_Custom_Failure()
        //{
        //    const int runId = 1234;
        //    const int customerId = 4321;
        //    _runAccessorMock.Arrange(a => a.FindRun(runId))
        //        .Returns(new Run
        //        {
        //            RunID = runId,
        //            CustomerID = customerId,
        //            Model = new Model
        //            {
        //                ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
        //                ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
        //                ModelExecutables = new List<ModelExecutable>
        //                {
        //                    new ModelExecutable
        //                    {
        //                        ExecutableName = "fake.exe",
        //                        Arguments = "fake.name"
        //                    }
        //                }
        //            },
        //            Scenario = new Scenario
        //            {
        //                InputImage = new Image { CpuCoreCount = 1, IsLinux = true, Memory = 4, ImageName = "some name" }
        //            }
        //        });
        //    _analysisEngineMock.Arrange(a => a.RunAnalysis())
        //       .Returns(new AnalysisResult() { Success = false });
        //    _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, customerId, RunStatus.AnalysisFailed.RunStatusID, Arg.AnyString))
        //     .Returns(true);
        //    _fileAccessorMock.Arrange(a => a.GetFilesInModflowDataFolder()).Returns(new List<FileModel>());
        //    _blobFileAccessorMock.Arrange(a => a.GetFilesInShareDirectory(Arg.AnyString)).Returns(new List<string>());
        //    _blobFileAccessorMock.Arrange(a => a.DeleteCloudFileShare(Arg.AnyString));

        //    var sut = CreateRunManager();
        //    sut.RunAnalysis(runId);

        //    _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, customerId, RunStatus.AnalysisFailed.RunStatusID, null), Occurs.Once());
        //    _analysisEngineMock.Assert(a => a.RunAnalysis(), Occurs.Once());
        //    _fileAccessorMock.Assert(a => a.GetFilesInModflowDataFolder(), Occurs.Once());
        //    _blobFileAccessorMock.Assert(a => a.GetFilesInShareDirectory(Arg.AnyString), Occurs.Once());
        //    _blobFileAccessorMock.Assert(a => a.DeleteCloudFileShare(Arg.AnyString), Occurs.Once());
        //}

        //[TestMethod]
        //[Timeout(5000)]
        //public void GenerateOutputFiles_AnalysisSuccess_Successful()
        //{
        //    const int runId = 1234;
        //    const int customerId = 4321;
        //    _runAccessorMock.Arrange(a => a.FindRun(runId))
        //        .Returns(new Run
        //        {
        //            RunID = runId,
        //            CustomerID = customerId,
        //            Model = new Model
        //            {
        //                ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
        //                ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
        //                ModelExecutables = new List<ModelExecutable>
        //                {
        //                    new ModelExecutable
        //                    {
        //                        ExecutableName = "fake.exe",
        //                        Arguments = "fake.name"
        //                    }
        //                }
        //            },
        //            RunStatusID = RunStatus.AnalysisSuccess.RunStatusID
        //        });
        //    _modelInputOutputEngineMock.Arrange(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.RunID == runId)));
        //    _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, customerId, RunStatus.Complete.RunStatusID, Arg.AnyString))
        //        .Returns(true);
        //    _apiFunctionsAccessorMock.Arrange(a => a.NotificationFunctionCall(runId, false, null));
        //    _blobFileAccessorMock.Arrange(a => a.DeleteCloudFileShare(Arg.AnyString));
        //    _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns((byte[])null);

        //    var sut = CreateRunManager();
        //    sut.GenerateOutputFiles(runId);

        //    _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, customerId, RunStatus.Complete.RunStatusID, null), Occurs.Once());
        //    _modelInputOutputEngineMock.Assert(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.RunID == runId)), Occurs.Once());
        //    _apiFunctionsAccessorMock.Assert(a => a.NotificationFunctionCall(runId, false, null), Occurs.Once());
        //    _blobFileAccessorMock.Assert(a => a.DeleteCloudFileShare(Arg.AnyString), Occurs.Once());
        //}

        //[TestMethod]
        //[Timeout(5000)]
        //public void GenerateOutputFiles_AnalysisFailure_Successful()
        //{
        //    const int runId = 1234;
        //    const int customerId = 4321;
        //    _runAccessorMock.Arrange(a => a.FindRun(runId))
        //        .Returns(new Run
        //        {
        //            RunID = runId,
        //            CustomerID = customerId,
        //            Model = new Model
        //            {
        //                ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
        //                ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
        //                ModelExecutables = new List<ModelExecutable>
        //                {
        //                    new ModelExecutable
        //                    {
        //                        ExecutableName = "fake.exe",
        //                        Arguments = "fake.name"
        //                    }
        //                }
        //            },
        //            RunStatusID = RunStatus.AnalysisFailed.RunStatusID
        //        });
        //    var exception = new Exception("Modflow failed to run successfully.");
        //    _modelInputOutputEngineMock.Arrange(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.RunID == runId)));
        //    _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, customerId, RunStatus.SystemError.RunStatusID, Arg.AnyString))
        //        .Returns(true);
        //    _apiFunctionsAccessorMock.Arrange(a => a.NotificationFunctionCall(runId, true, Arg.Matches<Exception>(x => x.Message.Equals(exception.Message))));
        //    _blobFileAccessorMock.Arrange(a => a.DeleteCloudFileShare(Arg.AnyString));

        //    _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns((byte[])null);

        //    var sut = CreateRunManager();
        //    sut.GenerateOutputFiles(runId);

        //    _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, customerId, RunStatus.SystemError.RunStatusID, null), Occurs.Once());
        //    _modelInputOutputEngineMock.Assert(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.RunID == runId)), Occurs.Once());
        //    _apiFunctionsAccessorMock.Assert(a => a.NotificationFunctionCall(runId, true, Arg.Matches<Exception>(x => x.Message.Equals(exception.Message))), Occurs.Once());
        //    _blobFileAccessorMock.Assert(a => a.DeleteCloudFileShare(Arg.AnyString), Occurs.Once());
        //}

        //[TestMethod]
        //[Timeout(5000)]
        //public void GenerateOutputFiles_OutputDataError()
        //{
        //    const int runId = 1234;
        //    const int customerId = 4321;
        //    _runAccessorMock.Arrange(a => a.FindRun(runId))
        //        .Returns(new Run
        //        {
        //            RunID = runId,
        //            CustomerID = customerId,
        //            Model = new Model
        //            {
        //                ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
        //                ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
        //                ModelExecutables = new List<ModelExecutable>
        //                {
        //                    new ModelExecutable
        //                    {
        //                        ExecutableName = "fake.exe",
        //                        Arguments = "fake.name"
        //                    }
        //                }
        //            },
        //            RunStatusID = RunStatus.AnalysisSuccess.RunStatusID
        //        });
        //    var exception = new OutputDataInvalidException("Fake message", RunStatus.InvalidOutput.RunStatusID);
        //    _modelInputOutputEngineMock.Arrange(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.RunID == runId)))
        //      .Throws(exception);
        //    _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, customerId, RunStatus.InvalidOutput.RunStatusID, Arg.AnyString))
        //        .Returns(true);
        //    _apiFunctionsAccessorMock.Arrange(a => a.NotificationFunctionCall(runId, false, exception));
        //    _blobFileAccessorMock.Arrange(a => a.DeleteCloudFileShare(Arg.AnyString));
        //    _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns((byte[])null);

        //    var sut = CreateRunManager();
        //    sut.GenerateOutputFiles(runId);

        //    _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, customerId, RunStatus.InvalidOutput.RunStatusID, null), Occurs.Once());
        //    _modelInputOutputEngineMock.Assert(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.RunID == runId)), Occurs.Once());
        //    _apiFunctionsAccessorMock.Assert(a => a.NotificationFunctionCall(runId, false, exception), Occurs.Once());
        //    _blobFileAccessorMock.Assert(a => a.DeleteCloudFileShare(Arg.AnyString), Occurs.Once());
        //}

        //[TestMethod]
        //[Timeout(5000)]
        //public void GenerateOutputFiles_OutputNonDataError()
        //{
        //    const int runId = 1234;
        //    const int customerId = 4321;
        //    _runAccessorMock.Arrange(a => a.FindRun(runId))
        //        .Returns(new Run
        //        {
        //            RunID = runId,
        //            CustomerID = customerId,
        //            Model = new Model
        //            {
        //                ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
        //                ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
        //                ModelExecutables = new List<ModelExecutable>
        //                {
        //                    new ModelExecutable
        //                    {
        //                        ExecutableName = "fake.exe",
        //                        Arguments = "fake.name"
        //                    }
        //                }
        //            },
        //            RunStatusID = RunStatus.AnalysisSuccess.RunStatusID
        //        });
        //    var exception = new Exception("Fake message");
        //    _modelInputOutputEngineMock.Arrange(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.RunID == runId)))
        //      .Throws(exception);
        //    _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, customerId, RunStatus.SystemError.RunStatusID, Arg.AnyString))
        //        .Returns(true);
        //    _apiFunctionsAccessorMock.Arrange(a => a.NotificationFunctionCall(runId, true, exception));
        //    _blobFileAccessorMock.Arrange(a => a.DeleteCloudFileShare(Arg.AnyString));
        //    _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns((byte[])null);

        //    var sut = CreateRunManager();
        //    sut.GenerateOutputFiles(runId);

        //    _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, customerId, RunStatus.SystemError.RunStatusID, null), Occurs.Once());
        //    _modelInputOutputEngineMock.Assert(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.RunID == runId)), Occurs.Once());
        //    _apiFunctionsAccessorMock.Assert(a => a.NotificationFunctionCall(runId, true, exception), Occurs.Once());
        //    _blobFileAccessorMock.Assert(a => a.DeleteCloudFileShare(Arg.AnyString), Occurs.Once());
        //}

        //[TestMethod]
        //[Timeout(5000)]
        //public void GenerateOutputFiles_HasDryCells()
        //{
        //    const int runId = 1234;
        //    const int customerId = 4321;
        //    _runAccessorMock.Arrange(a => a.FindRun(runId))
        //        .Returns(new Run
        //        {
        //            RunID = runId,
        //            CustomerID = customerId,
        //            Model = new Model
        //            {
        //                ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
        //                ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
        //                ModelExecutables = new List<ModelExecutable>
        //                {
        //                    new ModelExecutable
        //                    {
        //                        ExecutableName = "fake.exe",
        //                        Arguments = "fake.name"
        //                    }
        //                }
        //            },
        //            RunStatusID = RunStatus.AnalysisSuccess.RunStatusID
        //        });

        //    var exception = new OutputDataInvalidException("Fake message", RunStatus.HasDryCells.RunStatusID);
        //    _modelInputOutputEngineMock.Arrange(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.RunID == runId)))
        //      .Throws(exception);
        //    _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, customerId, RunStatus.HasDryCells.RunStatusID, Arg.AnyString))
        //       .Returns(true);
        //    _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns(default(byte[]));
        //    _apiFunctionsAccessorMock.Arrange(a => a.NotificationFunctionCall(runId, false, exception));
        //    _blobFileAccessorMock.Arrange(a => a.DeleteCloudFileShare(Arg.AnyString));
        //    _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns((byte[])null);

        //    var sut = CreateRunManager();
        //    sut.GenerateOutputFiles(runId);

        //    _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, customerId, RunStatus.HasDryCells.RunStatusID, null), Occurs.Once());
        //    _modelInputOutputEngineMock.Assert(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.RunID == runId)), Occurs.Once());
        //    _apiFunctionsAccessorMock.Assert(a => a.NotificationFunctionCall(runId, false, exception), Occurs.Once());
        //    _blobFileAccessorMock.Assert(a => a.DeleteCloudFileShare(Arg.AnyString), Occurs.Once());
        //}

        [TestMethod]
        public async Task StartContainer_Default_Success()
        {
            const int runId = 1234;
            const int customerId = 4321;
            _runAccessorMock.Arrange(a => a.FindRun(runId))
                .Returns(new Run
                {
                    RunID = runId,
                    CustomerID = customerId,
                    Image = new Image
                    {
                        ImageName = "someimagename",
                        Server = null,
                        CpuCoreCount = 1,
                        Memory = 4.0m,
                        IsLinux = false
                    },
                    Model = new Model
                    {
                        ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
                        ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
                        ModelExecutables = new List<ModelExecutable>
                        {
                            new ModelExecutable
                            {
                                ExecutableName = "fake.exe",
                                Arguments = "fake.name"
                            }
                        }
                    },
                    Scenario = new Scenario()
                });
            _containerAccessorMock.Arrange(a => a.StartAzureContainer(Arg.AnyString, Arg.AnyString, Arg.AnyDouble, Arg.AnyDouble, Arg.Matches<Dictionary<string, string>>(x => x.Count > 0), Arg.IsAny<AgentProcessType>(), Arg.AnyBool));
            _runAccessorMock.Arrange(a => a.CreateOrUpdateRun(Arg.Matches<Run>(b => b.RunID == runId))).Returns(default(Run));
            _containerAccessorMock.Arrange(a => a.CanQueueNewContainer()).Returns(Task.FromResult(true));

            var sut = CreateRunManager();
            await sut.StartContainer(runId, AgentProcessType.Input);

            _runAccessorMock.Assert(a => a.CreateOrUpdateRun(Arg.Matches<Run>(b => b.RunID == runId)), Occurs.Once());
            _containerAccessorMock.Assert(a => a.StartAzureContainer(Arg.AnyString, Arg.AnyString, Arg.AnyDouble, Arg.AnyDouble, Arg.Matches<Dictionary<string, string>>(x => x.Count > 0), Arg.IsAny<AgentProcessType>(), Arg.AnyBool), Occurs.Once());
        }

        [TestMethod]
        public async Task StartContainer_Default_CannotStart_Input()
        {
            const int runId = 1234;
            const int customerId = 4321;
            _runAccessorMock.Arrange(a => a.FindRun(runId))
                .Returns(new Run
                {
                    RunID = runId,
                    CustomerID = customerId,
                    Image = new Image
                    {
                        ImageName = "someimagename",
                        Server = null,
                        CpuCoreCount = 1,
                        Memory = 4.0m,
                        IsLinux = false
                    },
                    Model = new Model
                    {
                        ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
                        ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
                        ModelExecutables = new List<ModelExecutable>
                        {
                            new ModelExecutable
                            {
                                ExecutableName = "fake.exe",
                                Arguments = "fake.name"
                            }
                        }
                    },
                    Scenario = new Scenario()
                });
            _containerAccessorMock.Arrange(a => a.CanQueueNewContainer()).Returns(Task.FromResult(false));
            _queueAccessorMock.Arrange(a => a.CreateGenerateInputsMessage(runId, TimeSpan.FromMinutes(5)));

            var sut = CreateRunManager();
            await sut.StartContainer(runId, AgentProcessType.Input);

            _queueAccessorMock.Assert(a => a.CreateGenerateInputsMessage(runId, TimeSpan.FromMinutes(5)), Occurs.Once());
        }

        [TestMethod]
        public async Task StartContainer_Default_CannotStart_Analysis()
        {
            const int runId = 1234;
            const int customerId = 4321;
            _runAccessorMock.Arrange(a => a.FindRun(runId))
                .Returns(new Run
                {
                    RunID = runId,
                    CustomerID = customerId,
                    Image = new Image
                    {
                        ImageName = "someimagename",
                        Server = null,
                        CpuCoreCount = 1,
                        Memory = 4.0m,
                        IsLinux = false
                    },
                    Model = new Model
                    {
                        ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
                        ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
                        ModelExecutables = new List<ModelExecutable>
                        {
                            new ModelExecutable
                            {
                                ExecutableName = "fake.exe",
                                Arguments = "fake.name"
                            }
                        }
                    },
                    Scenario = new Scenario()
                });
            _containerAccessorMock.Arrange(a => a.CanQueueNewContainer()).Returns(Task.FromResult(false));
            _queueAccessorMock.Arrange(a => a.CreateRunAnalysisMessage(runId, TimeSpan.FromMinutes(5)));

            var sut = CreateRunManager();
            await sut.StartContainer(runId, AgentProcessType.Analysis);

            _queueAccessorMock.Assert(a => a.CreateRunAnalysisMessage(runId, TimeSpan.FromMinutes(5)), Occurs.Once());
        }

        [TestMethod]
        public async Task StartContainer_Default_CannotStart_Output()
        {
            const int runId = 1234;
            const int customerId = 4321;
            _runAccessorMock.Arrange(a => a.FindRun(runId))
                .Returns(new Run
                {
                    RunID = runId,
                    CustomerID = customerId,
                    Image = new Image
                    {
                        ImageName = "someimagename",
                        Server = null,
                        CpuCoreCount = 1,
                        Memory = 4.0m,
                        IsLinux = false
                    },
                    Model = new Model
                    {
                        ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
                        ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
                        ModelExecutables = new List<ModelExecutable>
                        {
                            new ModelExecutable
                            {
                                ExecutableName = "fake.exe",
                                Arguments = "fake.name"
                            }
                        }
                    },
                    Scenario = new Scenario()
                });
            _containerAccessorMock.Arrange(a => a.CanQueueNewContainer()).Returns(Task.FromResult(false));
            _queueAccessorMock.Arrange(a => a.CreateGenerateOutputsMessage(runId, TimeSpan.FromMinutes(5)));

            var sut = CreateRunManager();
            await sut.StartContainer(runId, AgentProcessType.Output);

            _queueAccessorMock.Assert(a => a.CreateGenerateOutputsMessage(runId, TimeSpan.FromMinutes(5)), Occurs.Once());
        }

        [TestMethod]
        public async Task StartContainer_Default_Failure()
        {
            const int runId = 1234;
            const int customerId = 4321;
            _runAccessorMock.Arrange(a => a.FindRun(runId))
                .Returns(new Run
                {
                    RunID = runId,
                    CustomerID = customerId,
                    Image = new Image
                    {
                        ImageName = "someimagename",
                        Server = "someserver"
                    },
                    Model = new Model
                    {
                        ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
                        ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
                        ModelExecutables = new List<ModelExecutable>
                        {
                            new ModelExecutable
                            {
                                ExecutableName = "fake.exe",
                                Arguments = "fake.name"
                            }
                        }
                    },
                    Scenario = new Scenario()
                });
            var exception = new Exception("Unable to start container.");
            _containerAccessorMock.Arrange(a => a.StartAzureContainer(Arg.AnyString, Arg.AnyString, Arg.AnyDouble, Arg.AnyDouble, Arg.Matches<Dictionary<string, string>>(x => x.Count > 0), Arg.IsAny<AgentProcessType>(), Arg.AnyBool))
                .Throws(exception);
            _runAccessorMock.Arrange(a => a.CreateOrUpdateRun(Arg.Matches<Run>(b => b.RunID == runId))).Returns(default(Run));
            _apiFunctionsAccessorMock.Arrange(a => a.NotificationFunctionCall(runId, Arg.AnyBool, Arg.Matches<Exception>(b => b.Message.Equals(exception.Message))));
            _containerAccessorMock.Arrange(a => a.CanQueueNewContainer()).Returns(Task.FromResult(true));

            var sut = CreateRunManager();
            await sut.StartContainer(runId, AgentProcessType.Input);

            _runAccessorMock.Assert(a => a.CreateOrUpdateRun(Arg.Matches<Run>(b => b.RunID == runId)), Occurs.Once());
            _containerAccessorMock.Assert(a => a.StartAzureContainer(Arg.AnyString, Arg.AnyString, Arg.AnyDouble, Arg.AnyDouble, Arg.Matches<Dictionary<string, string>>(x => x.Count > 0), Arg.IsAny<AgentProcessType>(), false), Occurs.Once());
            _apiFunctionsAccessorMock.Assert(a => a.NotificationFunctionCall(runId, Arg.AnyBool, Arg.Matches<Exception>(b => b.Message.Equals(exception.Message))), Occurs.Once());
        }

        [TestMethod]
        public async Task StartContainer_Custom_Success()
        {
            ConfigurationManager.AppSettings["AzureContainerCpuCoreCount"] = "1";
            ConfigurationManager.AppSettings["AzureContainerMemory"] = "2";

            const int runId = 1234;
            const int customerId = 4321;
            _runAccessorMock.Arrange(a => a.FindRun(runId))
                .Returns(new Run
                {
                    RunID = runId,
                    CustomerID = customerId,
                    Image = new Image
                    {
                        ImageName = "someimagename",
                        Server = "someserver"
                    },
                    Model = new Model
                    {
                        ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
                        ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
                        ModelExecutables = new List<ModelExecutable>
                        {
                            new ModelExecutable
                            {
                                ExecutableName = "fake.exe",
                                Arguments = "fake.name"
                            }
                        }
                    },
                    Scenario = new Scenario
                    {
                        InputImage = new Image { CpuCoreCount = 1, IsLinux = true, Memory = 4, ImageName = "some name" }
                    }
                });
            var inputFiles = new List<string> { "somefile.csv", "anotherfile.csv" };
            _blobFileAccessorMock.Arrange(a => a.CreateFileShare(Arg.AnyString)).Returns(Task.CompletedTask);
            _blobFileAccessorMock.Arrange(a => a.GetFilesInDirectory(Arg.AnyString, Arg.AnyString)).ReturnsAsync(inputFiles);
            _blobFileAccessorMock.Arrange(a => a.CopyFromBlobStorageToFileShare(Arg.AnyString, Arg.AnyString, Arg.AnyString, Arg.AnyString, Arg.AnyBool)).Returns(Task.CompletedTask);
            _containerAccessorMock.Arrange(a => a.StartAzureContainer(Arg.AnyString, Arg.AnyString, Arg.AnyDouble, Arg.AnyDouble, Arg.Matches<Dictionary<string, string>>(x => x.Count > 0), Arg.IsAny<AgentProcessType>(), Arg.AnyBool));
            _runAccessorMock.Arrange(a => a.CreateOrUpdateRun(Arg.Matches<Run>(b => b.RunID == runId))).Returns(default(Run));
            _containerAccessorMock.Arrange(a => a.CanQueueNewContainer()).Returns(Task.FromResult(true));

            var sut = CreateRunManager();
            await sut.StartContainer(runId, AgentProcessType.Input);

            _blobFileAccessorMock.Assert(a => a.GetFilesInDirectory(Arg.AnyString, Arg.AnyString), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.CopyFromBlobStorageToFileShare(Arg.AnyString, Arg.AnyString, Arg.AnyString, Arg.AnyString, Arg.AnyBool), Occurs.Exactly(inputFiles.Count()));
            _runAccessorMock.Assert(a => a.CreateOrUpdateRun(Arg.Matches<Run>(b => b.RunID == runId)), Occurs.Once());
            _containerAccessorMock.Assert(a => a.StartAzureContainer(Arg.AnyString, Arg.AnyString, Arg.AnyDouble, Arg.AnyDouble, Arg.Matches<Dictionary<string, string>>(x => x.Count > 0), Arg.IsAny<AgentProcessType>(), Arg.AnyBool), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.CreateFileShare(Arg.AnyString), Occurs.Once());
        }

        [TestMethod]
        public async Task StartContainer_Custom_Failure()
        {
            ConfigurationManager.AppSettings["AzureContainerCpuCoreCount"] = "1";
            ConfigurationManager.AppSettings["AzureContainerMemory"] = "2";

            const int runId = 1234;
            const int customerId = 4321;
            _runAccessorMock.Arrange(a => a.FindRun(runId))
                .Returns(new Run
                {
                    RunID = runId,
                    CustomerID = customerId,
                    Image = new Image
                    {
                        ImageName = "someimagename",
                        Server = "someserver"
                    },
                    Model = new Model
                    {
                        ModelEngineTypeID = (int)ModelEngineTypeEnum.Modflow,
                        ModelGridTypeID = (int)ModelGridTypeEnum.Structured,
                        ModelExecutables = new List<ModelExecutable>
                        {
                            new ModelExecutable
                            {
                                ExecutableName = "fake.exe",
                                Arguments = "fake.name"
                            }
                        }
                    },
                    Scenario = new Scenario
                    {
                        InputImage = new Image { CpuCoreCount = 1, IsLinux = true, Memory = 4, ImageName = "some name" }
                    }
                });
            var inputFiles = new List<string> { "somefile.csv" };
            var exception = new Exception("azure container error");
            _blobFileAccessorMock.Arrange(a => a.CreateFileShare(Arg.AnyString)).Returns(Task.CompletedTask);
            _blobFileAccessorMock.Arrange(a => a.GetFilesInDirectory(Arg.AnyString, Arg.AnyString)).ReturnsAsync(inputFiles);
            _blobFileAccessorMock.Arrange(a => a.CopyFromBlobStorageToFileShare(Arg.AnyString, Arg.AnyString, Arg.AnyString, Arg.AnyString, Arg.AnyBool)).Returns(Task.CompletedTask);
            _apiFunctionsAccessorMock.Arrange(a => a.NotificationFunctionCall(runId, Arg.AnyBool, Arg.Matches<Exception>(b => b.Message.Equals(exception.Message))));
            _containerAccessorMock.Arrange(a => a.StartAzureContainer(Arg.AnyString, Arg.AnyString, Arg.AnyDouble, Arg.AnyDouble, Arg.Matches<Dictionary<string, string>>(x => x.Count > 0), Arg.IsAny<AgentProcessType>(), Arg.AnyBool)).Throws(exception);
            _runAccessorMock.Arrange(a => a.CreateOrUpdateRun(Arg.Matches<Run>(b => b.RunID == runId))).Returns(default(Run));
            _containerAccessorMock.Arrange(a => a.CanQueueNewContainer()).Returns(Task.FromResult(true));

            var sut = CreateRunManager();
            await sut.StartContainer(runId, AgentProcessType.Input);

            _runAccessorMock.Assert(a => a.CreateOrUpdateRun(Arg.Matches<Run>(b => b.RunID == runId)), Occurs.Once());
            _containerAccessorMock.Assert(a => a.StartAzureContainer(Arg.AnyString, Arg.AnyString, Arg.AnyDouble, Arg.AnyDouble, Arg.Matches<Dictionary<string, string>>(x => x.Count > 0), Arg.IsAny<AgentProcessType>(), Arg.AnyBool), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.CreateFileShare(Arg.AnyString), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.GetFilesInDirectory(Arg.AnyString, Arg.AnyString), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.CopyFromBlobStorageToFileShare(Arg.AnyString, Arg.AnyString, Arg.AnyString, Arg.AnyString, Arg.AnyBool), Occurs.Exactly(inputFiles.Count()));
            _apiFunctionsAccessorMock.Assert(a => a.NotificationFunctionCall(runId, Arg.AnyBool, Arg.Matches<Exception>(b => b.Message.Equals(exception.Message))), Occurs.Once());
        }


        #region Private Methods
        private RunManager CreateRunManager()
        {
            _modelInputOutputEngineFactoryMock.Arrange(a => a.CreateModelInputOutputEngine(Arg.IsAny<Run>()))
                .Returns(() => _modelInputOutputEngineMock);
            var sut = new RunManager();
            sut.AccessorFactory = new AccessorFactory();
            sut.AccessorFactory.AddOverride(_blobFileAccessorMock);
            sut.AccessorFactory.AddOverride(_customerAccessorMock);
            sut.AccessorFactory.AddOverride(_fileAccessorMock);
            sut.AccessorFactory.AddOverride(_runAccessorMock);
            sut.AccessorFactory.AddOverride(_containerAccessorMock);
            sut.AccessorFactory.AddOverride(_apiFunctionsAccessorMock);
            sut.AccessorFactory.AddOverride(_queueAccessorMock);
            sut.EngineFactory = new EngineFactory();
            sut.EngineFactory.AddOverride(_modelInputOutputEngineFactoryMock);
            sut.ManagerFactory = new ManagerFactory();
            sut.ManagerFactory.AddOverride(_notificationManagerMock);
            return sut;
        }
        #endregion
    }
}