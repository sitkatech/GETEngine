//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[User]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class UserConfiguration : EntityTypeConfiguration<User>
    {
        public UserConfiguration() : this("dbo"){}

        public UserConfiguration(string schema)
        {
            ToTable("User", schema);
            HasKey(x => x.UserID);
            Property(x => x.UserID).HasColumnName(@"UserID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.FullName).HasColumnName(@"FullName").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(256);
            Property(x => x.UserName).HasColumnName(@"UserName").HasColumnType("nvarchar").IsRequired().HasMaxLength(256);
            Property(x => x.Password).HasColumnName(@"Password").HasColumnType("nvarchar").IsOptional();
            Property(x => x.IsLockedOut).HasColumnName(@"IsLockedOut").HasColumnType("bit").IsRequired();
            Property(x => x.LockoutExpiration).HasColumnName(@"LockoutExpiration").HasColumnType("datetimeoffset").IsOptional();
            Property(x => x.FailedAttemptCount).HasColumnName(@"FailedAttemptCount").HasColumnType("int").IsRequired();
            Property(x => x.SecurityStamp).HasColumnName(@"SecurityStamp").HasColumnType("nvarchar").IsOptional();
            Property(x => x.Email).HasColumnName(@"Email").HasColumnType("nvarchar").IsOptional().HasMaxLength(256);
            Property(x => x.EmailConfirmed).HasColumnName(@"EmailConfirmed").HasColumnType("bit").IsRequired();
            Property(x => x.CustomerID).HasColumnName(@"CustomerID").HasColumnType("int").IsRequired();
            Property(x => x.PhoneNumber).HasColumnName(@"PhoneNumber").HasColumnType("char").IsOptional().IsFixedLength().IsUnicode(false).HasMaxLength(50);
            Property(x => x.EulaAcceptedDate).HasColumnName(@"EulaAcceptedDate").HasColumnType("datetime").IsOptional();

            // Foreign keys
            HasRequired(a => a.Customer).WithMany(b => b.Users).HasForeignKey(c => c.CustomerID).WillCascadeOnDelete(false); // FK_User_Customer_CustomerID
        }
    }
}