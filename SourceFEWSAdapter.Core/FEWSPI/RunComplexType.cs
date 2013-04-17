using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using SourceFEWSAdapter.Core;
using SourceFEWSAdapter.SourceService;

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

            return Property("SourceInputFile") ?? "defaultFile.csv";
        }

        public TimeSeriesComplexType[] AllInputSeries()
        {
            var result = new List<TimeSeriesComplexType>();

            foreach (string fn in inputTimeSeriesFile)
                result.AddRange(FEWSPIProxy.ReadTimeSeries(fn).series);
            
            return result.ToArray();
        }

        public int TimeStepInSeconds
        {
            get
            {
                string timeStepProperty = Property("TimeStep");
                return timeStepProperty == null ? 86400 : int.Parse(timeStepProperty);
            }
        }

        public string ExecutionMode()
        {
            string serverAddress = FinderServer();

            if (serverAddress == "")
                return "";

            return String.Format("-m Client -a {0}", serverAddress);
        }

        private string FinderServer()
        {
            string configuredServer = ConfiguredServer();

            if (configuredServer != "")
            {
                if (!SourceServiceUtils.SourceServerExists(configuredServer))
                    return "";
            }

            return configuredServer;
        }

        public string ConfiguredServer()
        {
            string port = Property("Port");
            if (port != null)
                return String.Format("net.tcp://localhost:{0}/eWater/Services/RiverSystemService", port);

            string uri = Property("URI");
            if (uri != null)
                return uri;

            return "";
        }
    }
}
