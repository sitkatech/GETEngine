using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Olsson.GET.Common.DataContracts.Customers;
using Olsson.GET.Common.DataContracts.Scenarios;
using Microsoft.AspNetCore.Html;

namespace Olsson.GET.Common.DataContracts.Models
{
    public class Model
    {
        public int ModelID { get; set; }

        public string ModelName { get; set; }

        public int ImageID { get; set; }

        public DateTime StartDateTime { get; set; }

        public string RunFileName { get; set; }

        public string ListFileName { get; set; }

        public virtual Scenario[] Scenarios { get; set; }

        public double? AllowablePercentDiscrepancy { get; set; }

        public string MapSettings { get; set; }

        public string MapRunFileName { get; set; }

        public string MapDrawdownFileName { get; set; }

        public string MapModelArea { get; set; }

        public bool IsDoubleSizeHeatMapOutput { get; set; }

        public string InputZoneData { get; set; }

        public string OutputZoneData { get; set; }

        public int NumberOfStressPeriods { get; set; }

        public string CanalData { get; set; }

        public string BuddyGroup { get; set; }
        public int? BaseflowTableProcessingConfigurationID { get; set; }
        public virtual BaseflowTableProcessingConfiguration BaseflowTableProcessingConfiguration { get; set; }

        public List<ModelStressPeriodCustomStartDate> ModelStressPeriodCustomStartDates { get; set; }
        public string ModelDescription { get; set; }
        public List<CustomerModelScenario> CustomerModelScenarios { get; set; }
        public List<ModelDocumentationImage> ModelDocumentationImages { get; set; }
        public string ModelDocumentation { get; set; }
        [NotMapped]
        public HtmlString ModelDocumentationHtmlString
        {
            get { return ModelDocumentation == null ? null : new HtmlString(ModelDocumentation); }
        }

        public int ModelEngineTypeID { get; set; }
        public int ModelGridTypeID { get; set; }

        public List<ModelExecutable> ModelExecutables { get; set; }
        //public List<Run> Runs { get; set; }

    }
}
