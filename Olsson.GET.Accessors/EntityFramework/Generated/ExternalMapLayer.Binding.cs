//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ExternalMapLayer]
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[ExternalMapLayer] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[ExternalMapLayer]")]
    public partial class ExternalMapLayer : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public ExternalMapLayer()
        {
            this.ExternalMapLayerCustomerModels = new HashSet<ExternalMapLayerCustomerModel>();
        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public ExternalMapLayer(int externalMapLayerID, string externalMapLayerDisplayName, int externalMapLayerTypeID, string externalMapLayerURL, bool layerIsOnByDefault, bool isActive, string externalMapLayerDescription, bool isAvailableForAllConfigurations, string featureNameField, string token) : this()
        {
            this.ExternalMapLayerID = externalMapLayerID;
            this.ExternalMapLayerDisplayName = externalMapLayerDisplayName;
            this.ExternalMapLayerTypeID = externalMapLayerTypeID;
            this.ExternalMapLayerURL = externalMapLayerURL;
            this.LayerIsOnByDefault = layerIsOnByDefault;
            this.IsActive = isActive;
            this.ExternalMapLayerDescription = externalMapLayerDescription;
            this.IsAvailableForAllConfigurations = isAvailableForAllConfigurations;
            this.FeatureNameField = featureNameField;
            this.Token = token;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public ExternalMapLayer(string externalMapLayerDisplayName, int externalMapLayerTypeID, string externalMapLayerURL, bool layerIsOnByDefault, bool isActive, bool isAvailableForAllConfigurations) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ExternalMapLayerID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.ExternalMapLayerDisplayName = externalMapLayerDisplayName;
            this.ExternalMapLayerTypeID = externalMapLayerTypeID;
            this.ExternalMapLayerURL = externalMapLayerURL;
            this.LayerIsOnByDefault = layerIsOnByDefault;
            this.IsActive = isActive;
            this.IsAvailableForAllConfigurations = isAvailableForAllConfigurations;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public ExternalMapLayer(string externalMapLayerDisplayName, ExternalMapLayerType externalMapLayerType, string externalMapLayerURL, bool layerIsOnByDefault, bool isActive, bool isAvailableForAllConfigurations) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ExternalMapLayerID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.ExternalMapLayerDisplayName = externalMapLayerDisplayName;
            this.ExternalMapLayerTypeID = externalMapLayerType.ExternalMapLayerTypeID;
            this.ExternalMapLayerURL = externalMapLayerURL;
            this.LayerIsOnByDefault = layerIsOnByDefault;
            this.IsActive = isActive;
            this.IsAvailableForAllConfigurations = isAvailableForAllConfigurations;
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static ExternalMapLayer CreateNewBlank(ExternalMapLayerType externalMapLayerType)
        {
            return new ExternalMapLayer(default(string), externalMapLayerType, default(string), default(bool), default(bool), default(bool));
        }

        /// <summary>
        /// Does this object have any dependent objects? (If it does have dependent objects, these would need to be deleted before this object could be deleted.)
        /// </summary>
        /// <returns></returns>
        public bool HasDependentObjects()
        {
            return ExternalMapLayerCustomerModels.Any();
        }

        /// <summary>
        /// Active Dependent type names of this object
        /// </summary>
        public List<string> DependentObjectNames() 
        {
            var dependentObjects = new List<string>();
            
            if(ExternalMapLayerCustomerModels.Any())
            {
                dependentObjects.Add(typeof(ExternalMapLayerCustomerModel).Name);
            }
            return dependentObjects.Distinct().ToList();
        }

        /// <summary>
        /// Dependent type names of this entity
        /// </summary>
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(ExternalMapLayer).Name, typeof(ExternalMapLayerCustomerModel).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.ExternalMapLayers.Remove(this);
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

            foreach(var x in ExternalMapLayerCustomerModels.ToList())
            {
                x.DeleteFull(dbContext);
            }
        }

        [Key]
        public int ExternalMapLayerID { get; set; }
        public string ExternalMapLayerDisplayName { get; set; }
        public int ExternalMapLayerTypeID { get; set; }
        public string ExternalMapLayerURL { get; set; }
        public bool LayerIsOnByDefault { get; set; }
        public bool IsActive { get; set; }
        public string ExternalMapLayerDescription { get; set; }
        public bool IsAvailableForAllConfigurations { get; set; }
        public string FeatureNameField { get; set; }
        public string Token { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return ExternalMapLayerID; } set { ExternalMapLayerID = value; } }

        public virtual ICollection<ExternalMapLayerCustomerModel> ExternalMapLayerCustomerModels { get; set; }
        public ExternalMapLayerType ExternalMapLayerType { get { return ExternalMapLayerType.AllLookupDictionary[ExternalMapLayerTypeID]; } }

        public static class FieldLengths
        {
            public const int ExternalMapLayerDisplayName = 100;
            public const int ExternalMapLayerURL = 500;
            public const int ExternalMapLayerDescription = 200;
            public const int FeatureNameField = 100;
            public const int Token = 255;
        }
    }
}