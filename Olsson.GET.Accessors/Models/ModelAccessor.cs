using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Common.DataContracts.Customers;
using Olsson.GET.Common.DataContracts.Models;
using Image = Olsson.GET.Common.DataContracts.Models.Image;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using ModelDocumentationImage = Olsson.GET.Common.DataContracts.Models.ModelDocumentationImage;
using Scenario = Olsson.GET.Common.DataContracts.Scenarios.Scenario;
using ScenarioFile = Olsson.GET.Common.DataContracts.Scenarios.ScenarioFile;

namespace Olsson.GET.Accessors.Models
{
    class ModelAccessor : BaseTableAccessor, IModelAccessor
    {
        public Image FindImageForModel(int modelID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var image = (from i in db.Images
                             join m in db.Models on i.ImageID equals m.ImageID
                             where m.ModelID == modelID
                             select i).FirstOrDefault();

                return DTOMapper.Mapper.Map<Image>(image);
            }
        }

        public List<Model> List()
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var models = GetModelsImpl(db).OrderBy(x => x.ModelName).ToList();
                return models.Select(ToModelDto).ToList();
            }
        }

        private static ModelSimpleDto ToSimpleDto(EntityFramework.Model model)
        {
            return new ModelSimpleDto()
            {
                ModelID = model.ModelID,
                ModelDescription = model.ModelDescription,
                ModelName = model.ModelName,
                NumberOfStressPeriods = model.NumberOfStressPeriods
            };
        }

        private static Model ToModelDto(EntityFramework.Model model)
        {
            return new Model()
            {
                ModelID = model.ModelID,
                ModelName = model.ModelName,
                ImageID = model.ImageID,
                StartDateTime = model.StartDateTime,
                RunFileName = model.RunFileName,
                ListFileName = model.ListFileName,
                AllowablePercentDiscrepancy = model.AllowablePercentDiscrepancy,
                MapSettings = model.MapSettings,
                MapRunFileName = model.MapRunFileName,
                MapDrawdownFileName = model.MapDrawdownFileName,
                MapModelArea = model.ModelMapAreaBoundary?.MapAreaBoundary,
                IsDoubleSizeHeatMapOutput = model.IsDoubleSizeHeatMapOutput,
                InputZoneData = model.ModelInputZoneData?.InputZoneData,
                OutputZoneData = model.ModelOutputZoneData?.OutputZoneData,
                NumberOfStressPeriods = model.NumberOfStressPeriods,
                CanalData = model.CanalData,
                BuddyGroup = model.BuddyGroup,
                BaseflowTableProcessingConfigurationID = model.BaseflowTableProcessingConfigurationID,
                ModelDescription = model.ModelDescription,
                ModelDocumentation = model.ModelDocumentation,
                ModelExecutables = model.ModelExecutables.Select(x => new Common.DataContracts.Models.ModelExecutable()
                {
                    ModelID = x.ModelID,
                    ExecutableName = x.ExecutableName,
                    Arguments = x.Arguments,
                    RunOrder = x.RunOrder,
                    WorkingDirectory = x.WorkingDirectory,
                    WrapWithBatchFile = x.WrapWithBatchFile,
                    UseShellExecute = x.UseShellExecute,
                    RedirectStandardOutput = x.RedirectStandardOutput,
                    CreateNoWindow = x.CreateNoWindow
                }).ToList(),
                CustomerModelScenarios = model.CustomerModelScenarios.Select(x => new Common.DataContracts.Customers.CustomerModelScenario()
                {
                    CustomerID = x.CustomerID,
                    ModelID = x.ModelID,
                    ScenarioID = x.ScenarioID
                }).ToList(),
                Scenarios = model.ModelScenarios.Select(x => new Scenario()
                {
                    ScenarioID = x.ScenarioID,
                    ScenarioName = x.Scenario.ScenarioName,
                    InputControlType = (InputControlType)x.Scenario.InputControlType,
                    ShouldSwitchSign = x.Scenario.ShouldSwitchSign,
                    InputImageID = x.Scenario.InputImageID,
                    ScenarioDescription = x.Scenario.ScenarioDescription,
                    ShowToAllUsersInScenarioList = x.Scenario.ShowToAllUsersInScenarioList,
                    Models = x.Scenario.ModelScenarios.Select(y => ToSimpleDto(y.Model)).ToList()
                }).ToArray()
            };
        }

        public IQueryable<EntityFramework.Model> GetModelsImpl(PrimaryDBContext db)
        {
            return db.Models
                .Include("ModelStressPeriodCustomStartDates")
                .Include("ModelScenarios.Scenario")
                .Include(x => x.CustomerModelScenarios)
                .Include("CustomerModelScenarios.Scenario");
        }

        public Model GetById(int modelID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                return ToModelDto(GetModelsImpl(db).Include(x => x.ModelMapAreaBoundaries).SingleOrDefault(x => x.ModelID == modelID));
            }
        }

        public bool ChangeScenarioDescription(int modelID, string newDescription)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var model = db.Models.First(x => x.ModelID == modelID);

                model.ModelDescription = newDescription;

                var rowsAffected = db.SaveChanges();
                return rowsAffected == 0 || rowsAffected == 1;
            }
        }

        public List<Model> ListForCustomerID(int customerID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var models = GetModelsImpl(db)
                    .Where(x => x.CustomerModelScenarios.Any(y => y.CustomerID == customerID))
                    .OrderByDescending(x => x.CustomerModelScenarios.Count(y => y.CustomerID == customerID))
                    .ThenBy(x => x.ModelName).ToList();
                return DTOMapper.Mapper.Map<List<Model>>(models);
            }
        }

        public CustomerModelWithScenariosDto[] FindAllCustomerModels()
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var models = (from m in db.Models
                              select new
                              {
                                  m.ModelID,
                                  m.ModelName,
                                  Scenarios = m.ModelScenarios.Select(x => x.Scenario).Select(s => new
                                  {
                                      s.ScenarioID,
                                      s.ScenarioName,
                                      s.InputControlType,
                                      Enabled = s.CustomerModelScenarios.Any(a => a.ModelID == m.ModelID),
                                      s.ScenarioFiles
                                  })
                              }).ToList();

                var result = new List<CustomerModelWithScenariosDto>();

                foreach (var model in models)
                {
                    result.Add(new CustomerModelWithScenariosDto
                    {
                        ModelID = model.ModelID,
                        ModelName = model.ModelName,
                        Scenarios = model.Scenarios.Select(s =>
                            new CustomerScenario
                            {
                                ScenarioID = s.ScenarioID,
                                ScenarioName = s.ScenarioName,
                                Enabled = s.Enabled,
                                InputControlType = (InputControlType)s.InputControlType,
                                ScenarioFiles = s.ScenarioFiles.Select(f =>
                                    new ScenarioFile
                                    {
                                        ScenarioFileID = f.ScenarioFileID,
                                        ScenarioID = f.ScenarioID,
                                        ScenarioFileName = f.ScenarioFileName,
                                        ScenarioFileDescription = f.ScenarioFileDescription,
                                        IsRequired = f.IsRequired
                                    }
                                ).ToArray()
                            }).ToArray()
                    });
                }

                return result.ToArray();
            }
        }

        public bool UpdateModelDocumentation(int modelID, string newDocumentation)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var model = db.Models.Single(x => x.ModelID == modelID);

                if (model.ModelDocumentation == newDocumentation)
                {
                    return true;
                }

                model.ModelDocumentation = newDocumentation;

                var result = db.SaveChanges();

                return result == 0 || result == 1;
            }
        }

        public bool CreateModelDocumentationImage(int modelID, int fileResourceInfoID)
        {
            var modelDocumentationImage = new ModelDocumentationImage()
            {
                ModelID = modelID,
                FileResourceInfoID = fileResourceInfoID
            };

            return base.CreateOrUpdate<ModelDocumentationImage, EntityFramework.ModelDocumentationImage, PrimaryDBContext>(modelDocumentationImage) != null;
        }
    }
}
