//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ModelScenario]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class ModelScenarioConfiguration : EntityTypeConfiguration<ModelScenario>
    {
        public ModelScenarioConfiguration() : this("dbo"){}

        public ModelScenarioConfiguration(string schema)
        {
            ToTable("ModelScenario", schema);
            HasKey(x => x.ModelScenarioID);
            Property(x => x.ModelScenarioID).HasColumnName(@"ModelScenarioID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.ModelID).HasColumnName(@"ModelID").HasColumnType("int").IsRequired();
            Property(x => x.ScenarioID).HasColumnName(@"ScenarioID").HasColumnType("int").IsRequired();

            // Foreign keys
            HasRequired(a => a.Model).WithMany(b => b.ModelScenarios).HasForeignKey(c => c.ModelID).WillCascadeOnDelete(false); // FK_ModelScenario_Model_ModelID
            HasRequired(a => a.Scenario).WithMany(b => b.ModelScenarios).HasForeignKey(c => c.ScenarioID).WillCascadeOnDelete(false); // FK_ModelScenario_Scenario_ScenarioID
        }
    }
}