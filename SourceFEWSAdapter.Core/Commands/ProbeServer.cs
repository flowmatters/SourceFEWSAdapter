using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceFEWSAdapter.Core;
using SourceFEWSAdapter.FEWSPI;

namespace SourceFEWSAdapter.Commands
{
    static class ProbeServer
    {
        public static void Run(RunComplexType runSettings, Diagnostics diagnostics, string[] args)
        {
            string serverAddress = args[1];
            bool exists = SourceServiceUtils.SourceServerExists(serverAddress);
            Console.WriteLine(exists ? "OK: Server Running at {0}" : "NO: Server Not Running at {0}", serverAddress);
        }
    }
}
