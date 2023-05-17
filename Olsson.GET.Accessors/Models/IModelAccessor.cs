using System.Collections.Generic;
using Olsson.GET.Common.DataContracts.Customers;
using Olsson.GET.Common.DataContracts.Models;

namespace Olsson.GET.Accessors.Models
{
    public interface IModelAccessor
    {
        Image FindImageForModel(int modelID);

        List<Model> List();

        Model GetById(int modelID);
        bool ChangeScenarioDescription(int modelID, string newDescription);
        List<Model> ListForCustomerID(int customerID);
        CustomerModelWithScenariosDto[] FindAllCustomerModels();
        bool UpdateModelDocumentation(int modelID, string newDocumentation);
        bool CreateModelDocumentationImage(int modelID, int fileResourceInfoID);
    }
}
