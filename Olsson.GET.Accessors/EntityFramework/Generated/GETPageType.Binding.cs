//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[GETPageType]
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
    public abstract partial class GETPageType : IHavePrimaryKey
    {
        public static readonly GETPageTypeLaunchPad LaunchPad = GETPageTypeLaunchPad.Instance;
        public static readonly GETPageTypeExternalMapLayerList ExternalMapLayerList = GETPageTypeExternalMapLayerList.Instance;

        public static readonly List<GETPageType> All;
        public static readonly ReadOnlyDictionary<int, GETPageType> AllLookupDictionary;

        /// <summary>
        /// Static type constructor to coordinate static initialization order
        /// </summary>
        static GETPageType()
        {
            All = new List<GETPageType> { LaunchPad, ExternalMapLayerList };
            AllLookupDictionary = new ReadOnlyDictionary<int, GETPageType>(All.ToDictionary(x => x.GETPageTypeID));
        }

        /// <summary>
        /// Protected constructor only for use in instantiating the set of static lookup values that match database
        /// </summary>
        protected GETPageType(int gETPageTypeID, string gETPageTypeName, string getPageTypeDisplayName)
        {
            GETPageTypeID = gETPageTypeID;
            GETPageTypeName = gETPageTypeName;
            GetPageTypeDisplayName = getPageTypeDisplayName;
        }

        [Key]
        public int GETPageTypeID { get; private set; }
        public string GETPageTypeName { get; private set; }
        public string GetPageTypeDisplayName { get; private set; }
        [NotMapped]
        public int PrimaryKey { get { return GETPageTypeID; } }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public bool Equals(GETPageType other)
        {
            if (other == null)
            {
                return false;
            }
            return other.GETPageTypeID == GETPageTypeID;
        }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as GETPageType);
        }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public override int GetHashCode()
        {
            return GETPageTypeID;
        }

        public static bool operator ==(GETPageType left, GETPageType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GETPageType left, GETPageType right)
        {
            return !Equals(left, right);
        }

        public GETPageTypeEnum ToEnum { get { return (GETPageTypeEnum)GetHashCode(); } }

        public static GETPageType ToType(int enumValue)
        {
            return ToType((GETPageTypeEnum)enumValue);
        }

        public static GETPageType ToType(GETPageTypeEnum enumValue)
        {
            switch (enumValue)
            {
                case GETPageTypeEnum.ExternalMapLayerList:
                    return ExternalMapLayerList;
                case GETPageTypeEnum.LaunchPad:
                    return LaunchPad;
                default:
                    throw new ArgumentException(string.Format("Unable to map Enum: {0}", enumValue));
            }
        }
    }

    public enum GETPageTypeEnum
    {
        LaunchPad = 1,
        ExternalMapLayerList = 2
    }

    public partial class GETPageTypeLaunchPad : GETPageType
    {
        private GETPageTypeLaunchPad(int gETPageTypeID, string gETPageTypeName, string getPageTypeDisplayName) : base(gETPageTypeID, gETPageTypeName, getPageTypeDisplayName) {}
        public static readonly GETPageTypeLaunchPad Instance = new GETPageTypeLaunchPad(1, @"LaunchPad", @"Launch Pad");
    }

    public partial class GETPageTypeExternalMapLayerList : GETPageType
    {
        private GETPageTypeExternalMapLayerList(int gETPageTypeID, string gETPageTypeName, string getPageTypeDisplayName) : base(gETPageTypeID, gETPageTypeName, getPageTypeDisplayName) {}
        public static readonly GETPageTypeExternalMapLayerList Instance = new GETPageTypeExternalMapLayerList(2, @"ExternalMapLayerList", @"External Map Layer List");
    }
}