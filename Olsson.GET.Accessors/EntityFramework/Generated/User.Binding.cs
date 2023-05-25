//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[User]
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[User] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[User]")]
    public partial class User : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public User()
        {
            this.FileResourceInfos = new HashSet<FileResourceInfo>();
            this.Runs = new HashSet<Run>();
            this.RunBuckets = new HashSet<RunBucket>();
            this.UserRoles = new HashSet<UserRole>();
        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public User(int userID, string fullName, string userName, string password, bool isLockedOut, DateTimeOffset? lockoutExpiration, int failedAttemptCount, string securityStamp, string email, bool emailConfirmed, int customerID, string phoneNumber, DateTime? eulaAcceptedDate) : this()
        {
            this.UserID = userID;
            this.FullName = fullName;
            this.UserName = userName;
            this.Password = password;
            this.IsLockedOut = isLockedOut;
            this.LockoutExpiration = lockoutExpiration;
            this.FailedAttemptCount = failedAttemptCount;
            this.SecurityStamp = securityStamp;
            this.Email = email;
            this.EmailConfirmed = emailConfirmed;
            this.CustomerID = customerID;
            this.PhoneNumber = phoneNumber;
            this.EulaAcceptedDate = eulaAcceptedDate;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public User(string fullName, string userName, bool isLockedOut, int failedAttemptCount, bool emailConfirmed, int customerID) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.UserID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.FullName = fullName;
            this.UserName = userName;
            this.IsLockedOut = isLockedOut;
            this.FailedAttemptCount = failedAttemptCount;
            this.EmailConfirmed = emailConfirmed;
            this.CustomerID = customerID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public User(string fullName, string userName, bool isLockedOut, int failedAttemptCount, bool emailConfirmed, Customer customer) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.UserID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.FullName = fullName;
            this.UserName = userName;
            this.IsLockedOut = isLockedOut;
            this.FailedAttemptCount = failedAttemptCount;
            this.EmailConfirmed = emailConfirmed;
            this.CustomerID = customer.CustomerID;
            this.Customer = customer;
            customer.Users.Add(this);
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static User CreateNewBlank(Customer customer)
        {
            return new User(default(string), default(string), default(bool), default(int), default(bool), customer);
        }

        /// <summary>
        /// Does this object have any dependent objects? (If it does have dependent objects, these would need to be deleted before this object could be deleted.)
        /// </summary>
        /// <returns></returns>
        public bool HasDependentObjects()
        {
            return FileResourceInfos.Any() || Runs.Any() || RunBuckets.Any() || UserRoles.Any();
        }

        /// <summary>
        /// Active Dependent type names of this object
        /// </summary>
        public List<string> DependentObjectNames() 
        {
            var dependentObjects = new List<string>();
            
            if(FileResourceInfos.Any())
            {
                dependentObjects.Add(typeof(FileResourceInfo).Name);
            }

            if(Runs.Any())
            {
                dependentObjects.Add(typeof(Run).Name);
            }

            if(RunBuckets.Any())
            {
                dependentObjects.Add(typeof(RunBucket).Name);
            }

            if(UserRoles.Any())
            {
                dependentObjects.Add(typeof(UserRole).Name);
            }
            return dependentObjects.Distinct().ToList();
        }

        /// <summary>
        /// Dependent type names of this entity
        /// </summary>
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(User).Name, typeof(FileResourceInfo).Name, typeof(Run).Name, typeof(RunBucket).Name, typeof(UserRole).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.Users.Remove(this);
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

            foreach(var x in FileResourceInfos.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in Runs.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in RunBuckets.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in UserRoles.ToList())
            {
                x.DeleteFull(dbContext);
            }
        }

        [Key]
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTimeOffset? LockoutExpiration { get; set; }
        public int FailedAttemptCount { get; set; }
        public string SecurityStamp { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public int CustomerID { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? EulaAcceptedDate { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return UserID; } set { UserID = value; } }

        public virtual ICollection<FileResourceInfo> FileResourceInfos { get; set; }
        public virtual ICollection<Run> Runs { get; set; }
        public virtual ICollection<RunBucket> RunBuckets { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual Customer Customer { get; set; }

        public static class FieldLengths
        {
            public const int FullName = 256;
            public const int UserName = 256;
            public const int Email = 256;
            public const int PhoneNumber = 50;
        }
    }
}