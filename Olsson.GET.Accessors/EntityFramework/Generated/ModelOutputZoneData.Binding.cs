//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ModelOutputZoneData]

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[ModelOutputZoneData] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[ModelOutputZoneData]")]
    public partial class ModelOutputZoneData : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public ModelOutputZoneData()
        {

        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public ModelOutputZoneData(int modelOutputZoneDataID, int modelID, string outputZoneData) : this()
        {
            this.ModelOutputZoneDataID = modelOutputZoneDataID;
            this.ModelID = modelID;
            this.OutputZoneData = outputZoneData;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public ModelOutputZoneData(int modelID) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ModelOutputZoneDataID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.ModelID = modelID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public ModelOutputZoneData(Model model) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ModelOutputZoneDataID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.ModelID = model.ModelID;
            this.Model = model;
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static ModelOutputZoneData CreateNewBlank(Model model)
        {
            return new ModelOutputZoneData(model);
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
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(ModelOutputZoneData).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.ModelOutputZoneDatas.Remove(this);
        }
        
        /// <summary>
        /// Delete entity plus all children
        /// </summary>
        public void DeleteFull(PrimaryDBContext dbContext)
        {
            
            Delete(dbContext);
        }

        [Key]
        public int ModelOutputZoneDataID { get; set; }
        public int ModelID { get; set; }
        public string OutputZoneData { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return ModelOutputZoneDataID; } set { ModelOutputZoneDataID = value; } }

        public virtual Model Model { get; set; }

        public static class FieldLengths
        {

        }
    }
}