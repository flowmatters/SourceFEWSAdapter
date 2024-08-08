using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
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
        public SourceTimeSeriesIOProxy(string fn)
        {
            Filename = fn;
            Labels = new List<string>();
            IO = GetIO();
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

    }
}
