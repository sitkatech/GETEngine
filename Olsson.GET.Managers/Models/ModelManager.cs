using Serilog;
using Olsson.GET.Accessors.Models;
using Olsson.GET.Common.DataContracts.Customers;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.Utilities;
using System.Collections.Generic;

namespace Olsson.GET.Managers.Models
{
    public class ModelManager : BaseManager, IModelManager
    {
        private static readonly ILogger Logger = Logging.GetLogger<ModelManager>();

        public List<Model> List()
        {
            Logger.Information("Finding all models");

            return AccessorFactory.CreateAccessor<IModelAccessor>().List();
        }

        public Model GetById(int modelId)
        {
            Logger.Information($"Finding a model with Id {modelId}");

            return AccessorFactory.CreateAccessor<IModelAccessor>().GetById(modelId);
        }

        public bool ChangeModelDescription(int id, string newDescription)
        {
            Logger.Information($"Updating description for model with id:{id}");

            return AccessorFactory.CreateAccessor<IModelAccessor>().ChangeScenarioDescription(id, newDescription);
        }

        public List<Model> ListForCustomerID(int customerID)
        {
            Logger.Information($"Finding all models for customerID:{customerID}");

            return AccessorFactory.CreateAccessor<IModelAccessor>().ListForCustomerID(customerID);
        }

        public CustomerModelWithScenariosDto[] FindAllCustomerModels()
        {

            Logger.Information($"Finding all models");

            return AccessorFactory.CreateAccessor<IModelAccessor>().FindAllCustomerModels();
        }

        public bool UpdateModelDocumentation(int id, string newDocumentation)
        {
            Logger.Information($"Updating Model Documentation for model with id:{id}");

            return AccessorFactory.CreateAccessor<IModelAccessor>().UpdateModelDocumentation(id, newDocumentation);

        }

        public bool CreateModelDocumentationImage(int modelId, int fileResourceInfoId)
        {
            Logger.Information($"Creating Model Documentation Image for model with id:{modelId} and file resource info with id:{fileResourceInfoId}");

            return AccessorFactory.CreateAccessor<IModelAccessor>().CreateModelDocumentationImage(modelId, fileResourceInfoId);
        }
    }
}
