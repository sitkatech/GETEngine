using System.Collections.Generic;
using log4net;
using Olsson.GET.Accessors.Models;
using Olsson.GET.Common.DataContracts.Customers;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.Utilities;

namespace Olsson.GET.Managers.Models
{
    public class ModelManager : BaseManager, IModelManager
    {
        private static readonly ILog Logger = Logging.GetLogger(typeof(ModelManager));

        public List<Model> List()
        {
            Logger.Info("Finding all models");

            return AccessorFactory.CreateAccessor<IModelAccessor>().List();
        }

        public Model GetById(int modelId)
        {
            Logger.Info($"Finding a model with Id {modelId}");

            return AccessorFactory.CreateAccessor<IModelAccessor>().GetById(modelId);
        }

        public bool ChangeModelDescription(int id, string newDescription)
        {
            Logger.Info($"Updating description for model with id:{id}");

            return AccessorFactory.CreateAccessor<IModelAccessor>().ChangeScenarioDescription(id, newDescription);
        }

        public List<Model> ListForCustomerID(int customerID)
        {
            Logger.Info($"Finding all models for customerID:{customerID}");

            return AccessorFactory.CreateAccessor<IModelAccessor>().ListForCustomerID(customerID);
        }

        public CustomerModelWithScenariosDto[] FindAllCustomerModels()
        {

            Logger.Info($"Finding all models");

            return AccessorFactory.CreateAccessor<IModelAccessor>().FindAllCustomerModels();
        }

        public bool UpdateModelDocumentation(int id, string newDocumentation)
        {
            Logger.Info($"Updating Model Documentation for model with id:{id}");

            return AccessorFactory.CreateAccessor<IModelAccessor>().UpdateModelDocumentation(id, newDocumentation);

        }

        public bool CreateModelDocumentationImage(int modelId, int fileResourceInfoId)
        {
            Logger.Info($"Creating Model Documentation Image for model with id:{modelId} and file resource info with id:{fileResourceInfoId}");

            return AccessorFactory.CreateAccessor<IModelAccessor>().CreateModelDocumentationImage(modelId, fileResourceInfoId);
        }
    }
}
