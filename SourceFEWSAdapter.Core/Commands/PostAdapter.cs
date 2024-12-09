using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CsvHelper;
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
            var sourceOutputFn = runSettings.ResCSVFile();
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

            //WriteResultsToPIXML(runSettings, diagnostics, results);
            IncrementalWriteResultsToPIXML(runSettings, diagnostics, results);

            diagnostics.Log(Diagnostics.LEVEL_INFO, "All Done");
        }

        private static void WriteResultsToPIXML(RunComplexType runSettings, Diagnostics diagnostics, TimeSeries[] results)
        {
            string forcedTimeStep = runSettings.Property(Keys.FORCED_TIMESTAMP);
            TimeSeriesCollectionComplexType fewsTimeSeriesCollection =
                TIMEProxy.FromTimeSeriesCollection(results, runSettings, forcedTimeStep);
            string outputFn = runSettings.outputTimeSeriesFile[0];

            FEWSPIProxy.WriteTimeSeriesCollection(outputFn, fewsTimeSeriesCollection);
            diagnostics.Log(Diagnostics.LEVEL_INFO,
                $"Written {fewsTimeSeriesCollection.series.Length} time series to {outputFn}");
        }

        private static void IncrementalWriteResultsToPIXML(RunComplexType runSettings, Diagnostics diagnostics,
            TimeSeries[] results)
        {
            string outputFn = runSettings.outputTimeSeriesFile[0];
            TextWriter textWriter = new StreamWriter(outputFn);

            XmlWriterSettings settings = new XmlWriterSettings
            {
                ConformanceLevel = ConformanceLevel.Document,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
                Indent = true
            };

            var xmlWriter = XmlWriter.Create(textWriter, settings);
            xmlWriter.WriteStartElement("TimeSeries", FEWS_NS);
            xmlWriter.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            xmlWriter.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");

            runSettings.WriteTimeZoneTag(xmlWriter);
            //textWriter.WriteLine(runSettings.TimeZoneTag());

            var serializer = new XmlSerializer(typeof(TimeSeriesComplexType),null, Type.EmptyTypes,new XmlRootAttribute("series"),FEWS_NS);
            string forcedTimeStep = runSettings.Property(Keys.FORCED_TIMESTAMP);
            foreach (var ts in results)
            {
                var fewsSeries = TIMEProxy.FromTimeSeries(ts, forcedTimeStep);
                serializer.Serialize(xmlWriter, fewsSeries);
            }

            xmlWriter.WriteEndElement();
            xmlWriter.Close();
            textWriter.Close();

            diagnostics.Log(Diagnostics.LEVEL_INFO, $"Written {results.Length} time series to {outputFn}");
        }

        private const string FEWS_NS = "http://www.wldelft.nl/fews/PI";
    }
}
