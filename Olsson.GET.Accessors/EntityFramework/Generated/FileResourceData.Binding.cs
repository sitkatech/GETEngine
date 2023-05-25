//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[FileResourceData]

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[FileResourceData] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[FileResourceData]")]
    public partial class FileResourceData : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public FileResourceData()
        {

        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public FileResourceData(int fileResourceDataID, int fileResourceInfoID, byte[] data) : this()
        {
            this.FileResourceDataID = fileResourceDataID;
            this.FileResourceInfoID = fileResourceInfoID;
            this.Data = data;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public FileResourceData(int fileResourceInfoID, byte[] data) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.FileResourceDataID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.FileResourceInfoID = fileResourceInfoID;
            this.Data = data;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public FileResourceData(FileResourceInfo fileResourceInfo, byte[] data) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.FileResourceDataID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.FileResourceInfoID = fileResourceInfo.FileResourceInfoID;
            this.FileResourceInfo = fileResourceInfo;
            fileResourceInfo.FileResourceDatas.Add(this);
            this.Data = data;
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static FileResourceData CreateNewBlank(FileResourceInfo fileResourceInfo)
        {
            return new FileResourceData(fileResourceInfo, default(byte[]));
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
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(FileResourceData).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.FileResourceDatas.Remove(this);
        }
        
        /// <summary>
        /// Delete entity plus all children
        /// </summary>
        public void DeleteFull(PrimaryDBContext dbContext)
        {
            
            Delete(dbContext);
        }

        [Key]
        public int FileResourceDataID { get; set; }
        public int FileResourceInfoID { get; set; }
        public byte[] Data { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return FileResourceDataID; } set { FileResourceDataID = value; } }

        public virtual FileResourceInfo FileResourceInfo { get; set; }

        public static class FieldLengths
        {

        }
    }
}