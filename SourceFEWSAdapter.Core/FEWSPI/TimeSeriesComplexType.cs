using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceFEWSAdapter.FEWSPI
{
    public partial class TimeSeriesComplexType
    {
        public void AddQualifier(string qualifier)
        {
            if (header.qualifierId == null)
            {
                header.qualifierId = new[] { qualifier };
                return;
            }

            var arr = header.qualifierId;
            Array.Resize(ref arr,header.qualifierId.Length+1);
            arr[arr.Length - 1] = qualifier;
            header.qualifierId = arr;
        }
        public string SourceInputFile()
        {
            var fileQualifier = header.qualifierId?.FirstOrDefault(q => q.StartsWith("file:"));
            return fileQualifier?.Substring(5);
        }

        public int Length => @event?.Length ?? 0;

        public int SourceColumnNumber()
        {
            var columnQualifier = header.qualifierId?.FirstOrDefault(q => q.StartsWith("column:"));
            if (columnQualifier == null)
            {
                return -1;
            }

            return int.Parse(columnQualifier.Substring(7))-1;
        }
    }
}
