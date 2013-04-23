using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceFEWSAdapter.Commands;
using SourceFEWSAdapter.Core;
using SourceFEWSAdapter.FEWSPI;

namespace SourceFEWSAdapter
{
    internal class Program
    {
        public static Dictionary<string, Action<RunComplexType, Diagnostics, string[]>> commands = new Dictionary
            <string, Action<RunComplexType, Diagnostics, string[]>>
            {
                {"preadapter", PreAdapter.Run},
                {"postadapter", PostAdapter.Run},
                {"simulation", SimulationAdapter.Run},
                {"probe", ProbeServer.Run}
            };

        public static int Main(string[] args)
        {
            Action<RunComplexType, Diagnostics, string[]> command = null;
            if (args.Length > 0)
            {
                string cmd = args[0];
                command = MatchCommand(cmd);
            }

            if (command == null)
            {
                ShowHelpText();
                return 0;
            }
            return LoggedExecution.Program(args, command);
        }

        private static readonly string[] HELP_TEXT = new string[]
            {
                "Usage:",
                "\tSourceFEWSAdapter <command> <command-args>",
                "",
                "Where <command> is one of:",
                "\tpreadapter, simulation, postadapter, probe",
                "",
                "preadapter runsettingsfile.xml",
                "\tConvert FEWS PI file to Source csv inputs based on settings in runsettingsfile.xml",
                "",
                "simulation runsettingsfile.xml",
                "\tRun Source (either standalone or on a Source server) with settings configured in runsettingsfile.xml",
                "",
                "postadapter runsettingsfile.xml",
                "\tConvert Source csv outputs to FEWS PI based on settings in runsettingsfile.xml",
                "",
                "probe serverendpoint",
                "\tTest for the existence of a live Source server at the given URI.",
                "\tExampe: SourceFEWSAdapter probe net.tcp://localhost:8765/eWater/Services/RiverSystemService",
                "",
                "For details on configuring FEWS and Source for use with the adapter (including what to include in the runsettingsfile.xml), see the documentation at:",
                "\thttps://github.com/flowmatters/SourceFEWSAdapter",
                "",
                "The SourceFEWSAdapter is supported by Joel Rahman (joel@flowmatters.com.au)",
                ""
            };

        private static void ShowHelpText()
        {
            foreach (var s in HELP_TEXT)
                Console.WriteLine(s);
        }

        private static Action<RunComplexType, Diagnostics, string[]> MatchCommand(string cmd)
        {
            cmd = cmd.ToLower();
            return (from command in commands
                    where command.Key.IndexOf(cmd, StringComparison.Ordinal) == 0 
                    select command.Value).FirstOrDefault();
        }
    }
}
