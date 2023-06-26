using System.Collections.Generic;
using log4net;
using Microsoft.Extensions.Logging;
using Olsson.GET.Accessors.ReportTemplate;
using Olsson.GET.Common.DataContracts.FileResource;
using Olsson.GET.Common.DataContracts.ReportTemplate;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Utilities;
using Olsson.GET.Managers.FileResource;

namespace Olsson.GET.Managers.ReportTemplate
{
    internal class ReportTemplateManager : BaseManager, IReportTemplateManager
    {
        private static readonly ILogger Logger = Logging.GetLogger<FileResourceManager>();

        public Common.DataContracts.ReportTemplate.ReportTemplate CreateReportTemplate(string displayName, string description, int fileResourceInfoID)
        {
            Logger.LogInformation($"Creating report template {displayName}");

            return AccessorFactory.CreateAccessor<IReportTemplateAccessor>()
                .CreateReportTemplate(displayName, description, fileResourceInfoID);
        }

        public List<Common.DataContracts.ReportTemplate.ReportTemplate> FindAll()
        {
            Logger.LogInformation($"Finding all report templates");

            return AccessorFactory.CreateAccessor<IReportTemplateAccessor>().FindAll();
        }

        public List<Common.DataContracts.ReportTemplate.ReportTemplate> FindAllByCustomerID(int customerID)
        {
            Logger.LogInformation($"Finding all report templates for customer with ID:{customerID}");

            return AccessorFactory.CreateAccessor<IReportTemplateAccessor>().FindAllByCustomerID(customerID);
        }

        public bool IsDisplayNameUnused(string displayName, int? id = null)
        {
            Logger.LogInformation($"Checking for report template with display name {displayName}");

            return AccessorFactory.CreateAccessor<IReportTemplateAccessor>().IsDisplayNameUnused(displayName, id);
        }

        public void Delete(int id)
        {
            Logger.LogInformation($"Deleting report template {id}");

            AccessorFactory.CreateAccessor<IReportTemplateAccessor>().Delete(id);
        }

        public Common.DataContracts.ReportTemplate.ReportTemplate FindByID(int id)
        {
            Logger.LogInformation($"Finding report template {id}");
            return AccessorFactory.CreateAccessor<IReportTemplateAccessor>().FindByID(id);
        }

        public void Update(int id, string displayName, string description)
        {
            Logger.LogInformation($"Updating report template {id}");
            AccessorFactory.CreateAccessor<IReportTemplateAccessor>().Update(id, displayName, description);
        }

        public void ReplaceFileResource(Common.DataContracts.ReportTemplate.ReportTemplate reportTemplate, FileResourceInfo fileResourceInfo)
        {
            Logger.LogInformation($"Replacing file resource for report template {reportTemplate.ReportTemplateID}");

            AccessorFactory.CreateAccessor<IReportTemplateAccessor>()
                .ReplaceFileResource(reportTemplate, fileResourceInfo);
        }

        public void ConfigureReportTemplate(int id, bool availableForAllConfigurations,
            List<ReportTemplateCustomerModelScenarioDto> reportCustomerModelScenarioDtos, int? customerId)
        {
            Logger.LogInformation($"Updating configuration for report template {id}");

            AccessorFactory.CreateAccessor<IReportTemplateAccessor>().ConfigureReportTemplate(id,
                availableForAllConfigurations, reportCustomerModelScenarioDtos, customerId);
        }

        public List<ReportTemplateCustomerModelScenarioDto> FindReportCustomerModelScenarios(int id)
        {
            Logger.LogInformation($"Getting RCMS for report template {id}");

            return AccessorFactory.CreateAccessor<IReportTemplateAccessor>().FindReportTemplateCustomerModelScenarios(id);
        }

        public List<Common.DataContracts.ReportTemplate.ReportTemplate> FindReportTemplatesByRun(Run run)
        {
            Logger.LogInformation($"Finding report templates for action {run.RunID}");

            return AccessorFactory.CreateAccessor<IReportTemplateAccessor>().FindReportTemplatesByRun(run);
        }
    }
}