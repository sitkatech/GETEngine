using System.Collections.Generic;
using Olsson.GET.Common.DataContracts.Customers;
using Olsson.GET.Common.DataContracts.Users;

namespace Olsson.GET.Managers.Customers
{
    public interface ICustomerManager
    {
        CustomerDto[] FindAllCustomers();

        CustomerDto FindCustomerById(int customerId);

        User[] FindUsersForCustomer(int customerId);

        CustomerDto CreateOrUpdateCustomer(CustomerDto customerDto);

        CustomerModelScenario[] SaveCustomerModelScenarios(int customerId, CustomerModelScenario[] customerModelScenarios);

        CustomerModelWithScenariosDto[] FindAllModelsForCustomer(int customerId);

        CustomerModelWithScenariosDto FindModelForCustomer(int customerId, int modelId, int scenarioId);

        vModelCountScenarioCountForCustomerID GetModelCountScenarioCountForCustomerId(int customerId);

        bool CanCreateNewTrialRuns(int customerId);

        List<CustomerModelScenarioDto> FindCustomerModelScenarioDtos();
    }
}