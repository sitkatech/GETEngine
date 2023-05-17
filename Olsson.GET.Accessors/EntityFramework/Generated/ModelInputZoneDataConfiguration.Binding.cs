//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ModelInputZoneData]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class ModelInputZoneDataConfiguration : EntityTypeConfiguration<ModelInputZoneData>
    {
        public ModelInputZoneDataConfiguration() : this("dbo"){}

        public ModelInputZoneDataConfiguration(string schema)
        {
            ToTable("ModelInputZoneData", schema);
            HasKey(x => x.ModelInputZoneDataID);
            Property(x => x.ModelInputZoneDataID).HasColumnName(@"ModelInputZoneDataID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.ModelID).HasColumnName(@"ModelID").HasColumnType("int").IsRequired();
            Property(x => x.InputZoneData).HasColumnName(@"InputZoneData").HasColumnType("varchar").IsOptional();

            // Foreign keys
            HasRequired(a => a.Model).WithMany(b => b.ModelInputZoneDatas).HasForeignKey(c => c.ModelID).WillCascadeOnDelete(false); // FK_ModelInputZoneData_Model_ModelID
        }
    }
}