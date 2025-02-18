//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[FileResourceInfo]
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[FileResourceInfo] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[FileResourceInfo]")]
    public partial class FileResourceInfo : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public FileResourceInfo()
        {
            this.FileResourceDatas = new HashSet<FileResourceData>();
            this.GETPageImages = new HashSet<GETPageImage>();
            this.ModelDocumentationImages = new HashSet<ModelDocumentationImage>();
            this.ReportTemplates = new HashSet<ReportTemplate>();
            this.ScenarioDocumentationImages = new HashSet<ScenarioDocumentationImage>();
        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public FileResourceInfo(int fileResourceInfoID, int fileResourceMimeTypeID, string originalBaseFilename, string originalFileExtension, Guid fileResourceGUID, int userID, DateTime createDate) : this()
        {
            this.FileResourceInfoID = fileResourceInfoID;
            this.FileResourceMimeTypeID = fileResourceMimeTypeID;
            this.OriginalBaseFilename = originalBaseFilename;
            this.OriginalFileExtension = originalFileExtension;
            this.FileResourceGUID = fileResourceGUID;
            this.UserID = userID;
            this.CreateDate = createDate;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public FileResourceInfo(int fileResourceMimeTypeID, string originalBaseFilename, string originalFileExtension, Guid fileResourceGUID, int userID, DateTime createDate) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.FileResourceInfoID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.FileResourceMimeTypeID = fileResourceMimeTypeID;
            this.OriginalBaseFilename = originalBaseFilename;
            this.OriginalFileExtension = originalFileExtension;
            this.FileResourceGUID = fileResourceGUID;
            this.UserID = userID;
            this.CreateDate = createDate;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public FileResourceInfo(FileResourceMimeType fileResourceMimeType, string originalBaseFilename, string originalFileExtension, Guid fileResourceGUID, User user, DateTime createDate) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.FileResourceInfoID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.FileResourceMimeTypeID = fileResourceMimeType.FileResourceMimeTypeID;
            this.OriginalBaseFilename = originalBaseFilename;
            this.OriginalFileExtension = originalFileExtension;
            this.FileResourceGUID = fileResourceGUID;
            this.UserID = user.UserID;
            this.User = user;
            user.FileResourceInfos.Add(this);
            this.CreateDate = createDate;
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static FileResourceInfo CreateNewBlank(FileResourceMimeType fileResourceMimeType, User user)
        {
            return new FileResourceInfo(fileResourceMimeType, default(string), default(string), default(Guid), user, default(DateTime));
        }

        /// <summary>
        /// Does this object have any dependent objects? (If it does have dependent objects, these would need to be deleted before this object could be deleted.)
        /// </summary>
        /// <returns></returns>
        public bool HasDependentObjects()
        {
            return FileResourceDatas.Any() || GETPageImages.Any() || ModelDocumentationImages.Any() || ReportTemplates.Any() || ScenarioDocumentationImages.Any();
        }

        /// <summary>
        /// Active Dependent type names of this object
        /// </summary>
        public List<string> DependentObjectNames() 
        {
            var dependentObjects = new List<string>();
            
            if(FileResourceDatas.Any())
            {
                dependentObjects.Add(typeof(FileResourceData).Name);
            }

            if(GETPageImages.Any())
            {
                dependentObjects.Add(typeof(GETPageImage).Name);
            }

            if(ModelDocumentationImages.Any())
            {
                dependentObjects.Add(typeof(ModelDocumentationImage).Name);
            }

            if(ReportTemplates.Any())
            {
                dependentObjects.Add(typeof(ReportTemplate).Name);
            }

            if(ScenarioDocumentationImages.Any())
            {
                dependentObjects.Add(typeof(ScenarioDocumentationImage).Name);
            }
            return dependentObjects.Distinct().ToList();
        }

        /// <summary>
        /// Dependent type names of this entity
        /// </summary>
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(FileResourceInfo).Name, typeof(FileResourceData).Name, typeof(GETPageImage).Name, typeof(ModelDocumentationImage).Name, typeof(ReportTemplate).Name, typeof(ScenarioDocumentationImage).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.FileResourceInfos.Remove(this);
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

            foreach(var x in FileResourceDatas.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in GETPageImages.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in ModelDocumentationImages.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in ReportTemplates.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in ScenarioDocumentationImages.ToList())
            {
                x.DeleteFull(dbContext);
            }
        }

        [Key]
        public int FileResourceInfoID { get; set; }
        public int FileResourceMimeTypeID { get; set; }
        public string OriginalBaseFilename { get; set; }
        public string OriginalFileExtension { get; set; }
        public Guid FileResourceGUID { get; set; }
        public int UserID { get; set; }
        public DateTime CreateDate { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return FileResourceInfoID; } set { FileResourceInfoID = value; } }

        public virtual ICollection<FileResourceData> FileResourceDatas { get; set; }
        public virtual ICollection<GETPageImage> GETPageImages { get; set; }
        public virtual ICollection<ModelDocumentationImage> ModelDocumentationImages { get; set; }
        public virtual ICollection<ReportTemplate> ReportTemplates { get; set; }
        public virtual ICollection<ScenarioDocumentationImage> ScenarioDocumentationImages { get; set; }
        public FileResourceMimeType FileResourceMimeType { get { return FileResourceMimeType.AllLookupDictionary[FileResourceMimeTypeID]; } }
        public virtual User User { get; set; }

        public static class FieldLengths
        {
            public const int OriginalBaseFilename = 255;
            public const int OriginalFileExtension = 255;
        }
    }
}