using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SourceFEWSAdapter
{
    public static class Keys
    {
        public const string PLUGIN_DIR = "PluginDir";
        public const string PLUGIN_FN = "Plugin";
        public const string PROJECT_FILE = "RSPROJ";
        public const string PROJECT_FOLDER = "RSPROJ_Folder";
        public const string OUTPUT_FILE = "SourceOutputFile";
        public const string SOURCE64 = "Source_64EXE";
        public const string SOURCE32 = "Source_32EXE";
        public const string TIMESTEP = "TimeStep";
        public const string INPUT_FILE = "SourceInputFile";
        public const string FORCED_TIMESTAMP = "ForceTimeStamp";
        public const string USER_PORT_OFFSETS_FILE = "UserPortOffsets";
        public const string PORT = "Port";
        public const string URI = "URI";
        public const string INPUT_SET = "InputSet";
        public const string INPUT_MAPPING_FILE = "InputMappingFile";
        //public const string RECORDER_SET = "RecorderSetFile";
        public static string HelpText()
        {
            var t = typeof(Keys);
            var labels = t.GetFields(BindingFlags.Static|BindingFlags.Public).Select(f => f.GetValue(null)).ToArray();
            return "\t" + String.Join("\n\t",labels);
        }
    }
}
