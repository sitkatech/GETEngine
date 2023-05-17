namespace Olsson.GET.Common.DataContracts.Customers
{
    public class CustomerModelWithScenariosDto
    {
        public int ModelID { get; set; }

        public string ModelName { get; set; }

        public virtual CustomerScenario[] Scenarios { get; set; }
    }
}
