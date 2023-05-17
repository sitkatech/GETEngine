//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[UserRole]
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;


namespace Olsson.GET.Accessors.EntityFramework
{
    // Table [dbo].[UserRole] is NOT multi-tenant, so is attributed as ICanDeleteFull
    [Table("[dbo].[UserRole]")]
    public partial class UserRole : IHavePrimaryKey, ICanDeleteFull
    {
        /// <summary>
        /// Default Constructor; only used by EF
        /// </summary>
        public UserRole()
        {

        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public UserRole(int userRoleID, int userID, int roleID) : this()
        {
            this.UserRoleID = userRoleID;
            this.UserID = userID;
            this.RoleID = roleID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields in preparation for insert into database
        /// </summary>
        public UserRole(int userID, int roleID) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.UserRoleID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            
            this.UserID = userID;
            this.RoleID = roleID;
        }

        /// <summary>
        /// Constructor for building a new object with MinimalConstructor required fields, using objects whenever possible
        /// </summary>
        public UserRole(User user, Role role) : this()
        {
            // Mark this as a new object by setting primary key with special value
            this.UserRoleID = ModelObjectHelpers.MakeNextUnsavedPrimaryKeyValue();
            this.UserID = user.UserID;
            this.User = user;
            user.UserRoles.Add(this);
            this.RoleID = role.RoleID;
        }

        /// <summary>
        /// Creates a "blank" object of this type and populates primitives with defaults
        /// </summary>
        public static UserRole CreateNewBlank(User user, Role role)
        {
            return new UserRole(user, role);
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
        public static readonly List<string> DependentEntityTypeNames = new List<string> {typeof(UserRole).Name};


        /// <summary>
        /// Delete just the entity 
        /// </summary>
        public void Delete(PrimaryDBContext dbContext)
        {
            dbContext.UserRoles.Remove(this);
        }
        
        /// <summary>
        /// Delete entity plus all children
        /// </summary>
        public void DeleteFull(PrimaryDBContext dbContext)
        {
            
            Delete(dbContext);
        }

        [Key]
        public int UserRoleID { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }
        [NotMapped]
        public int PrimaryKey { get { return UserRoleID; } set { UserRoleID = value; } }

        public virtual User User { get; set; }
        public Role Role { get { return Role.AllLookupDictionary[RoleID]; } }

        public static class FieldLengths
        {

        }
    }
}