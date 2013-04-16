using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SourceFEWSAdapter.Core;
using SourceFEWSAdapter.FEWSPI;

namespace SourceFEWSAdapter.Commands
{
    public static class SimulationAdapter
    {
        public static void Run(RunComplexType runSettings, Diagnostics diagnostics, string[] args)
        {
            var start = DateTimeComplexType.DateTimeFromPI(runSettings.startDateTime);
            var end = DateTimeComplexType.DateTimeFromPI(runSettings.endDateTime);
            
            string dateFormat = "dd/MM/yyyy";
            if (runSettings.TimeStepInSeconds != 86400)
                dateFormat += " HH:mm:ss";

            string sourceExe = args[2];
            string sourceProject = args.Length>3?args[3]:"";

            string sourceOutput = runSettings.Property("SourceOutputFile");
            
            if (File.Exists(sourceOutput))
            {
                diagnostics.Log(3,string.Format("Deleting old source output file {0}",sourceOutput));
                File.Delete(sourceOutput);
            }

            string mode = runSettings.executionMode();

            if (mode != "")
                sourceProject = "";

            string sourceCommand = string.Format("-p \"{0};;{1};{2}\" {4} -o {3}",
                                                 sourceProject, start.ToString(dateFormat),
                                                 end.ToString(dateFormat), sourceOutput,mode);
            diagnostics.Log(3, string.Format("Starting Source Run with command line {0}",sourceCommand));

            ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = sourceCommand,
                    CreateNoWindow = true,
                    FileName = sourceExe,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
            Process p = Process.Start(startInfo);
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            string errors = p.StandardError.ReadToEnd();

            if (!File.Exists(sourceOutput))
            {
                File.WriteAllText(runSettings.workDir+"\\SourceErrors.txt",errors);
                File.WriteAllText(runSettings.workDir+"\\SourceOutput.txt",output);
                foreach (string line in errors.Split('\n'))
                    diagnostics.Log(1, line);
                throw new Exception(string.Format("Source run failed. No output file: {0}", sourceOutput));
            }

            diagnostics.Log(3,"All done");
        }
    }
}
