using System;
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
            TimeSeries[] results =
                (TimeSeries[])
                NonInteractiveIO.Load(runSettings.workDir + "\\" + runSettings.Property("SourceOutputFile"));
            diagnostics.Log(3,string.Format("Loaded {0} time series",results.Length));

            TimeSeriesCollectionComplexType fewsTimeSeriesCollection = TIMEProxy.FromTimeSeriesCollection(results,runSettings.timeZone);
            string outputFn = runSettings.outputTimeSeriesFile[0];

            FEWSPIProxy.WriteTimeSeriesCollection(outputFn, fewsTimeSeriesCollection);
            diagnostics.Log(3,string.Format("Written {0} time series to {1}",fewsTimeSeriesCollection.series.Length,outputFn));

            diagnostics.Log(3,"All Done");
        }
    }
}
