//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[CustomerModelScenario]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class CustomerModelScenarioConfiguration : EntityTypeConfiguration<CustomerModelScenario>
    {
        public CustomerModelScenarioConfiguration() : this("dbo"){}

        public CustomerModelScenarioConfiguration(string schema)
        {
            ToTable("CustomerModelScenario", schema);
            HasKey(x => x.CustomerModelScenarioID);
            Property(x => x.CustomerModelScenarioID).HasColumnName(@"CustomerModelScenarioID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.CustomerID).HasColumnName(@"CustomerID").HasColumnType("int").IsRequired();
            Property(x => x.ModelID).HasColumnName(@"ModelID").HasColumnType("int").IsRequired();
            Property(x => x.ScenarioID).HasColumnName(@"ScenarioID").HasColumnType("int").IsRequired();

            // Foreign keys
            HasRequired(a => a.Customer).WithMany(b => b.CustomerModelScenarios).HasForeignKey(c => c.CustomerID).WillCascadeOnDelete(false); // FK_CustomerModelScenario_Customer_CustomerID
            HasRequired(a => a.Model).WithMany(b => b.CustomerModelScenarios).HasForeignKey(c => c.ModelID).WillCascadeOnDelete(false); // FK_CustomerModelScenario_Model_ModelID
            HasRequired(a => a.Scenario).WithMany(b => b.CustomerModelScenarios).HasForeignKey(c => c.ScenarioID).WillCascadeOnDelete(false); // FK_CustomerModelScenario_Scenario_ScenarioID
        }
    }
}