using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using SourceFEWSAdapter.Core;
using SourceFEWSAdapter.FEWSPI;

namespace SourceFEWSAdapter.Commands
{
    public  static class LoadPluginAdapter
    {
        public static void Run(RunComplexType runSettings, Diagnostics diagnostics, string[] args)
        {
            string sourceExe = runSettings.SourceExeToUse().Replace('/', '\\');
            string registryName = "SourceFromFEWSPlugins";

            // Set RegistryPath for sourceExe to X
            RegistryKey timeKey = Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("TIME");
            RegistryKey registryPaths = timeKey.CreateSubKey("RegistryPaths");
            registryPaths.SetValue(sourceExe, registryName);
            registryPaths.SetValue(sourceExe.Replace("CommandLine","Forms"),registryName);

            RegistryKey pluginsKey = timeKey.CreateSubKey(registryName);
            pluginsKey.ClearValues();

            int i = 0;
            foreach (string plugin in Directory.EnumerateFiles(runSettings.Property(Keys.PLUGIN_DIR), "*.dll"))
            {
                i++;
                pluginsKey.SetValue(String.Format("Assembly{0}",i),plugin);
            }
        }
    }

    public static class RegistryKeyExtensions
    {
        public static void ClearValues(this RegistryKey key)
        {
            foreach( string name in key.GetValueNames() )
                key.DeleteValue(name);
        }
    }
}
