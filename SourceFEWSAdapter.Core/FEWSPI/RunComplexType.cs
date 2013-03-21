using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceFEWSAdapter.Core;

namespace SourceFEWSAdapter.FEWSPI
{
    partial class RunComplexType
    {
        public string Property(string key)
        {
            return (from property in properties.Items where GetKey(property) == key select GetValue(property)).FirstOrDefault();
        }

        private string GetKey(object propertyItem)
        {
            StringPropertyComplexType stringProp = propertyItem as StringPropertyComplexType;
            if (stringProp != null)
                return stringProp.key;
            IntPropertyComplexType intProp = propertyItem as IntPropertyComplexType;
            if (intProp != null)
                return intProp.key;
            FloatPropertyComplexType floatProp = propertyItem as FloatPropertyComplexType;
            if (floatProp != null)
                return floatProp.key;
            BoolPropertyComplexType boolProp = propertyItem as BoolPropertyComplexType;
            if (boolProp != null)
                return boolProp.key;
            return null;
        }

        private string GetValue(object propertyItem)
        {
            StringPropertyComplexType stringProp = propertyItem as StringPropertyComplexType;
            if (stringProp != null)
                return stringProp.value;
            IntPropertyComplexType intProp = propertyItem as IntPropertyComplexType;
            if (intProp != null)
                return intProp.value.ToString();
            FloatPropertyComplexType floatProp = propertyItem as FloatPropertyComplexType;
            if (floatProp != null)
                return floatProp.value.ToString();
            BoolPropertyComplexType boolProp = propertyItem as BoolPropertyComplexType;
            if (boolProp != null)
                return boolProp.value.ToString();
            return null;
        }

        public string FilenameForTimeSeriesParameter(string parameter)
        {
            string marker = "ExpectedFile_";
            foreach (var property in properties.Items)
            {
                if (GetKey(property).IndexOf(marker + parameter, System.StringComparison.Ordinal) == 0)
                    return GetValue(property);
            }
            return "defaultFile.csv";
        }

        public TimeSeriesComplexType[] AllInputSeries()
        {
            var result = new List<TimeSeriesComplexType>();

            foreach (string fn in inputTimeSeriesFile)
                result.AddRange(FEWSPIProxy.ReadTimeSeries(fn).series);
            
            return result.ToArray();
        }
    }
}
