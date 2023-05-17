//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[RunBucket]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class RunBucketConfiguration : EntityTypeConfiguration<RunBucket>
    {
        public RunBucketConfiguration() : this("dbo"){}

        public RunBucketConfiguration(string schema)
        {
            ToTable("RunBucket", schema);
            HasKey(x => x.RunBucketID);
            Property(x => x.RunBucketID).HasColumnName(@"RunBucketID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.RunBucketName).HasColumnName(@"RunBucketName").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(256);
            Property(x => x.CreatedDate).HasColumnName(@"CreatedDate").HasColumnType("datetime").IsRequired();
            Property(x => x.UserID).HasColumnName(@"UserID").HasColumnType("int").IsRequired();
            Property(x => x.CustomerID).HasColumnName(@"CustomerID").HasColumnType("int").IsRequired();
            Property(x => x.RunBucketDescription).HasColumnName(@"RunBucketDescription").HasColumnType("varchar").IsOptional();

            // Foreign keys
            HasRequired(a => a.User).WithMany(b => b.RunBuckets).HasForeignKey(c => c.UserID).WillCascadeOnDelete(false); // FK_RunBucket_User_UserID
            HasRequired(a => a.Customer).WithMany(b => b.RunBuckets).HasForeignKey(c => c.CustomerID).WillCascadeOnDelete(false); // FK_RunBucket_Customer_CustomerID
        }
    }
}