using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Accessors;
using Olsson.GET.Accessors.Runs;
using Olsson.GET.Accessors.Customers;
using Olsson.GET.Accessors.Authentication;
using Olsson.GET.Common.DataContracts.Customers;
using Olsson.GET.Common.DataContracts.Runs;
using FluentAssertions;
using Olsson.GET.Accessors.EntityFramework;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;
using RunBucket = Olsson.GET.Common.DataContracts.Runs.RunBucket;
using User = Olsson.GET.Common.DataContracts.Users.User;
using RunStatus = Olsson.GET.Accessors.EntityFramework.RunStatus;

namespace Olsson.GET.Tests.AccessorTests
{
    [TestClass]
    public class RunAccessorTests : BaseAccessorTest
    {
        IRunAccessor _runAccessor = new AccessorFactory().CreateAccessor<IRunAccessor>();
        ICustomerAccessor _customerAccessor = new AccessorFactory().CreateAccessor<ICustomerAccessor>();
        IUserAccessor _userAccessor = new AccessorFactory().CreateAccessor<IUserAccessor>();

        CustomerDto _customer1;
        CustomerDto _customer2;

        User _user1ForCustomer1;
        User _user2ForCustomer1;

        User _user1ForCustomer2;

        [TestMethod]
        public void RunAccessorTests_FindRun_NonDifferential()
        {
            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, IsDifferential = false, RunDescription = "test description" });

            run1.Should().NotBeNull();

            var findResult = _runAccessor.FindRun(run1.RunID, _customer1.CustomerID);

