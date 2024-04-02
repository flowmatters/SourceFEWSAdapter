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
            RunComplexType runSettings = null;
            Diagnostics diagnostics = null;
            if (File.Exists(args[1]))
            {
                runSettings = FEWSPIProxy.ReadRunFile(args[1]);
                diagnostics = new Diagnostics(runSettings.outputDiagnosticFile, args[0]);
            }

            try
            {
                programAction(runSettings, diagnostics, args);
            }
            catch (Exception e)
            {
                diagnostics.Log(Diagnostics.LEVEL_ERROR, string.Format("Exception ({0}: {1}", e.GetType(), e.Message));
                foreach (string s in e.StackTrace.Split('\n'))
                {
                    diagnostics.Log(Diagnostics.LEVEL_ERROR,s);
                }
                exception = true;
            }
            finally
            {
                if(diagnostics!=null) diagnostics.Save();
            }

            return exception ? 1 : 0;
        }
    }
}
