//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[RunBucketRun]

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[RunBucketRun] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[RunBucketRun]")]
    public partial class RunBucketRun : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public RunBucketRun()
        {

        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public RunBucketRun(int runBucketRunID, int runBucketID, int runID) : this()
        {
            this.RunBucketRunID = runBucketRunID;
            this.RunBucketID = runBucketID;
            this.RunID = runID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public RunBucketRun(int runBucketID, int runID) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.RunBucketRunID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.RunBucketID = runBucketID;
            this.RunID = runID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public RunBucketRun(RunBucket runBucket, Run run) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.RunBucketRunID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.RunBucketID = runBucket.RunBucketID;
            this.RunBucket = runBucket;
            runBucket.RunBucketRuns.Add(this);
            this.RunID = run.RunID;
            this.Run = run;
            run.RunBucketRuns.Add(this);
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static RunBucketRun CreateNewBlank(RunBucket runBucket, Run run)
        {
            return new RunBucketRun(runBucket, run);
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
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(RunBucketRun).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.RunBucketRuns.Remove(this);
        }
        
        /// <summary>
        /// Delete entity plus all children
        /// </summary>
        public void DeleteFull(PrimaryDBContext dbContext)
        {
            
            Delete(dbContext);
        }

        [Key]
        public int RunBucketRunID { get; set; }
        public int RunBucketID { get; set; }
        public int RunID { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return RunBucketRunID; } set { RunBucketRunID = value; } }

        public virtual RunBucket RunBucket { get; set; }
        public virtual Run Run { get; set; }

        public static class FieldLengths
        {

        }
    }
}