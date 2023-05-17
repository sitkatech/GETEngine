using System.Collections.Generic;
using System.Linq;
using Olsson.GET.Accessors.EntityFramework;
using Olsson.GET.Common.DataContracts.ReportTemplate;
using FileResourceInfo = Olsson.GET.Common.DataContracts.FileResource.FileResourceInfo;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;

namespace Olsson.GET.Accessors.ReportTemplate
{
    internal class ReportTemplateAccessor : BaseTableAccessor, IReportTemplateAccessor
    {
        public Common.DataContracts.ReportTemplate.ReportTemplate CreateReportTemplate(string displayName,
            string description, int fileResourceInfoID)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                EntityFramework.ReportTemplate reportTemplate = new EntityFramework.ReportTemplate()
                {
                    FileResourceInfoID = fileResourceInfoID,
                    Description = description,
                    DisplayName = displayName,
                    // actions are the only available model for reports right now, and only single actions are currently supported...
                    ReportTemplateModelID = 1,
                    ReportTemplateModelTypeID = 1
                };

                db.ReportTemplates.Add(reportTemplate);
                db.SaveChanges();

                return DTOMapper.Mapper.Map<Common.DataContracts.ReportTemplate.ReportTemplate>(reportTemplate);

            }
        }

        public List<Common.DataContracts.ReportTemplate.ReportTemplate> FindAll()
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                return db.ReportTemplates.Include("FileResourceInfo").ToList()
                    .Select(x => DTOMapper.Mapper.Map<Common.DataContracts.ReportTemplate.ReportTemplate>(x)).ToList();
            }
        }

        public List<Common.DataContracts.ReportTemplate.ReportTemplate> FindAllByCustomerID(int customerID)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                return db.ReportTemplates.Include("FileResourceInfo").Include("ReportTemplateCustomerModelScenarios").Where(x =>x.IsAvailableForAllConfigurations ||  x.ReportTemplateCustomerModelScenarios.Any(y => y.CustomerID == customerID)).ToList()
                    .Select(x => DTOMapper.Mapper.Map<Common.DataContracts.ReportTemplate.ReportTemplate>(x)).ToList();
            }
        }

        public bool IsDisplayNameUnused(string displayName, int? id)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                if (id.HasValue)
                {
                    var idValue = id.GetValueOrDefault();
                    return !db.ReportTemplates.Any(x => x.DisplayName == displayName && x.ReportTemplateID != idValue);
                }

                return !db.ReportTemplates.Any(x => x.DisplayName == displayName);

            }
        }

        public void Delete(int reportTemplateID)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                var rcms = db.ReportTemplateCustomerModelScenarios.Where(x => x.ReportTemplateID == reportTemplateID);

                db.ReportTemplateCustomerModelScenarios.RemoveRange(rcms);
                db.SaveChanges();

                var reportTemplate = db.ReportTemplates.Include("FileResourceInfo").Include("FileResourceInfo.FileResourceDatas").Single(x=>x.ReportTemplateID == reportTemplateID);
                var reportTemplateFileResourceInfo = reportTemplate.FileResourceInfo;
                var fileResourceData = reportTemplateFileResourceInfo.FileResourceDatas.Single();

                db.ReportTemplates.Remove(reportTemplate);
                db.SaveChanges();

                db.FileResourceDatas.Remove(fileResourceData);
                db.SaveChanges();

                db.FileResourceInfos.Remove(reportTemplateFileResourceInfo);
                db.SaveChanges();
            }
        }

        public Common.DataContracts.ReportTemplate.ReportTemplate FindByID(int id)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                var singleOrDefault = db.ReportTemplates.Include("FileResourceInfo").SingleOrDefault(x => x.ReportTemplateID == id);

                return DTOMapper.Mapper.Map<Common.DataContracts.ReportTemplate.ReportTemplate>(singleOrDefault);
            }
        }

        public void Update(int id, string displayName, string description)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                var reportTemplate = db.ReportTemplates.Single(x => x.ReportTemplateID == id);

                reportTemplate.DisplayName = displayName;
                reportTemplate.Description = description;

                db.SaveChanges();
            }
        }

        public void ReplaceFileResource(Common.DataContracts.ReportTemplate.ReportTemplate reportTemplate, FileResourceInfo newFileResourceInfo)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                var reportTemplateEntity = db.ReportTemplates.Include("FileResourceInfo").Include("FileResourceInfo.FileResourceDatas").Single(x => x.ReportTemplateID == reportTemplate.ReportTemplateID);

                var reportTemplateFileResourceInfo = reportTemplateEntity.FileResourceInfo;
                var fileResourceData = reportTemplateFileResourceInfo.FileResourceDatas.Single();
                
                reportTemplateEntity.FileResourceInfoID = newFileResourceInfo.FileResourceInfoID;

                db.SaveChanges();

                db.FileResourceDatas.Remove(fileResourceData);
                db.SaveChanges();

                db.FileResourceInfos.Remove(reportTemplateFileResourceInfo);
                db.SaveChanges();
            }
        }

        public void ConfigureReportTemplate(int reportTemplateID, bool availableForAllConfigurations, List<ReportTemplateCustomerModelScenarioDto> reportTemplateCustomerModelScenarioDtos, int? customerID)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                IEnumerable<ReportTemplateCustomerModelScenario> oldReportTemplateCustomerModelScenarios = db.ReportTemplateCustomerModelScenarios.Where(x => x.ReportTemplateID == reportTemplateID);

                if (availableForAllConfigurations)
                {
                    db.ReportTemplateCustomerModelScenarios.RemoveRange(oldReportTemplateCustomerModelScenarios);
                    db.SaveChanges();

                    var reportTemplate = db.ReportTemplates.Single(x=>x.ReportTemplateID == reportTemplateID);

                    reportTemplate.IsAvailableForAllConfigurations = true;
                    db.SaveChanges();
                }
                else
                {
                    if (customerID != null)
                    {
                        oldReportTemplateCustomerModelScenarios =
                            oldReportTemplateCustomerModelScenarios.ToList().Where(x => x.CustomerID == customerID.Value);
                    }
                    db.ReportTemplateCustomerModelScenarios.RemoveRange(oldReportTemplateCustomerModelScenarios);
                    db.SaveChanges();

                    var reportTemplate = db.ReportTemplates.Single(x => x.ReportTemplateID == reportTemplateID);
                    reportTemplate.IsAvailableForAllConfigurations = false;

                    var newReportTemplateCustomerModelScenarios = reportTemplateCustomerModelScenarioDtos.Select(x => new ReportTemplateCustomerModelScenario()
                    {
                        CustomerID = x.CustomerID, ModelID = x.ModelID, ScenarioID = x.ScenarioID, ReportTemplateID = reportTemplateID
                    });
                    db.ReportTemplateCustomerModelScenarios.AddRange(newReportTemplateCustomerModelScenarios);
                    db.SaveChanges();
                }
            }
        }

        public List<ReportTemplateCustomerModelScenarioDto> FindReportTemplateCustomerModelScenarios(int reportTemplateID)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                return db.ReportTemplateCustomerModelScenarios.Where(x=>x.ReportTemplateID == reportTemplateID).ToList().Select(x => new ReportTemplateCustomerModelScenarioDto()
                    {
                        CustomerID = x.CustomerID, ModelID = x.ModelID, ScenarioID = x.ScenarioID
                    })
                    .ToList();
            }
        }

        public List<Common.DataContracts.ReportTemplate.ReportTemplate> FindReportTemplatesByRun(Run run)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                var reportTemplatesFromRun = db.ReportTemplateCustomerModelScenarios.Include("ReportTemplate")
                    .Where(x =>
                        x.CustomerID == run.CustomerID
                        && x.ModelID == run.ModelID
                        && x.ScenarioID == run.ScenarioID)
                    .ToList()
                    .Select(x =>
                        DTOMapper.Mapper.Map<Common.DataContracts.ReportTemplate.ReportTemplate>(x.ReportTemplate))
                    .ToList();

                var reportTemplatesAvailableForAllConfigurations = db.ReportTemplates
                    .Where(x => x.IsAvailableForAllConfigurations).ToList().Select(x =>
                        DTOMapper.Mapper.Map<Common.DataContracts.ReportTemplate.ReportTemplate>(x));

                reportTemplatesFromRun.AddRange(
                    reportTemplatesAvailableForAllConfigurations);

                return reportTemplatesFromRun;
            }
        }
    }
}