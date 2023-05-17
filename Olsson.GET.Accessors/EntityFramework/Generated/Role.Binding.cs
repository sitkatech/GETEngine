//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[Role]
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
    public abstract partial class Role : IHavePrimaryKey
    {
        public static readonly RoleAdmin Admin = RoleAdmin.Instance;
        public static readonly RolePowerUser PowerUser = RolePowerUser.Instance;
        public static readonly RoleNormal Normal = RoleNormal.Instance;

        public static readonly List<Role> All;
        public static readonly ReadOnlyDictionary<int, Role> AllLookupDictionary;

        /// <summary>
        /// Static type constructor to coordinate static initialization order
        /// </summary>
        static Role()
        {
            All = new List<Role> { Admin, PowerUser, Normal };
            AllLookupDictionary = new ReadOnlyDictionary<int, Role>(All.ToDictionary(x => x.RoleID));
        }

        /// <summary>
        /// Protected constructor only for use in instantiating the set of static lookup values that match database
        /// </summary>
        protected Role(int roleID, string roleName, string roleDisplayName, int roleCategory)
        {
            RoleID = roleID;
            RoleName = roleName;
            RoleDisplayName = roleDisplayName;
            RoleCategory = roleCategory;
        }

        [Key]
        public int RoleID { get; private set; }
        public string RoleName { get; private set; }
        public string RoleDisplayName { get; private set; }
        public int RoleCategory { get; private set; }
        [NotMapped]
        public int PrimaryKey { get { return RoleID; } }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public bool Equals(Role other)
        {
            if (other == null)
            {
                return false;
            }
            return other.RoleID == RoleID;
        }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as Role);
        }

        /// <summary>
        /// Enum types are equal by primary key
        /// </summary>
        public override int GetHashCode()
        {
            return RoleID;
        }

        public static bool operator ==(Role left, Role right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Role left, Role right)
        {
            return !Equals(left, right);
        }

        public RoleEnum ToEnum { get { return (RoleEnum)GetHashCode(); } }

        public static Role ToType(int enumValue)
        {
            return ToType((RoleEnum)enumValue);
        }

        public static Role ToType(RoleEnum enumValue)
        {
            switch (enumValue)
            {
                case RoleEnum.Admin:
                    return Admin;
                case RoleEnum.Normal:
                    return Normal;
                case RoleEnum.PowerUser:
                    return PowerUser;
                default:
                    throw new ArgumentException(string.Format("Unable to map Enum: {0}", enumValue));
            }
        }
    }

    public enum RoleEnum
    {
        Admin = 1,
        PowerUser = 2,
        Normal = 3
    }

    public partial class RoleAdmin : Role
    {
        private RoleAdmin(int roleID, string roleName, string roleDisplayName, int roleCategory) : base(roleID, roleName, roleDisplayName, roleCategory) {}
        public static readonly RoleAdmin Instance = new RoleAdmin(1, @"Admin", @"Administrator", 1);
    }

    public partial class RolePowerUser : Role
    {
        private RolePowerUser(int roleID, string roleName, string roleDisplayName, int roleCategory) : base(roleID, roleName, roleDisplayName, roleCategory) {}
        public static readonly RolePowerUser Instance = new RolePowerUser(2, @"PowerUser", @"Power User", 1);
    }

    public partial class RoleNormal : Role
    {
        private RoleNormal(int roleID, string roleName, string roleDisplayName, int roleCategory) : base(roleID, roleName, roleDisplayName, roleCategory) {}
        public static readonly RoleNormal Instance = new RoleNormal(3, @"Normal", @"Normal", 1);
    }
}