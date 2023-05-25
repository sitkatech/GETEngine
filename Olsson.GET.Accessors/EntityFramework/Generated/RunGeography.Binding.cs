//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[RunGeography]

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[RunGeography] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[RunGeography]")]
    public partial class RunGeography : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public RunGeography()
        {

        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public RunGeography(int runGeographyID, int runID, int stressPeriod, string color, DbGeography geography) : this()
        {
            this.RunGeographyID = runGeographyID;
            this.RunID = runID;
            this.StressPeriod = stressPeriod;
            this.Color = color;
            this.Geography = geography;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public RunGeography(int runID, int stressPeriod, string color) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.RunGeographyID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.RunID = runID;
            this.StressPeriod = stressPeriod;
            this.Color = color;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public RunGeography(Run run, int stressPeriod, string color) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.RunGeographyID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.RunID = run.RunID;
            this.Run = run;
            run.RunGeographies.Add(this);
            this.StressPeriod = stressPeriod;
            this.Color = color;
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static RunGeography CreateNewBlank(Run run)
        {
            return new RunGeography(run, default(int), default(string));
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
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(RunGeography).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.RunGeographies.Remove(this);
        }
        
        /// <summary>
        /// Delete entity plus all children
        /// </summary>
        public void DeleteFull(PrimaryDBContext dbContext)
        {
            
            Delete(dbContext);
        }

        [Key]
        public int RunGeographyID { get; set; }
        public int RunID { get; set; }
        public int StressPeriod { get; set; }
        public string Color { get; set; }
        public DbGeography Geography { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return RunGeographyID; } set { RunGeographyID = value; } }

        public virtual Run Run { get; set; }

        public static class FieldLengths
        {
            public const int Color = 7;
        }
    }
}