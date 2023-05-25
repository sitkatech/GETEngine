using System;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using Olsson.GET.Common.Utilities;

namespace Olsson.GET.Accessors
{
    public static class DatabaseFactory
    {
        public static T Create<T>() where T : DbContext, new()
        {
            System.Data.Entity.Database.SetInitializer<T>(null);

            EntityConnectionStringBuilder connectionString = new EntityConnectionStringBuilder();
            connectionString.ProviderConnectionString = ConfigurationHelper.ConnectionStrings.GetPrimaryConnectionString;

            T ret = Activator.CreateInstance(typeof(T)) as T;
            ret.Database.Connection.ConnectionString = connectionString.ProviderConnectionString;
            ret.Configuration.ProxyCreationEnabled = false;
            ret.Configuration.LazyLoadingEnabled = false;
            return ret;
        }
    }
}
