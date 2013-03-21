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
        private const double TimeZone = 10.0;

        public static TimeSeriesCollectionComplexType FromTimeSeriesCollection(ICollection<TimeSeries> collection)
        {
            TimeSeriesCollectionComplexType result = new TimeSeriesCollectionComplexType();
            IEnumerable<TimeSeriesComplexType> piCollection = collection.Select(FromTimeSeries);
            result.series = piCollection.ToArray();
            result.timeZone = TimeZone;
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
            return NameComponents(name)[2];
        }

        private static string LocationName(string name)
        {
            return NameComponents(name)[1];
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
                    multiplier = "86400",
                    unit = timeStepUnitEnumStringType.second
                };
            //return new TimeStepComplexType
            //    {
            //        unit = timeStepBaseConversion[timeStep],
            //        multiplier = "1"
            //    };
        }

        private static string PIUnits(Unit units)
        {
            return units.SIUnits;
        }
    }
}
