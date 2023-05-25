using System.Collections.Generic;
using Scenario =  Olsson.GET.Common.DataContracts.Scenarios.Scenario;

namespace Olsson.GET.Accessors.Scenarios
{
    public interface IScenarioAccessor
    { 
        List<Scenario> List();
        List<Scenario> ListForCustomerId(int customerID);
        Scenario GetById(int scenarioID);
        bool ChangeScenarioDescription(int scenarioID, string newDescription);
        bool ChangeShowToAllUsersInScenarioList(int scenarioID, bool showToAllUsersInScenarioList);
        bool UpdateScenarioDocumentation(int scenarioID, string newDocumentation);
        bool CreateScenarioDocumentationImage(int scenarioId, int fileResourceInfoId);
    }
}