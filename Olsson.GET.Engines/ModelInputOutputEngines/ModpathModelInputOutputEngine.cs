using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using Olsson.GET.Accessors.FileIO;
using Olsson.GET.Common.DataContracts.Models;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.Utilities;

namespace Olsson.GET.Engines.ModelInputOutputEngines
{
    public class ModpathModelInputOutputEngine : BaseInputOutputEngine, IModelInputOutputEngine
    {
        private static double ToRadians = (Math.PI / 180);
        private static readonly ILogger Logger = Logging.GetLogger<ModpathModelInputOutputEngine>();

        public ModpathModelInputOutputEngine()
        {
            AccessorFactory = new Accessors.AccessorFactory();
        }

        public void GenerateInputFiles(Run run)
        {
            CreateLocationFile(run);
        }

        public void GenerateOutputFiles(Run run)
        {
            var currResultId = 0;

            CreateListFile(run, ref currResultId);
            CreateKMZFile(run, ref currResultId);

        }

        private void CreateListFile(Run run, ref int currResultId)
        {
            Logger.LogInformation($"Creating list file for run {run.RunID}");
            currResultId++;

            var modflowFileAccessor = AccessorFactory.CreateAccessor<IModelFileAccessorFactory>().CreateModflowFileAccessor(run.Model);
            var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            var listFileName = modflowFileAccessor.GetModpathListFileName(run.Model.ModelExecutables.OrderBy(x => x.RunOrder).First().Arguments);
            var data = modflowFileAccessor.GetModpathListFileContent(listFileName);

            var result = new RunResultDetails()
            {
                RunResultId = currResultId,
                RunResultName = "List File Output",
                ResultSets = new List<RunResultSet>() { new RunResultSet() {
                    DisplayType = RunResultDisplayType.Text,
                    TextDisplay = new TextDisplay(){ Text = data, FileName = $"ListFile-{run.RunID}.txt" }
                } }
            };

            WriteOutputFile(run, fileAccessor, result);
        }

        private void CreateLocationFile(Run run)
        {
            Logger.LogInformation($"Begin: Creating Location file for run: {run.RunID}");
            var modelFileAccessor = AccessorFactory.CreateAccessor<IModelFileAccessorFactory>().CreateModflowFileAccessor(run.Model);

            var settings = modelFileAccessor.GetSettings();

            var output = new StringBuilder();

            //headers rows - Hardcoded 1 for now
            output.AppendLine("1");
            output.AppendLine("1");

            //Particle Count row 
            output.AppendLine($"{run.RunWellParticleInputs.Sum(i => i.ParticleCount)} 0");

            //each well
            foreach (var well in run.RunWellParticleInputs)
            {
                var wellCoordinates = new Coordinate() { Lat = well.Lat, Lng = well.Lng };

                //each particle
                foreach (var particleCoordinates in GetCoordinatesForWellParticles(wellCoordinates, well.ParticleCount, settings.ParticleRadius))
                {
                    output.AppendLine($"{CoordinateToCell(particleCoordinates, modelFileAccessor, settings.LocalZ)} {settings.TimeOffset.ToString("N1")} {settings.Drape}");
                }
            }

            //write output
            var locFileName = modelFileAccessor.GetModpathLocationFileName(run.Model.ModelExecutables.OrderBy(x => x.RunOrder).First().Arguments);
            modelFileAccessor.WriteLocationFile(locFileName, output.ToString());
            Logger.LogInformation($"Finished: Creating Location file for run: {run.RunID}");
        }

        private Coordinate[] GetCoordinatesForWellParticles(Coordinate wellCoordinate, int particleCount, double particleRadius)
        {
            //given a well coordinate, will return a coordinate per particle evenly distributed from the center at the particle radius distance
            if (particleCount == 1)//if one particle, just keep it centered
            {
                return new Coordinate[] { wellCoordinate };
            }
            else
            {
                var result = new List<Coordinate>();
                double angleFromWell = 360d / Convert.ToDouble(particleCount);

                for (var i = 0; i < particleCount; i++)
                {
                    var currentPointDegrees = i * angleFromWell;

                    var lng = Math.Round((Math.Cos(currentPointDegrees * ToRadians) * particleRadius) + wellCoordinate.Lng, 9);
                    var lat = Math.Round((Math.Sin(currentPointDegrees * ToRadians) * particleRadius) + wellCoordinate.Lat, 9);

                    result.Add(new Coordinate() { Lat = lat, Lng = lng });
                }


                return result.ToArray();
            }
        }

        private string CoordinateToCell(Coordinate particleCoordinate, IModelFileAccessor modelFileAccessor, double localZ)
        {
            //this will return "{cell location} {localX} {localY} {localZ}"  where cell location can be {layer row col} or {cell number}
            var bounds = FindCellBounds(particleCoordinate, modelFileAccessor);
            var CellLocation = CalculateCellLocation(particleCoordinate, bounds.BoundCoordinates, localZ);

            return $"{bounds.Location} {CellLocation.LocalX.ToString("N4")} {CellLocation.LocalY.ToString("N4")} {CellLocation.LocalZ.ToString("N4")}";
        }

