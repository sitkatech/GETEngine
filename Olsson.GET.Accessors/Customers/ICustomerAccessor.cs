using System.Collections.Generic;
using Olsson.GET.Common.DataContracts.Customers;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.DataContracts.Users;

namespace Olsson.GET.Accessors.Customers
{
    public interface ICustomerAccessor
    {
        CustomerDto[] FindAllCustomers();

        CustomerDto FindCustomerById(int customerID);

        User[] FindUsersForCustomer(int customerID);

        CustomerDto CreateOrUpdateCustomer(CustomerDto customerDto);

        CustomerModelScenario[] SaveCustomerModelScenarios(int customerID, CustomerModelScenario[] customerModelScenarios);

        CustomerModelWithScenariosDto[] FindAllModelsForCustomer(int customerID);

        CustomerModelWithScenariosDto FindModelForCustomer(int customerID, int modelID, int scenarioID);

        vModelCountScenarioCountForCustomerID GetModelCountScenarioCountForCustomerId(int customerId);

        int GetExecutedRunCountForCustomer(int customerID);
        List<CustomerModelScenarioDto> FindCustomerModelScenarios();
    }
}
