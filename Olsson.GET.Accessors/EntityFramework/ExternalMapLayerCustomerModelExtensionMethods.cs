using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Olsson.GET.Accessors.EntityFramework
{
    public  static partial class ExternalMapLayerCustomerModelExtensionMethods
    {
            public static ExternalMapLayerCustomerModelSimpleDto AsSimpleDto(this ExternalMapLayerCustomerModel  externalMapLayerCustomerModel)
            {
                return new ExternalMapLayerCustomerModelSimpleDto()
                {
                    ExternalMapLayerID = externalMapLayerCustomerModel.ExternalMapLayerID,
                    CustomerID = externalMapLayerCustomerModel.CustomerID,
                    ModelID = externalMapLayerCustomerModel.ModelID
                };
            }
    }

    public class ExternalMapLayerCustomerModelSimpleDto
    {
        public int ExternalMapLayerID { get; set; }
        public int CustomerID { get; set; }
        public int ModelID { get; set; }
    }
}