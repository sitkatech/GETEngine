using System.Runtime.Serialization;
using Olsson.GET.Common.DataContracts.FileResource;

namespace Olsson.GET.Common.DataContracts.ReportTemplate
{
    [DataContract]
    public class ReportTemplate
    {

        [DataMember]
        public int ReportTemplateID { get; set; }

        [DataMember]
        public int FileResourceInfoID { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int ReportTemplateModelTypeID { get; set; }

        [DataMember]
        public int ReportTemplateModelID { get; set; }

        [DataMember]
        public FileResourceInfo FileResourceInfo { get; set; }

        [DataMember]
        public bool IsAvailableForAllConfigurations { get; set; }

        public ReportTemplateModelEnum ReportTemplateModel { get; set; }
        public ReportTemplateModelTypeEnum ReportTemplateModelType { get; set; }
    }

    public enum ReportTemplateModelEnum
    {
        Run = 1,
    }

    public enum ReportTemplateModelTypeEnum
    {
        SingleModel=1,
        MultipleModels=2,
    }
}