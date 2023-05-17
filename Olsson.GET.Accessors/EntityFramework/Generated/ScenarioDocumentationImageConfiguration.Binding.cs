//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ScenarioDocumentationImage]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class ScenarioDocumentationImageConfiguration : EntityTypeConfiguration<ScenarioDocumentationImage>
    {
        public ScenarioDocumentationImageConfiguration() : this("dbo"){}

        public ScenarioDocumentationImageConfiguration(string schema)
        {
            ToTable("ScenarioDocumentationImage", schema);
            HasKey(x => x.ScenarioDocumentationImageID);
            Property(x => x.ScenarioDocumentationImageID).HasColumnName(@"ScenarioDocumentationImageID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.ScenarioID).HasColumnName(@"ScenarioID").HasColumnType("int").IsRequired();
            Property(x => x.FileResourceInfoID).HasColumnName(@"FileResourceInfoID").HasColumnType("int").IsRequired();

            // Foreign keys
            HasRequired(a => a.Scenario).WithMany(b => b.ScenarioDocumentationImages).HasForeignKey(c => c.ScenarioID).WillCascadeOnDelete(false); // FK_ScenarioDocumentationImage_Scenario_ScenarioID
            HasRequired(a => a.FileResourceInfo).WithMany(b => b.ScenarioDocumentationImages).HasForeignKey(c => c.FileResourceInfoID).WillCascadeOnDelete(false); // FK_ScenarioDocumentationImage_FileResourceInfo_FileResourceInfoID
        }
    }
}