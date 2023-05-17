//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[RunBucketRun]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class RunBucketRunConfiguration : EntityTypeConfiguration<RunBucketRun>
    {
        public RunBucketRunConfiguration() : this("dbo"){}

        public RunBucketRunConfiguration(string schema)
        {
            ToTable("RunBucketRun", schema);
            HasKey(x => x.RunBucketRunID);
            Property(x => x.RunBucketRunID).HasColumnName(@"RunBucketRunID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.RunBucketID).HasColumnName(@"RunBucketID").HasColumnType("int").IsRequired();
            Property(x => x.RunID).HasColumnName(@"RunID").HasColumnType("int").IsRequired();

            // Foreign keys
            HasRequired(a => a.RunBucket).WithMany(b => b.RunBucketRuns).HasForeignKey(c => c.RunBucketID).WillCascadeOnDelete(false); // FK_RunBucketRun_RunBucket_RunBucketID
            HasRequired(a => a.Run).WithMany(b => b.RunBucketRuns).HasForeignKey(c => c.RunID).WillCascadeOnDelete(false); // FK_RunBucketRun_Run_RunID
        }
    }
}