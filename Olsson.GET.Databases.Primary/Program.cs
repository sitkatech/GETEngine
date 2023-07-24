using Olsson.GET.Common.Utilities;

namespace Olsson.GET.Databases.Primary
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigurationHelper.Build();
            System.Environment.Exit(new DatabaseMigrator(args.Length > 0 ? args[0] : ConfigurationHelper.ConnectionStrings.GetPrimaryDatabase).RunMigrations());
        }
    }
}
