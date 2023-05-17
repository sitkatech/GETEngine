using System.Data.Entity;

namespace Olsson.GET.Accessors
{
    abstract class BaseTableAccessor
    {
        protected TEntity CreateOrUpdate<TEntity, TEFEntity, TDbContext>(TEntity dto)
          where TEntity : class, new()
          where TEFEntity : class, new()
          where TDbContext : DbContext, new()
        {
            var entity = DTOMapper.Mapper.Map<TEFEntity>(dto);

            using (var db = DatabaseFactory.Create<TDbContext>())
            {
                if (GetIdValue(entity) == default(int))
                {
                    db.Set<TEFEntity>().Add(entity);
                    db.SaveChanges();
                }
                else
                {
                    if (db.Entry(entity).State == EntityState.Detached)
                    {
                        db.Set<TEFEntity>().Attach(entity);
                    }

                    db.Entry(entity).State = EntityState.Modified;
                    db.SaveChanges();
                }

                DTOMapper.Mapper.Map(entity, dto);
            }

            return dto;
        }

        private int GetIdValue(object obj)
        {
            return (int)obj.GetType().GetProperty("PrimaryKey").GetValue(obj, null);
        }
    }
}
