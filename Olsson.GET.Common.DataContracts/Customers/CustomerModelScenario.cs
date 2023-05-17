namespace Olsson.GET.Common.DataContracts.Customers
{
    public class CustomerModelScenarioDto
    {
        public int CustomerID { get; set; }

        public string CustomerName { get; set; }

        public int ModelID { get; set; }

        public string ModelName { get; set; }

        public int ScenarioID { get; set; }

        public string ScenarioName { get; set; }
    }

    public class CustomerModelScenario
    {
        public int CustomerID { get; set; }


        public int ModelID { get; set; }


        public int ScenarioID { get; set; }
    }
}
