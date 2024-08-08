using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SourceFEWSAdapter.Core;
using SourceFEWSAdapter.FEWSPI;
using System.Threading;

namespace SourceFEWSAdapter.Commands
{
    public static class SimulationAdapter
    {
        public static void Run(RunComplexType runSettings, Diagnostics diagnostics, string[] args)
        {
            string sourceExe = runSettings.SourceExeToUse();

            var sourceCommand = BuildCommandLine(runSettings, diagnostics, out var sourceOutput);

            diagnostics.Log(Diagnostics.LEVEL_INFO, string.Format("Starting Source Run with command line {0}",sourceCommand));

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
            string errors = p.StandardError.ReadToEnd();
            p.WaitForExit();

            Console.WriteLine(output);
            Console.WriteLine(errors);
            foreach (string line in errors.Split('\n'))
                diagnostics.Log(Diagnostics.LEVEL_WARNING, line);

            if ((sourceOutput!=null)&&!File.Exists(sourceOutput))
            {
                File.WriteAllText(runSettings.workDir + "\\SourceErrors.txt", errors);
                File.WriteAllText(runSettings.workDir + "\\SourceOutput.txt", output);
                throw new Exception(string.Format("Source run failed. No output file: {0}", sourceOutput));
            }

            diagnostics.Log(Diagnostics.LEVEL_INFO, "All done");
        }

        private static string BuildCommandLine(RunComplexType runSettings, Diagnostics diagnostics, out string sourceOutput)
        {
            var start = DateTimeComplexType.DateTimeFromPI(runSettings.startDateTime);
            var end = DateTimeComplexType.DateTimeFromPI(runSettings.endDateTime);

//            string dateFormat = "dd/MM/yyyy";
            var dtf = Thread.CurrentThread.CurrentCulture.DateTimeFormat;

            string dateFormat = dtf.ShortDatePattern; // "MM/dd/yyyy";
            if (runSettings.TimeStepInSeconds != 86400)
                dateFormat += " HH:mm:ss";

            string sourceProject = runSettings.Property(Keys.PROJECT_FILE);

            sourceOutput = runSettings.ResCSVFile();

            if (File.Exists(sourceOutput))
            {
                diagnostics.Log(Diagnostics.LEVEL_INFO, string.Format("Deleting old source output file {0}", sourceOutput));
                File.Delete(sourceOutput);
            }

            string mode = runSettings.ExecutionMode();

            if (mode == "")
            {
                if (runSettings.ConfiguredServer() != "")
                    diagnostics.Log(Diagnostics.LEVEL_INFO,
                        string.Format("Running locally because configured server ({0}) is unavailable",
                            runSettings.ConfiguredServer()));
            }
            else
                sourceProject = "";

            var sourceCommand = $"-p \"{sourceProject};;{start.ToString(dateFormat)};{end.ToString(dateFormat)}\" {mode}";
            if (sourceOutput == null)
            {
                diagnostics.Log(Diagnostics.LEVEL_WARNING, "No output file configured");
                sourceCommand += "--resultsOutputMode NoOutput";
            }
            else
            {
                sourceCommand += $" -o {sourceOutput}";
            }

            var inputSet = runSettings.InputSet();
            if (inputSet != null)
            {
                sourceCommand += $" --inputset \"{inputSet}\"";
            }

            var plugins = runSettings.Plugins();
            foreach (var plugin in plugins)
            {
                sourceCommand += $" -l \"{plugin}\"";
            }

            return sourceCommand;
        }
    }
}
