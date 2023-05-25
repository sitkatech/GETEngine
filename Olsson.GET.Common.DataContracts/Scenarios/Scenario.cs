using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Olsson.GET.Common.DataContracts.Customers;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.DataContracts.Runs;
using Microsoft.AspNetCore.Html;

namespace Olsson.GET.Common.DataContracts.Scenarios
{
    public class Scenario
    {
        public int ScenarioID { get; set; }

        public string ScenarioName { get; set; }

        public InputControlType InputControlType { get; set; }

        public bool ShouldSwitchSign { get; set; }

        public int? InputImageID { get; set; }

        public string ScenarioDescription { get; set; }

        public bool ShowToAllUsersInScenarioList { get; set; }

        public string ScenarioDocumentation { get; set; }
        [NotMapped]
        public HtmlString ScenarioDocumentationHtmlString
        {
            get { return ScenarioDocumentation == null ? null : new HtmlString(ScenarioDocumentation); }
        }

        public Image InputImage { get; set; }

        public ScenarioFile[] ScenarioFiles { get; set; }
        public List<ModelSimpleDto> Models { get; set; }
        public List<CustomerModelScenario> CustomerModelScenarios { get; set; }
        public List<ScenarioDocumentationImage> ScenarioDocumentationImages { get; set; }
        public List<RunSimpleDto> Runs { get; set; }
    }
}
