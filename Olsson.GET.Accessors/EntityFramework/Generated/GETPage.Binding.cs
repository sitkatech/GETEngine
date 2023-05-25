//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[GETPage]

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[GETPage] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[GETPage]")]
    public partial class GETPage : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public GETPage()
        {
            this.GETPageImages = new HashSet<GETPageImage>();
        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public GETPage(int gETPageID, int gETPageTypeID, string gETPageContent) : this()
        {
            this.GETPageID = gETPageID;
            this.GETPageTypeID = gETPageTypeID;
            this.GETPageContent = gETPageContent;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public GETPage(int gETPageTypeID) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.GETPageID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.GETPageTypeID = gETPageTypeID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public GETPage(GETPageType gETPageType) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.GETPageID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.GETPageTypeID = gETPageType.GETPageTypeID;
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static GETPage CreateNewBlank(GETPageType gETPageType)
        {
            return new GETPage(gETPageType);
        }

        /// <summary>
        /// Does this object have any dependent objects? (If it does have dependent objects, these would need to be deleted before this object could be deleted.)
        /// </summary>
        /// <returns></returns>
        public bool HasDependentObjects()
        {
            return GETPageImages.Any();
        }

        /// <summary>
        /// Active Dependent type names of this object
        /// </summary>
        public List<string> DependentObjectNames() 
        {
            var dependentObjects = new List<string>();
            
            if(GETPageImages.Any())
            {
                dependentObjects.Add(typeof(GETPageImage).Name);
            }
            return dependentObjects.Distinct().ToList();
        }

        /// <summary>
        /// Dependent type names of this entity
        /// </summary>
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(GETPage).Name, typeof(GETPageImage).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.GETPages.Remove(this);
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

            foreach(var x in GETPageImages.ToList())
            {
                x.DeleteFull(dbContext);
            }
        }

        [Key]
        public int GETPageID { get; set; }
        public int GETPageTypeID { get; set; }
        public string GETPageContent { get; set; }
        [NotMapped]
        public HtmlString GETPageContentHtmlString
        { 
            get { return GETPageContent == null ? null : new HtmlString(GETPageContent); }
            set { GETPageContent = value?.ToString(); }
        }
        [NotMapped]
        public int PrimaryKey { get { return GETPageID; } set { GETPageID = value; } }

        public virtual ICollection<GETPageImage> GETPageImages { get; set; }
        public GETPageType GETPageType { get { return GETPageType.AllLookupDictionary[GETPageTypeID]; } }

        public static class FieldLengths
        {

        }
    }
}