        private LocationWithBounds FindCellBounds(Coordinate coordinate, IModelFileAccessor modelFileAccessor)
        {
            var cell = modelFileAccessor.FindLocationCell(coordinate.Lat, coordinate.Lng);

            if (cell == null)
            {
                throw new Exception("Location is outside model bounds");
            }

            return cell;
        }

        private CellLocation CalculateCellLocation(Coordinate particleCoordiante, List<Coordinate> cellBounds, double localZ)
        {
            //each local value is between 0 and 1 based on relative position starting from bottom left to top right of the cell  
            //we are working 2-dimmensionally with lat long so the z is static

            var width = Math.Abs(cellBounds.Max(c => c.Lng) - cellBounds.Min(c => c.Lng)); //absolute width in degrees
            var height = Math.Abs(cellBounds.Max(c => c.Lat) - cellBounds.Min(c => c.Lat)); //absolute height in degrees

            var relativeX = Math.Abs(particleCoordiante.Lng - cellBounds.Min(c => c.Lng));
            var relativeY = Math.Abs(particleCoordiante.Lat - cellBounds.Min(c => c.Lat));

            return new CellLocation() { LocalX = relativeX / width, LocalY = relativeY / height, LocalZ = localZ };
        }

        private void CreateKMZFile(Run run, ref int currResultId)
        {
            currResultId++;

            var modflowFileAccessor = AccessorFactory.CreateAccessor<IModelFileAccessorFactory>().CreateModflowFileAccessor(run.Model);
            var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            var timeSeriesFileName = modflowFileAccessor.GetModpathTimeSeriesFileName(run.Model.ModelExecutables.OrderBy(x => x.RunOrder).First().Arguments);
            var timeseries = modflowFileAccessor.GetModpathTimeSeriesResult(timeSeriesFileName);

            var particleTimeLocations = TimeSeriesToLocation(timeseries, modflowFileAccessor);

            var kmlData = BuildKMLOutput(particleTimeLocations, modflowFileAccessor, run);

            var result = new RunResultDetails()
            {
                RunResultId = currResultId,
                RunResultName = "KML File Output",
                ResultSets = new List<RunResultSet>() { new RunResultSet() {
                    DisplayType = RunResultDisplayType.Text,
                    TextDisplay = new TextDisplay(){ Text = kmlData, FileName = $"MapFile-{run.RunID}.kml" }
                } }
            };

            WriteOutputFile(run, fileAccessor, result);
        }

        private List<ParticleLocation> TimeSeriesToLocation(List<ModpathTimeSeries> timeSeries, IModelFileAccessor modelFileAccessor)
        {
            var result = new List<ParticleLocation>();

            var timeSeriesGroupByParticle = timeSeries.GroupBy(ts => ts.ParticleId);

            foreach (var group in timeSeriesGroupByParticle)
            {
                result.Add(new ParticleLocation()
                {
                    PaticleId = group.Key,
                    TimeLocations = group.Select(ts => new ParticleTimeLocations() { TimeSinceBeginingOfModelInDays = ts.TrackingTime, Coordinate = CoordinateFromCellNumber(ts.CellNumber, ts.Layer, ts.LocalX, ts.LocalY, modelFileAccessor) }).ToList()
                });
            }

            return result;
        }

        private Coordinate CoordinateFromCellNumber(int cellNumber, int layer, double localX, double localY, IModelFileAccessor modelFileAccessor)
        {
            var settings = modelFileAccessor.GetSettings();

            if (layer > 1)
            {
                //Compute cell number for layer one above actual cell
                cellNumber = cellNumber - ((settings.ColumnCount * settings.RowCount) * (layer - 1));
                //Hardcode layer number to layer one until we can handle multilayer mapping 
                layer = 1;
            }
            //Colby formulas!
            var row = Math.Ceiling((double)cellNumber / (double)settings.ColumnCount);
            var col = cellNumber - ((row - 1) * settings.ColumnCount);

            //build the key
            var key = $"{layer}|{row}|{col}";

            //Find the points that make up the cell
            var cellBounds = modelFileAccessor.FindCellBounds(key);

            //find where in the cell the localX and localY fall
            var width = Math.Abs(cellBounds.Max(cb => cb.Lng) - cellBounds.Min(cb => cb.Lng)); //absolute width in degrees
            var height = Math.Abs(cellBounds.Max(cb => cb.Lat) - cellBounds.Min(cb => cb.Lat)); //absolute height in degrees

            return new Coordinate()
            {
                Lng = cellBounds.Min(cb => cb.Lng) + (width * localX),
                Lat = cellBounds.Min(cb => cb.Lat) + (height * localY)
            };
        }

