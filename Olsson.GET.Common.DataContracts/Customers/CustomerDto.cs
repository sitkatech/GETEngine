namespace Olsson.GET.Common.DataContracts.Customers
{
    public class CustomerDto
    {
        public int CustomerID { get; set; }

        public string CustomerName { get; set; }

        public bool IsTrial { get; set; }
        
        public virtual CustomerModelScenario[] CustomerModelScenarios { get; set; }
    }
}
