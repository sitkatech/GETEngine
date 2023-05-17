//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[Image]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class ImageConfiguration : EntityTypeConfiguration<Image>
    {
        public ImageConfiguration() : this("dbo"){}

        public ImageConfiguration(string schema)
        {
            ToTable("Image", schema);
            HasKey(x => x.ImageID);
            Property(x => x.ImageID).HasColumnName(@"ImageID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(x => x.ImageName).HasColumnName(@"ImageName").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(256);
            Property(x => x.Server).HasColumnName(@"Server").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(256);
            Property(x => x.IsLinux).HasColumnName(@"IsLinux").HasColumnType("bit").IsRequired();
            Property(x => x.CpuCoreCount).HasColumnName(@"CpuCoreCount").HasColumnType("int").IsOptional();
            Property(x => x.Memory).HasColumnName(@"Memory").HasColumnType("decimal").IsOptional().HasPrecision(4,1);

            // Foreign keys

        }
    }
}