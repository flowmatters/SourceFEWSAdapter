using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SourceFEWSAdapter.FEWSPI;

namespace SourceFEWSAdapter.Core
{
    public class Diagnostics
    {
        public Diagnostics(string path, string command)
        {
            Command = command;
            FilePath = path;
            diagnosticsDOM = new DiagComplexType();
            diagnosticsDOM.line = new LineComplexType[0];
        }

        private string Command;

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            private set
            { 
                _filePath = value;
            }
        }

        public void Log(int level, string message)
        {
            LineComplexType line = new LineComplexType();
            line.level = (sbyte) level;
            line.description = string.Format("SOURCE {0}: {1}",Command, message);
            diagnosticsDOM.line = diagnosticsDOM.line.Concat(new LineComplexType[] {line}).ToArray();
        }

        public void Save()
        {
            FEWSPIProxy.WriteToXML<DiagComplexType>(FilePath, diagnosticsDOM);
        }

        private DiagComplexType diagnosticsDOM;
    }
}
