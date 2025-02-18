//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[MeasurementType]
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
    public abstract partial class MeasurementType : IHavePrimaryKey
    {
        public static readonly MeasurementTypeNone None = MeasurementTypeNone.Instance;
        public static readonly MeasurementTypeVolume Volume = MeasurementTypeVolume.Instance;
        public static readonly MeasurementTypeRate Rate = MeasurementTypeRate.Instance;

        public static readonly List<MeasurementType> All;
        public static readonly ReadOnlyDictionary<int, MeasurementType> AllLookupDictionary;

        /// <summary>
        /// Static type constructor to coordinate static initialization order
        /// </summary>
        static MeasurementType()
        {
            All = new List<MeasurementType> { None, Volume, Rate };
            AllLookupDictionary = new ReadOnlyDictionary<int, MeasurementType>(All.ToDictionary(x => x.MeasurementTypeID));
        }

        /// <summary>
        /// Protected constructor only for use in instantiating the set of static lookup values that match database
        /// </summary>
        protected MeasurementType(int measurementTypeID, string measurementTypeName, string measurementTypeDisplayName)
        {
            MeasurementTypeID = measurementTypeID;
            MeasurementTypeName = measurementTypeName;
            MeasurementTypeDisplayName = measurementTypeDisplayName;
        }

        [Key]
        public int MeasurementTypeID { get; private set; }
        public string MeasurementTypeName { get; private set; }
        public string MeasurementTypeDisplayName { get; private set; }
        [NotMapped]
        public int PrimaryKey { get { return MeasurementTypeID; } }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public bool Equals(MeasurementType other)
        {
            if (other == null)
            {
                return false;
            }
            return other.MeasurementTypeID == MeasurementTypeID;
        }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as MeasurementType);
        }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public override int GetHashCode()
        {
            return MeasurementTypeID;
        }

        public static bool operator ==(MeasurementType left, MeasurementType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MeasurementType left, MeasurementType right)
        {
            return !Equals(left, right);
        }

        public MeasurementTypeEnum ToEnum { get { return (MeasurementTypeEnum)GetHashCode(); } }

        public static MeasurementType ToType(int enumValue)
        {
            return ToType((MeasurementTypeEnum)enumValue);
        }

        public static MeasurementType ToType(MeasurementTypeEnum enumValue)
        {
            switch (enumValue)
            {
                case MeasurementTypeEnum.None:
                    return None;
                case MeasurementTypeEnum.Rate:
                    return Rate;
                case MeasurementTypeEnum.Volume:
                    return Volume;
                default:
                    throw new ArgumentException(string.Format("Unable to map Enum: {0}", enumValue));
            }
        }
    }

    public enum MeasurementTypeEnum
    {
        None = 1,
        Volume = 2,
        Rate = 3
    }

    public partial class MeasurementTypeNone : MeasurementType
    {
        private MeasurementTypeNone(int measurementTypeID, string measurementTypeName, string measurementTypeDisplayName) : base(measurementTypeID, measurementTypeName, measurementTypeDisplayName) {}
        public static readonly MeasurementTypeNone Instance = new MeasurementTypeNone(1, @"None", @"None");
    }

    public partial class MeasurementTypeVolume : MeasurementType
    {
        private MeasurementTypeVolume(int measurementTypeID, string measurementTypeName, string measurementTypeDisplayName) : base(measurementTypeID, measurementTypeName, measurementTypeDisplayName) {}
        public static readonly MeasurementTypeVolume Instance = new MeasurementTypeVolume(2, @"Volume", @"Volume");
    }

    public partial class MeasurementTypeRate : MeasurementType
    {
        private MeasurementTypeRate(int measurementTypeID, string measurementTypeName, string measurementTypeDisplayName) : base(measurementTypeID, measurementTypeName, measurementTypeDisplayName) {}
        public static readonly MeasurementTypeRate Instance = new MeasurementTypeRate(3, @"Rate", @"Rate");
    }
}