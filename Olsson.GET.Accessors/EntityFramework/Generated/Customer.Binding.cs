//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[Customer]

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[Customer] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[Customer]")]
    public partial class Customer : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public Customer()
        {
            this.CustomerModelScenarios = new HashSet<CustomerModelScenario>();
            this.ExternalMapLayerCustomerModels = new HashSet<ExternalMapLayerCustomerModel>();
            this.ReportTemplateCustomerModelScenarios = new HashSet<ReportTemplateCustomerModelScenario>();
            this.Runs = new HashSet<Run>();
            this.RunBuckets = new HashSet<RunBucket>();
            this.Users = new HashSet<User>();
        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public Customer(int customerID, string customerName, bool isTrial) : this()
        {
            this.CustomerID = customerID;
            this.CustomerName = customerName;
            this.IsTrial = isTrial;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public Customer(string customerName, bool isTrial) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.CustomerID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.CustomerName = customerName;
            this.IsTrial = isTrial;
        }


        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static Customer CreateNewBlank()
        {
            return new Customer(default(string), default(bool));
        }

        /// <summary>
        /// Does this object have any dependent objects? (If it does have dependent objects, these would need to be deleted before this object could be deleted.)
        /// </summary>
        /// <returns></returns>
        public bool HasDependentObjects()
        {
            return CustomerModelScenarios.Any() || ExternalMapLayerCustomerModels.Any() || ReportTemplateCustomerModelScenarios.Any() || Runs.Any() || RunBuckets.Any() || Users.Any();
        }

        /// <summary>
        /// Active Dependent type names of this object
        /// </summary>
        public List<string> DependentObjectNames() 
        {
            var dependentObjects = new List<string>();
            
            if(CustomerModelScenarios.Any())
            {
                dependentObjects.Add(typeof(CustomerModelScenario).Name);
            }

            if(ExternalMapLayerCustomerModels.Any())
            {
                dependentObjects.Add(typeof(ExternalMapLayerCustomerModel).Name);
            }

            if(ReportTemplateCustomerModelScenarios.Any())
            {
                dependentObjects.Add(typeof(ReportTemplateCustomerModelScenario).Name);
            }

            if(Runs.Any())
            {
                dependentObjects.Add(typeof(Run).Name);
            }

            if(RunBuckets.Any())
            {
                dependentObjects.Add(typeof(RunBucket).Name);
            }

            if(Users.Any())
            {
                dependentObjects.Add(typeof(User).Name);
            }
            return dependentObjects.Distinct().ToList();
        }

        /// <summary>
        /// Dependent type names of this entity
        /// </summary>
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(Customer).Name, typeof(CustomerModelScenario).Name, typeof(ExternalMapLayerCustomerModel).Name, typeof(ReportTemplateCustomerModelScenario).Name, typeof(Run).Name, typeof(RunBucket).Name, typeof(User).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.Customers.Remove(this);
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

            foreach(var x in CustomerModelScenarios.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in ExternalMapLayerCustomerModels.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in ReportTemplateCustomerModelScenarios.ToList())
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

            foreach(var x in Users.ToList())
            {
                x.DeleteFull(dbContext);
            }
        }

        [Key]
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public bool IsTrial { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return CustomerID; } set { CustomerID = value; } }

        public virtual ICollection<CustomerModelScenario> CustomerModelScenarios { get; set; }
        public virtual ICollection<ExternalMapLayerCustomerModel> ExternalMapLayerCustomerModels { get; set; }
        public virtual ICollection<ReportTemplateCustomerModelScenario> ReportTemplateCustomerModelScenarios { get; set; }
        public virtual ICollection<Run> Runs { get; set; }
        public virtual ICollection<RunBucket> RunBuckets { get; set; }
        public virtual ICollection<User> Users { get; set; }

        public static class FieldLengths
        {
            public const int CustomerName = 256;
        }
    }
}