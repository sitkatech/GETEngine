//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[TimeframeType]
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
    public abstract partial class TimeframeType : IHavePrimaryKey
    {
        public static readonly TimeframeTypeMinute Minute = TimeframeTypeMinute.Instance;
        public static readonly TimeframeTypeHour Hour = TimeframeTypeHour.Instance;
        public static readonly TimeframeTypeDay Day = TimeframeTypeDay.Instance;
        public static readonly TimeframeTypeWeek Week = TimeframeTypeWeek.Instance;
        public static readonly TimeframeTypeMonth Month = TimeframeTypeMonth.Instance;

        public static readonly List<TimeframeType> All;
        public static readonly ReadOnlyDictionary<int, TimeframeType> AllLookupDictionary;

        /// <summary>
        /// Static type constructor to coordinate static initialization order
        /// </summary>
        static TimeframeType()
        {
            All = new List<TimeframeType> { Minute, Hour, Day, Week, Month };
            AllLookupDictionary = new ReadOnlyDictionary<int, TimeframeType>(All.ToDictionary(x => x.TimeframeTypeID));
        }

        /// <summary>
        /// Protected constructor only for use in instantiating the set of static lookup values that match database
        /// </summary>
        protected TimeframeType(int timeframeTypeID, string timeframeTypeName, string timeframeTypeDisplayName, string timeframeTypePluralizedName)
        {
            TimeframeTypeID = timeframeTypeID;
            TimeframeTypeName = timeframeTypeName;
            TimeframeTypeDisplayName = timeframeTypeDisplayName;
            TimeframeTypePluralizedName = timeframeTypePluralizedName;
        }
        public List<VolumeUnit> VolumeUnits { get { return VolumeUnit.All.Where(x => x.TimeframeTypeID == TimeframeTypeID).ToList(); } }
        [Key]
        public int TimeframeTypeID { get; private set; }
        public string TimeframeTypeName { get; private set; }
        public string TimeframeTypeDisplayName { get; private set; }
        public string TimeframeTypePluralizedName { get; private set; }
        [NotMapped]
        public int PrimaryKey { get { return TimeframeTypeID; } }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public bool Equals(TimeframeType other)
        {
            if (other == null)
            {
                return false;
            }
            return other.TimeframeTypeID == TimeframeTypeID;
        }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as TimeframeType);
        }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public override int GetHashCode()
        {
            return TimeframeTypeID;
        }

        public static bool operator ==(TimeframeType left, TimeframeType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TimeframeType left, TimeframeType right)
        {
            return !Equals(left, right);
        }

        public TimeframeTypeEnum ToEnum { get { return (TimeframeTypeEnum)GetHashCode(); } }

        public static TimeframeType ToType(int enumValue)
        {
            return ToType((TimeframeTypeEnum)enumValue);
        }

        public static TimeframeType ToType(TimeframeTypeEnum enumValue)
        {
            switch (enumValue)
            {
                case TimeframeTypeEnum.Day:
                    return Day;
                case TimeframeTypeEnum.Hour:
                    return Hour;
                case TimeframeTypeEnum.Minute:
                    return Minute;
                case TimeframeTypeEnum.Month:
                    return Month;
                case TimeframeTypeEnum.Week:
                    return Week;
                default:
                    throw new ArgumentException(string.Format("Unable to map Enum: {0}", enumValue));
            }
        }
    }

    public enum TimeframeTypeEnum
    {
        Minute = 1,
        Hour = 2,
        Day = 3,
        Week = 4,
        Month = 5
    }

    public partial class TimeframeTypeMinute : TimeframeType
    {
        private TimeframeTypeMinute(int timeframeTypeID, string timeframeTypeName, string timeframeTypeDisplayName, string timeframeTypePluralizedName) : base(timeframeTypeID, timeframeTypeName, timeframeTypeDisplayName, timeframeTypePluralizedName) {}
        public static readonly TimeframeTypeMinute Instance = new TimeframeTypeMinute(1, @"Minute", @"Minute", @"Minutes");
    }

    public partial class TimeframeTypeHour : TimeframeType
    {
        private TimeframeTypeHour(int timeframeTypeID, string timeframeTypeName, string timeframeTypeDisplayName, string timeframeTypePluralizedName) : base(timeframeTypeID, timeframeTypeName, timeframeTypeDisplayName, timeframeTypePluralizedName) {}
        public static readonly TimeframeTypeHour Instance = new TimeframeTypeHour(2, @"Hour", @"Hour", @"Hours");
    }

    public partial class TimeframeTypeDay : TimeframeType
    {
        private TimeframeTypeDay(int timeframeTypeID, string timeframeTypeName, string timeframeTypeDisplayName, string timeframeTypePluralizedName) : base(timeframeTypeID, timeframeTypeName, timeframeTypeDisplayName, timeframeTypePluralizedName) {}
        public static readonly TimeframeTypeDay Instance = new TimeframeTypeDay(3, @"Day", @"Day", @"Days");
    }

    public partial class TimeframeTypeWeek : TimeframeType
    {
        private TimeframeTypeWeek(int timeframeTypeID, string timeframeTypeName, string timeframeTypeDisplayName, string timeframeTypePluralizedName) : base(timeframeTypeID, timeframeTypeName, timeframeTypeDisplayName, timeframeTypePluralizedName) {}
        public static readonly TimeframeTypeWeek Instance = new TimeframeTypeWeek(4, @"Week", @"Week", @"Weeks");
    }

    public partial class TimeframeTypeMonth : TimeframeType
    {
        private TimeframeTypeMonth(int timeframeTypeID, string timeframeTypeName, string timeframeTypeDisplayName, string timeframeTypePluralizedName) : base(timeframeTypeID, timeframeTypeName, timeframeTypeDisplayName, timeframeTypePluralizedName) {}
        public static readonly TimeframeTypeMonth Instance = new TimeframeTypeMonth(5, @"Month", @"Month", @"Months");
    }
}