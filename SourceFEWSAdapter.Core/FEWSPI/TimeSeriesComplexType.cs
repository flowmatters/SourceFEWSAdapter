using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceFEWSAdapter.FEWSPI
{
    public partial class TimeSeriesComplexType
    {

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
