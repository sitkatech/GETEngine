//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[ModelExecutable]
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Olsson.GET.Accessors.EntityFramework
{
    public class ModelExecutableConfiguration : EntityTypeConfiguration<ModelExecutable>
    {
        public ModelExecutableConfiguration() : this("dbo"){}

        public ModelExecutableConfiguration(string schema)
        {
            ToTable("ModelExecutable", schema);
            HasKey(x => x.ModelExecutableID);
            Property(x => x.ModelExecutableID).HasColumnName(@"ModelExecutableID").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.ModelID).HasColumnName(@"ModelID").HasColumnType("int").IsRequired();
            Property(x => x.ExecutableName).HasColumnName(@"ExecutableName").HasColumnType("varchar").IsRequired().IsUnicode(false).HasMaxLength(200);
            Property(x => x.Arguments).HasColumnName(@"Arguments").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(200);
            Property(x => x.RunOrder).HasColumnName(@"RunOrder").HasColumnType("int").IsRequired();
            Property(x => x.WorkingDirectory).HasColumnName(@"WorkingDirectory").HasColumnType("varchar").IsOptional().IsUnicode(false).HasMaxLength(200);
            Property(x => x.WrapWithBatchFile).HasColumnName(@"WrapWithBatchFile").HasColumnType("bit").IsRequired();
            Property(x => x.UseShellExecute).HasColumnName(@"UseShellExecute").HasColumnType("bit").IsRequired();
            Property(x => x.RedirectStandardOutput).HasColumnName(@"RedirectStandardOutput").HasColumnType("bit").IsRequired();
            Property(x => x.CreateNoWindow).HasColumnName(@"CreateNoWindow").HasColumnType("bit").IsRequired();

            // Foreign keys
            HasRequired(a => a.Model).WithMany(b => b.ModelExecutables).HasForeignKey(c => c.ModelID).WillCascadeOnDelete(false); // FK_ModelExecutable_Model_ModelID
        }
    }
}