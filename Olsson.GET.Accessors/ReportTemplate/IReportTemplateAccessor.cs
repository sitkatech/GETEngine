
using System.Collections.Generic;
using Olsson.GET.Common.DataContracts.FileResource;
using Olsson.GET.Common.DataContracts.ReportTemplate;
using Olsson.GET.Common.DataContracts.Runs;

namespace Olsson.GET.Accessors.ReportTemplate
{
    public interface IReportTemplateAccessor
    {
        Common.DataContracts.ReportTemplate.ReportTemplate CreateReportTemplate(string displayName, string description,
            int fileResourceInfoID);

        List<Common.DataContracts.ReportTemplate.ReportTemplate> FindAll();
        List<Common.DataContracts.ReportTemplate.ReportTemplate> FindAllByCustomerID(int customerID);
        bool IsDisplayNameUnused(string displayName, int? id);
        void Delete(int reportTemplateID);
        Common.DataContracts.ReportTemplate.ReportTemplate FindByID(int id);
        void Update(int id, string displayName, string description);
        void ReplaceFileResource(Common.DataContracts.ReportTemplate.ReportTemplate reportTemplate, FileResourceInfo newFileResourceInfo);
        void ConfigureReportTemplate(int reportTemplateID, bool availableForAllConfigurations, List<ReportTemplateCustomerModelScenarioDto> reportCustomerModelScenarioDtos, int? customerID);
        List<ReportTemplateCustomerModelScenarioDto> FindReportTemplateCustomerModelScenarios(int id);
        List<Common.DataContracts.ReportTemplate.ReportTemplate> FindReportTemplatesByRun(Run run);
    }
}
