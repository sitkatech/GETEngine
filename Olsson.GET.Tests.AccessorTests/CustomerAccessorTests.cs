using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Accessors;
using Olsson.GET.Accessors.Customers;
using FluentAssertions;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Common.DataContracts.Customers;
using CustomerModelScenario = Olsson.GET.Common.DataContracts.Customers.CustomerModelScenario;
using PrimaryDBContext = Olsson.GET.Accessors.EntityFramework.PrimaryDBContext;

namespace Olsson.GET.Tests.AccessorTests
{
    [TestClass]
    public class CustomerAccessorTests : BaseAccessorTest
    {
        private readonly ICustomerAccessor _customerAccessor = new AccessorFactory().CreateAccessor<ICustomerAccessor>();

        [TestMethod]
        public void CustomerAccessor_CustomerCreateAndFind()
        {
            var name = "Name";

            var newCustomer =
                _customerAccessor.CreateOrUpdateCustomer(new CustomerDto
                {
                    CustomerName = name
                });

            Assert.IsNotNull(newCustomer);
            Assert.IsTrue(newCustomer.CustomerID > 0);

            var foundCustomerById = _customerAccessor.FindCustomerById(newCustomer.CustomerID);

            Assert.IsNotNull(foundCustomerById);
            Assert.IsTrue(newCustomer.CustomerID == foundCustomerById.CustomerID);
            Assert.IsTrue(foundCustomerById.CustomerName == name);
        }

        [TestMethod]
        public void CustomerAccessor_FindAllCustomers()
        {
            //make sure the default customer exists
            var customers = _customerAccessor.FindAllCustomers();

            Assert.IsTrue(customers.Any(c => c.CustomerID == 1));
        }

        [TestMethod]
        public void ModelAccessorTests_ModelsForCustomer()
        {
            var newCustomer = _customerAccessor.CreateOrUpdateCustomer(new CustomerDto { CustomerName = "test" });
            var saveResult = _customerAccessor.SaveCustomerModelScenarios(newCustomer.CustomerID, new[] { new CustomerModelScenario { CustomerID = newCustomer.CustomerID, ModelID = 1, ScenarioID = 4 } });

            saveResult.Should().NotBeNull();
            saveResult.Length.Should().Be(1);
            saveResult[0].CustomerID.Should().Be(newCustomer.CustomerID);
            saveResult[0].ModelID.Should().Be(1);
            saveResult[0].ScenarioID.Should().Be(4);

            var result = _customerAccessor.FindAllModelsForCustomer(newCustomer.CustomerID);

            result.Should().NotBeNull();

            result.Length.Should().Be(1);

            foreach (var scenario in result[0].Scenarios)
            {
                if (scenario.ScenarioID == 4)
                {
                    scenario.Enabled.Should().BeTrue();
                    scenario.ScenarioName.Should().Be("Canal Recharge");
                }
                else
                {
                    scenario.Enabled.Should().BeFalse();
                    scenario.ScenarioName.Should().NotBe("Canal Recharge");
                }
            }
        }

        [TestMethod]
        public void ModelAccessorTests_ModelsForCustomer_IncludesFiles()
        {
            var newCustomer = _customerAccessor.CreateOrUpdateCustomer(new CustomerDto { CustomerName = "test" });
            var saveResult = _customerAccessor.SaveCustomerModelScenarios(newCustomer.CustomerID, new[] { new CustomerModelScenario { CustomerID = newCustomer.CustomerID, ModelID = 1, ScenarioID = 4 } });

            ScenarioFile scenarioFile;
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                db.Scenarios.Single(a => a.ScenarioID == 4).ScenarioFiles.Should().HaveCount(0);
                scenarioFile = new ScenarioFile
                {
                    ScenarioFileID = 745231,
                    ScenarioFileDescription = "Fake Desc",
                    ScenarioFileName = "Fake Name",
                    IsRequired = true
                };
                db.Scenarios.Single(a => a.ScenarioID == 4).ScenarioFiles = new List<ScenarioFile> { scenarioFile };

                db.SaveChanges();

                scenarioFile.ScenarioFileID.Should().NotBe(0);
            }

