//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[Model]
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[Model] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[Model]")]
    public partial class Model : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public Model()
        {
            this.CustomerModelScenarios = new HashSet<CustomerModelScenario>();
            this.ExternalMapLayerCustomerModels = new HashSet<ExternalMapLayerCustomerModel>();
            this.ModelDocumentationImages = new HashSet<ModelDocumentationImage>();
            this.ModelExecutables = new HashSet<ModelExecutable>();
            this.ModelInputZoneDatas = new HashSet<ModelInputZoneData>();
            this.ModelMapAreaBoundaries = new HashSet<ModelMapAreaBoundary>();
            this.ModelOutputZoneDatas = new HashSet<ModelOutputZoneData>();
            this.ModelScenarios = new HashSet<ModelScenario>();
            this.ModelStressPeriodCustomStartDates = new HashSet<ModelStressPeriodCustomStartDate>();
            this.ReportTemplateCustomerModelScenarios = new HashSet<ReportTemplateCustomerModelScenario>();
            this.Runs = new HashSet<Run>();
        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public Model(int modelID, string modelName, int imageID, DateTime startDateTime, string runFileName, double? allowablePercentDiscrepancy, string mapSettings, string mapRunFileName, bool isDoubleSizeHeatMapOutput, int numberOfStressPeriods, string canalData, string buddyGroup, string mapDrawdownFileName, string listFileName, int? baseflowTableProcessingConfigurationID, string modelDescription, string modelDocumentation, int modelEngineTypeID, int modelGridTypeID) : this()
        {
            this.ModelID = modelID;
            this.ModelName = modelName;
            this.ImageID = imageID;
            this.StartDateTime = startDateTime;
            this.RunFileName = runFileName;
            this.AllowablePercentDiscrepancy = allowablePercentDiscrepancy;
            this.MapSettings = mapSettings;
            this.MapRunFileName = mapRunFileName;
            this.IsDoubleSizeHeatMapOutput = isDoubleSizeHeatMapOutput;
            this.NumberOfStressPeriods = numberOfStressPeriods;
            this.CanalData = canalData;
            this.BuddyGroup = buddyGroup;
            this.MapDrawdownFileName = mapDrawdownFileName;
            this.ListFileName = listFileName;
            this.BaseflowTableProcessingConfigurationID = baseflowTableProcessingConfigurationID;
            this.ModelDescription = modelDescription;
            this.ModelDocumentation = modelDocumentation;
            this.ModelEngineTypeID = modelEngineTypeID;
            this.ModelGridTypeID = modelGridTypeID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public Model(string modelName, int imageID, DateTime startDateTime, bool isDoubleSizeHeatMapOutput, int numberOfStressPeriods, int modelEngineTypeID, int modelGridTypeID) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ModelID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.ModelName = modelName;
            this.ImageID = imageID;
            this.StartDateTime = startDateTime;
            this.IsDoubleSizeHeatMapOutput = isDoubleSizeHeatMapOutput;
            this.NumberOfStressPeriods = numberOfStressPeriods;
            this.ModelEngineTypeID = modelEngineTypeID;
            this.ModelGridTypeID = modelGridTypeID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public Model(string modelName, Image image, DateTime startDateTime, bool isDoubleSizeHeatMapOutput, int numberOfStressPeriods, ModelEngineType modelEngineType, ModelGridType modelGridType) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ModelID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.ModelName = modelName;
            this.ImageID = image.ImageID;
            this.Image = image;
            image.Models.Add(this);
            this.StartDateTime = startDateTime;
            this.IsDoubleSizeHeatMapOutput = isDoubleSizeHeatMapOutput;
            this.NumberOfStressPeriods = numberOfStressPeriods;
            this.ModelEngineTypeID = modelEngineType.ModelEngineTypeID;
            this.ModelGridTypeID = modelGridType.ModelGridTypeID;
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static Model CreateNewBlank(Image image, ModelEngineType modelEngineType, ModelGridType modelGridType)
        {
            return new Model(default(string), image, default(DateTime), default(bool), default(int), modelEngineType, modelGridType);
        }

        /// <summary>
        /// Does this object have any dependent objects? (If it does have dependent objects, these would need to be deleted before this object could be deleted.)
        /// </summary>
        /// <returns></returns>
        public bool HasDependentObjects()
        {
            return CustomerModelScenarios.Any() || ExternalMapLayerCustomerModels.Any() || ModelDocumentationImages.Any() || ModelExecutables.Any() || (ModelInputZoneData != null) || (ModelMapAreaBoundary != null) || (ModelOutputZoneData != null) || ModelScenarios.Any() || ModelStressPeriodCustomStartDates.Any() || ReportTemplateCustomerModelScenarios.Any() || Runs.Any();
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

            if(ModelDocumentationImages.Any())
            {
                dependentObjects.Add(typeof(ModelDocumentationImage).Name);
            }

            if(ModelExecutables.Any())
            {
                dependentObjects.Add(typeof(ModelExecutable).Name);
            }

            if((ModelInputZoneData != null))
            {
                dependentObjects.Add(typeof(ModelInputZoneData).Name);
            }

            if((ModelMapAreaBoundary != null))
            {
                dependentObjects.Add(typeof(ModelMapAreaBoundary).Name);
            }

            if((ModelOutputZoneData != null))
            {
                dependentObjects.Add(typeof(ModelOutputZoneData).Name);
            }

            if(ModelScenarios.Any())
            {
                dependentObjects.Add(typeof(ModelScenario).Name);
            }

            if(ModelStressPeriodCustomStartDates.Any())
            {
                dependentObjects.Add(typeof(ModelStressPeriodCustomStartDate).Name);
            }

            if(ReportTemplateCustomerModelScenarios.Any())
            {
                dependentObjects.Add(typeof(ReportTemplateCustomerModelScenario).Name);
            }

            if(Runs.Any())
            {
                dependentObjects.Add(typeof(Run).Name);
            }
            return dependentObjects.Distinct().ToList();
        }

        /// <summary>
        /// Dependent type names of this entity
        /// </summary>
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(Model).Name, typeof(CustomerModelScenario).Name, typeof(ExternalMapLayerCustomerModel).Name, typeof(ModelDocumentationImage).Name, typeof(ModelExecutable).Name, typeof(ModelInputZoneData).Name, typeof(ModelMapAreaBoundary).Name, typeof(ModelOutputZoneData).Name, typeof(ModelScenario).Name, typeof(ModelStressPeriodCustomStartDate).Name, typeof(ReportTemplateCustomerModelScenario).Name, typeof(Run).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.Models.Remove(this);
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

            foreach(var x in ModelDocumentationImages.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in ModelExecutables.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in ModelInputZoneDatas.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in ModelMapAreaBoundaries.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in ModelOutputZoneDatas.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in ModelScenarios.ToList())
            {
                x.DeleteFull(dbContext);
            }

            foreach(var x in ModelStressPeriodCustomStartDates.ToList())
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
        }

        [Key]
        public int ModelID { get; set; }
        public string ModelName { get; set; }
        public int ImageID { get; set; }
        public DateTime StartDateTime { get; set; }
        public string RunFileName { get; set; }
        public double? AllowablePercentDiscrepancy { get; set; }
        public string MapSettings { get; set; }
        public string MapRunFileName { get; set; }
        public bool IsDoubleSizeHeatMapOutput { get; set; }
        public int NumberOfStressPeriods { get; set; }
        public string CanalData { get; set; }
        public string BuddyGroup { get; set; }
        public string MapDrawdownFileName { get; set; }
        public string ListFileName { get; set; }
        public int? BaseflowTableProcessingConfigurationID { get; set; }
        public string ModelDescription { get; set; }
        public string ModelDocumentation { get; set; }
        [NotMapped]
        public HtmlString ModelDocumentationHtmlString
        { 
            get { return ModelDocumentation == null ? null : new HtmlString(ModelDocumentation); }
            set { ModelDocumentation = value?.ToString(); }
        }
        public int ModelEngineTypeID { get; set; }
        public int ModelGridTypeID { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return ModelID; } set { ModelID = value; } }

        public virtual ICollection<CustomerModelScenario> CustomerModelScenarios { get; set; }
        public virtual ICollection<ExternalMapLayerCustomerModel> ExternalMapLayerCustomerModels { get; set; }
        public virtual ICollection<ModelDocumentationImage> ModelDocumentationImages { get; set; }
        public virtual ICollection<ModelExecutable> ModelExecutables { get; set; }
        public virtual ICollection<ModelInputZoneData> ModelInputZoneDatas { get; set; }
        [NotMapped]
        public ModelInputZoneData ModelInputZoneData { get { return ModelInputZoneDatas.SingleOrDefault(); } set { ModelInputZoneDatas = new List<ModelInputZoneData>{value};} }
        public virtual ICollection<ModelMapAreaBoundary> ModelMapAreaBoundaries { get; set; }
        [NotMapped]
        public ModelMapAreaBoundary ModelMapAreaBoundary { get { return ModelMapAreaBoundaries.SingleOrDefault(); } set { ModelMapAreaBoundaries = new List<ModelMapAreaBoundary>{value};} }
        public virtual ICollection<ModelOutputZoneData> ModelOutputZoneDatas { get; set; }
        [NotMapped]
        public ModelOutputZoneData ModelOutputZoneData { get { return ModelOutputZoneDatas.SingleOrDefault(); } set { ModelOutputZoneDatas = new List<ModelOutputZoneData>{value};} }
        public virtual ICollection<ModelScenario> ModelScenarios { get; set; }
        public virtual ICollection<ModelStressPeriodCustomStartDate> ModelStressPeriodCustomStartDates { get; set; }
        public virtual ICollection<ReportTemplateCustomerModelScenario> ReportTemplateCustomerModelScenarios { get; set; }
        public virtual ICollection<Run> Runs { get; set; }
        public virtual Image Image { get; set; }
        public virtual BaseflowTableProcessingConfiguration BaseflowTableProcessingConfiguration { get; set; }
        public ModelEngineType ModelEngineType { get { return ModelEngineType.AllLookupDictionary[ModelEngineTypeID]; } }
        public ModelGridType ModelGridType { get { return ModelGridType.AllLookupDictionary[ModelGridTypeID]; } }

        public static class FieldLengths
        {
            public const int ModelName = 256;
            public const int RunFileName = 50;
            public const int MapSettings = 1024;
            public const int MapRunFileName = 50;
            public const int BuddyGroup = 128;
            public const int MapDrawdownFileName = 50;
            public const int ListFileName = 50;
            public const int ModelDescription = 500;
        }
    }
}