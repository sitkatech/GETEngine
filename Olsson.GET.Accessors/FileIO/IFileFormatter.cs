using CsvHelper;
using Microsoft.SqlServer.Types;
using Olsson.GET.Common.DataContracts.Runs;
using System.Collections.Generic;
using System.IO;

namespace Olsson.GET.Accessors.FileIO
{
    public interface IFileFormatter
    {
        string ParseWelFileHeaderData(string line);

        (int RowCount, int Option, int? AlternateWellRowCount) ParseStressPeriodHeaderData(string line);

        void WriteLocationRateRow(StreamWriter sw, LocationRate locationRate);

        (string Location, double Rate) ParseLocationRateData(string line);

        void WriteStressPeriodHeaderRow(TextWriter tw, int wellCount, int flag, int? alternateWellCount);

        void WriteTotalHeaderRow(TextWriter tw, int count, string headerValue);

        Dictionary<string, (SqlGeography Geography, List<LocationPumpingProportion> LocationPumpingProportions)> CreateLocationPositionMap(TextReader fileData);

        double GetDryLocationIndicator(string fileLine);

        IEnumerable<MapOutputData> GetRecordMapOutputValues(BinaryReader br, uint stressPeriod, uint timeStep);

        IEnumerable<(string Location, string Zone)> GetLocationZoneData(CsvReader reader);
    }   
}
