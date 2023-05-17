//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ModelStressPeriodCustomStartDate]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class ModelStressPeriodCustomStartDateConfiguration : EntityTypeConfiguration<ModelStressPeriodCustomStartDate>
    {
        public ModelStressPeriodCustomStartDateConfiguration() : this("dbo"){}

        public ModelStressPeriodCustomStartDateConfiguration(string schema)
        {
            ToTable("ModelStressPeriodCustomStartDate", schema);
            HasKey(x => x.ModelStressPeriodCustomStartDateID);
            Property(x => x.ModelStressPeriodCustomStartDateID).HasColumnName(@"ModelStressPeriodCustomStartDateID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.ModelID).HasColumnName(@"ModelID").HasColumnType("int").IsRequired();
            Property(x => x.StressPeriod).HasColumnName(@"StressPeriod").HasColumnType("int").IsRequired();
            Property(x => x.StressPeriodStartDate).HasColumnName(@"StressPeriodStartDate").HasColumnType("datetime").IsRequired();

            // Foreign keys
            HasRequired(a => a.Model).WithMany(b => b.ModelStressPeriodCustomStartDates).HasForeignKey(c => c.ModelID).WillCascadeOnDelete(false); // FK_ModelStressPeriodCustomStartDate_Model_ModelID
        }
    }
}