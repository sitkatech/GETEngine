using DbUp;
using DbUp.Engine;
using Olsson.GET.Common.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace Olsson.GET.Common.Utilities
{
    public class DatabaseMigrator
    {
        private string _connectionString;

        public DatabaseMigrator(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int RunMigrations()
        {
            try
            {
                EnsureDatabase.For.SqlDatabase(_connectionString);

                UpgradeEngine migrator = DeployChanges.To
                                                      .SqlDatabase(_connectionString)
                                                      .WithTransactionPerScript()
                                                      .WithScriptsEmbeddedInAssembly(Assembly.GetEntryAssembly())
                                                      .WithVariables(GetSubstitutionVariables())
                                                      .WithExecutionTimeout(TimeSpan.FromSeconds(180))
                                                      .LogToConsole()
                                                      .Build();

                DatabaseUpgradeResult result = migrator.PerformUpgrade();


                if (!result.Successful)
                {
                    Console.WriteLine(result.Error);
#if DEBUG
                    Console.ReadLine();
#endif
                    return -1;
                }

                Console.WriteLine("Success!");

                return 0;
            }
#if DEBUG
            catch (Exception ex)
            {
                Console.WriteLine(ex.AllExceptionMessages());
                Console.ReadLine();
#else
            catch (Exception)
            {
#endif
                return -1;
            }
        }

        private Dictionary<string, string> GetSubstitutionVariables()
        {
            var variables = (NameValueCollection) ConfigurationManager.GetSection("SqlScriptsVariableSubstitution");
            return variables.AllKeys.ToDictionary(a => a, a => variables[a]);
        }
    }
}
