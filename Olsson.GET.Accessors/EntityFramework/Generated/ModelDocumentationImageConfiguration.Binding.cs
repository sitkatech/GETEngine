//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ModelDocumentationImage]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class ModelDocumentationImageConfiguration : EntityTypeConfiguration<ModelDocumentationImage>
    {
        public ModelDocumentationImageConfiguration() : this("dbo"){}

        public ModelDocumentationImageConfiguration(string schema)
        {
            ToTable("ModelDocumentationImage", schema);
            HasKey(x => x.ModelDocumentationImageID);
            Property(x => x.ModelDocumentationImageID).HasColumnName(@"ModelDocumentationImageID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.ModelID).HasColumnName(@"ModelID").HasColumnType("int").IsRequired();
            Property(x => x.FileResourceInfoID).HasColumnName(@"FileResourceInfoID").HasColumnType("int").IsRequired();

            // Foreign keys
            HasRequired(a => a.Model).WithMany(b => b.ModelDocumentationImages).HasForeignKey(c => c.ModelID).WillCascadeOnDelete(false); // FK_ModelDocumentationImage_Model_ModelID
            HasRequired(a => a.FileResourceInfo).WithMany(b => b.ModelDocumentationImages).HasForeignKey(c => c.FileResourceInfoID).WillCascadeOnDelete(false); // FK_ModelDocumentationImage_FileResourceInfo_FileResourceInfoID
        }
    }
}