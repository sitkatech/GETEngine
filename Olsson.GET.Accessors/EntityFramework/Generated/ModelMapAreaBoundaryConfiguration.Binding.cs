//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ModelMapAreaBoundary]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class ModelMapAreaBoundaryConfiguration : EntityTypeConfiguration<ModelMapAreaBoundary>
    {
        public ModelMapAreaBoundaryConfiguration() : this("dbo"){}

        public ModelMapAreaBoundaryConfiguration(string schema)
        {
            ToTable("ModelMapAreaBoundary", schema);
            HasKey(x => x.ModelMapAreaBoundaryID);
            Property(x => x.ModelMapAreaBoundaryID).HasColumnName(@"ModelMapAreaBoundaryID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.ModelID).HasColumnName(@"ModelID").HasColumnType("int").IsRequired();
            Property(x => x.MapAreaBoundary).HasColumnName(@"MapAreaBoundary").HasColumnType("varchar").IsOptional();

            // Foreign keys
            HasRequired(a => a.Model).WithMany(b => b.ModelMapAreaBoundaries).HasForeignKey(c => c.ModelID).WillCascadeOnDelete(false); // FK_ModelMapAreaBoundary_Model_ModelID
        }
    }
}