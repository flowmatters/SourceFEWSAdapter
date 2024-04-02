using System;
using System.IO;
using SourceFEWSAdapter.Core;
using SourceFEWSAdapter.FEWSPI;
using TIME.DataTypes;
using TIME.Management;

namespace SourceFEWSAdapter.Commands
{
    static class PostAdapter
    {
        public static void Run(RunComplexType runSettings, Diagnostics diagnostics, string[] args)
        {
            var sourceOutputFn = runSettings.Property(Keys.OUTPUT_FILE);
            if (!Path.IsPathRooted(sourceOutputFn))
            {
                sourceOutputFn = runSettings.workDir + Path.PathSeparator + sourceOutputFn;
            }
            diagnostics.Log(Diagnostics.LEVEL_INFO,$"Loading results from {sourceOutputFn}");
            TimeSeries[] results =
                (TimeSeries[])
                NonInteractiveIO.Load(sourceOutputFn);

            if (results is null)
            {
                diagnostics.Log(Diagnostics.LEVEL_ERROR, $"No results!");
            }
            diagnostics.Log(Diagnostics.LEVEL_INFO, $"Loaded {results.Length} time series");

            string forcedTimeStep = runSettings.Property(Keys.FORCED_TIMESTAMP);
            TimeSeriesCollectionComplexType fewsTimeSeriesCollection = TIMEProxy.FromTimeSeriesCollection(results,runSettings,forcedTimeStep);
            string outputFn = runSettings.outputTimeSeriesFile[0];

            FEWSPIProxy.WriteTimeSeriesCollection(outputFn, fewsTimeSeriesCollection);
            diagnostics.Log(Diagnostics.LEVEL_INFO,
                $"Written {fewsTimeSeriesCollection.series.Length} time series to {outputFn}");

            diagnostics.Log(Diagnostics.LEVEL_INFO, "All Done");
        }
    }
}
