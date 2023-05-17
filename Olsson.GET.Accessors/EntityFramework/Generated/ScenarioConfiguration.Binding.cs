//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[Scenario]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class ScenarioConfiguration : EntityTypeConfiguration<Scenario>
    {
        public ScenarioConfiguration() : this("dbo"){}

        public ScenarioConfiguration(string schema)
        {
            ToTable("Scenario", schema);
            HasKey(x => x.ScenarioID);
            Property(x => x.ScenarioID).HasColumnName(@"ScenarioID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(x => x.ScenarioName).HasColumnName(@"ScenarioName").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(256);
            Property(x => x.InputControlType).HasColumnName(@"InputControlType").HasColumnType("int").IsRequired();
            Property(x => x.ShouldSwitchSign).HasColumnName(@"ShouldSwitchSign").HasColumnType("bit").IsRequired();
            Property(x => x.InputImageID).HasColumnName(@"InputImageID").HasColumnType("int").IsOptional();
            Property(x => x.ScenarioDescription).HasColumnName(@"ScenarioDescription").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(500);
            Property(x => x.ShowToAllUsersInScenarioList).HasColumnName(@"ShowToAllUsersInScenarioList").HasColumnType("bit").IsRequired();
            Property(x => x.ScenarioDocumentation).HasColumnName(@"ScenarioDocumentation").HasColumnType("varchar").IsOptional();

            // Foreign keys
            HasOptional(a => a.InputImage).WithMany(b => b.ScenariosWhereYouAreTheInputImage).HasForeignKey(c => c.InputImageID).WillCascadeOnDelete(false); // FK_Scenario_Image_InputImageID_ImageID
        }
    }
}