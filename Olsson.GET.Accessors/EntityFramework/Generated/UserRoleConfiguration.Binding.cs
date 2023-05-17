//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[UserRole]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class UserRoleConfiguration : EntityTypeConfiguration<UserRole>
    {
        public UserRoleConfiguration() : this("dbo"){}

        public UserRoleConfiguration(string schema)
        {
            ToTable("UserRole", schema);
            HasKey(x => x.UserRoleID);
            Property(x => x.UserRoleID).HasColumnName(@"UserRoleID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.UserID).HasColumnName(@"UserID").HasColumnType("int").IsRequired();
            Property(x => x.RoleID).HasColumnName(@"RoleID").HasColumnType("int").IsRequired();

            // Foreign keys
            HasRequired(a => a.User).WithMany(b => b.UserRoles).HasForeignKey(c => c.UserID).WillCascadeOnDelete(false); // FK_UserRole_User_UserID
        }
    }
}