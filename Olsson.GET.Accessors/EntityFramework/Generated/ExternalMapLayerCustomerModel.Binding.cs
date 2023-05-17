//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ExternalMapLayerCustomerModel]
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[ExternalMapLayerCustomerModel] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[ExternalMapLayerCustomerModel]")]
    public partial class ExternalMapLayerCustomerModel : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public ExternalMapLayerCustomerModel()
        {

        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public ExternalMapLayerCustomerModel(int externalMapLayerCustomerModelID, int externalMapLayerID, int customerID, int modelID) : this()
        {
            this.ExternalMapLayerCustomerModelID = externalMapLayerCustomerModelID;
            this.ExternalMapLayerID = externalMapLayerID;
            this.CustomerID = customerID;
            this.ModelID = modelID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public ExternalMapLayerCustomerModel(int externalMapLayerID, int customerID, int modelID) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ExternalMapLayerCustomerModelID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.ExternalMapLayerID = externalMapLayerID;
            this.CustomerID = customerID;
            this.ModelID = modelID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public ExternalMapLayerCustomerModel(ExternalMapLayer externalMapLayer, Customer customer, Model model) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ExternalMapLayerCustomerModelID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.ExternalMapLayerID = externalMapLayer.ExternalMapLayerID;
            this.ExternalMapLayer = externalMapLayer;
            externalMapLayer.ExternalMapLayerCustomerModels.Add(this);
            this.CustomerID = customer.CustomerID;
            this.Customer = customer;
            customer.ExternalMapLayerCustomerModels.Add(this);
            this.ModelID = model.ModelID;
            this.Model = model;
            model.ExternalMapLayerCustomerModels.Add(this);
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static ExternalMapLayerCustomerModel CreateNewBlank(ExternalMapLayer externalMapLayer, Customer customer, Model model)
        {
            return new ExternalMapLayerCustomerModel(externalMapLayer, customer, model);
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
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(ExternalMapLayerCustomerModel).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.ExternalMapLayerCustomerModels.Remove(this);
        }
        
        /// <summary>
        /// Delete entity plus all children
        /// </summary>
        public void DeleteFull(PrimaryDBContext dbContext)
        {
            
            Delete(dbContext);
        }

        [Key]
        public int ExternalMapLayerCustomerModelID { get; set; }
        public int ExternalMapLayerID { get; set; }
        public int CustomerID { get; set; }
        public int ModelID { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return ExternalMapLayerCustomerModelID; } set { ExternalMapLayerCustomerModelID = value; } }

        public virtual ExternalMapLayer ExternalMapLayer { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Model Model { get; set; }

        public static class FieldLengths
        {

        }
    }
}