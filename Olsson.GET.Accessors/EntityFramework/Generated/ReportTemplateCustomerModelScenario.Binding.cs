//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ReportTemplateCustomerModelScenario]

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[ReportTemplateCustomerModelScenario] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[ReportTemplateCustomerModelScenario]")]
    public partial class ReportTemplateCustomerModelScenario : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public ReportTemplateCustomerModelScenario()
        {

        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public ReportTemplateCustomerModelScenario(int reportTemplateCustomerModelScenarioID, int reportTemplateID, int customerID, int modelID, int scenarioID) : this()
        {
            this.ReportTemplateCustomerModelScenarioID = reportTemplateCustomerModelScenarioID;
            this.ReportTemplateID = reportTemplateID;
            this.CustomerID = customerID;
            this.ModelID = modelID;
            this.ScenarioID = scenarioID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public ReportTemplateCustomerModelScenario(int reportTemplateID, int customerID, int modelID, int scenarioID) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ReportTemplateCustomerModelScenarioID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.ReportTemplateID = reportTemplateID;
            this.CustomerID = customerID;
            this.ModelID = modelID;
            this.ScenarioID = scenarioID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public ReportTemplateCustomerModelScenario(ReportTemplate reportTemplate, Customer customer, Model model, Scenario scenario) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ReportTemplateCustomerModelScenarioID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.ReportTemplateID = reportTemplate.ReportTemplateID;
            this.ReportTemplate = reportTemplate;
            reportTemplate.ReportTemplateCustomerModelScenarios.Add(this);
            this.CustomerID = customer.CustomerID;
            this.Customer = customer;
            customer.ReportTemplateCustomerModelScenarios.Add(this);
            this.ModelID = model.ModelID;
            this.Model = model;
            model.ReportTemplateCustomerModelScenarios.Add(this);
            this.ScenarioID = scenario.ScenarioID;
            this.Scenario = scenario;
            scenario.ReportTemplateCustomerModelScenarios.Add(this);
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static ReportTemplateCustomerModelScenario CreateNewBlank(ReportTemplate reportTemplate, Customer customer, Model model, Scenario scenario)
        {
            return new ReportTemplateCustomerModelScenario(reportTemplate, customer, model, scenario);
        }

        /// <summary>
        /// Does this object have any dependent objects? (If it does have dependent objects, these would need to be deleted before this object could be deleted.)
        /// </summary>
        /// <returns></returns>
        public bool HasDependentObjects()
        {
            return false;
        }

        /// <summary>
        /// Active Dependent type names of this object
        /// </summary>
        public List<string> DependentObjectNames() 
        {
            var dependentObjects = new List<string>();
            
            return dependentObjects.Distinct().ToList();
        }

        /// <summary>
        /// Dependent type names of this entity
        /// </summary>
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(ReportTemplateCustomerModelScenario).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.ReportTemplateCustomerModelScenarios.Remove(this);
        }
        
        /// <summary>
        /// Delete entity plus all children
        /// </summary>
        public void DeleteFull(PrimaryDBContext dbContext)
        {
            
            Delete(dbContext);
        }

        [Key]
        public int ReportTemplateCustomerModelScenarioID { get; set; }
        public int ReportTemplateID { get; set; }
        public int CustomerID { get; set; }
        public int ModelID { get; set; }
        public int ScenarioID { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return ReportTemplateCustomerModelScenarioID; } set { ReportTemplateCustomerModelScenarioID = value; } }

        public virtual ReportTemplate ReportTemplate { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Model Model { get; set; }
        public virtual Scenario Scenario { get; set; }

        public static class FieldLengths
        {

        }
    }
}