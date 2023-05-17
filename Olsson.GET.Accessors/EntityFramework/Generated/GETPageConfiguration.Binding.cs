//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[GETPage]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class GETPageConfiguration : EntityTypeConfiguration<GETPage>
    {
        public GETPageConfiguration() : this("dbo"){}

        public GETPageConfiguration(string schema)
        {
            ToTable("GETPage", schema);
            HasKey(x => x.GETPageID);
            Property(x => x.GETPageID).HasColumnName(@"GETPageID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.GETPageTypeID).HasColumnName(@"GETPageTypeID").HasColumnType("int").IsRequired();
            Property(x => x.GETPageContent).HasColumnName(@"GETPageContent").HasColumnType("varchar").IsOptional();

            // Foreign keys

        }
    }
}