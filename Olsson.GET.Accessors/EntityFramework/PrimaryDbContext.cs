using System;
using System.Data.Entity.Validation;
using System.Text;

namespace Olsson.GET.Accessors.EntityFramework
{
    public partial class PrimaryDBContext
    {
        #region Overrides

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();

            int changes;
            try
            {
                changes = base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var sb = new StringBuilder();

                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }

                throw new DbEntityValidationException(
                    "Entity Validation Failed - errors follow:\n" +
                    sb, ex
                ); // Add the original exception as the innerException
            }
            catch (Exception e)
            {
                throw e;
            }

            return changes;
        }

        #endregion
    }
}
