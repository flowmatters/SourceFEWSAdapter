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
        public static TimeSeriesCollectionComplexType FromTimeSeriesCollection(ICollection<TimeSeries> collection, RunComplexType run,string forcedTimeStamp)
        {
            TimeSeriesCollectionComplexType result = new TimeSeriesCollectionComplexType();
            IEnumerable<TimeSeriesComplexType> piCollection = collection.Select(series => FromTimeSeries(series,forcedTimeStamp));
            result.series = piCollection.ToArray();
            result.CopyTimeZoneInfo(run);
            return result;
        }

        public static TimeSeriesComplexType FromTimeSeries(TimeSeries ts,string forcedTimeStamp)
        {
            TimeSeriesComplexType result = new TimeSeriesComplexType( );

            string locationName = LocationName(ts.name);
            result.header = new HeaderComplexType
                {
                    type = timeSeriesType.mean,
                    locationId = locationName,
                    //stationName = locationName,
                    startDate = new DateTimeComplexType {DateTime = MergeDT(ts.Start,forcedTimeStamp)},
                    endDate = new DateTimeComplexType {DateTime = MergeDT(ts.End,forcedTimeStamp)},
                    missVal = ts.NullValue,
                    units = ts.units.SIUnits,
                    timeStep = PITimeStep(ts.timeStep),
                    parameterId = ParameterName(ts.name)
                };

            IList<EventComplexType> events = new List<EventComplexType>();
            for (int i = 0; i < ts.Count; i++)
            {
                var fewsDT = new DateTimeComplexType {DateTime = MergeDT(ts.timeForItem(i),forcedTimeStamp)};
                double value = ts[i];
                if (Double.IsInfinity(value) || Double.IsNaN(value))
                {
                    value = ts.NullValue;
                }
                events.Add( new EventComplexType
                    {
                        date = fewsDT.date,
                        time = fewsDT.time,
                        flag = 2,
                        flagSpecified = true,
                        value = value,
                        valueSpecified = true
                    });
            }

            result.@event = events.ToArray();
            return result;

        }

        private static DateTime MergeDT(DateTime original, string forcedTimeStamp)
        {
            if(forcedTimeStamp==null)
                return original;
            string[] bits = forcedTimeStamp.Split(':');
            int hour = int.Parse(bits[0]);
            int minute = int.Parse(bits[1]);
            return new DateTime(original.Year,original.Month,original.Day,hour,minute,0);
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
            return name.Trim('"').Trim().Split('\\');
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

        //private static string PIUnits(TimeSeries ts)
        //{
        //    Unit units = ts.units;
        //    if (Unit.PredefinedUnit(CommonUnits.MLPerDay).Equals(units))
        //        return "MLD";

        //    if (Unit.PredefinedUnit(CommonUnits.megaLitre).Equals(units))
        //        return "ML_"+ts.timeStep.Name;

        //    if (ts.timeStep.Equals(TimeStep.Daily)) return "MLD";

        //    if (ts.timeStep.Equals(TimeStep.Hourly)) return "MLH";

        //    return units.Name;
        //}
    }
}
