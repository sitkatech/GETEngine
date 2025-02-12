//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ModelEngineType]
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Web;


namespace Olsson.GET.Accessors.EntityFramework
{
    public abstract partial class ModelEngineType : IHavePrimaryKey
    {
        public static readonly ModelEngineTypeModpath Modpath = ModelEngineTypeModpath.Instance;
        public static readonly ModelEngineTypeModflow Modflow = ModelEngineTypeModflow.Instance;
        public static readonly ModelEngineTypeModflow6 Modflow6 = ModelEngineTypeModflow6.Instance;
        public static readonly ModelEngineTypeIWFM IWFM = ModelEngineTypeIWFM.Instance;

        public static readonly List<ModelEngineType> All;
        public static readonly ReadOnlyDictionary<int, ModelEngineType> AllLookupDictionary;

        /// <summary>
        /// Static type constructor to coordinate static initialization order
        /// </summary>
        static ModelEngineType()
        {
            All = new List<ModelEngineType> { Modpath, Modflow, Modflow6, IWFM };
            AllLookupDictionary = new ReadOnlyDictionary<int, ModelEngineType>(All.ToDictionary(x => x.ModelEngineTypeID));
        }

        /// <summary>
        /// Protected constructor only for use in instantiating the set of static lookup values that match database
        /// </summary>
        protected ModelEngineType(int modelEngineTypeID, string modelEngineTypeName, string modelEngineTypeDisplayName)
        {
            ModelEngineTypeID = modelEngineTypeID;
            ModelEngineTypeName = modelEngineTypeName;
            ModelEngineTypeDisplayName = modelEngineTypeDisplayName;
        }

        [Key]
        public int ModelEngineTypeID { get; private set; }
        public string ModelEngineTypeName { get; private set; }
        public string ModelEngineTypeDisplayName { get; private set; }
        [NotMapped]
        public int PrimaryKey { get { return ModelEngineTypeID; } }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public bool Equals(ModelEngineType other)
        {
            if (other == null)
            {
                return false;
            }
            return other.ModelEngineTypeID == ModelEngineTypeID;
        }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as ModelEngineType);
        }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public override int GetHashCode()
        {
            return ModelEngineTypeID;
        }

        public static bool operator ==(ModelEngineType left, ModelEngineType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ModelEngineType left, ModelEngineType right)
        {
            return !Equals(left, right);
        }

        public ModelEngineTypeEnum ToEnum { get { return (ModelEngineTypeEnum)GetHashCode(); } }

        public static ModelEngineType ToType(int enumValue)
        {
            return ToType((ModelEngineTypeEnum)enumValue);
        }

        public static ModelEngineType ToType(ModelEngineTypeEnum enumValue)
        {
            switch (enumValue)
            {
                case ModelEngineTypeEnum.IWFM:
                    return IWFM;
                case ModelEngineTypeEnum.Modflow:
                    return Modflow;
                case ModelEngineTypeEnum.Modflow6:
                    return Modflow6;
                case ModelEngineTypeEnum.Modpath:
                    return Modpath;
                default:
                    throw new ArgumentException(string.Format("Unable to map Enum: {0}", enumValue));
            }
        }
    }

    public enum ModelEngineTypeEnum
    {
        Modpath = 1,
        Modflow = 2,
        Modflow6 = 3,
        IWFM = 4
    }

    public partial class ModelEngineTypeModpath : ModelEngineType
    {
        private ModelEngineTypeModpath(int modelEngineTypeID, string modelEngineTypeName, string modelEngineTypeDisplayName) : base(modelEngineTypeID, modelEngineTypeName, modelEngineTypeDisplayName) {}
        public static readonly ModelEngineTypeModpath Instance = new ModelEngineTypeModpath(1, @"Modpath", @"Modpath");
    }

    public partial class ModelEngineTypeModflow : ModelEngineType
    {
        private ModelEngineTypeModflow(int modelEngineTypeID, string modelEngineTypeName, string modelEngineTypeDisplayName) : base(modelEngineTypeID, modelEngineTypeName, modelEngineTypeDisplayName) {}
        public static readonly ModelEngineTypeModflow Instance = new ModelEngineTypeModflow(2, @"Modflow", @"Modflow");
    }

    public partial class ModelEngineTypeModflow6 : ModelEngineType
    {
        private ModelEngineTypeModflow6(int modelEngineTypeID, string modelEngineTypeName, string modelEngineTypeDisplayName) : base(modelEngineTypeID, modelEngineTypeName, modelEngineTypeDisplayName) {}
        public static readonly ModelEngineTypeModflow6 Instance = new ModelEngineTypeModflow6(3, @"Modflow6", @"Modflow 6");
    }

    public partial class ModelEngineTypeIWFM : ModelEngineType
    {
        private ModelEngineTypeIWFM(int modelEngineTypeID, string modelEngineTypeName, string modelEngineTypeDisplayName) : base(modelEngineTypeID, modelEngineTypeName, modelEngineTypeDisplayName) {}
        public static readonly ModelEngineTypeIWFM Instance = new ModelEngineTypeIWFM(4, @"IWFM", @"IWFM");
    }
}