//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ModelOutputZoneData]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class ModelOutputZoneDataConfiguration : EntityTypeConfiguration<ModelOutputZoneData>
    {
        public ModelOutputZoneDataConfiguration() : this("dbo"){}

        public ModelOutputZoneDataConfiguration(string schema)
        {
            ToTable("ModelOutputZoneData", schema);
            HasKey(x => x.ModelOutputZoneDataID);
            Property(x => x.ModelOutputZoneDataID).HasColumnName(@"ModelOutputZoneDataID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.ModelID).HasColumnName(@"ModelID").HasColumnType("int").IsRequired();
            Property(x => x.OutputZoneData).HasColumnName(@"OutputZoneData").HasColumnType("varchar").IsOptional();

            // Foreign keys
            HasRequired(a => a.Model).WithMany(b => b.ModelOutputZoneDatas).HasForeignKey(c => c.ModelID).WillCascadeOnDelete(false); // FK_ModelOutputZoneData_Model_ModelID
        }
    }
}