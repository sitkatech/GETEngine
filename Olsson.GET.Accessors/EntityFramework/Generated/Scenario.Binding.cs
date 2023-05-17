//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[Scenario]
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[Scenario] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[Scenario]")]
    public partial class Scenario : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public Scenario()
        {
            this.CustomerModelScenarios = new HashSet<CustomerModelScenario>();
            this.ModelScenarios = new HashSet<ModelScenario>();
            this.ReportTemplateCustomerModelScenarios = new HashSet<ReportTemplateCustomerModelScenario>();
            this.Runs = new HashSet<Run>();
            this.ScenarioDocumentationImages = new HashSet<ScenarioDocumentationImage>();
            this.ScenarioFiles = new HashSet<ScenarioFile>();
        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public Scenario(int scenarioID, string scenarioName, int inputControlType, bool shouldSwitchSign, int? inputImageID, string scenarioDescription, bool showToAllUsersInScenarioList, string scenarioDocumentation) : this()
        {
            this.ScenarioID = scenarioID;
            this.ScenarioName = scenarioName;
            this.InputControlType = inputControlType;
            this.ShouldSwitchSign = shouldSwitchSign;
            this.InputImageID = inputImageID;
            this.ScenarioDescription = scenarioDescription;
            this.ShowToAllUsersInScenarioList = showToAllUsersInScenarioList;
            this.ScenarioDocumentation = scenarioDocumentation;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public Scenario(string scenarioName, int inputControlType, bool shouldSwitchSign, bool showToAllUsersInScenarioList) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ScenarioID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.ScenarioName = scenarioName;
            this.InputControlType = inputControlType;
            this.ShouldSwitchSign = shouldSwitchSign;
            this.ShowToAllUsersInScenarioList = showToAllUsersInScenarioList;
        }


        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static Scenario CreateNewBlank()
        {
            return new Scenario(default(string), default(int), default(bool), default(bool));
        }

        /// <summary>
        /// Does this object have any dependent objects? (If it does have dependent objects, these would need to be deleted before this object could be deleted.)
        /// </summary>
        /// <returns></returns>
        public bool HasDependentObjects()
        {
            return CustomerModelScenarios.Any() || ModelScenarios.Any() || ReportTemplateCustomerModelScenarios.Any() || Runs.Any() || ScenarioDocumentationImages.Any() || ScenarioFiles.Any();
        }

        /// <summary>
        /// Active Dependent type names of this object
        /// </summary>
        public List<string> DependentObjectNames() 
        {
            var dependentObjects = new List<string>();
            
            if(CustomerModelScenarios.Any())
            {
                dependentObjects.Add(typeof(CustomerModelScenario).Name);
            }

            if(ModelScenarios.Any())
            {
                dependentObjects.Add(typeof(ModelScenario).Name);
            }

            if(ReportTemplateCustomerModelScenarios.Any())
            {
                dependentObjects.Add(typeof(ReportTemplateCustomerModelScenario).Name);
            }

            if(Runs.Any())
            {
                dependentObjects.Add(typeof(Run).Name);
            }

            if(ScenarioDocumentationImages.Any())
            {
                dependentObjects.Add(typeof(ScenarioDocumentationImage).Name);
            }

            if(ScenarioFiles.Any())
            {
                dependentObjects.Add(typeof(ScenarioFile).Name);
            }
            return dependentObjects.Distinct().ToList();
        }

        /// <summary>
        /// Dependent type names of this entity
        /// </summary>
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(Scenario).Name, typeof(CustomerModelScenario).Name, typeof(ModelScenario).Name, typeof(ReportTemplateCustomerModelScenario).Name, typeof(Run).Name, typeof(ScenarioDocumentationImage).Name, typeof(ScenarioFile).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.Scenarios.Remove(this);
        }
        
        /// <summary>
        /// Delete entity plus all children
        /// </summary>
        public void DeleteFull(PrimaryDBContext dbContext)
        {
            DeleteChildren(dbContext);
            Delete(dbContext);
        }
        /// <summary>
        /// Dependent type names of this entity
        /// </summary>
        public void DeleteChildren(PrimaryDBContext dbContext)
        {

            foreach(var x in CustomerModelScenarios.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in ModelScenarios.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in ReportTemplateCustomerModelScenarios.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in Runs.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in ScenarioDocumentationImages.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in ScenarioFiles.ToList())
            {
                x.DeleteFull(dbContext);
            }
        }

        [Key]
        public int ScenarioID { get; set; }
        public string ScenarioName { get; set; }
        public int InputControlType { get; set; }
        public bool ShouldSwitchSign { get; set; }
        public int? InputImageID { get; set; }
        public string ScenarioDescription { get; set; }
        public bool ShowToAllUsersInScenarioList { get; set; }
        public string ScenarioDocumentation { get; set; }
        [NotMapped]
        public HtmlString ScenarioDocumentationHtmlString
        { 
            get { return ScenarioDocumentation == null ? null : new HtmlString(ScenarioDocumentation); }
            set { ScenarioDocumentation = value?.ToString(); }
        }
        [NotMapped]
        public int PrimaryKey { get { return ScenarioID; } set { ScenarioID = value; } }

        public virtual ICollection<CustomerModelScenario> CustomerModelScenarios { get; set; }
        public virtual ICollection<ModelScenario> ModelScenarios { get; set; }
        public virtual ICollection<ReportTemplateCustomerModelScenario> ReportTemplateCustomerModelScenarios { get; set; }
        public virtual ICollection<Run> Runs { get; set; }
        public virtual ICollection<ScenarioDocumentationImage> ScenarioDocumentationImages { get; set; }
        public virtual ICollection<ScenarioFile> ScenarioFiles { get; set; }
        public virtual Image InputImage { get; set; }

        public static class FieldLengths
        {
            public const int ScenarioName = 256;
            public const int ScenarioDescription = 500;
        }
    }
}