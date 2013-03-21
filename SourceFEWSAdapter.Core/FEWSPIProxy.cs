using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using SourceFEWSAdapter.FEWSPI;
using TIME.DataTypes;

namespace SourceFEWSAdapter.Core
{
    public class FEWSPIProxy
    {
        public static RunComplexType ReadRunFile(string path)
        {
            return ReadFromXML<RunComplexType>(path);
        }

        public static TimeSeriesCollectionComplexType ReadTimeSeries(string path)
        {
            return ReadFromXML<TimeSeriesCollectionComplexType>(path);
        }

        public static void WriteTimeSeriesCollection(string path, TimeSeriesCollectionComplexType collection)
        {
            WriteToXML(path,collection);
        }

        public static T ReadFromXML<T>(string path)
        {
            var xmlReader = new XmlSerializer(typeof(T));
            TextReader reader = new StreamReader(path);
            var result = (T)xmlReader.Deserialize(reader);
            reader.Close();

            return result;
        }

        public static void WriteToXML<T>(string path, T values)
        {
            var xmlWriter = new XmlSerializer(typeof(T));
            TextWriter textWriter = new StreamWriter(path);
            xmlWriter.Serialize(textWriter, values);
            textWriter.Close();
        }


        public static TimeSeries ConvertTimeSeriesFromFEWS(TimeSeriesComplexType fewsTS)
        {
            var header = fewsTS.header;
            DateTime tsStart = DateTimeComplexType.DateTimeFromPI(header.startDate);
            DateTime tsEnd = DateTimeComplexType.DateTimeFromPI(header.endDate);
            TimeStep step = TimeStepFromPI(header.timeStep);

            TimeSeries result = new TimeSeries(tsStart, tsEnd, step);
            result.name = header.parameterId + "_" + header.locationId;

            foreach (var e in fewsTS.@event)
            {
                var dt = DateTimeComplexType.Merge(e.date, e.time);
                result.setTime(dt,e.value);
            }

            return result;
        }

        public static TimeStep TimeStepFromPI(TimeStepComplexType timeStep)
        {
            if (timeStep.unit == timeStepUnitEnumStringType.second)
            {
                if (timeStep.multiplier == "3600")
                    return TimeStep.Hourly;
                if (timeStep.multiplier == "86400")
                    return TimeStep.Daily;
            }
            return null;
        }
    }
}
