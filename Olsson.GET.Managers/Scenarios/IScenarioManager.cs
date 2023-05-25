using System.Collections.Generic;
using Olsson.GET.Common.DataContracts.Scenarios;

namespace Olsson.GET.Managers.Scenarios
{
    public interface IScenarioManager
    {
        List<Scenario> List();
        List<Scenario> ListForCustomerId(int customerId);
        Scenario GetById(int id);
        bool ChangeScenarioDescription(int id, string newDescription);
        bool ChangeShowToAllUsersInScenarioList(int id, bool showToAllUsersInScenarioList);
        bool UpdateScenarioDocumentation(int id, string newDocumentation);
        bool CreateScenarioDocumentationImage(int scenarioId, int fileResourceInfoId);

    }
}