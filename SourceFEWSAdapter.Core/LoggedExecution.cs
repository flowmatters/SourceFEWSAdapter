using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SourceFEWSAdapter.FEWSPI;
using TIME.DataTypes;
using TIME.DataTypes.IO.CsvFileIo;

namespace SourceFEWSAdapter.Core
{
    public class LoggedExecution
    {
        public static int Program(string[] args, Action<RunComplexType, Diagnostics, string[]> programAction )
        {
            bool exception = false;
            var runSettings = FEWSPIProxy.ReadRunFile(args[1]);
            var diagnostics = new Diagnostics(runSettings.outputDiagnosticFile,args[0]);

            try
            {
                programAction(runSettings, diagnostics, args);
            }
            catch (Exception e)
            {
                diagnostics.Log(1, string.Format("Exception ({0}: {1}", e.GetType(), e.Message));
                exception = true;
            }
            finally
            {
                diagnostics.Save();
            }

            return exception ? 1 : 0;
        }
    }
}
