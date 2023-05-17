//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ExternalMapLayer]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class ExternalMapLayerConfiguration : EntityTypeConfiguration<ExternalMapLayer>
    {
        public ExternalMapLayerConfiguration() : this("dbo"){}

        public ExternalMapLayerConfiguration(string schema)
        {
            ToTable("ExternalMapLayer", schema);
            HasKey(x => x.ExternalMapLayerID);
            Property(x => x.ExternalMapLayerID).HasColumnName(@"ExternalMapLayerID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.ExternalMapLayerDisplayName).HasColumnName(@"ExternalMapLayerDisplayName").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(100);
            Property(x => x.ExternalMapLayerTypeID).HasColumnName(@"ExternalMapLayerTypeID").HasColumnType("int").IsRequired();
            Property(x => x.ExternalMapLayerURL).HasColumnName(@"ExternalMapLayerURL").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(500);
            Property(x => x.LayerIsOnByDefault).HasColumnName(@"LayerIsOnByDefault").HasColumnType("bit").IsRequired();
            Property(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType("bit").IsRequired();
            Property(x => x.ExternalMapLayerDescription).HasColumnName(@"ExternalMapLayerDescription").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(200);
            Property(x => x.IsAvailableForAllConfigurations).HasColumnName(@"IsAvailableForAllConfigurations").HasColumnType("bit").IsRequired();
            Property(x => x.FeatureNameField).HasColumnName(@"FeatureNameField").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(100);
            Property(x => x.Token).HasColumnName(@"Token").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(255);

            // Foreign keys

        }
    }
}