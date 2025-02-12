//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ReportTemplate]
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[ReportTemplate] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[ReportTemplate]")]
    public partial class ReportTemplate : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public ReportTemplate()
        {
            this.ReportTemplateCustomerModelScenarios = new HashSet<ReportTemplateCustomerModelScenario>();
        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public ReportTemplate(int reportTemplateID, int fileResourceInfoID, string displayName, string description, int reportTemplateModelTypeID, int reportTemplateModelID, bool isAvailableForAllConfigurations) : this()
        {
            this.ReportTemplateID = reportTemplateID;
            this.FileResourceInfoID = fileResourceInfoID;
            this.DisplayName = displayName;
            this.Description = description;
            this.ReportTemplateModelTypeID = reportTemplateModelTypeID;
            this.ReportTemplateModelID = reportTemplateModelID;
            this.IsAvailableForAllConfigurations = isAvailableForAllConfigurations;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public ReportTemplate(int fileResourceInfoID, string displayName, int reportTemplateModelTypeID, int reportTemplateModelID, bool isAvailableForAllConfigurations) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ReportTemplateID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.FileResourceInfoID = fileResourceInfoID;
            this.DisplayName = displayName;
            this.ReportTemplateModelTypeID = reportTemplateModelTypeID;
            this.ReportTemplateModelID = reportTemplateModelID;
            this.IsAvailableForAllConfigurations = isAvailableForAllConfigurations;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public ReportTemplate(FileResourceInfo fileResourceInfo, string displayName, ReportTemplateModelType reportTemplateModelType, ReportTemplateModel reportTemplateModel, bool isAvailableForAllConfigurations) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ReportTemplateID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.FileResourceInfoID = fileResourceInfo.FileResourceInfoID;
            this.FileResourceInfo = fileResourceInfo;
            fileResourceInfo.ReportTemplates.Add(this);
            this.DisplayName = displayName;
            this.ReportTemplateModelTypeID = reportTemplateModelType.ReportTemplateModelTypeID;
            this.ReportTemplateModelType = reportTemplateModelType;
            reportTemplateModelType.ReportTemplates.Add(this);
            this.ReportTemplateModelID = reportTemplateModel.ReportTemplateModelID;
            this.ReportTemplateModel = reportTemplateModel;
            reportTemplateModel.ReportTemplates.Add(this);
            this.IsAvailableForAllConfigurations = isAvailableForAllConfigurations;
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static ReportTemplate CreateNewBlank(FileResourceInfo fileResourceInfo, ReportTemplateModelType reportTemplateModelType, ReportTemplateModel reportTemplateModel)
        {
            return new ReportTemplate(fileResourceInfo, default(string), reportTemplateModelType, reportTemplateModel, default(bool));
        }

        /// <summary>
        /// Does this object have any dependent objects? (If it does have dependent objects, these would need to be deleted before this object could be deleted.)
        /// </summary>
        /// <returns></returns>
        public bool HasDependentObjects()
        {
            return ReportTemplateCustomerModelScenarios.Any();
        }

        /// <summary>
        /// Active Dependent type names of this object
        /// </summary>
        public List<string> DependentObjectNames() 
        {
            var dependentObjects = new List<string>();
            
            if(ReportTemplateCustomerModelScenarios.Any())
            {
                dependentObjects.Add(typeof(ReportTemplateCustomerModelScenario).Name);
            }
            return dependentObjects.Distinct().ToList();
        }

        /// <summary>
        /// Dependent type names of this entity
        /// </summary>
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(ReportTemplate).Name, typeof(ReportTemplateCustomerModelScenario).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.ReportTemplates.Remove(this);
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

            foreach(var x in ReportTemplateCustomerModelScenarios.ToList())
            {
                x.DeleteFull(dbContext);
            }
        }

        [Key]
        public int ReportTemplateID { get; set; }
        public int FileResourceInfoID { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public int ReportTemplateModelTypeID { get; set; }
        public int ReportTemplateModelID { get; set; }
        public bool IsAvailableForAllConfigurations { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return ReportTemplateID; } set { ReportTemplateID = value; } }

        public virtual ICollection<ReportTemplateCustomerModelScenario> ReportTemplateCustomerModelScenarios { get; set; }
        public virtual FileResourceInfo FileResourceInfo { get; set; }
        public virtual ReportTemplateModelType ReportTemplateModelType { get; set; }
        public virtual ReportTemplateModel ReportTemplateModel { get; set; }

        public static class FieldLengths
        {
            public const int DisplayName = 50;
            public const int Description = 250;
        }
    }
}