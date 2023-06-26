using Microsoft.Extensions.Logging;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Common.Utilities;
using System.Collections.Generic;
using System.Linq;
using Scenario = Olsson.GET.Common.DataContracts.Scenarios.Scenario;
using ScenarioDocumentationImage = Olsson.GET.Common.DataContracts.Scenarios.ScenarioDocumentationImage;

namespace Olsson.GET.Accessors.Scenarios
{
    internal class ScenarioAccessor : BaseTableAccessor, IScenarioAccessor
    {
        private static readonly ILogger Logger = Logging.GetLogger<ScenarioAccessor>();

        public IQueryable<EntityFramework.Scenario> GetScenariosImpl(PrimaryDBContext db)
        {
            return db.Scenarios
                .Include("Runs")
                .Include("Runs.User")
                .Include("ModelScenarios.Model")
                .Include("InputImage")
                .Include("CustomerModelScenarios")
                .Include("ScenarioFiles");
        }

        public List<Scenario> List()
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                return DTOMapper.Mapper.Map<List<Scenario>>(
                    GetScenariosImpl(db)
                        .OrderByDescending(x => x.ModelScenarios.Count)
                        .ThenBy(x => x.ScenarioName).ToList());
            }
        }

        public List<Scenario> ListForCustomerId(int customerID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                return DTOMapper.Mapper.Map<List<Scenario>>(
                    GetScenariosImpl(db)
                        .Where(x => x.ShowToAllUsersInScenarioList || x.CustomerModelScenarios.Any(y => y.CustomerID == customerID))
                        .OrderByDescending(x => x.CustomerModelScenarios.Count(y => y.CustomerID == customerID))
                        .ThenBy(x => x.ScenarioName).ToList());
            }
        }

        public Scenario GetById(int scenarioID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                return DTOMapper.Mapper.Map<Scenario>(GetScenariosImpl(db).SingleOrDefault(x => x.ScenarioID == scenarioID));
            }
        }
        public bool ChangeScenarioDescription(int scenarioID, string newDescription)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var scenario = db.Scenarios.First(x => x.ScenarioID == scenarioID);

                scenario.ScenarioDescription = newDescription;

                return db.SaveChanges() == 1;
            }
        }

        public bool ChangeShowToAllUsersInScenarioList(int scenarioID, bool showToAllUsersInScenarioList)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var scenario = db.Scenarios.Single(x => x.ScenarioID == scenarioID);

                scenario.ShowToAllUsersInScenarioList = showToAllUsersInScenarioList;

                return db.SaveChanges() == 1;
            }
        }

        public bool UpdateScenarioDocumentation(int scenarioID, string newDocumentation)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var scenario = db.Scenarios.Single(x => x.ScenarioID == scenarioID);

                if (scenario.ScenarioDocumentation == newDocumentation)
                {
                    return true;
                }

                scenario.ScenarioDocumentation = newDocumentation;

                var result = db.SaveChanges();

                return result == 0 || result == 1;
            }
        }

        public bool CreateScenarioDocumentationImage(int scenarioId, int fileResourceInfoId)
        {
            var scenarioDocumentationImage = new ScenarioDocumentationImage()
            {
                ScenarioID = scenarioId,
                FileResourceInfoID = fileResourceInfoId
            };

            return base.CreateOrUpdate<ScenarioDocumentationImage, EntityFramework.ScenarioDocumentationImage, PrimaryDBContext>(scenarioDocumentationImage) != null;
        }
    }
}