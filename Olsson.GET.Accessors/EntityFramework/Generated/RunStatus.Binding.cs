//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[RunStatus]
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;


namespace Olsson.GET.Accessors.EntityFramework
{
    public abstract partial class RunStatus : IHavePrimaryKey
    {
        public static readonly RunStatusCreated Created = RunStatusCreated.Instance;
        public static readonly RunStatusQueued Queued = RunStatusQueued.Instance;
        public static readonly RunStatusProcessing Processing = RunStatusProcessing.Instance;
        public static readonly RunStatusComplete Complete = RunStatusComplete.Instance;
        public static readonly RunStatusSystemError SystemError = RunStatusSystemError.Instance;
        public static readonly RunStatusInvalidOutput InvalidOutput = RunStatusInvalidOutput.Instance;
        public static readonly RunStatusInvalidInput InvalidInput = RunStatusInvalidInput.Instance;
        public static readonly RunStatusHasDryCells HasDryCells = RunStatusHasDryCells.Instance;
        public static readonly RunStatusAnalysisFailed AnalysisFailed = RunStatusAnalysisFailed.Instance;
        public static readonly RunStatusAnalysisSuccess AnalysisSuccess = RunStatusAnalysisSuccess.Instance;
        public static readonly RunStatusProcesingInputs ProcesingInputs = RunStatusProcesingInputs.Instance;
        public static readonly RunStatusRunningAnalysis RunningAnalysis = RunStatusRunningAnalysis.Instance;

        public static readonly List<RunStatus> All;
        public static readonly ReadOnlyDictionary<int, RunStatus> AllLookupDictionary;

        /// <summary>
        /// Static type constructor to coordinate static initialization order
        /// </summary>
        static RunStatus()
        {
            All = new List<RunStatus> { Created, Queued, Processing, Complete, SystemError, InvalidOutput, InvalidInput, HasDryCells, AnalysisFailed, AnalysisSuccess, ProcesingInputs, RunningAnalysis };
            AllLookupDictionary = new ReadOnlyDictionary<int, RunStatus>(All.ToDictionary(x => x.RunStatusID));
        }

        /// <summary>
        /// Protected constructor only for use in instantiating the set of static lookup values that match database
        /// </summary>
        protected RunStatus(int runStatusID, string runStatusName, string runStatusDisplayName, string runStatusColor, bool isTerminal)
        {
            RunStatusID = runStatusID;
            RunStatusName = runStatusName;
            RunStatusDisplayName = runStatusDisplayName;
            RunStatusColor = runStatusColor;
            IsTerminal = isTerminal;
        }

