//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ScenarioFile]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class ScenarioFileConfiguration : EntityTypeConfiguration<ScenarioFile>
    {
        public ScenarioFileConfiguration() : this("dbo"){}

        public ScenarioFileConfiguration(string schema)
        {
            ToTable("ScenarioFile", schema);
            HasKey(x => x.ScenarioFileID);
            Property(x => x.ScenarioFileID).HasColumnName(@"ScenarioFileID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(x => x.ScenarioID).HasColumnName(@"ScenarioID").HasColumnType("int").IsRequired();
            Property(x => x.ScenarioFileName).HasColumnName(@"ScenarioFileName").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(256);
            Property(x => x.ScenarioFileDescription).HasColumnName(@"ScenarioFileDescription").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(512);
            Property(x => x.IsRequired).HasColumnName(@"IsRequired").HasColumnType("bit").IsRequired();

            // Foreign keys
            HasRequired(a => a.Scenario).WithMany(b => b.ScenarioFiles).HasForeignKey(c => c.ScenarioID).WillCascadeOnDelete(false); // FK_ScenarioFile_Scenario_ScenarioID
        }
    }
}