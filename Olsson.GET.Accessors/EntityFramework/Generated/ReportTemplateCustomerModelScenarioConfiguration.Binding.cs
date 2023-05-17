//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ReportTemplateCustomerModelScenario]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class ReportTemplateCustomerModelScenarioConfiguration : EntityTypeConfiguration<ReportTemplateCustomerModelScenario>
    {
        public ReportTemplateCustomerModelScenarioConfiguration() : this("dbo"){}

        public ReportTemplateCustomerModelScenarioConfiguration(string schema)
        {
            ToTable("ReportTemplateCustomerModelScenario", schema);
            HasKey(x => x.ReportTemplateCustomerModelScenarioID);
            Property(x => x.ReportTemplateCustomerModelScenarioID).HasColumnName(@"ReportTemplateCustomerModelScenarioID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.ReportTemplateID).HasColumnName(@"ReportTemplateID").HasColumnType("int").IsRequired();
            Property(x => x.CustomerID).HasColumnName(@"CustomerID").HasColumnType("int").IsRequired();
            Property(x => x.ModelID).HasColumnName(@"ModelID").HasColumnType("int").IsRequired();
            Property(x => x.ScenarioID).HasColumnName(@"ScenarioID").HasColumnType("int").IsRequired();

            // Foreign keys
            HasRequired(a => a.ReportTemplate).WithMany(b => b.ReportTemplateCustomerModelScenarios).HasForeignKey(c => c.ReportTemplateID).WillCascadeOnDelete(false); // FK_ReportTemplateCustomerModelScenario_ReportTemplate_ReportTemplateID
            HasRequired(a => a.Customer).WithMany(b => b.ReportTemplateCustomerModelScenarios).HasForeignKey(c => c.CustomerID).WillCascadeOnDelete(false); // FK_ReportTemplateCustomerModelScenario_Customer_CustomerID
            HasRequired(a => a.Model).WithMany(b => b.ReportTemplateCustomerModelScenarios).HasForeignKey(c => c.ModelID).WillCascadeOnDelete(false); // FK_ReportTemplateCustomerModelScenario_Model_ModelID
            HasRequired(a => a.Scenario).WithMany(b => b.ReportTemplateCustomerModelScenarios).HasForeignKey(c => c.ScenarioID).WillCascadeOnDelete(false); // FK_ReportTemplateCustomerModelScenario_Scenario_ScenarioID
        }
    }
}