            findResult.Should().NotBeNull();
            findResult.RunID.Should().Equals(run1.RunID);
            findResult.IsDifferential.Should().Be(false);
        }

        [TestMethod]
        public void RunAccessorTests_FindRun()
        {
            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, IsDifferential = true, RunDescription = "test description" });

            run1.Should().NotBeNull();

            var findResult = _runAccessor.FindRun(run1.RunID, _customer1.CustomerID);

            findResult.Should().NotBeNull();
            findResult.RunID.Should().Equals(run1.RunID);

            var findResultWrongCustomer = _runAccessor.FindRun(run1.RunID, _customer2.CustomerID);

            findResultWrongCustomer.Should().BeNull();
        }

        [TestMethod]
        public void RunAccessorTests_RenameRun()
        {
            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, IsDifferential = true, RunDescription = "test description" });

            var newName = "new name";
            _runAccessor.RenameRun(run1.RunID, _customer1.CustomerID, newName);

            var findResult = _runAccessor.FindRun(run1.RunID, _customer1.CustomerID);

            findResult.RunName.Should().Be(newName);
        }

        [TestMethod]
        public void RunAccessorTests_ChangeRunDescription()
        {
            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, IsDifferential = true, RunDescription = "test description" });

            var newDescription = "new description";
            _runAccessor.ChangeRunDescription(run1.RunID, _customer1.CustomerID, newDescription);

            var findResult = _runAccessor.FindRun(run1.RunID, _customer1.CustomerID);

            findResult.RunDescription.Should().Be(newDescription);
        }

        [TestMethod]
        public void RunAccessorTests_UpdateRunStatus()
        {
            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, IsDifferential = true, RunDescription = "test description" });

            var newStatus = RunStatus.Queued;
            _runAccessor.UpdateRunStatus(run1.RunID, _customer1.CustomerID, newStatus.RunStatusID);

            var findResult = _runAccessor.FindRun(run1.RunID, _customer1.CustomerID);

            findResult.RunStatusID.Should().Be(newStatus.RunStatusID);
        }

        [TestMethod]
        public void RunAccessorTests_UpdateRunStatus_HasDryCellsUpdatesProcessEndTime()
        {
            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, IsDifferential = true, RunDescription = "test description" });

            var newStatus = RunStatus.HasDryCells;
            _runAccessor.UpdateRunStatus(run1.RunID, _customer1.CustomerID, newStatus.RunStatusID);

            var findResult = _runAccessor.FindRun(run1.RunID, _customer1.CustomerID);

            findResult.RunStatusID.Should().Be(newStatus.RunStatusID);
            findResult.ProcessingEndDate.Should().NotBeNull();
        }

        [TestMethod]
        public void RunAccessorTests_FindRuns()
        {
            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test1", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, IsDifferential = true, RunDescription = "test1 description" });
            var run2 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test2", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user2ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, IsDifferential = true, RunDescription = "test2 description" });
            var run3 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test3", CustomerID = _customer2.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer2.UserID, RunStatusID = RunStatus.Created.RunStatusID, IsDifferential = true, RunDescription = "test3 description" });

            var filter = new RunFilter();
            filter.UserID = _user1ForCustomer1.UserID;

            var findResultForUser = _runAccessor.FindRuns(_user1ForCustomer1.UserID, _customer1.CustomerID, filter, 0, 20);
            findResultForUser.Should().NotBeNull();
            findResultForUser.Count.Should().Be(1);

            filter.UserID = null;

            var findResultForCustomer = _runAccessor.FindRuns(_user1ForCustomer1.UserID, _customer1.CustomerID, filter, 0, 20);
            findResultForCustomer.Should().NotBeNull();
            findResultForCustomer.Count.Should().Be(2);

            _runAccessor.DeleteRun(run2.RunID, _customer1.CustomerID);

            findResultForCustomer = _runAccessor.FindRuns(_user1ForCustomer1.UserID, _customer1.CustomerID, filter, 0, 20);
            findResultForCustomer.Should().NotBeNull();
            findResultForCustomer.Count.Should().Be(1);
        }

        [TestMethod]
        public void RunAccessorTests_GetRunStatus_Success()
        {
            var run = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test1", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, IsDifferential = true, RunDescription = "test description" });

            var result = _runAccessor.GetRunStatus(run.RunID, _customer1.CustomerID);
            result.Should().NotBeNull();
            result.RunStatusID.Should().Be((int)RunStatusEnum.Created);
        }

        [TestMethod]
        public void RunAccessorTests_GetRunStatus_Failure()
        {
            var run = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test1", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, IsDifferential = true, RunDescription = "test description" });

            var result = _runAccessor.GetRunStatus(run.RunID, _customer2.CustomerID);
            result.Should().BeNull();
        }

        [TestMethod]
        public void RunAccessorTests_GetRunBuckets_NoBuckets()
        {
            var result = _runAccessor.GetRunBuckets(_user1ForCustomer1.UserID, _customer1.CustomerID);
            result.Count.Should().Be(0);
        }

        [TestMethod]
        public void RunAccessorTests_GetRunBuckets()
        {
            _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                RunBucketName = "TestBucket 1",
                CreatedDate = DateTime.Now,
                CustomerID = _customer1.CustomerID,
                UserID = _user1ForCustomer1.UserID,
                Runs = new List<Run>() { }
            });
             
            _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                RunBucketName = "TestBucket 2",
                CreatedDate = DateTime.Now,
                CustomerID = _customer2.CustomerID,
                UserID = _user1ForCustomer2.UserID,
                Runs = new List<Run>() { }
            });

            var result1 = _runAccessor.GetRunBuckets(_user1ForCustomer1.UserID, _customer1.CustomerID);
            result1.Count.Should().Be(1);
            result1[0].RunBucketName.Should().Be("TestBucket 1");
            result1[0].Runs.Count.Should().Be(0);
            
            var result2 = _runAccessor.GetRunBuckets(_user1ForCustomer2.UserID, _customer2.CustomerID);
            result2.Count.Should().Be(1);
            result2[0].RunBucketName.Should().Be("TestBucket 2");
            result2[0].Runs.Count.Should().Be(0);
        }
        
        [TestMethod]
        public void RunAccessorTests_GetRunBuckets_Runs()
        {
            var actionBucket1 = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                RunBucketName = "TestBucket 1",
                CreatedDate = DateTime.Now,
                CustomerID = _customer1.CustomerID,
                UserID = _user1ForCustomer1.UserID,
                Runs = new List<Run>() { }
            });
            
            var actionBucket2 = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                RunBucketName = "TestBucket 1",
                CreatedDate = DateTime.Now,
                CustomerID = _customer2.CustomerID,
                UserID = _user1ForCustomer2.UserID,
                Runs = new List<Run>() { }
            });

            var result1 = _runAccessor.GetRunBuckets(_user1ForCustomer1.UserID, _customer1.CustomerID);
            result1.Count.Should().Be(1);
            result1[0].Runs.Count.Should().Be(0);

            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test1", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, RunDescription = "test1 description" });
            var run2 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test1", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, RunDescription = "test1 description" });

            _runAccessor.AddRunToRunBucket(run1.RunID, _customer1.CustomerID, actionBucket1.RunBucketID);
            _runAccessor.AddRunToRunBucket(run2.RunID, _customer1.CustomerID, actionBucket1.RunBucketID);
            _runAccessor.AddRunToRunBucket(run2.RunID, _customer2.CustomerID, actionBucket2.RunBucketID);

            var result2 = _runAccessor.GetRunBuckets(_user1ForCustomer1.UserID, _customer1.CustomerID);
            result2.Count.Should().Be(1);
            result2[0].Runs.Count.Should().Be(2);
        }

        [TestMethod]
        public void RunAccessorTests_FindRunBucket_Empty()
        {
            var bucket = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                RunBucketName = "TestBucket 1",
                CreatedDate = DateTime.Now,
                CustomerID = _customer1.CustomerID,
                UserID = _user1ForCustomer1.UserID,
                Runs = new List<Run>() { }
            });

            var result = _runAccessor.FindRunBucket(bucket.RunBucketID, _customer1.CustomerID);
            result.RunBucketName.Should().Be("TestBucket 1");
            result.Runs.Count.Should().Be(0);
        }

        [TestMethod]
        public void RunAccessorTests_FindRunBucket_Runs()
        {
            var bucket = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                RunBucketName = "TestBucket 1",
                CreatedDate = DateTime.Now,
                CustomerID = _customer1.CustomerID,
                UserID = _user1ForCustomer1.UserID,
                Runs = new List<Run>() { }
            });

            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test1", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, RunDescription = "test1 description" });
            var run2 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test1", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, RunDescription = "test1 description" });

            _runAccessor.AddRunToRunBucket(run1.RunID, _customer1.CustomerID, bucket.RunBucketID);
            _runAccessor.AddRunToRunBucket(run2.RunID, _customer1.CustomerID, bucket.RunBucketID);

            var result = _runAccessor.FindRunBucket(bucket.RunBucketID, _customer1.CustomerID);
            result.RunBucketName.Should().Be("TestBucket 1");
            result.Runs.Count.Should().Be(2);
        }

        [TestMethod]
        public void RunAccessorTests_RenameRunBucket()
        {
            var bucket = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                RunBucketName = "TestBucket 1",
                CreatedDate = DateTime.Now,
                CustomerID = _customer1.CustomerID,
                UserID = _user1ForCustomer1.UserID,
                Runs = new List<Run>() { }
            });

            _runAccessor.RenameRunBucket(bucket.RunBucketID, _customer1.CustomerID, "TestBucket New Name");

            var result = _runAccessor.FindRunBucket(bucket.RunBucketID, _customer1.CustomerID);
            result.RunBucketName.Should().Be("TestBucket New Name");
        }

        [TestMethod]
        public void RunAccessorTests_ChangeRunBucketDescription()
        {
            var bucket = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                RunBucketName = "TestBucket 1",
                CreatedDate = DateTime.Now,
                CustomerID = _customer1.CustomerID,
                UserID = _user1ForCustomer1.UserID,
                Runs = new List<Run>() { },
                RunBucketDescription = "Test Description"
            });

            _runAccessor.ChangeRunBucketDescription(bucket.RunBucketID, _customer1.CustomerID, "TestBucket New Description");

            var result = _runAccessor.FindRunBucket(bucket.RunBucketID, _customer1.CustomerID);
            result.RunBucketDescription.Should().Be("TestBucket New Description");
        }
        
        [TestMethod]
        public void RunAccessorTests_DeleteRunBucket()
        {
            var bucket = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                RunBucketName = "TestBucket 1",
                CreatedDate = DateTime.Now,
                CustomerID = _customer1.CustomerID,
                UserID = _user1ForCustomer1.UserID,
                Runs = new List<Run>() { }
            });

            _runAccessor.DeleteRunBucket(bucket.RunBucketID, _customer1.CustomerID);

            var result = _runAccessor.FindRunBucket(bucket.RunBucketID, _customer1.CustomerID);
            result.Should().Be(null);
        }
        
        [TestMethod]
        public void RunAccessorTests_DuplicateRunBucket()
        {
            var bucket = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                RunBucketName = "TestBucket 1",
                CreatedDate = DateTime.Now,
                CustomerID = _customer1.CustomerID,
                UserID = _user1ForCustomer1.UserID,
                Runs = new List<Run>() { }
            });

            _runAccessor.DuplicateRunBucket(bucket.RunBucketID, _customer1.CustomerID, _user1ForCustomer1.UserID);

            var result = _runAccessor.GetRunBuckets(_user1ForCustomer1.UserID, _customer1.CustomerID);
            result.Count.Should().Be(2);
        }

        [TestMethod]
        public void RunAccessorTests_AddRunToRunBucket()
        {
            var bucket = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                RunBucketName = "TestBucket 1",
                CreatedDate = DateTime.Now,
                CustomerID = _customer1.CustomerID,
                UserID = _user1ForCustomer1.UserID,
                Runs = new List<Run>() { }
            });

            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test1", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, RunDescription = "test1 description" });
            var run2 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test1", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, RunDescription = "test1 description" });

            _runAccessor.AddRunToRunBucket(run1.RunID, _customer1.CustomerID, bucket.RunBucketID);
            _runAccessor.AddRunToRunBucket(run2.RunID, _customer1.CustomerID, bucket.RunBucketID);

            var result = _runAccessor.FindRunBucket(bucket.RunBucketID, _customer1.CustomerID);
            result.RunBucketName.Should().Be("TestBucket 1");
            result.Runs.Count.Should().Be(2);
            foreach(var run in result.Runs)
            {
                run.RunName.Should().Be("test1");
            }
        }

        [TestMethod]
        public void RunAccessorTests_RemoveRunFromRunBucket()
        {
            var bucket = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                RunBucketName = "TestBucket 1",
                CreatedDate = DateTime.Now,
                CustomerID = _customer1.CustomerID,
                UserID = _user1ForCustomer1.UserID,
                Runs = new List<Run>() { }
            });

            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test1", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, RunDescription = "test description" });
            var run2 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, RunName = "test1", CustomerID = _customer1.CustomerID, FileStorageLocator = Guid.NewGuid().ToString(), ImageID = 1, ModelID = 1, ScenarioID = 1, UserID = _user1ForCustomer1.UserID, RunStatusID = RunStatus.Created.RunStatusID, RunDescription = "test description" });

            _runAccessor.AddRunToRunBucket(run1.RunID, _customer1.CustomerID, bucket.RunBucketID);
            _runAccessor.AddRunToRunBucket(run2.RunID, _customer1.CustomerID, bucket.RunBucketID);

            var result1 = _runAccessor.FindRunBucket(bucket.RunBucketID, _customer1.CustomerID);
            result1.RunBucketName.Should().Be("TestBucket 1");
            result1.Runs.Count.Should().Be(2);
            foreach (var run in result1.Runs)
            {
                run.RunName.Should().Be("test1");
            }

            _runAccessor.RemoveRunFromRunBucket(run1.RunID, _customer1.CustomerID, bucket.RunBucketID);

            var result2 = _runAccessor.FindRunBucket(bucket.RunBucketID, _customer1.CustomerID);
            result2.Runs.Count.Should().Be(1);
            foreach (var run in result2.Runs)
            {
                run.RunID.Should().NotBe(run1.RunID);
            }
        }

        [TestInitialize]
        public void InitializeCustomerData()
        {
            _customer1 = _customerAccessor.CreateOrUpdateCustomer(new CustomerDto() { CustomerName = "Customer 1" });
            _customer2 = _customerAccessor.CreateOrUpdateCustomer(new CustomerDto() { CustomerName = "Customer 2" });

            _user1ForCustomer1 = _userAccessor.CreateOrUpdateUser(new User() { CustomerID = _customer1.CustomerID, Email = "u1@test.com", PhoneNumber = "999-999-9999", FullName = "Bob", UserName = "User 1" });
            _user1ForCustomer2 = _userAccessor.CreateOrUpdateUser(new User() { CustomerID = _customer1.CustomerID, Email = "u3@test.com", PhoneNumber = "999-999-9999", FullName = "TED!", UserName = "User 3" });
            _user2ForCustomer1 = _userAccessor.CreateOrUpdateUser(new User() { CustomerID = _customer1.CustomerID, Email = "u2@test.com", PhoneNumber = "999-999-9999", FullName = "Lesley", UserName = "User 2" });
        }
    }
}
