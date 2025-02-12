//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[RunBucket]
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[RunBucket] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[RunBucket]")]
    public partial class RunBucket : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public RunBucket()
        {
            this.RunBucketRuns = new HashSet<RunBucketRun>();
        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public RunBucket(int runBucketID, string runBucketName, DateTime createdDate, int userID, int customerID, string runBucketDescription) : this()
        {
            this.RunBucketID = runBucketID;
            this.RunBucketName = runBucketName;
            this.CreatedDate = createdDate;
            this.UserID = userID;
            this.CustomerID = customerID;
            this.RunBucketDescription = runBucketDescription;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public RunBucket(string runBucketName, DateTime createdDate, int userID, int customerID) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.RunBucketID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.RunBucketName = runBucketName;
            this.CreatedDate = createdDate;
            this.UserID = userID;
            this.CustomerID = customerID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public RunBucket(string runBucketName, DateTime createdDate, User user, Customer customer) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.RunBucketID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.RunBucketName = runBucketName;
            this.CreatedDate = createdDate;
            this.UserID = user.UserID;
            this.User = user;
            user.RunBuckets.Add(this);
            this.CustomerID = customer.CustomerID;
            this.Customer = customer;
            customer.RunBuckets.Add(this);
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static RunBucket CreateNewBlank(User user, Customer customer)
        {
            return new RunBucket(default(string), default(DateTime), user, customer);
        }

        /// <summary>
        /// Does this object have any dependent objects? (If it does have dependent objects, these would need to be deleted before this object could be deleted.)
        /// </summary>
        /// <returns></returns>
        public bool HasDependentObjects()
        {
            return RunBucketRuns.Any();
        }

        /// <summary>
        /// Active Dependent type names of this object
        /// </summary>
        public List<string> DependentObjectNames() 
        {
            var dependentObjects = new List<string>();
            
            if(RunBucketRuns.Any())
            {
                dependentObjects.Add(typeof(RunBucketRun).Name);
            }
            return dependentObjects.Distinct().ToList();
        }

        /// <summary>
        /// Dependent type names of this entity
        /// </summary>
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(RunBucket).Name, typeof(RunBucketRun).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.RunBuckets.Remove(this);
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

            foreach(var x in RunBucketRuns.ToList())
            {
                x.DeleteFull(dbContext);
            }
        }

        [Key]
        public int RunBucketID { get; set; }
        public string RunBucketName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserID { get; set; }
        public int CustomerID { get; set; }
        public string RunBucketDescription { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return RunBucketID; } set { RunBucketID = value; } }

        public virtual ICollection<RunBucketRun> RunBucketRuns { get; set; }
        public virtual User User { get; set; }
        public virtual Customer Customer { get; set; }

        public static class FieldLengths
        {
            public const int RunBucketName = 256;
        }
    }
}