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

            Console.WriteLine($"{sourceExe} {sourceCommand}");
            var result = RunSimulation(sourceCommand, sourceExe);

            foreach (string line in result.StandardErr.Split('\n'))
            {
                var trimmed = line.Trim();
                if(trimmed.Length>0)
                    diagnostics.Log(Diagnostics.LEVEL_WARNING, line);
            }

            if ((sourceOutput!=null)&&!File.Exists(sourceOutput))
            {
                File.WriteAllText(runSettings.workDir + "\\SourceErrors.txt", result.StandardErr);
                File.WriteAllText(runSettings.workDir + "\\SourceOutput.txt", result.StandardOut);
                throw new Exception(string.Format("Source run failed. No output file: {0}", sourceOutput));
            }

            diagnostics.Log(Diagnostics.LEVEL_INFO, "All done");
        }

        private static ProcessResult RunSimulation(string sourceCommand, string sourceExe)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = sourceExe;
                process.StartInfo.Arguments = sourceCommand;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) => {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            output.AppendLine(e.Data);
                            Console.Out.WriteLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            error.AppendLine(e.Data);
                            Console.Error.WriteLine(e.Data);
                        }
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    outputWaitHandle.WaitOne();
                    errorWaitHandle.WaitOne();
                    // Process completed. Check process.ExitCode here.
                    return new ProcessResult
                    {
                        ExitCode = process.ExitCode,
                        StandardOut = output.ToString(),
                        StandardErr = error.ToString()
                    };
                }
            }

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

            string sourceProject = runSettings.ProjectFile;

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

            sourceCommand += " " + String.Join(" ", runSettings.CommandLineArguments());
            //sourceCommand += RecorderCommands(runSettings);
            return sourceCommand;
        }

        //private static string RecorderCommands(RunComplexType runSettings)
        //{
        //    var recorderSet = runSettings.RecorderSet();
        //    if (recorderSet == null)
        //    {
        //        return "";
        //    }

        //    var lines = File.ReadAllLines(recorderSet);
        //    var output = new StringBuilder();
        //    foreach (var line in lines)
        //    {
        //        var command = line.Trim();
        //        if (command.Length == 0)
        //        {
        //            continue;
        //        }
        //        output.Append($" -r \"{command}\"");
        //    }
        //    return output.ToString();
        //}
    }

    internal class ProcessResult
    {
        public int ExitCode { get; set; }
        public string StandardOut { get; set; }
        public string StandardErr { get; set; }
    }
}
