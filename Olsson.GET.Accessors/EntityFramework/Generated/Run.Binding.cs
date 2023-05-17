//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[Run]
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[Run] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[Run]")]
    public partial class Run : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public Run()
        {
            this.RunBucketRuns = new HashSet<RunBucketRun>();
            this.RunGeographies = new HashSet<RunGeography>();
        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public Run(int runID, string runName, string fileStorageLocator, int? imageID, int modelID, int scenarioID, int userID, int customerID, int runStatusID, DateTime createdDate, bool isDeleted, string inputFileName, DateTime? processingStartDate, DateTime? processingEndDate, bool? shouldCreateMaps, string output, int restartCount, int inputVolumeUnitID, int outputVolumeUnitID, bool isDifferential, string runDescription) : this()
        {
            this.RunID = runID;
            this.RunName = runName;
            this.FileStorageLocator = fileStorageLocator;
            this.ImageID = imageID;
            this.ModelID = modelID;
            this.ScenarioID = scenarioID;
            this.UserID = userID;
            this.CustomerID = customerID;
            this.RunStatusID = runStatusID;
            this.CreatedDate = createdDate;
            this.IsDeleted = isDeleted;
            this.InputFileName = inputFileName;
            this.ProcessingStartDate = processingStartDate;
            this.ProcessingEndDate = processingEndDate;
            this.ShouldCreateMaps = shouldCreateMaps;
            this.Output = output;
            this.RestartCount = restartCount;
            this.InputVolumeUnitID = inputVolumeUnitID;
            this.OutputVolumeUnitID = outputVolumeUnitID;
            this.IsDifferential = isDifferential;
            this.RunDescription = runDescription;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public Run(string runName, string fileStorageLocator, int modelID, int scenarioID, int userID, int customerID, int runStatusID, DateTime createdDate, bool isDeleted, int restartCount, int inputVolumeUnitID, int outputVolumeUnitID, bool isDifferential) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.RunID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.RunName = runName;
            this.FileStorageLocator = fileStorageLocator;
            this.ModelID = modelID;
            this.ScenarioID = scenarioID;
            this.UserID = userID;
            this.CustomerID = customerID;
            this.RunStatusID = runStatusID;
            this.CreatedDate = createdDate;
            this.IsDeleted = isDeleted;
            this.RestartCount = restartCount;
            this.InputVolumeUnitID = inputVolumeUnitID;
            this.OutputVolumeUnitID = outputVolumeUnitID;
            this.IsDifferential = isDifferential;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public Run(string runName, string fileStorageLocator, Model model, Scenario scenario, User user, Customer customer, RunStatus runStatus, DateTime createdDate, bool isDeleted, int restartCount, VolumeUnit inputVolumeUnit, VolumeUnit outputVolumeUnit, bool isDifferential) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.RunID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.RunName = runName;
            this.FileStorageLocator = fileStorageLocator;
            this.ModelID = model.ModelID;
            this.Model = model;
            model.Runs.Add(this);
            this.ScenarioID = scenario.ScenarioID;
            this.Scenario = scenario;
            scenario.Runs.Add(this);
            this.UserID = user.UserID;
            this.User = user;
            user.Runs.Add(this);
            this.CustomerID = customer.CustomerID;
            this.Customer = customer;
            customer.Runs.Add(this);
            this.RunStatusID = runStatus.RunStatusID;
            this.CreatedDate = createdDate;
            this.IsDeleted = isDeleted;
            this.RestartCount = restartCount;
            this.InputVolumeUnitID = inputVolumeUnit.VolumeUnitID;
            this.OutputVolumeUnitID = outputVolumeUnit.VolumeUnitID;
            this.IsDifferential = isDifferential;
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static Run CreateNewBlank(Model model, Scenario scenario, User user, Customer customer, RunStatus runStatus, VolumeUnit inputVolumeUnit, VolumeUnit outputVolumeUnit)
        {
            return new Run(default(string), default(string), model, scenario, user, customer, runStatus, default(DateTime), default(bool), default(int), inputVolumeUnit, outputVolumeUnit, default(bool));
        }

        /// <summary>
        /// Does this object have any dependent objects? (If it does have dependent objects, these would need to be deleted before this object could be deleted.)
        /// </summary>
        /// <returns></returns>
        public bool HasDependentObjects()
        {
            return RunBucketRuns.Any() || RunGeographies.Any();
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

            if(RunGeographies.Any())
            {
                dependentObjects.Add(typeof(RunGeography).Name);
            }
            return dependentObjects.Distinct().ToList();
        }

        /// <summary>
        /// Dependent type names of this entity
        /// </summary>
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(Run).Name, typeof(RunBucketRun).Name, typeof(RunGeography).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.Runs.Remove(this);
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

            foreach(var x in RunGeographies.ToList())
            {
                x.DeleteFull(dbContext);
            }
        }

        [Key]
        public int RunID { get; set; }
        public string RunName { get; set; }
        public string FileStorageLocator { get; set; }
        public int? ImageID { get; set; }
        public int ModelID { get; set; }
        public int ScenarioID { get; set; }
        public int UserID { get; set; }
        public int CustomerID { get; set; }
        public int RunStatusID { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string InputFileName { get; set; }
        public DateTime? ProcessingStartDate { get; set; }
        public DateTime? ProcessingEndDate { get; set; }
        public bool? ShouldCreateMaps { get; set; }
        public string Output { get; set; }
        public int RestartCount { get; set; }
        public int InputVolumeUnitID { get; set; }
        public int OutputVolumeUnitID { get; set; }
        public bool IsDifferential { get; set; }
        public string RunDescription { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return RunID; } set { RunID = value; } }

        public virtual ICollection<RunBucketRun> RunBucketRuns { get; set; }
        public virtual ICollection<RunGeography> RunGeographies { get; set; }
        public virtual Image Image { get; set; }
        public virtual Model Model { get; set; }
        public virtual Scenario Scenario { get; set; }
        public virtual User User { get; set; }
        public virtual Customer Customer { get; set; }
        public RunStatus RunStatus { get { return RunStatus.AllLookupDictionary[RunStatusID]; } }
        public VolumeUnit InputVolumeUnit { get { return VolumeUnit.AllLookupDictionary[InputVolumeUnitID]; } }
        public VolumeUnit OutputVolumeUnit { get { return VolumeUnit.AllLookupDictionary[OutputVolumeUnitID]; } }

        public static class FieldLengths
        {
            public const int RunName = 256;
            public const int FileStorageLocator = 50;
            public const int InputFileName = 256;
        }
    }
}