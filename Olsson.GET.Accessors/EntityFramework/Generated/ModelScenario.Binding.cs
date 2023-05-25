//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ModelScenario]

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[ModelScenario] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[ModelScenario]")]
    public partial class ModelScenario : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public ModelScenario()
        {

        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public ModelScenario(int modelScenarioID, int modelID, int scenarioID) : this()
        {
            this.ModelScenarioID = modelScenarioID;
            this.ModelID = modelID;
            this.ScenarioID = scenarioID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public ModelScenario(int modelID, int scenarioID) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ModelScenarioID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.ModelID = modelID;
            this.ScenarioID = scenarioID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public ModelScenario(Model model, Scenario scenario) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ModelScenarioID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.ModelID = model.ModelID;
            this.Model = model;
            model.ModelScenarios.Add(this);
            this.ScenarioID = scenario.ScenarioID;
            this.Scenario = scenario;
            scenario.ModelScenarios.Add(this);
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static ModelScenario CreateNewBlank(Model model, Scenario scenario)
        {
            return new ModelScenario(model, scenario);
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
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(ModelScenario).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.ModelScenarios.Remove(this);
        }
        
        /// <summary>
        /// Delete entity plus all children
        /// </summary>
        public void DeleteFull(PrimaryDBContext dbContext)
        {
            
            Delete(dbContext);
        }

        [Key]
        public int ModelScenarioID { get; set; }
        public int ModelID { get; set; }
        public int ScenarioID { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return ModelScenarioID; } set { ModelScenarioID = value; } }

        public virtual Model Model { get; set; }
        public virtual Scenario Scenario { get; set; }

        public static class FieldLengths
        {

        }
    }
}