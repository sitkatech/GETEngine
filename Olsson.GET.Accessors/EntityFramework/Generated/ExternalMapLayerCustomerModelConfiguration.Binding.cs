//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ExternalMapLayerCustomerModel]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class ExternalMapLayerCustomerModelConfiguration : EntityTypeConfiguration<ExternalMapLayerCustomerModel>
    {
        public ExternalMapLayerCustomerModelConfiguration() : this("dbo"){}

        public ExternalMapLayerCustomerModelConfiguration(string schema)
        {
            ToTable("ExternalMapLayerCustomerModel", schema);
            HasKey(x => x.ExternalMapLayerCustomerModelID);
            Property(x => x.ExternalMapLayerCustomerModelID).HasColumnName(@"ExternalMapLayerCustomerModelID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.ExternalMapLayerID).HasColumnName(@"ExternalMapLayerID").HasColumnType("int").IsRequired();
            Property(x => x.CustomerID).HasColumnName(@"CustomerID").HasColumnType("int").IsRequired();
            Property(x => x.ModelID).HasColumnName(@"ModelID").HasColumnType("int").IsRequired();

            // Foreign keys
            HasRequired(a => a.ExternalMapLayer).WithMany(b => b.ExternalMapLayerCustomerModels).HasForeignKey(c => c.ExternalMapLayerID).WillCascadeOnDelete(false); // FK_ExternalMapLayerCustomerModel_ExternalMapLayer_ExternalMapLayerID
            HasRequired(a => a.Customer).WithMany(b => b.ExternalMapLayerCustomerModels).HasForeignKey(c => c.CustomerID).WillCascadeOnDelete(false); // FK_ExternalMapLayerCustomerModel_Customer_CustomerID
            HasRequired(a => a.Model).WithMany(b => b.ExternalMapLayerCustomerModels).HasForeignKey(c => c.ModelID).WillCascadeOnDelete(false); // FK_ExternalMapLayerCustomerModel_Model_ModelID
        }
    }
}