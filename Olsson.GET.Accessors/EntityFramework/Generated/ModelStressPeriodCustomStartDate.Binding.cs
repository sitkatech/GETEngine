//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ModelStressPeriodCustomStartDate]
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[ModelStressPeriodCustomStartDate] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[ModelStressPeriodCustomStartDate]")]
    public partial class ModelStressPeriodCustomStartDate : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public ModelStressPeriodCustomStartDate()
        {

        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public ModelStressPeriodCustomStartDate(int modelStressPeriodCustomStartDateID, int modelID, int stressPeriod, DateTime stressPeriodStartDate) : this()
        {
            this.ModelStressPeriodCustomStartDateID = modelStressPeriodCustomStartDateID;
            this.ModelID = modelID;
            this.StressPeriod = stressPeriod;
            this.StressPeriodStartDate = stressPeriodStartDate;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public ModelStressPeriodCustomStartDate(int modelID, int stressPeriod, DateTime stressPeriodStartDate) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ModelStressPeriodCustomStartDateID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.ModelID = modelID;
            this.StressPeriod = stressPeriod;
            this.StressPeriodStartDate = stressPeriodStartDate;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public ModelStressPeriodCustomStartDate(Model model, int stressPeriod, DateTime stressPeriodStartDate) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ModelStressPeriodCustomStartDateID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.ModelID = model.ModelID;
            this.Model = model;
            model.ModelStressPeriodCustomStartDates.Add(this);
            this.StressPeriod = stressPeriod;
            this.StressPeriodStartDate = stressPeriodStartDate;
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static ModelStressPeriodCustomStartDate CreateNewBlank(Model model)
        {
            return new ModelStressPeriodCustomStartDate(model, default(int), default(DateTime));
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
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(ModelStressPeriodCustomStartDate).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.ModelStressPeriodCustomStartDates.Remove(this);
        }
        
        /// <summary>
        /// Delete entity plus all children
        /// </summary>
        public void DeleteFull(PrimaryDBContext dbContext)
        {
            
            Delete(dbContext);
        }

        [Key]
        public int ModelStressPeriodCustomStartDateID { get; set; }
        public int ModelID { get; set; }
        public int StressPeriod { get; set; }
        public DateTime StressPeriodStartDate { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return ModelStressPeriodCustomStartDateID; } set { ModelStressPeriodCustomStartDateID = value; } }

        public virtual Model Model { get; set; }

        public static class FieldLengths
        {

        }
    }
}