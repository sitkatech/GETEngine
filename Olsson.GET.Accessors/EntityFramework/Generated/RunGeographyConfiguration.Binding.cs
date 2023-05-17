//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[RunGeography]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class RunGeographyConfiguration : EntityTypeConfiguration<RunGeography>
    {
        public RunGeographyConfiguration() : this("dbo"){}

        public RunGeographyConfiguration(string schema)
        {
            ToTable("RunGeography", schema);
            HasKey(x => x.RunGeographyID);
            Property(x => x.RunGeographyID).HasColumnName(@"RunGeographyID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.RunID).HasColumnName(@"RunID").HasColumnType("int").IsRequired();
            Property(x => x.StressPeriod).HasColumnName(@"StressPeriod").HasColumnType("int").IsRequired();
            Property(x => x.Color).HasColumnName(@"Color").HasColumnType("char").IsRequired().IsFixedLength().IsUnicode(false).HasMaxLength(7);
            Property(x => x.Geography).HasColumnName(@"Geography").HasColumnType("geography").IsOptional();

            // Foreign keys
            HasRequired(a => a.Run).WithMany(b => b.RunGeographies).HasForeignKey(c => c.RunID).WillCascadeOnDelete(false); // FK_RunGeography_Run_RunID
        }
    }
}