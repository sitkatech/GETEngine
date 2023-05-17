using Olsson.GET.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olsson.GET.Databases.Primary
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Environment.Exit(new DatabaseMigrator(args.Length > 0 ? args[0] : ConfigurationHelper.ConnectionStrings.GetPrimaryConnectionString).RunMigrations());
        }
    }
}
