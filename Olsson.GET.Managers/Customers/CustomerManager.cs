using System.Collections.Generic;
using log4net;
using Olsson.GET.Accessors.Customers;
using Olsson.GET.Common.DataContracts.Customers;
using Olsson.GET.Common.Utilities;
using Olsson.GET.Common.DataContracts.Users;

namespace Olsson.GET.Managers.Customers
{
    public class CustomerManager : BaseManager, ICustomerManager
    {
        private static readonly ILog Logger = Logging.GetLogger(typeof(CustomerManager));

        public CustomerDto[] FindAllCustomers()
        {
            Logger.Info($"Finding all customers");

            return AccessorFactory.CreateAccessor<ICustomerAccessor>().FindAllCustomers();
        }

        public CustomerDto FindCustomerById(int customerId)
        {
            Logger.Info($"Finding customer by id {customerId}");

            var customer = AccessorFactory.CreateAccessor<ICustomerAccessor>().FindCustomerById(customerId);

            return customer;
        }

        public bool CanCreateNewTrialRuns(int customerId)
        {
            Logger.Info($"Checking for trial run creating for customer {customerId}");

            var customer = AccessorFactory.CreateAccessor<ICustomerAccessor>().FindCustomerById(customerId);

            if (customer.IsTrial)
            {
                return AccessorFactory.CreateAccessor<ICustomerAccessor>().GetExecutedRunCountForCustomer(customerId) < ConfigurationHelper.AppSettings.TrialRunLimit;
            }
            else
            {
                return true;
            }
        }

        public List<CustomerModelScenarioDto> FindCustomerModelScenarioDtos()
        {
            Logger.Info("Finding all customermodelscenarios");

            return AccessorFactory.CreateAccessor<ICustomerAccessor>().FindCustomerModelScenarios();
        }

        public User[] FindUsersForCustomer(int customerId)
        {
            Logger.Info($"Finding users for customer {customerId}");

            return AccessorFactory.CreateAccessor<ICustomerAccessor>().FindUsersForCustomer(customerId);
        }

        public CustomerDto CreateOrUpdateCustomer(CustomerDto customerDto)
        {
            Logger.Info($"Creating or updating customer {customerDto.CustomerName}");

            return AccessorFactory.CreateAccessor<ICustomerAccessor>().CreateOrUpdateCustomer(customerDto);
        }

        public CustomerModelScenario[] SaveCustomerModelScenarios(int customerId, CustomerModelScenario[] customerModelScenarios)
        {
            Logger.Info($"Saving customer model scenarios.");

            return AccessorFactory.CreateAccessor<ICustomerAccessor>().SaveCustomerModelScenarios(customerId, customerModelScenarios);
        }

        public CustomerModelWithScenariosDto[] FindAllModelsForCustomer(int customerId)
        {
            Logger.Info($"Finding all models for customer: {customerId}");

            return AccessorFactory.CreateAccessor<ICustomerAccessor>().FindAllModelsForCustomer(customerId);
        }

        public CustomerModelWithScenariosDto FindModelForCustomer(int customerId, int modelId, int scenarioId)
        {
            Logger.Info($"Finding model for customer: {customerId}, model: {modelId}, scenario: {scenarioId}");

            return AccessorFactory.CreateAccessor<ICustomerAccessor>().FindModelForCustomer(customerId, modelId, scenarioId);
        }

        public vModelCountScenarioCountForCustomerID GetModelCountScenarioCountForCustomerId(int customerId)
        {
            Logger.Info($"Finding model count scenario count for customer: {customerId}");

            return AccessorFactory.CreateAccessor<ICustomerAccessor>().GetModelCountScenarioCountForCustomerId(customerId);
        }
    }
}
