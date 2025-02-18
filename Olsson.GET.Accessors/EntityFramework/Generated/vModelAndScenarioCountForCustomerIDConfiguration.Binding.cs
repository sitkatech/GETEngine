//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source View: [dbo].[vModelAndScenarioCountForCustomerID]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class vModelAndScenarioCountForCustomerIDConfiguration : EntityTypeConfiguration<vModelAndScenarioCountForCustomerID>
    {
        public vModelAndScenarioCountForCustomerIDConfiguration() : this("dbo"){}

        public vModelAndScenarioCountForCustomerIDConfiguration(string schema)
        {
            ToTable("vModelAndScenarioCountForCustomerID", schema);
            HasKey(x => x.PrimaryKey);
            
            
            
            
        }
    }
}