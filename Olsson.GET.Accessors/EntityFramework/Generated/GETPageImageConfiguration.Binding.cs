//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[GETPageImage]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class GETPageImageConfiguration : EntityTypeConfiguration<GETPageImage>
    {
        public GETPageImageConfiguration() : this("dbo"){}

        public GETPageImageConfiguration(string schema)
        {
            ToTable("GETPageImage", schema);
            HasKey(x => x.GETPageImageID);
            Property(x => x.GETPageImageID).HasColumnName(@"GETPageImageID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.GETPageID).HasColumnName(@"GETPageID").HasColumnType("int").IsRequired();
            Property(x => x.FileResourceInfoID).HasColumnName(@"FileResourceInfoID").HasColumnType("int").IsRequired();

            // Foreign keys
            HasRequired(a => a.GETPage).WithMany(b => b.GETPageImages).HasForeignKey(c => c.GETPageID).WillCascadeOnDelete(false); // FK_GETPageImage_GETPage_GETPageID
            HasRequired(a => a.FileResourceInfo).WithMany(b => b.GETPageImages).HasForeignKey(c => c.FileResourceInfoID).WillCascadeOnDelete(false); // FK_GETPageImage_FileResourceInfo_FileResourceInfoID
        }
    }
}