//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[Image]

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[Image] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[Image]")]
    public partial class Image : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public Image()
        {
            this.Models = new HashSet<Model>();
            this.Runs = new HashSet<Run>();
            this.ScenariosWhereYouAreTheInputImage = new HashSet<Scenario>();
        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public Image(int imageID, string imageName, string server, bool isLinux, int? cpuCoreCount, decimal? memory) : this()
        {
            this.ImageID = imageID;
            this.ImageName = imageName;
            this.Server = server;
            this.IsLinux = isLinux;
            this.CpuCoreCount = cpuCoreCount;
            this.Memory = memory;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public Image(string imageName, string server, bool isLinux) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ImageID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.ImageName = imageName;
            this.Server = server;
            this.IsLinux = isLinux;
        }


        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static Image CreateNewBlank()
        {
            return new Image(default(string), default(string), default(bool));
        }

        /// <summary>
        /// Does this object have any dependent objects? (If it does have dependent objects, these would need to be deleted before this object could be deleted.)
        /// </summary>
        /// <returns></returns>
        public bool HasDependentObjects()
        {
            return Models.Any() || Runs.Any() || ScenariosWhereYouAreTheInputImage.Any();
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

            if(Runs.Any())
            {
                dependentObjects.Add(typeof(Run).Name);
            }

            if(ScenariosWhereYouAreTheInputImage.Any())
            {
                dependentObjects.Add(typeof(Scenario).Name);
            }
            return dependentObjects.Distinct().ToList();
        }

        /// <summary>
        /// Dependent type names of this entity
        /// </summary>
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(Image).Name, typeof(Model).Name, typeof(Run).Name, typeof(Scenario).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.Images.Remove(this);
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

            foreach(var x in Runs.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in ScenariosWhereYouAreTheInputImage.ToList())
            {
                x.DeleteFull(dbContext);
            }
        }

        [Key]
        public int ImageID { get; set; }
        public string ImageName { get; set; }
        public string Server { get; set; }
        public bool IsLinux { get; set; }
        public int? CpuCoreCount { get; set; }
        public decimal? Memory { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return ImageID; } set { ImageID = value; } }

        public virtual ICollection<Model> Models { get; set; }
        public virtual ICollection<Run> Runs { get; set; }
        public virtual ICollection<Scenario> ScenariosWhereYouAreTheInputImage { get; set; }

        public static class FieldLengths
        {
            public const int ImageName = 256;
            public const int Server = 256;
        }
    }
}