        [Key]
        public int RunStatusID { get; private set; }
        public string RunStatusName { get; private set; }
        public string RunStatusDisplayName { get; private set; }
        public string RunStatusColor { get; private set; }
        public bool IsTerminal { get; private set; }
        [NotMapped]
        public int PrimaryKey { get { return RunStatusID; } }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public bool Equals(RunStatus other)
        {
            if (other == null)
            {
                return false;
            }
            return other.RunStatusID == RunStatusID;
        }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as RunStatus);
        }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public override int GetHashCode()
        {
            return RunStatusID;
        }

        public static bool operator ==(RunStatus left, RunStatus right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RunStatus left, RunStatus right)
        {
            return !Equals(left, right);
        }

        public RunStatusEnum ToEnum { get { return (RunStatusEnum)GetHashCode(); } }

        public static RunStatus ToType(int enumValue)
        {
            return ToType((RunStatusEnum)enumValue);
        }

        public static RunStatus ToType(RunStatusEnum enumValue)
        {
            switch (enumValue)
            {
                case RunStatusEnum.AnalysisFailed:
                    return AnalysisFailed;
                case RunStatusEnum.AnalysisSuccess:
                    return AnalysisSuccess;
                case RunStatusEnum.Complete:
                    return Complete;
                case RunStatusEnum.Created:
                    return Created;
                case RunStatusEnum.HasDryCells:
                    return HasDryCells;
                case RunStatusEnum.InvalidInput:
                    return InvalidInput;
                case RunStatusEnum.InvalidOutput:
                    return InvalidOutput;
                case RunStatusEnum.ProcesingInputs:
                    return ProcesingInputs;
                case RunStatusEnum.Processing:
                    return Processing;
                case RunStatusEnum.Queued:
                    return Queued;
                case RunStatusEnum.RunningAnalysis:
                    return RunningAnalysis;
                case RunStatusEnum.SystemError:
                    return SystemError;
                default:
                    throw new ArgumentException(string.Format("Unable to map Enum: {0}", enumValue));
            }
        }
    }

    public enum RunStatusEnum
    {
        Created = 0,
        Queued = 1,
        Processing = 2,
        Complete = 3,
        SystemError = 4,
        InvalidOutput = 5,
        InvalidInput = 6,
        HasDryCells = 7,
        AnalysisFailed = 8,
        AnalysisSuccess = 9,
        ProcesingInputs = 10,
        RunningAnalysis = 11
    }

    public partial class RunStatusCreated : RunStatus
    {
        private RunStatusCreated(int runStatusID, string runStatusName, string runStatusDisplayName, string runStatusColor, bool isTerminal) : base(runStatusID, runStatusName, runStatusDisplayName, runStatusColor, isTerminal) {}
        public static readonly RunStatusCreated Instance = new RunStatusCreated(0, @"Created", @"Created", @"#e5ed4f", false);
    }

    public partial class RunStatusQueued : RunStatus
    {
        private RunStatusQueued(int runStatusID, string runStatusName, string runStatusDisplayName, string runStatusColor, bool isTerminal) : base(runStatusID, runStatusName, runStatusDisplayName, runStatusColor, isTerminal) {}
        public static readonly RunStatusQueued Instance = new RunStatusQueued(1, @"Queued", @"Queued", @"#e5ed4f", false);
    }

    public partial class RunStatusProcessing : RunStatus
    {
        private RunStatusProcessing(int runStatusID, string runStatusName, string runStatusDisplayName, string runStatusColor, bool isTerminal) : base(runStatusID, runStatusName, runStatusDisplayName, runStatusColor, isTerminal) {}
        public static readonly RunStatusProcessing Instance = new RunStatusProcessing(2, @"Processing", @"Processing", @"#e5ed4f", false);
    }

    public partial class RunStatusComplete : RunStatus
    {
        private RunStatusComplete(int runStatusID, string runStatusName, string runStatusDisplayName, string runStatusColor, bool isTerminal) : base(runStatusID, runStatusName, runStatusDisplayName, runStatusColor, isTerminal) {}
        public static readonly RunStatusComplete Instance = new RunStatusComplete(3, @"Complete", @"Complete", @"#23d776", true);
    }

    public partial class RunStatusSystemError : RunStatus
    {
        private RunStatusSystemError(int runStatusID, string runStatusName, string runStatusDisplayName, string runStatusColor, bool isTerminal) : base(runStatusID, runStatusName, runStatusDisplayName, runStatusColor, isTerminal) {}
        public static readonly RunStatusSystemError Instance = new RunStatusSystemError(4, @"SystemError", @"System Error", @"#db4142", true);
    }

    public partial class RunStatusInvalidOutput : RunStatus
    {
        private RunStatusInvalidOutput(int runStatusID, string runStatusName, string runStatusDisplayName, string runStatusColor, bool isTerminal) : base(runStatusID, runStatusName, runStatusDisplayName, runStatusColor, isTerminal) {}
        public static readonly RunStatusInvalidOutput Instance = new RunStatusInvalidOutput(5, @"InvalidOutput", @"Invalid Output", @"#db4142", true);
    }

    public partial class RunStatusInvalidInput : RunStatus
    {
        private RunStatusInvalidInput(int runStatusID, string runStatusName, string runStatusDisplayName, string runStatusColor, bool isTerminal) : base(runStatusID, runStatusName, runStatusDisplayName, runStatusColor, isTerminal) {}
        public static readonly RunStatusInvalidInput Instance = new RunStatusInvalidInput(6, @"InvalidInput", @"Invalid Input", @"#db4142", true);
    }

    public partial class RunStatusHasDryCells : RunStatus
    {
        private RunStatusHasDryCells(int runStatusID, string runStatusName, string runStatusDisplayName, string runStatusColor, bool isTerminal) : base(runStatusID, runStatusName, runStatusDisplayName, runStatusColor, isTerminal) {}
        public static readonly RunStatusHasDryCells Instance = new RunStatusHasDryCells(7, @"HasDryCells", @"Completed with Dry Cells", @"#23d776", true);
    }

    public partial class RunStatusAnalysisFailed : RunStatus
    {
        private RunStatusAnalysisFailed(int runStatusID, string runStatusName, string runStatusDisplayName, string runStatusColor, bool isTerminal) : base(runStatusID, runStatusName, runStatusDisplayName, runStatusColor, isTerminal) {}
        public static readonly RunStatusAnalysisFailed Instance = new RunStatusAnalysisFailed(8, @"AnalysisFailed", @"Analysis Failed", @"#db4142", true);
    }

    public partial class RunStatusAnalysisSuccess : RunStatus
    {
        private RunStatusAnalysisSuccess(int runStatusID, string runStatusName, string runStatusDisplayName, string runStatusColor, bool isTerminal) : base(runStatusID, runStatusName, runStatusDisplayName, runStatusColor, isTerminal) {}
        public static readonly RunStatusAnalysisSuccess Instance = new RunStatusAnalysisSuccess(9, @"AnalysisSuccess", @"Analysis Succeeded", @"#e5ed4f", false);
    }

    public partial class RunStatusProcesingInputs : RunStatus
    {
        private RunStatusProcesingInputs(int runStatusID, string runStatusName, string runStatusDisplayName, string runStatusColor, bool isTerminal) : base(runStatusID, runStatusName, runStatusDisplayName, runStatusColor, isTerminal) {}
        public static readonly RunStatusProcesingInputs Instance = new RunStatusProcesingInputs(10, @"ProcesingInputs", @"Processing Inputs", @"#e5ed4f", false);
    }

    public partial class RunStatusRunningAnalysis : RunStatus
    {
        private RunStatusRunningAnalysis(int runStatusID, string runStatusName, string runStatusDisplayName, string runStatusColor, bool isTerminal) : base(runStatusID, runStatusName, runStatusDisplayName, runStatusColor, isTerminal) {}
        public static readonly RunStatusRunningAnalysis Instance = new RunStatusRunningAnalysis(11, @"RunningAnalysis", @"Running Analysis", @"#e5ed4f", false);
    }
}