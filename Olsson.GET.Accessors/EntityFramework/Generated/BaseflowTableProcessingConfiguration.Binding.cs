//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[BaseflowTableProcessingConfiguration]
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[BaseflowTableProcessingConfiguration] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[BaseflowTableProcessingConfiguration]")]
    public partial class BaseflowTableProcessingConfiguration : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public BaseflowTableProcessingConfiguration()
        {
            this.Models = new HashSet<Model>();
        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public BaseflowTableProcessingConfiguration(int baseflowTableProcessingConfigurationID, string baseflowTableIndicatorRegexPattern, int segmentColumnNum, int flowToAquiferColumnNum, int? reachColumnNum) : this()
        {
            this.BaseflowTableProcessingConfigurationID = baseflowTableProcessingConfigurationID;
            this.BaseflowTableIndicatorRegexPattern = baseflowTableIndicatorRegexPattern;
            this.SegmentColumnNum = segmentColumnNum;
            this.FlowToAquiferColumnNum = flowToAquiferColumnNum;
            this.ReachColumnNum = reachColumnNum;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public BaseflowTableProcessingConfiguration(string baseflowTableIndicatorRegexPattern, int segmentColumnNum, int flowToAquiferColumnNum) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.BaseflowTableProcessingConfigurationID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.BaseflowTableIndicatorRegexPattern = baseflowTableIndicatorRegexPattern;
            this.SegmentColumnNum = segmentColumnNum;
            this.FlowToAquiferColumnNum = flowToAquiferColumnNum;
        }


        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static BaseflowTableProcessingConfiguration CreateNewBlank()
        {
            return new BaseflowTableProcessingConfiguration(default(string), default(int), default(int));
        }

        /// <summary>
        /// Does this object have any dependent objects? (If it does have dependent objects, these would need to be deleted before this object could be deleted.)
        /// </summary>
        /// <returns></returns>
        public bool HasDependentObjects()
        {
            return Models.Any();
        }

        /// <summary>
        /// Active Dependent type names of this object
        /// </summary>
        public List<string> DependentObjectNames() 
        {
            var dependentObjects = new List<string>();
            
            if(Models.Any())
            {
                dependentObjects.Add(typeof(Model).Name);
            }
            return dependentObjects.Distinct().ToList();
        }

        /// <summary>
        /// Dependent type names of this entity
        /// </summary>
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(BaseflowTableProcessingConfiguration).Name, typeof(Model).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.BaseflowTableProcessingConfigurations.Remove(this);
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

            foreach(var x in Models.ToList())
            {
                x.DeleteFull(dbContext);
            }
        }

        [Key]
        public int BaseflowTableProcessingConfigurationID { get; set; }
        public string BaseflowTableIndicatorRegexPattern { get; set; }
        public int SegmentColumnNum { get; set; }
        public int FlowToAquiferColumnNum { get; set; }
        public int? ReachColumnNum { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return BaseflowTableProcessingConfigurationID; } set { BaseflowTableProcessingConfigurationID = value; } }

        public virtual ICollection<Model> Models { get; set; }

        public static class FieldLengths
        {
            public const int BaseflowTableIndicatorRegexPattern = 200;
        }
    }
}