using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using SourceFEWSAdapter.Core;
using SourceFEWSAdapter.FEWSPI;
using TIME.DataTypes;
using TIME.DataTypes.IO.CsvFileIo;
using TIME.ManagedExtensions;

namespace SourceFEWSAdapter.Commands
{
    public static class PreAdapter
    {
        public static void Run(RunComplexType runSettings, Diagnostics diagnostics, string[] args)
        {
            var inputSeries = runSettings.AllInputSeries();
            if (inputSeries.Length == 0)
            {
                diagnostics.Log(Diagnostics.LEVEL_WARNING, "Pre adapter called with no exported timeseries");
                return;
            }

            var haveInputMapping = inputSeries.Any(s => s.SourceInputFile() != null);
            if (runSettings.InputTimeSeriesMapping() != null)
            {
                WriteInputsUsingMappingFile(runSettings, inputSeries, diagnostics);
            }
            else if (haveInputMapping)
            {
                diagnostics.Log(Diagnostics.LEVEL_INFO,"Using qualifierId to determine target CSV files");
                WriteInputsUsingQualifier(runSettings, inputSeries,diagnostics);
            }
            else
            {
                diagnostics.Log(Diagnostics.LEVEL_INFO,"Writing inputs to CSV files by parameter");
                WriteSourceInputsToFilePerParameter(runSettings, inputSeries);
            }

            diagnostics.Log(Diagnostics.LEVEL_INFO, "All Done");
        }

        private static void WriteInputsUsingMappingFile(RunComplexType runSettings, TimeSeriesComplexType[] inputSeries, Diagnostics diagnostics)
        {
            var inputMappingFn = runSettings.InputTimeSeriesMapping();
            var mapping = ReadInputMapping(inputMappingFn);

            foreach (var ts in inputSeries)
            {
                try
                {
                    var dest = mapping[new Tuple<string, string>(ts.header.locationId, ts.header.parameterId)];
                    ts.AddQualifier($"file:{dest.Item1}");
                    ts.AddQualifier($"column:{dest.Item2}");
                }
                catch (KeyNotFoundException)
                {
                    diagnostics.Log(Diagnostics.LEVEL_WARNING,$"No file mapping found for timeseries ${ts.header.locationId}/${ts.header.parameterId}");
                }
            }
            WriteInputsUsingQualifier(runSettings, inputSeries, diagnostics);
        }

        private static Dictionary<Tuple<string, string>, Tuple<string, int>> ReadInputMapping(string inputMappingFn)
        {
            const string
                COL_LOCATION = "FewsLocationId",
                COL_PARAM = "FewsParameterId",
                COL_FILE = "ToCsvFileName",
                COL_COL_NUM = "ToColumnNumber";
            var mapping =
                new Dictionary<Tuple<string, string>, Tuple<string, int>>();
            using (var csvParser = new TextFieldParser(inputMappingFn))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                var columnNames = csvParser.ReadFields();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    string location  = fields[columnNames.IndexOf(COL_LOCATION)];
                    string parameter = fields[columnNames.IndexOf(COL_PARAM)];
                    string filename  = fields[columnNames.IndexOf(COL_FILE)];
                    int column = int.Parse(fields[columnNames.IndexOf(COL_COL_NUM)]);
                    mapping[new Tuple<string, string>(location,parameter)] = new Tuple<string, int>(filename, column);
                }
            }

            return mapping;
        }

        private static void WriteInputsUsingQualifier(RunComplexType runSettings,
            TimeSeriesComplexType[] inputSeries, Diagnostics diagnostics)
        {
            Func<TimeSeriesComplexType, bool> missingQualifiers = s => (s.SourceInputFile() == null) || (s.SourceColumnNumber() < 0);

            foreach (var timeSeriesComplexType in inputSeries.Where(missingQualifiers))
            {
                diagnostics.Log(Diagnostics.LEVEL_WARNING,$"Timeseries missing qualifier: {timeSeriesComplexType?.header?.locationId}/{timeSeriesComplexType?.header?.parameterId}");
            }

            var files = inputSeries.Where(s=>!missingQualifiers(s)).Select(s => s.SourceInputFile()).ToHashSet();

            foreach (var relFn in files)
            {
                var inputTS = inputSeries.Where(s => s.SourceInputFile() == relFn).ToList();
                var start = inputTS[0].header.startDate.DateTime;
                var end = inputTS[0].header.endDate.DateTime;
                var destFn = Path.IsPathRooted(relFn) ? relFn : runSettings.PathRelativeToProject(relFn);
                var proxy = new SourceTimeSeriesIOProxy(destFn);
                var existing = proxy.Load();
                diagnostics.Log(Diagnostics.LEVEL_INFO,$"Replacing {inputTS.Count}/{existing.Length} time series in {destFn}");
                var clipped = existing.Select(ts => ts.extract(start, end)).ToArray();
                foreach (var ts in inputTS)
                {
                    var sourceTS = FEWSPIProxy.ConvertTimeSeriesFromFEWS(ts);
                    var col = ts.SourceColumnNumber();
                    sourceTS.name = clipped[col].name;
                    clipped[col] = sourceTS;
                }
                proxy.Save(clipped);
            }
        }

        private static void WriteSourceInputsToFilePerParameter(RunComplexType runSettings, TimeSeriesComplexType[] inputSeries)
        {
            Array.Sort(inputSeries,
                ((x, y) =>
                {
                    int locationCompare = String.Compare(x.header.locationId, y.header.locationId,
                        StringComparison.Ordinal);
                    return locationCompare == 0
                        ? String.Compare(x.header.parameterId, y.header.parameterId,
                            StringComparison.Ordinal)
                        : locationCompare;
                }));

            Dictionary<string, CSVFileIO> outputSets = new Dictionary<string, CSVFileIO>();
            foreach (TimeSeriesComplexType fewsTS in inputSeries)
            {
                string destinationFn = runSettings.FilenameForTimeSeriesParameter(fewsTS.header.parameterId);
                if (!outputSets.ContainsKey(destinationFn))
                    outputSets[destinationFn] = new CSVFileIO();

                TimeSeries converted = FEWSPIProxy.ConvertTimeSeriesFromFEWS(fewsTS);
                outputSets[destinationFn].Use(converted.name, converted);
            }

            Directory.CreateDirectory(runSettings.workDir);
            foreach (var outputSet in outputSets)
                outputSet.Value.Save(runSettings.workDir + "\\" + outputSet.Key);
        }
    }
}
