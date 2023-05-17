using System.Collections.Generic;
using Olsson.GET.Common.DataContracts.ReportTemplate;
using Olsson.GET.Common.DataContracts.Runs;
using FileResourceInfo = Olsson.GET.Common.DataContracts.FileResource.FileResourceInfo;

namespace Olsson.GET.Managers.ReportTemplate
{
    public interface IReportTemplateManager
    {
        Common.DataContracts.ReportTemplate.ReportTemplate CreateReportTemplate(string displayName, string description, int fileResourceInfoID);
        List<Common.DataContracts.ReportTemplate.ReportTemplate> FindAll();
        List<Common.DataContracts.ReportTemplate.ReportTemplate> FindAllByCustomerID(int customerID);
        bool IsDisplayNameUnused(string displayName, int? id = null);
        void Delete(int id);
        Common.DataContracts.ReportTemplate.ReportTemplate FindByID(int id);
        void Update(int id, string displayName, string description);
        void ReplaceFileResource(Common.DataContracts.ReportTemplate.ReportTemplate reportTemplate, FileResourceInfo fileResourceInfo);
        void ConfigureReportTemplate(int id, bool availableForAllConfigurations, List<ReportTemplateCustomerModelScenarioDto> reportCustomerModelScenarioDtos, int? customerId);
        List<ReportTemplateCustomerModelScenarioDto> FindReportCustomerModelScenarios(int id);
        List<Common.DataContracts.ReportTemplate.ReportTemplate> FindReportTemplatesByRun(Run run);
    }
}
