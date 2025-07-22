using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using SourceFEWSAdapter.Core;
using TIME.DataTypes;
using TIME.DataTypes.IO.CsvFileIo;
using TIME.Management;

namespace SourceFEWSAdapter
{
    internal class SourceTimeSeriesIOProxy
    {
        public string Filename { get; private set; }
        public AbstractCsvStyleFileIo IO {get; private set; }

        public List<string> Labels { get; private set; }

        public Diagnostics Diagnostics { get; private set; }
        public SourceTimeSeriesIOProxy(string fn, Diagnostics diag)
        {
            Filename = fn;
            Labels = new List<string>();
            IO = GetIO();
            Diagnostics = diag;
        }

        public TimeSeries[] Load()
        {
            if (IO == null)
            {
                var io = new CSVFileIO();
                io.Load(Filename);
                return io.DataSets.Select(d => d as TimeSeries).ToArray();
            }

            try
            {
                var data = new ArrayList();
                IO.Load(Filename, data, Labels);
                return data.OfType<TimeSeries>().ToArray();
            }
            catch
            {
                return (TimeSeries[])NonInteractiveIO.Load(Filename);
            }

        }

        public void Save(TimeSeries[] data)
        {
            if (IO == null)
            {
                NonInteractiveIO.Save(Filename, data);
                return;
            }
            try
            {
                using (var writer = new FileWriter(Filename))
                {
                    using (var sw = new StreamWriter(writer.Create(), Encoding.Default, 512000))
                        IO.Save(sw, new ArrayList(data), Labels);
                }
            }
            catch
            {
                NonInteractiveIO.Save(Filename, data);
                if (IO is ResultsCsvIoV1)
                {
                    Diagnostics?.Log(Diagnostics.LEVEL_INFO, $"Converting {Filename} to res-csv V1 to match original");
                    ConvertResCSVV3ToV1(Filename);
                }
            }
        }

        private AbstractCsvStyleFileIo GetIO()
        {
            using (var fp = new FileReader(Filename))
            {
                var styles = (from t in AssemblyManager.FindTypes(typeof(AbstractCsvStyleFileIo))
                    select Activator.CreateInstance(t) as AbstractCsvStyleFileIo).ToList();
                var firstValidStyle = styles.FirstOrDefault(item => item.DetectIfValid(fp));
                return firstValidStyle;
            }
        }
        public static void ConvertResCSVV3ToV1(string filename)
        {
            string[] data = null;
            using (var reader = new FileReader(filename))
            {
                using (var sr = new StreamReader(reader.Create()))
                {
                    data = sr.ReadToEnd().Split(new string[] { "\r\n", "\r", "\n" },StringSplitOptions.None);
                    data[0] = "File version,1";
                    for (int line = 1; line < data.Length; line++)
                    {
                        var txt = data[line];
                        data[line] = "";
                        if (txt == "EOM")
                        {
                            break;
                        }
                    }
                }
            }
            WriteLines(filename, data);
        }

        private static void WriteLines(string filename, string[] data)
        {
            data = data.Where(ln => ln.Length > 0).ToArray();

            using (var writer = new FileWriter(filename))
            {
                using (var sw = new StreamWriter(writer.Create()))
                {
                    foreach (var line in data)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
        }
    }
}
