using System.Linq;
using Olsson.GET.Common.DataContracts.Customers;
using Olsson.GET.Common.DataContracts.Models;
using System.Collections.Generic;
using Olsson.GET.Accessors.EntityFramework;
using CustomerModelScenario = Olsson.GET.Common.DataContracts.Customers.CustomerModelScenario;
using ScenarioFile = Olsson.GET.Common.DataContracts.Scenarios.ScenarioFile;
using User = Olsson.GET.Common.DataContracts.Users.User;

namespace Olsson.GET.Accessors.Customers
{
    class CustomerAccessor : BaseTableAccessor, ICustomerAccessor
    {
        public CustomerDto[] FindAllCustomers()
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                var entity = db.Customers.Include("CustomerModelScenarios").Select(c => c);

                return DTOMapper.Mapper.Map<CustomerDto[]>(entity);
            }
        }

        public CustomerDto FindCustomerById(int customerID)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                var entity = db.Customers.Include("CustomerModelScenarios").SingleOrDefault(c => c.CustomerID == customerID);

                return DTOMapper.Mapper.Map<CustomerDto>(entity);
            }
        }

        public User[] FindUsersForCustomer(int customerID)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                var entity = from u in db.Users
                             where u.CustomerID == customerID
                             select u;

                return DTOMapper.Mapper.Map<User[]>(entity);
            }
        }

        public CustomerDto CreateOrUpdateCustomer(CustomerDto customerDto)
        {
            return CreateOrUpdate<CustomerDto, Customer, PrimaryDBContext>(customerDto);
        }

        public CustomerModelScenario[] SaveCustomerModelScenarios(int customerID,
            CustomerModelScenario[] customerModelScenarios)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                var existingCustomerModelScenarios =
                    db.CustomerModelScenarios.Where(cms => cms.CustomerID == customerID);

                db.CustomerModelScenarios.RemoveRange(existingCustomerModelScenarios);

                if (customerModelScenarios != null)
                {
                    db.CustomerModelScenarios.AddRange(
                        DTOMapper.Mapper.Map<EntityFramework.CustomerModelScenario[]>(customerModelScenarios));
                }

                db.SaveChanges();

                var entity = from cms in db.CustomerModelScenarios
                             where cms.CustomerID == customerID
                             select cms;

                return DTOMapper.Mapper.Map<CustomerModelScenario[]>(entity);
            }
        }

        public CustomerModelWithScenariosDto[] FindAllModelsForCustomer(int customerID)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                var models = (from m in db.Models
                              where m.CustomerModelScenarios.Any(a => a.CustomerID == customerID)
                              select new
                              {
                                  m.ModelID,
                                  m.ModelName,
                                  Scenarios = m.ModelScenarios.Select(x => x.Scenario).Select(s => new
                                  {
                                      s.ScenarioID,
                                      s.ScenarioName,
                                      s.InputControlType,
                                      Enabled = s.CustomerModelScenarios.Any(a => a.ModelID == m.ModelID && a.CustomerID == customerID),
                                      s.ScenarioFiles
                                  })
                              }).ToList();

                var result = new List<CustomerModelWithScenariosDto>();

                foreach (var model in models)
                {
                    result.Add(new CustomerModelWithScenariosDto
                    {
                        ModelID = model.ModelID,
                        ModelName = model.ModelName,
                        Scenarios = model.Scenarios.Select(s =>
                            new CustomerScenario
                            {
                                ScenarioID = s.ScenarioID,
                                ScenarioName = s.ScenarioName,
                                Enabled = s.Enabled,
                                InputControlType = (InputControlType)s.InputControlType,
                                ScenarioFiles = s.ScenarioFiles.Select(f =>
                                    new ScenarioFile
                                    {
                                        ScenarioFileID = f.ScenarioFileID,
                                        ScenarioID = f.ScenarioID,
                                        ScenarioFileName = f.ScenarioFileName,
                                        ScenarioFileDescription = f.ScenarioFileDescription,
                                        IsRequired = f.IsRequired
                                    }
                                ).ToArray()
                            }).ToArray()
                    });
                }

                return result.ToArray();
            }
        }

        public CustomerModelWithScenariosDto FindModelForCustomer(int customerID, int modelID, int scenarioID)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                var customerModelScenario = (from cms in db.CustomerModelScenarios
                                             where cms.CustomerID == customerID
                                             && cms.ModelID == modelID
                                             && cms.ScenarioID == scenarioID
                                             select cms).FirstOrDefault();

                if (customerModelScenario != null)
                {
                    var model = (from m in db.Models
                                 where m.ModelID == modelID
                                 select m).First();

                    var scenario = (from s in db.Scenarios.Include("ScenarioFiles")
                                    where s.ScenarioID == scenarioID
                                    select s).First();

                    return new CustomerModelWithScenariosDto()
                    {
                        ModelID = model.ModelID,
                        ModelName = model.ModelName,
                        Scenarios = new CustomerScenario[]
                        {
                            new CustomerScenario
                            {
                                 ScenarioID = scenario.ScenarioID,
                                ScenarioName = scenario.ScenarioName,
                                Enabled = true,
                                InputControlType = (InputControlType)scenario.InputControlType,
                                ScenarioFiles = DTOMapper.Mapper.Map<ScenarioFile[]>(scenario.ScenarioFiles)
                            }
                        }
                    };
                }

                return null;
            }
        }

        public int GetExecutedRunCountForCustomer(int customerID)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                var runs = from r in db.Runs
                           where r.CustomerID == customerID
                                 && r.RunStatusID != (int)RunStatusEnum.Created
                           select r;

                return runs.Count();
            }
        }

        public List<CustomerModelScenarioDto> FindCustomerModelScenarios()
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                return db.CustomerModelScenarios
                    .Include("Customer").Include("Scenario").Include("Model").ToList().Select(x => DTOMapper.Mapper.Map<CustomerModelScenarioDto>(x))
                    .ToList();
            }
        }

        public vModelCountScenarioCountForCustomerID GetModelCountScenarioCountForCustomerId(int customerId)
        {
            using (var db = DatabaseFactory.Create<EntityFramework.PrimaryDBContext>())
            {
                return DTOMapper.Mapper.Map<vModelCountScenarioCountForCustomerID>(
                    db.vModelAndScenarioCountForCustomerIDs.Single(x => x.CustomerID == customerId));
            }
        }
    }
}