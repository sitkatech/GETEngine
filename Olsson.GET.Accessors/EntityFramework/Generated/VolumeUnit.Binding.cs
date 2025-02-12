//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[VolumeUnit]
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
    public abstract partial class VolumeUnit : IHavePrimaryKey
    {
        public static readonly VolumeUnitNotApplicable NotApplicable = VolumeUnitNotApplicable.Instance;
        public static readonly VolumeUnitAcreFeet AcreFeet = VolumeUnitAcreFeet.Instance;
        public static readonly VolumeUnitCubicFeet CubicFeet = VolumeUnitCubicFeet.Instance;
        public static readonly VolumeUnitCubicYard CubicYard = VolumeUnitCubicYard.Instance;
        public static readonly VolumeUnitCubicMeter CubicMeter = VolumeUnitCubicMeter.Instance;
        public static readonly VolumeUnitGallon Gallon = VolumeUnitGallon.Instance;
        public static readonly VolumeUnitGallonsInMillions GallonsInMillions = VolumeUnitGallonsInMillions.Instance;

        public static readonly List<VolumeUnit> All;
        public static readonly ReadOnlyDictionary<int, VolumeUnit> AllLookupDictionary;

        /// <summary>
        /// Static type constructor to coordinate static initialization order
        /// </summary>
        static VolumeUnit()
        {
            All = new List<VolumeUnit> { NotApplicable, AcreFeet, CubicFeet, CubicYard, CubicMeter, Gallon, GallonsInMillions };
            AllLookupDictionary = new ReadOnlyDictionary<int, VolumeUnit>(All.ToDictionary(x => x.VolumeUnitID));
        }

        /// <summary>
        /// Protected constructor only for use in instantiating the set of static lookup values that match database
        /// </summary>
        protected VolumeUnit(int volumeUnitID, string volumeUnitName, string volumeUnitDisplayName, string volumeUnitPluralizedName, int? timeframeTypeID)
        {
            VolumeUnitID = volumeUnitID;
            VolumeUnitName = volumeUnitName;
            VolumeUnitDisplayName = volumeUnitDisplayName;
            VolumeUnitPluralizedName = volumeUnitPluralizedName;
            TimeframeTypeID = timeframeTypeID;
        }
        public TimeframeType TimeframeType { get { return TimeframeTypeID.HasValue ? TimeframeType.AllLookupDictionary[TimeframeTypeID.Value] : null; } }
        [Key]
        public int VolumeUnitID { get; private set; }
        public string VolumeUnitName { get; private set; }
        public string VolumeUnitDisplayName { get; private set; }
        public string VolumeUnitPluralizedName { get; private set; }
        public int? TimeframeTypeID { get; private set; }
        [NotMapped]
        public int PrimaryKey { get { return VolumeUnitID; } }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public bool Equals(VolumeUnit other)
        {
            if (other == null)
            {
                return false;
            }
            return other.VolumeUnitID == VolumeUnitID;
        }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as VolumeUnit);
        }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public override int GetHashCode()
        {
            return VolumeUnitID;
        }

        public static bool operator ==(VolumeUnit left, VolumeUnit right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(VolumeUnit left, VolumeUnit right)
        {
            return !Equals(left, right);
        }

        public VolumeUnitEnum ToEnum { get { return (VolumeUnitEnum)GetHashCode(); } }

        public static VolumeUnit ToType(int enumValue)
        {
            return ToType((VolumeUnitEnum)enumValue);
        }

        public static VolumeUnit ToType(VolumeUnitEnum enumValue)
        {
            switch (enumValue)
            {
                case VolumeUnitEnum.AcreFeet:
                    return AcreFeet;
                case VolumeUnitEnum.CubicFeet:
                    return CubicFeet;
                case VolumeUnitEnum.CubicMeter:
                    return CubicMeter;
                case VolumeUnitEnum.CubicYard:
                    return CubicYard;
                case VolumeUnitEnum.Gallon:
                    return Gallon;
                case VolumeUnitEnum.GallonsInMillions:
                    return GallonsInMillions;
                case VolumeUnitEnum.NotApplicable:
                    return NotApplicable;
                default:
                    throw new ArgumentException(string.Format("Unable to map Enum: {0}", enumValue));
            }
        }
    }

    public enum VolumeUnitEnum
    {
        NotApplicable = 0,
        AcreFeet = 1,
        CubicFeet = 2,
        CubicYard = 3,
        CubicMeter = 4,
        Gallon = 5,
        GallonsInMillions = 6
    }

    public partial class VolumeUnitNotApplicable : VolumeUnit
    {
        private VolumeUnitNotApplicable(int volumeUnitID, string volumeUnitName, string volumeUnitDisplayName, string volumeUnitPluralizedName, int? timeframeTypeID) : base(volumeUnitID, volumeUnitName, volumeUnitDisplayName, volumeUnitPluralizedName, timeframeTypeID) {}
        public static readonly VolumeUnitNotApplicable Instance = new VolumeUnitNotApplicable(0, @"NotApplicable", @"N/A", null, null);
    }

    public partial class VolumeUnitAcreFeet : VolumeUnit
    {
        private VolumeUnitAcreFeet(int volumeUnitID, string volumeUnitName, string volumeUnitDisplayName, string volumeUnitPluralizedName, int? timeframeTypeID) : base(volumeUnitID, volumeUnitName, volumeUnitDisplayName, volumeUnitPluralizedName, timeframeTypeID) {}
        public static readonly VolumeUnitAcreFeet Instance = new VolumeUnitAcreFeet(1, @"Acre Feet", @"Acre-Feet", @"Acre-Feet", 5);
    }

    public partial class VolumeUnitCubicFeet : VolumeUnit
    {
        private VolumeUnitCubicFeet(int volumeUnitID, string volumeUnitName, string volumeUnitDisplayName, string volumeUnitPluralizedName, int? timeframeTypeID) : base(volumeUnitID, volumeUnitName, volumeUnitDisplayName, volumeUnitPluralizedName, timeframeTypeID) {}
        public static readonly VolumeUnitCubicFeet Instance = new VolumeUnitCubicFeet(2, @"Cubic Feet", @"Cubic Feet", @"Cubic Feet", 3);
    }

    public partial class VolumeUnitCubicYard : VolumeUnit
    {
        private VolumeUnitCubicYard(int volumeUnitID, string volumeUnitName, string volumeUnitDisplayName, string volumeUnitPluralizedName, int? timeframeTypeID) : base(volumeUnitID, volumeUnitName, volumeUnitDisplayName, volumeUnitPluralizedName, timeframeTypeID) {}
        public static readonly VolumeUnitCubicYard Instance = new VolumeUnitCubicYard(3, @"Cubic Yard", @"Cubic Yard", @"Cubic Yards", 3);
    }

    public partial class VolumeUnitCubicMeter : VolumeUnit
    {
        private VolumeUnitCubicMeter(int volumeUnitID, string volumeUnitName, string volumeUnitDisplayName, string volumeUnitPluralizedName, int? timeframeTypeID) : base(volumeUnitID, volumeUnitName, volumeUnitDisplayName, volumeUnitPluralizedName, timeframeTypeID) {}
        public static readonly VolumeUnitCubicMeter Instance = new VolumeUnitCubicMeter(4, @"Cubic Meter", @"Cubic Meter", @"Cubic Meters", 3);
    }

    public partial class VolumeUnitGallon : VolumeUnit
    {
        private VolumeUnitGallon(int volumeUnitID, string volumeUnitName, string volumeUnitDisplayName, string volumeUnitPluralizedName, int? timeframeTypeID) : base(volumeUnitID, volumeUnitName, volumeUnitDisplayName, volumeUnitPluralizedName, timeframeTypeID) {}
        public static readonly VolumeUnitGallon Instance = new VolumeUnitGallon(5, @"Gallon", @"Gallon", @"Gallons", 1);
    }

    public partial class VolumeUnitGallonsInMillions : VolumeUnit
    {
        private VolumeUnitGallonsInMillions(int volumeUnitID, string volumeUnitName, string volumeUnitDisplayName, string volumeUnitPluralizedName, int? timeframeTypeID) : base(volumeUnitID, volumeUnitName, volumeUnitDisplayName, volumeUnitPluralizedName, timeframeTypeID) {}
        public static readonly VolumeUnitGallonsInMillions Instance = new VolumeUnitGallonsInMillions(6, @"GallonsInMillions", @"Million Gallon", @"Million Gallons", 3);
    }
}