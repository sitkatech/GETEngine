//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[BaseflowTableProcessingConfiguration]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class BaseflowTableProcessingConfigurationConfiguration : EntityTypeConfiguration<BaseflowTableProcessingConfiguration>
    {
        public BaseflowTableProcessingConfigurationConfiguration() : this("dbo"){}

        public BaseflowTableProcessingConfigurationConfiguration(string schema)
        {
            ToTable("BaseflowTableProcessingConfiguration", schema);
            HasKey(x => x.BaseflowTableProcessingConfigurationID);
            Property(x => x.BaseflowTableProcessingConfigurationID).HasColumnName(@"BaseflowTableProcessingConfigurationID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.BaseflowTableIndicatorRegexPattern).HasColumnName(@"BaseflowTableIndicatorRegexPattern").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(200);
            Property(x => x.SegmentColumnNum).HasColumnName(@"SegmentColumnNum").HasColumnType("int").IsRequired();
            Property(x => x.FlowToAquiferColumnNum).HasColumnName(@"FlowToAquiferColumnNum").HasColumnType("int").IsRequired();
            Property(x => x.ReachColumnNum).HasColumnName(@"ReachColumnNum").HasColumnType("int").IsOptional();

            // Foreign keys

        }
    }
}