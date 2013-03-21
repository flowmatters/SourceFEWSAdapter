using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceFEWSAdapter.Commands;
using SourceFEWSAdapter.Core;
using SourceFEWSAdapter.FEWSPI;

namespace SourceFEWSAdapter
{
    class Program
    {
        public static Dictionary<string, Action<RunComplexType, Diagnostics, string[]>> commands = new Dictionary
            <string, Action<RunComplexType, Diagnostics, string[]>>
            {
                {"preadapter", PreAdapter.Run},
                {"postadapter", PostAdapter.Run},
                {"simulation",SimulationAdapter.Run}
            };

        public static int Main(string[] args)
        {
            string cmd = args[0];
            Action<RunComplexType, Diagnostics, string[]> command = MatchCommand(cmd);
    
            return LoggedExecution.Program(args,command);
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
