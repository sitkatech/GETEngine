//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[Run]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class RunConfiguration : EntityTypeConfiguration<Run>
    {
        public RunConfiguration() : this("dbo"){}

        public RunConfiguration(string schema)
        {
            ToTable("Run", schema);
            HasKey(x => x.RunID);
            Property(x => x.RunID).HasColumnName(@"RunID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.RunName).HasColumnName(@"RunName").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(256);
            Property(x => x.FileStorageLocator).HasColumnName(@"FileStorageLocator").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(50);
            Property(x => x.ImageID).HasColumnName(@"ImageID").HasColumnType("int").IsOptional();
            Property(x => x.ModelID).HasColumnName(@"ModelID").HasColumnType("int").IsRequired();
            Property(x => x.ScenarioID).HasColumnName(@"ScenarioID").HasColumnType("int").IsRequired();
            Property(x => x.UserID).HasColumnName(@"UserID").HasColumnType("int").IsRequired();
            Property(x => x.CustomerID).HasColumnName(@"CustomerID").HasColumnType("int").IsRequired();
            Property(x => x.RunStatusID).HasColumnName(@"RunStatusID").HasColumnType("int").IsRequired();
            Property(x => x.CreatedDate).HasColumnName(@"CreatedDate").HasColumnType("datetime").IsRequired();
            Property(x => x.IsDeleted).HasColumnName(@"IsDeleted").HasColumnType("bit").IsRequired();
            Property(x => x.InputFileName).HasColumnName(@"InputFileName").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(256);
            Property(x => x.ProcessingStartDate).HasColumnName(@"ProcessingStartDate").HasColumnType("datetime").IsOptional();
            Property(x => x.ProcessingEndDate).HasColumnName(@"ProcessingEndDate").HasColumnType("datetime").IsOptional();
            Property(x => x.ShouldCreateMaps).HasColumnName(@"ShouldCreateMaps").HasColumnType("bit").IsOptional();
            Property(x => x.Output).HasColumnName(@"Output").HasColumnType("varchar").IsOptional();
            Property(x => x.RestartCount).HasColumnName(@"RestartCount").HasColumnType("int").IsRequired();
            Property(x => x.InputVolumeUnitID).HasColumnName(@"InputVolumeUnitID").HasColumnType("int").IsRequired();
            Property(x => x.OutputVolumeUnitID).HasColumnName(@"OutputVolumeUnitID").HasColumnType("int").IsRequired();
            Property(x => x.IsDifferential).HasColumnName(@"IsDifferential").HasColumnType("bit").IsRequired();
            Property(x => x.RunDescription).HasColumnName(@"RunDescription").HasColumnType("varchar").IsOptional();

            // Foreign keys
            HasOptional(a => a.Image).WithMany(b => b.Runs).HasForeignKey(c => c.ImageID).WillCascadeOnDelete(false); // FK_Run_Image_ImageID
            HasRequired(a => a.Model).WithMany(b => b.Runs).HasForeignKey(c => c.ModelID).WillCascadeOnDelete(false); // FK_Run_Model_ModelID
            HasRequired(a => a.Scenario).WithMany(b => b.Runs).HasForeignKey(c => c.ScenarioID).WillCascadeOnDelete(false); // FK_Run_Scenario_ScenarioID
            HasRequired(a => a.User).WithMany(b => b.Runs).HasForeignKey(c => c.UserID).WillCascadeOnDelete(false); // FK_Run_User_UserID
            HasRequired(a => a.Customer).WithMany(b => b.Runs).HasForeignKey(c => c.CustomerID).WillCascadeOnDelete(false); // FK_Run_Customer_CustomerID
        }
    }
}