        private string BuildKMLOutput(List<ParticleLocation> particleLocations, IModelFileAccessor modelFileAccessor, Run run)
        {
            using (var sw = new StringWriter())
            {
                XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
                using (var kml = XmlWriter.Create(sw, settings))
                {
                    kml.WriteStartDocument();

                    kml.WriteStartElement("kml", "http://www.opengis.net/kml/2.2");
                    kml.WriteStartElement("Document");

                    kml.WriteElementString("name", run.RunName);
                    kml.WriteElementString("description", $"GET Action results for {run.RunName} - {DateTime.UtcNow} created by {run.User?.FullName} - {run.User?.Email} with model: {run.Model.ModelName}");

                    BuildStyles(kml, modelFileAccessor);

                    //add center point for wells
                    if (run.RunWellParticleInputs != null)
                    {
                        foreach (var well in run.RunWellParticleInputs)
                        {
                            BuildKMLPoint(kml, well.Lat, well.Lng, well.Name, well.ParticleCount);
                        }
                    }

                    foreach (var particle in particleLocations)
                    {
                        if (particle.TimeLocations.Count() > 0)
                        {
                            foreach (var line in GetLineWithColor(particle.TimeLocations, modelFileAccessor))
                            {
                                BuildKMLLine(kml, line.Coordinates, particle.PaticleId.ToString(), line.Color, line.Min, line.Max);
                            }
                        }
                    }

                    kml.WriteEndElement(); // <Document>
                    kml.WriteEndDocument(); // <kml>

                    kml.Close();
                }
                return sw.ToString();
            }
        }

        private void BuildStyles(XmlWriter xmlWriter, IModelFileAccessor modelFileAccessor)
        {
            var ranges = modelFileAccessor.GetSettings().ColorRanges;

            foreach (var range in ranges)
            {
                //LineStyle
                xmlWriter.WriteStartElement("Style");
                xmlWriter.WriteAttributeString("id", range.Color);
                xmlWriter.WriteStartElement("LineStyle");
                xmlWriter.WriteElementString("color", range.Color);
                xmlWriter.WriteElementString("width", "2");
                xmlWriter.WriteEndElement(); // <LineStyle>
                xmlWriter.WriteEndElement(); // <Style>
            }

            //pin for centerpoint
            xmlWriter.WriteStartElement("Style");
            xmlWriter.WriteAttributeString("id", "pin");
            xmlWriter.WriteStartElement("IconStyle");
            xmlWriter.WriteStartElement("Icon");
            xmlWriter.WriteElementString("href", "https://maps.google.com/mapfiles/ms/icons/red.png");
            xmlWriter.WriteEndElement(); // <Icon>
            xmlWriter.WriteEndElement(); // <IconStyle>
            xmlWriter.WriteEndElement(); // <Style>
        }

        private void BuildKMLLine(XmlWriter xmlWriter, List<Coordinate> points, string particleId, string color, double min, double max)
        {
            xmlWriter.WriteStartElement("Placemark");
            xmlWriter.WriteAttributeString("id", $"{particleId}_{color}");

            xmlWriter.WriteElementString("name", $"Particle {particleId}_{color}");
            xmlWriter.WriteElementString("description", $"Particle {particleId} between {min} and {max} days in color {color}");

            xmlWriter.WriteElementString("styleUrl", $"#{color}"); //link style

            xmlWriter.WriteStartElement("LineString");
            xmlWriter.WriteElementString("coordinates", string.Join(" ", points.Select(p => $"{p.Lng},{p.Lat},0")));//coordinates format is lng,lat,altitude space separated
            xmlWriter.WriteEndElement(); // <LineString>
            xmlWriter.WriteEndElement(); // <Placemark>
        }

        private void BuildKMLPoint(XmlWriter xmlWriter, double lat, double lng, string name, int cnt)
        {
            xmlWriter.WriteStartElement("Placemark");
            xmlWriter.WriteAttributeString("id", $"{name}");

            xmlWriter.WriteElementString("name", name);
            xmlWriter.WriteElementString("description", $"{name} at latitude:{lat}, longitude:{lng} particles { cnt}");

            xmlWriter.WriteElementString("styleUrl", $"#pin"); //link style

            xmlWriter.WriteStartElement("Point");
            xmlWriter.WriteElementString("coordinates", $"{lng},{lat},0");//coordinates format is lng,lat,altitude
            xmlWriter.WriteEndElement(); // <Point>
            xmlWriter.WriteEndElement(); // <Placemark>
        }

        private List<LineWithColor> GetLineWithColor(List<ParticleTimeLocations> particleTimeLocations, IModelFileAccessor modelFileAccessor)
        {
            var ranges = modelFileAccessor.GetSettings().ColorRanges;

            var result = new List<LineWithColor>();

            foreach (var range in ranges)
            {
                result.Add(new LineWithColor()
                {
                    Color = range.Color,
                    Min = range.Min,
                    Max = range.Max,
                    Coordinates = particleTimeLocations.Where(pl => range.Min <= pl.TimeSinceBeginingOfModelInDays && pl.TimeSinceBeginingOfModelInDays <= range.Max)?.Select(l => l.Coordinate)?.ToList()
                });
            }

            return result;
        }
    }



    internal class CellLocation
    {
        public double LocalX { get; set; }

        public double LocalY { get; set; }

        public double LocalZ { get; set; }
    }

    internal class LineWithColor
    {
        public string Color { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public List<Coordinate> Coordinates { get; set; }
    }
}
