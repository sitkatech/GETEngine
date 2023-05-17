using System.Collections.Generic;
using Olsson.GET.Common.DataContracts.Customers;
using Olsson.GET.Common.DataContracts.Models;

namespace Olsson.GET.Managers.Models
{
    public interface IModelManager
    {
        List<Model> List();
        Model GetById(int modelId);
        bool ChangeModelDescription(int id, string newDescription);
        List<Model> ListForCustomerID(int customerID);

        CustomerModelWithScenariosDto[] FindAllCustomerModels();
        bool UpdateModelDocumentation(int id, string newDocumentation);
        bool CreateModelDocumentationImage(int modelId, int fileResourceInfoId);
    }
}
