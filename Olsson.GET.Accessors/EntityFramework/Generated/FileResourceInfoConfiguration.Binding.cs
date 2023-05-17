//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[FileResourceInfo]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class FileResourceInfoConfiguration : EntityTypeConfiguration<FileResourceInfo>
    {
        public FileResourceInfoConfiguration() : this("dbo"){}

        public FileResourceInfoConfiguration(string schema)
        {
            ToTable("FileResourceInfo", schema);
            HasKey(x => x.FileResourceInfoID);
            Property(x => x.FileResourceInfoID).HasColumnName(@"FileResourceInfoID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.FileResourceMimeTypeID).HasColumnName(@"FileResourceMimeTypeID").HasColumnType("int").IsRequired();
            Property(x => x.OriginalBaseFilename).HasColumnName(@"OriginalBaseFilename").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(255);
            Property(x => x.OriginalFileExtension).HasColumnName(@"OriginalFileExtension").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(255);
            Property(x => x.FileResourceGUID).HasColumnName(@"FileResourceGUID").HasColumnType("uniqueidentifier").IsRequired();
            Property(x => x.UserID).HasColumnName(@"UserID").HasColumnType("int").IsRequired();
            Property(x => x.CreateDate).HasColumnName(@"CreateDate").HasColumnType("datetime").IsRequired();

            // Foreign keys
            HasRequired(a => a.User).WithMany(b => b.FileResourceInfos).HasForeignKey(c => c.UserID).WillCascadeOnDelete(false); // FK_FileResourceInfo_User_UserID
        }
    }
}