            saveResult.Should().NotBeNull();
            saveResult.Length.Should().Be(1);
            saveResult[0].CustomerID.Should().Be(newCustomer.CustomerID);
            saveResult[0].ModelID.Should().Be(1);
            saveResult[0].ScenarioID.Should().Be(4);

            var result = _customerAccessor.FindAllModelsForCustomer(newCustomer.CustomerID);

            result.Should().NotBeNull();

            result.Length.Should().Be(1);

            foreach (var scenario in result[0].Scenarios)
            {
                if (scenario.ScenarioID == 4)
                {
                    scenario.Enabled.Should().BeTrue();
                    scenario.ScenarioName.Should().Be("Canal Recharge");
                    scenario.ScenarioFiles.Should().HaveCount(1);
                    scenario.ScenarioFiles[0].ScenarioFileID.Should().Be(scenarioFile.ScenarioFileID);
                    scenario.ScenarioFiles[0].ScenarioFileName.Should().Be("Fake Name");
                    scenario.ScenarioFiles[0].ScenarioFileDescription.Should().Be("Fake Desc");
                    scenario.ScenarioFiles[0].IsRequired.Should().Be(true);
                }
                else
                {
                    scenario.Enabled.Should().BeFalse();
                    scenario.ScenarioName.Should().NotBe("Canal Recharge");
                }
            }
        }

        [TestMethod]
        public void ModelAccessorTests_ModelScenarioForCustomer_Success()
        {
            var modelId = 1;
            var scenarioId = 4;

            var newCustomer = _customerAccessor.CreateOrUpdateCustomer(new CustomerDto { CustomerName = "test" });
            var saveResult = _customerAccessor.SaveCustomerModelScenarios(newCustomer.CustomerID, new[] { new CustomerModelScenario() { CustomerID = newCustomer.CustomerID, ModelID = modelId, ScenarioID = scenarioId } });

            saveResult.Should().NotBeNull();
            saveResult.Length.Should().Be(1);
            saveResult[0].CustomerID.Should().Be(newCustomer.CustomerID);
            saveResult[0].ModelID.Should().Be(modelId);
            saveResult[0].ScenarioID.Should().Be(scenarioId);

            var result = _customerAccessor.FindModelForCustomer(newCustomer.CustomerID, modelId, scenarioId);

            result.Should().NotBeNull();

            foreach (var scenario in result.Scenarios)
            {
                if (scenario.ScenarioID == scenarioId)
                {
                    scenario.Enabled.Should().BeTrue();
                    scenario.ScenarioName.Should().Be("Canal Recharge");
                }
                else
                {
                    scenario.Enabled.Should().BeFalse();
                    scenario.ScenarioName.Should().NotBe("Canal Recharge");
                }
            }
        }

        [TestMethod]
        public void ModelAccessorTests_ModelScenarioForCustomer_Failure()
        {
            var modelId = 1;
            var scenarioId = 4;

            var newCustomer = _customerAccessor.CreateOrUpdateCustomer(new CustomerDto { CustomerName = "test" });
            var saveResult = _customerAccessor.SaveCustomerModelScenarios(newCustomer.CustomerID, new[] { new CustomerModelScenario() { CustomerID = newCustomer.CustomerID, ModelID = modelId, ScenarioID = scenarioId } });

            saveResult.Should().NotBeNull();
            saveResult.Length.Should().Be(1);
            saveResult[0].CustomerID.Should().Be(newCustomer.CustomerID);
            saveResult[0].ModelID.Should().Be(modelId);
            saveResult[0].ScenarioID.Should().Be(scenarioId);

            var result = _customerAccessor.FindModelForCustomer(newCustomer.CustomerID, modelId, scenarioId + 1);

            result.Should().BeNull();
        }
    }
}
