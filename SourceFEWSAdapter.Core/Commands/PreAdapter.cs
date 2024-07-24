using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using System.Reflection.Emit;
using System.Text;
using SourceFEWSAdapter.Core;
using SourceFEWSAdapter.FEWSPI;
using TIME.DataTypes;
using TIME.DataTypes.IO.CsvFileIo;
using TIME.Management;

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

            var haveQualifiers = inputSeries.All(s => (s.SourceInputFile() != null) && (s.SourceColumnNumber() >= 0));
            if (haveQualifiers)
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

        private static void WriteInputsUsingQualifier(RunComplexType runSettings,
            TimeSeriesComplexType[] inputSeries, Diagnostics diagnostics)
        {
            var files = inputSeries.Select(s => s.SourceInputFile()).ToHashSet();

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
                foreach (var ts in inputSeries)
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
