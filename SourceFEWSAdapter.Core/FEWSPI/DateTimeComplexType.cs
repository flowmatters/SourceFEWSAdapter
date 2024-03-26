using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SourceFEWSAdapter.FEWSPI
{
    partial class DateTimeComplexType
    {
        private DateTime? _dateTime;

        [System.Xml.Serialization.XmlIgnore]
        public DateTime DateTime
        {
            get
            {
                if (_dateTime == null)
                    _dateTime = Merge(date, time);

                return _dateTime.Value;
            }
            set
            {
                _dateTime = value;
                date = _dateTime.Value;
                time = _dateTime.Value;//.ToString("HH:mm:ss");
            }
        }

        public static DateTime DateTimeFromPI(DateTimeComplexType pi)
        {
            return pi.DateTime; // Merge(pi.date, pi.time);
        }

        public static DateTime Merge(DateTime date, DateTime time)
        {
            //int[] components = time.Split(':').Select(int.Parse).ToArray();

            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute,
                                time.Second);

        }
    }
}
