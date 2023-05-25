//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ModelExecutable]

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[ModelExecutable] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[ModelExecutable]")]
    public partial class ModelExecutable : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public ModelExecutable()
        {

        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public ModelExecutable(int modelExecutableID, int modelID, string executableName, string arguments, int runOrder, string workingDirectory, bool wrapWithBatchFile, bool useShellExecute, bool redirectStandardOutput, bool createNoWindow) : this()
        {
            this.ModelExecutableID = modelExecutableID;
            this.ModelID = modelID;
            this.ExecutableName = executableName;
            this.Arguments = arguments;
            this.RunOrder = runOrder;
            this.WorkingDirectory = workingDirectory;
            this.WrapWithBatchFile = wrapWithBatchFile;
            this.UseShellExecute = useShellExecute;
            this.RedirectStandardOutput = redirectStandardOutput;
            this.CreateNoWindow = createNoWindow;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public ModelExecutable(int modelID, string executableName, int runOrder, bool wrapWithBatchFile, bool useShellExecute, bool redirectStandardOutput, bool createNoWindow) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ModelExecutableID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.ModelID = modelID;
            this.ExecutableName = executableName;
            this.RunOrder = runOrder;
            this.WrapWithBatchFile = wrapWithBatchFile;
            this.UseShellExecute = useShellExecute;
            this.RedirectStandardOutput = redirectStandardOutput;
            this.CreateNoWindow = createNoWindow;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public ModelExecutable(Model model, string executableName, int runOrder, bool wrapWithBatchFile, bool useShellExecute, bool redirectStandardOutput, bool createNoWindow) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.ModelExecutableID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.ModelID = model.ModelID;
            this.Model = model;
            model.ModelExecutables.Add(this);
            this.ExecutableName = executableName;
            this.RunOrder = runOrder;
            this.WrapWithBatchFile = wrapWithBatchFile;
            this.UseShellExecute = useShellExecute;
            this.RedirectStandardOutput = redirectStandardOutput;
            this.CreateNoWindow = createNoWindow;
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static ModelExecutable CreateNewBlank(Model model)
        {
            return new ModelExecutable(model, default(string), default(int), default(bool), default(bool), default(bool), default(bool));
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
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(ModelExecutable).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.ModelExecutables.Remove(this);
        }
        
        /// <summary>
        /// Delete entity plus all children
        /// </summary>
        public void DeleteFull(PrimaryDBContext dbContext)
        {
            
            Delete(dbContext);
        }

        [Key]
        public int ModelExecutableID { get; set; }
        public int ModelID { get; set; }
        public string ExecutableName { get; set; }
        public string Arguments { get; set; }
        public int RunOrder { get; set; }
        public string WorkingDirectory { get; set; }
        public bool WrapWithBatchFile { get; set; }
        public bool UseShellExecute { get; set; }
        public bool RedirectStandardOutput { get; set; }
        public bool CreateNoWindow { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return ModelExecutableID; } set { ModelExecutableID = value; } }

        public virtual Model Model { get; set; }

        public static class FieldLengths
        {
            public const int ExecutableName = 200;
            public const int Arguments = 200;
            public const int WorkingDirectory = 200;
        }
    }
}