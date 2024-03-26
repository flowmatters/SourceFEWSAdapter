using System;
using System.Collections.Generic;
using System.IO;
using SourceFEWSAdapter.Core;
using SourceFEWSAdapter.FEWSPI;
using TIME.DataTypes;
using TIME.DataTypes.IO.CsvFileIo;

namespace SourceFEWSAdapter.Commands
{
    public static class PreAdapter
    {
        public static void Run(RunComplexType runSettings, Diagnostics diagnostics, string[] args)
        {
            var inputSeries = runSettings.AllInputSeries();

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

            diagnostics.Log(3, "All Done");
        }
    }
}
