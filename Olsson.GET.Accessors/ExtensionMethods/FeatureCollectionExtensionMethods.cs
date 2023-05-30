using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;

namespace Olsson.GET.Accessors.ExtensionMethods
{
    public static class FeatureCollectionExtensionMethods
    {
        public static string Serialize(this FeatureCollection featureCollection)
        {
            var serializer = GeoJsonSerializer.Create();
            string geoJson;
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            serializer.Serialize(jsonWriter, featureCollection);
            geoJson = stringWriter.ToString();
            return geoJson;
        }
    }
}
