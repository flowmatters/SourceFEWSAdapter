using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceFEWSAdapter.FEWSPI
{
    public enum TimeZoneItemEnum
    {
        timeZone,
        daylightSavingObservingTimeZone
    }
    public class TimeZoned
    {
        private object itemField;
        private TimeZoneItemEnum itemElementNameField;

        [System.Xml.Serialization.XmlElementAttribute("timeZone", typeof(double))]
        [System.Xml.Serialization.XmlElementAttribute("daylightSavingObservingTimeZone", typeof(DaylightSavingObservedTimeZoneEnumStringType))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("TimeZoneItemElementName")]
        public object TimeZoneItem
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public TimeZoneItemEnum TimeZoneItemElementName
        {
            get
            {
                return this.itemElementNameField;
            }
            set
            {
                this.itemElementNameField = value;
            }
        }

        public void CopyTimeZoneInfo(TimeZoned other)
        {
            TimeZoneItem = other.TimeZoneItem;
            TimeZoneItemElementName = other.TimeZoneItemElementName;
        }

        public void CopyTimeZoneInfo(RunComplexType run)
        {
            CopyTimeZoneInfo(run.timeZone,run.daylightSavingObservingTimeZone);
        }

        public void CopyTimeZoneInfo(double fixedTimeZone, DaylightSavingObservedTimeZoneEnumStringType dstZone)
        {
            if (dstZone == DaylightSavingObservedTimeZoneEnumStringType.None)
            {
                TimeZoneItem = fixedTimeZone;
                TimeZoneItemElementName = TimeZoneItemEnum.timeZone;
            }
            else
            {
                TimeZoneItem = dstZone;
                TimeZoneItemElementName = TimeZoneItemEnum.daylightSavingObservingTimeZone;
            }
        }
    }

    public partial class DiagComplexType : TimeZoned { }

    public partial class EventComplexType : TimeZoned { }

    public partial class TaskRunsComplexType : TimeZoned { }

    public partial class ProfilesComplexType : TimeZoned { }

    public partial class MapStacksComplexType : TimeZoned { }

    public partial class PolygonsComplexType : TimeZoned { }

    public partial class StateComplexType : TimeZoned { }

    public partial class TimeSeriesCollectionComplexType : TimeZoned { }

    public partial class TableComplexType : TimeZoned { }

}
