//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ReportTemplate]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class ReportTemplateConfiguration : EntityTypeConfiguration<ReportTemplate>
    {
        public ReportTemplateConfiguration() : this("dbo"){}

        public ReportTemplateConfiguration(string schema)
        {
            ToTable("ReportTemplate", schema);
            HasKey(x => x.ReportTemplateID);
            Property(x => x.ReportTemplateID).HasColumnName(@"ReportTemplateID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.FileResourceInfoID).HasColumnName(@"FileResourceInfoID").HasColumnType("int").IsRequired();
            Property(x => x.DisplayName).HasColumnName(@"DisplayName").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(50);
            Property(x => x.Description).HasColumnName(@"Description").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(250);
            Property(x => x.ReportTemplateModelTypeID).HasColumnName(@"ReportTemplateModelTypeID").HasColumnType("int").IsRequired();
            Property(x => x.ReportTemplateModelID).HasColumnName(@"ReportTemplateModelID").HasColumnType("int").IsRequired();
            Property(x => x.IsAvailableForAllConfigurations).HasColumnName(@"IsAvailableForAllConfigurations").HasColumnType("bit").IsRequired();

            // Foreign keys
            HasRequired(a => a.FileResourceInfo).WithMany(b => b.ReportTemplates).HasForeignKey(c => c.FileResourceInfoID).WillCascadeOnDelete(false); // FK_ReportTemplate_FileResourceInfo_FileResourceInfoID
            HasRequired(a => a.ReportTemplateModelType).WithMany(b => b.ReportTemplates).HasForeignKey(c => c.ReportTemplateModelTypeID).WillCascadeOnDelete(false); // FK_ReportTemplate_ReportTemplateModelType_ReportTemplateModelTypeID
            HasRequired(a => a.ReportTemplateModel).WithMany(b => b.ReportTemplates).HasForeignKey(c => c.ReportTemplateModelID).WillCascadeOnDelete(false); // FK_ReportTemplate_ReportTemplateModel_ReportTemplateModelID
        }
    }
}