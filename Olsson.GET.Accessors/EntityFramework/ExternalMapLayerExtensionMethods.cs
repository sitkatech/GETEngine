using System.Collections.Generic;
using System.Linq;

namespace Olsson.GET.Accessors.EntityFramework
{
    public static partial class ExternalMapLayerExtensionMethods 
    {
        public static ExternalMapLayerSimpleDto AsSimpleDto(this ExternalMapLayer externalMapLayer)
        {
            return new ExternalMapLayerSimpleDto()
            {
                ExternalMapLayerID = externalMapLayer.ExternalMapLayerID,
                ExternalMapLayerDisplayName = externalMapLayer.ExternalMapLayerDisplayName,
                ExternalMapLayerTypeID = externalMapLayer.ExternalMapLayerTypeID,
                ExternalMapLayerURL = externalMapLayer.ExternalMapLayerURL,
                LayerIsOnByDefault = externalMapLayer.LayerIsOnByDefault,
                ExternalMapLayerDescription = externalMapLayer.ExternalMapLayerDescription,
                IsAvailableForAllConfigurations = externalMapLayer.IsAvailableForAllConfigurations,
                ExternalMapLayerCustomerModels = externalMapLayer.ExternalMapLayerCustomerModels
                    .Select(x => ExternalMapLayerCustomerModelExtensionMethods.AsSimpleDto(x)).ToList(),
                FeatureNameField = externalMapLayer.FeatureNameField,
                Token = externalMapLayer.Token
            };
        }
    }

    public class ExternalMapLayerSimpleDto
    {
        public int ExternalMapLayerID { get; set; }
        public string ExternalMapLayerDisplayName { get; set; }
        public int ExternalMapLayerTypeID { get; set; }
        public string ExternalMapLayerURL { get; set; }
        public bool LayerIsOnByDefault { get; set; }
        public bool IsActive { get; set; }
        public string ExternalMapLayerDescription { get; set; }
        public bool IsAvailableForAllConfigurations { get; set; }
        public List<ExternalMapLayerCustomerModelSimpleDto> ExternalMapLayerCustomerModels { get; set; }
        public string FeatureNameField { get; set; }
        public string Token { get; set; }
    }
}