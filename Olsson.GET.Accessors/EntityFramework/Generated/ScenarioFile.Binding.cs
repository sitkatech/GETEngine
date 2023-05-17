//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ScenarioFile]
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[ScenarioFile] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[ScenarioFile]")]
    public partial class ScenarioFile : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public ScenarioFile()
        {

        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public ScenarioFile(int scenarioFileID, int scenarioID, string scenarioFileName, string scenarioFileDescription, bool isRequired) : this()
        {
            this.ScenarioFileID = scenarioFileID;
            this.ScenarioID = scenarioID;
            this.ScenarioFileName = scenarioFileName;
            this.ScenarioFileDescription = scenarioFileDescription;
            this.IsRequired = isRequired;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public ScenarioFile(int scenarioID, string scenarioFileName, bool isRequired) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ScenarioFileID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.ScenarioID = scenarioID;
            this.ScenarioFileName = scenarioFileName;
            this.IsRequired = isRequired;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public ScenarioFile(Scenario scenario, string scenarioFileName, bool isRequired) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ScenarioFileID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.ScenarioID = scenario.ScenarioID;
            this.Scenario = scenario;
            scenario.ScenarioFiles.Add(this);
            this.ScenarioFileName = scenarioFileName;
            this.IsRequired = isRequired;
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static ScenarioFile CreateNewBlank(Scenario scenario)
        {
            return new ScenarioFile(scenario, default(string), default(bool));
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
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(ScenarioFile).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.ScenarioFiles.Remove(this);
        }
        
        /// <summary>
        /// Delete entity plus all children
        /// </summary>
        public void DeleteFull(PrimaryDBContext dbContext)
        {
            
            Delete(dbContext);
        }

        [Key]
        public int ScenarioFileID { get; set; }
        public int ScenarioID { get; set; }
        public string ScenarioFileName { get; set; }
        public string ScenarioFileDescription { get; set; }
        public bool IsRequired { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return ScenarioFileID; } set { ScenarioFileID = value; } }

        public virtual Scenario Scenario { get; set; }

        public static class FieldLengths
        {
            public const int ScenarioFileName = 256;
            public const int ScenarioFileDescription = 512;
        }
    }
}