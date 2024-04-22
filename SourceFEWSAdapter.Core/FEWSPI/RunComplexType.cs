using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using SourceFEWSAdapter.Core;
using SourceFEWSAdapter.SourceService;
using TIME.ManagedExtensions;

namespace SourceFEWSAdapter.FEWSPI
{
    partial class RunComplexType
    {
        public string Property(string key)
        {
            return Properties(key).FirstOrDefault();
        }

        public string[] Properties(string key)
        {
            return (from property in properties.Items where GetKey(property) == key select GetValue(property)).ToArray();
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
                if (GetKey(property).IndexOf(marker + parameter, StringComparison.Ordinal) == 0)
                    return GetValue(property);
            }

            return Property(Keys.INPUT_FILE) ?? "defaultFile.csv";
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
                string timeStepProperty = Property(Keys.TIMESTEP);
                return timeStepProperty == null ? 86400 : Int32.Parse(timeStepProperty);
            }
        }

        public string ProjectFile
        {
            get
            {
                return Property(Keys.PROJECT_FILE);
            }
        }

        public string PathRelativeToProject(string relativePath)
        {
            var projectDir = Path.GetDirectoryName(ProjectFile);
            return Path.Combine(projectDir, relativePath);
        }

        public string ExecutionMode()
        {
            string serverAddress = FindServer();

            if (serverAddress == "")
                return "";

            return String.Format("-m Client -a {0}", serverAddress);
        }

        private string FindServer()
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
            int port = ConfiguredPort();
            if (port > 0)
            {
                return String.Format("net.tcp://localhost:{0}/eWater/Services/RiverSystemService", port);
            }

            string uri = Property(Keys.URI);
            if (uri != null)
                return uri;

            return "";
        }

        private int ConfiguredPort()
        {
            string portOption = Property(Keys.PORT);
            if (portOption == null) return -1;

            int basePort = int.Parse(portOption);

            return basePort + PortOffset();
        }

        private int PortOffset()
        {
            string offsetsFile = Property(Keys.USER_PORT_OFFSETS_FILE);
            if (offsetsFile == null)
                return 0;

            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\').Last();
            Console.WriteLine(userName);

            string[] entries = File.ReadAllLines(offsetsFile);
            foreach (string entry in entries)
            {
                string[] components = entry.Split(',');
                if (components[0].ToLower() == userName.ToLower())
                    return int.Parse(components[1]);
            }
                
            return 0;
        }

        public string SourceExeToUse()
        {
            return Property(Environment.Is64BitOperatingSystem ? Keys.SOURCE64 : Keys.SOURCE32);
        }

        private string[] FromRunSettingsOrParameters(string key, string parameterGroup = "ModelInput")
        {
            var fromRunSettings = Properties(key);
            var paramGroup = ParameterGroup(parameterGroup);
            if (paramGroup == null)
            {
                return fromRunSettings;
            }
            var fromParams = paramGroup.Items.Where(p => (p as ModelParameterComplexType).id == key)
                .Select(p => (p as ModelParameterComplexType).Item as string).ToHashSet();
            fromParams.AddRange(fromRunSettings);
            return fromParams.ToArray();
        }
        public string PluginFolder()
        {
            return FromRunSettingsOrParameters(Keys.PLUGIN_DIR).FirstOrDefault() ?? "";
        }

        public string[] Plugins()
        {
            var inRunSettings = Properties(Keys.PLUGIN_FN);
            var paramGroup = ParameterGroup("ModelInput");
            if (paramGroup == null)
            {
                return inRunSettings;
            }
            var fromParams = paramGroup.Items.Where(p=> (p as ModelParameterComplexType).id == Keys.PLUGIN_FN)
                .Select(p=>(p as ModelParameterComplexType).Item as string).ToHashSet();
            fromParams.AddRange(inRunSettings);
            var folder = PluginFolder();
            return fromParams.Select(p=>Path.IsPathRooted(p)?p:Path.Combine(folder,p)).ToHashSet().ToArray();
        }

        private IEnumerable<ModelParameterGroupComplexType> ParameterGroups()
        {
            return inputParameterFile.SelectMany(fn => FEWSPIProxy.ReadParametersFile(fn).group);
        }

        private ModelParameterGroupComplexType ParameterGroup(string name)
        {
            return ParameterGroups().FirstOrDefault(p => p.id == name);
        }

        public string InputSet()
        {
            var parameterGroups = ParameterGroups();
            foreach (var grp in parameterGroups)
            {
                foreach (ModelParameterComplexType item in grp.Items)
                {
                    if (item.id.ToLower() == "inputset")
                    {
                        return item.Item as string;
                    }
                }
            }

            return null;
        }

        public string ResCSVFile()
        {
            var filename = Property(Keys.OUTPUT_FILE);
            if ((filename == null)&&(outputTimeSeriesFile.Length>0))
            {
                var fi = new FileInfo(outputTimeSeriesFile[0]);
                filename = fi.FullName.Replace(fi.Extension, ".res.csv");
            }

            return filename;
        }
    }
}
