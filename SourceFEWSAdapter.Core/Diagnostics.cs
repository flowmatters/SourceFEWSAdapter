﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SourceFEWSAdapter.FEWSPI;

namespace SourceFEWSAdapter.Core
{
    public class Diagnostics
    {
        public const int LEVEL_INFO = 3;
        public const int LEVEL_WARNING = 2;
        public const int LEVEL_ERROR = 1;
        public const int LEVEL_FATAL = 0;

        public Diagnostics(string path, string command, RunComplexType tzInfo)
        {
            Command = command;
            FilePath = path;

            if (File.Exists(FilePath))
            {
                diagnosticsDOM = FEWSPIProxy.ReadFromXML<DiagComplexType>(FilePath);
            }
            else
            {
                diagnosticsDOM = new DiagComplexType();
                diagnosticsDOM.CopyTimeZoneInfo(tzInfo);
                diagnosticsDOM.line = new LineComplexType[0];
            }
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
            Console.WriteLine(message);
        }

        public void Save()
        {
            FEWSPIProxy.WriteToXML<DiagComplexType>(FilePath, diagnosticsDOM);
        }

        private DiagComplexType diagnosticsDOM;
    }
}
