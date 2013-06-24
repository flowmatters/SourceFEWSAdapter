using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SourceFEWSAdapter.FEWSPI;
using TIME.Core;
using TIME.DataTypes;

namespace SourceFEWSAdapter.Core
{
    public class TIMEProxy
    {
        public static TimeSeriesCollectionComplexType FromTimeSeriesCollection(ICollection<TimeSeries> collection, double timeZone)
        {
            TimeSeriesCollectionComplexType result = new TimeSeriesCollectionComplexType();
            IEnumerable<TimeSeriesComplexType> piCollection = collection.Select(FromTimeSeries);
            result.series = piCollection.ToArray();
            result.timeZone = timeZone;
            return result;
        }

        public static TimeSeriesComplexType FromTimeSeries(TimeSeries ts)
        {
            TimeSeriesComplexType result = new TimeSeriesComplexType( );

            string locationName = LocationName(ts.name);

            result.header = new HeaderComplexType
                {
                    type = timeSeriesType.mean,
                    locationId = locationName,
                    //stationName = locationName,
                    startDate = new DateTimeComplexType {DateTime = ts.Start},
                    endDate = new DateTimeComplexType {DateTime = ts.End},
                    missVal = ts.NullValue,
                    units = "MLD",//"cumecs", //PIUnits(ts.units),
                    timeStep = PITimeStep(ts.timeStep),
                    parameterId = ParameterName(ts.name)
                };

            IList<EventComplexType> events = new List<EventComplexType>();
            for (int i = 0; i < ts.count(); i++)
            {
                var fewsDT = new DateTimeComplexType {DateTime = ts.timeForItem(i)};
                events.Add( new EventComplexType
                    {
                        date = fewsDT.date,
                        time = fewsDT.time,
                        flag = 2,
                        flagSpecified = true,
                        value = ts[i]
                    });
            }

            result.@event = events.ToArray();
            return result;

        }

        private static string ParameterName(string name)
        {
            string[] components = NameComponents(name);
            if (components.Last() == "Flow")
                return components[components.Length - 2];
            return NameComponents(name).Last();//>3?NameComponents(name)[2]:name;
        }

        private static string LocationName(string name)
        {
            return NameComponents(name).Length>1? NameComponents(name)[1]:name;
        }

        private static string[] NameComponents(string name)
        {
            return name.Split('\\');
        }

        private static BiDictionary<TimeStep, timeStepUnitEnumStringType> timeStepBaseConversion = new BiDictionary<TimeStep, timeStepUnitEnumStringType>()
            {
                {TimeStep.Hourly,timeStepUnitEnumStringType.hour},
                {TimeStep.Daily,timeStepUnitEnumStringType.day},
            };

        private static TimeStepComplexType PITimeStep(TimeStep timeStep)
        {
            return new TimeStepComplexType
                {
                    multiplier = TimeStepInSeconds(timeStep).ToString(),
                    unit = timeStepUnitEnumStringType.second
                };
        }

        private static int TimeStepInSeconds(TimeStep timeStep)
        {
            return (int) timeStep.GetTimeSpan().TotalSeconds;
        }

        private static string PIUnits(Unit units)
        {
            return units.SIUnits;
        }
    }
}
