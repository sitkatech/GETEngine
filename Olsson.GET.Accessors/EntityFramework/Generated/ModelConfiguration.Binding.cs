//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[Model]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class ModelConfiguration : EntityTypeConfiguration<Model>
    {
        public ModelConfiguration() : this("dbo"){}

        public ModelConfiguration(string schema)
        {
            ToTable("Model", schema);
            HasKey(x => x.ModelID);
            Property(x => x.ModelID).HasColumnName(@"ModelID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(x => x.ModelName).HasColumnName(@"ModelName").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(256);
            Property(x => x.ImageID).HasColumnName(@"ImageID").HasColumnType("int").IsRequired();
            Property(x => x.StartDateTime).HasColumnName(@"StartDateTime").HasColumnType("datetime").IsRequired();
            Property(x => x.RunFileName).HasColumnName(@"RunFileName").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(50);
            Property(x => x.AllowablePercentDiscrepancy).HasColumnName(@"AllowablePercentDiscrepancy").HasColumnType("float").IsOptional();
            Property(x => x.MapSettings).HasColumnName(@"MapSettings").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(1024);
            Property(x => x.MapRunFileName).HasColumnName(@"MapRunFileName").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(50);
            Property(x => x.IsDoubleSizeHeatMapOutput).HasColumnName(@"IsDoubleSizeHeatMapOutput").HasColumnType("bit").IsRequired();
            Property(x => x.NumberOfStressPeriods).HasColumnName(@"NumberOfStressPeriods").HasColumnType("int").IsRequired();
            Property(x => x.CanalData).HasColumnName(@"CanalData").HasColumnType("varchar").IsOptional();
            Property(x => x.BuddyGroup).HasColumnName(@"BuddyGroup").HasColumnType("nvarchar").IsOptional().HasMaxLength(128);
            Property(x => x.MapDrawdownFileName).HasColumnName(@"MapDrawdownFileName").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(50);
            Property(x => x.ListFileName).HasColumnName(@"ListFileName").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(50);
            Property(x => x.BaseflowTableProcessingConfigurationID).HasColumnName(@"BaseflowTableProcessingConfigurationID").HasColumnType("int").IsOptional();
            Property(x => x.ModelDescription).HasColumnName(@"ModelDescription").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(500);
            Property(x => x.ModelDocumentation).HasColumnName(@"ModelDocumentation").HasColumnType("varchar").IsOptional();
            Property(x => x.ModelEngineTypeID).HasColumnName(@"ModelEngineTypeID").HasColumnType("int").IsRequired();
            Property(x => x.ModelGridTypeID).HasColumnName(@"ModelGridTypeID").HasColumnType("int").IsRequired();

            // Foreign keys
            HasRequired(a => a.Image).WithMany(b => b.Models).HasForeignKey(c => c.ImageID).WillCascadeOnDelete(false); // FK_Model_Image_ImageID
            HasOptional(a => a.BaseflowTableProcessingConfiguration).WithMany(b => b.Models).HasForeignKey(c => c.BaseflowTableProcessingConfigurationID).WillCascadeOnDelete(false); // FK_Model_BaseflowTableProcessingConfiguration_BaseflowTableProcessingConfigurationID
        }
    }
}