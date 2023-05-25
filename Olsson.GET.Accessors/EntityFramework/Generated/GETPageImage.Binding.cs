//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[GETPageImage]

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[GETPageImage] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[GETPageImage]")]
    public partial class GETPageImage : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public GETPageImage()
        {

        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public GETPageImage(int gETPageImageID, int gETPageID, int fileResourceInfoID) : this()
        {
            this.GETPageImageID = gETPageImageID;
            this.GETPageID = gETPageID;
            this.FileResourceInfoID = fileResourceInfoID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public GETPageImage(int gETPageID, int fileResourceInfoID) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.GETPageImageID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.GETPageID = gETPageID;
            this.FileResourceInfoID = fileResourceInfoID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public GETPageImage(GETPage gETPage, FileResourceInfo fileResourceInfo) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.GETPageImageID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.GETPageID = gETPage.GETPageID;
            this.GETPage = gETPage;
            gETPage.GETPageImages.Add(this);
            this.FileResourceInfoID = fileResourceInfo.FileResourceInfoID;
            this.FileResourceInfo = fileResourceInfo;
            fileResourceInfo.GETPageImages.Add(this);
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static GETPageImage CreateNewBlank(GETPage gETPage, FileResourceInfo fileResourceInfo)
        {
            return new GETPageImage(gETPage, fileResourceInfo);
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
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(GETPageImage).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.GETPageImages.Remove(this);
        }
        
        /// <summary>
        /// Delete entity plus all children
        /// </summary>
        public void DeleteFull(PrimaryDBContext dbContext)
        {
            
            Delete(dbContext);
        }

        [Key]
        public int GETPageImageID { get; set; }
        public int GETPageID { get; set; }
        public int FileResourceInfoID { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return GETPageImageID; } set { GETPageImageID = value; } }

        public virtual GETPage GETPage { get; set; }
        public virtual FileResourceInfo FileResourceInfo { get; set; }

        public static class FieldLengths
        {

        }
    }
}