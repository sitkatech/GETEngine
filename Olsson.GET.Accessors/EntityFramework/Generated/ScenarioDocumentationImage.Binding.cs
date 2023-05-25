//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ScenarioDocumentationImage]

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[ScenarioDocumentationImage] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[ScenarioDocumentationImage]")]
    public partial class ScenarioDocumentationImage : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public ScenarioDocumentationImage()
        {

        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public ScenarioDocumentationImage(int scenarioDocumentationImageID, int scenarioID, int fileResourceInfoID) : this()
        {
            this.ScenarioDocumentationImageID = scenarioDocumentationImageID;
            this.ScenarioID = scenarioID;
            this.FileResourceInfoID = fileResourceInfoID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public ScenarioDocumentationImage(int scenarioID, int fileResourceInfoID) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ScenarioDocumentationImageID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.ScenarioID = scenarioID;
            this.FileResourceInfoID = fileResourceInfoID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public ScenarioDocumentationImage(Scenario scenario, FileResourceInfo fileResourceInfo) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ScenarioDocumentationImageID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.ScenarioID = scenario.ScenarioID;
            this.Scenario = scenario;
            scenario.ScenarioDocumentationImages.Add(this);
            this.FileResourceInfoID = fileResourceInfo.FileResourceInfoID;
            this.FileResourceInfo = fileResourceInfo;
            fileResourceInfo.ScenarioDocumentationImages.Add(this);
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static ScenarioDocumentationImage CreateNewBlank(Scenario scenario, FileResourceInfo fileResourceInfo)
        {
            return new ScenarioDocumentationImage(scenario, fileResourceInfo);
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
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(ScenarioDocumentationImage).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.ScenarioDocumentationImages.Remove(this);
        }
        
        /// <summary>
        /// Delete entity plus all children
        /// </summary>
        public void DeleteFull(PrimaryDBContext dbContext)
        {
            
            Delete(dbContext);
        }

        [Key]
        public int ScenarioDocumentationImageID { get; set; }
        public int ScenarioID { get; set; }
        public int FileResourceInfoID { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return ScenarioDocumentationImageID; } set { ScenarioDocumentationImageID = value; } }

        public virtual Scenario Scenario { get; set; }
        public virtual FileResourceInfo FileResourceInfo { get; set; }

        public static class FieldLengths
        {

        }
    }
}