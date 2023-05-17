using Olsson.GET.Accessors.EntityFramework;

namespace Olsson.GET.Accessors
{
    public interface ICanDeleteFull
    {
        void DeleteFull(EntityFramework.PrimaryDBContext dbContext);
    }
}