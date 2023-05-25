//  IMPORTANT:
//  This file is generated. Your changes will be lost.
//  Use the corresponding partial class for customizations.
//  Source Table: [dbo].[vModelAndScenarioCountForCustomerID]

namespace Olsson.GET.Accessors.EntityFramework
{
    public partial class vModelAndScenarioCountForCustomerID
    {
        /// <summary>
        /// Needed by ModelBinder
        /// </summary>
        public vModelAndScenarioCountForCustomerID()
        {
        }

        /// <summary>
        /// Constructor for building a new object with MaximalConstructor required fields in preparation for insert into database
        /// </summary>
        public vModelAndScenarioCountForCustomerID(int primaryKey, int customerID, int? modelCount, int? scenarioCount) : this()
        {
            this.PrimaryKey = primaryKey;
            this.CustomerID = customerID;
            this.ModelCount = modelCount;
            this.ScenarioCount = scenarioCount;
        }

        /// <summary>
        /// Constructor for building a new simple object with the POCO class
        /// </summary>
        public vModelAndScenarioCountForCustomerID(vModelAndScenarioCountForCustomerID vModelAndScenarioCountForCustomerID) : this()
        {
            this.PrimaryKey = vModelAndScenarioCountForCustomerID.PrimaryKey;
            this.CustomerID = vModelAndScenarioCountForCustomerID.CustomerID;
            this.ModelCount = vModelAndScenarioCountForCustomerID.ModelCount;
            this.ScenarioCount = vModelAndScenarioCountForCustomerID.ScenarioCount;
            CallAfterConstructor(vModelAndScenarioCountForCustomerID);
        }

        partial void CallAfterConstructor(vModelAndScenarioCountForCustomerID vModelAndScenarioCountForCustomerID);

        public int PrimaryKey { get; set; }
        public int CustomerID { get; set; }
        public int? ModelCount { get; set; }
        public int? ScenarioCount { get; set; }
    }
}