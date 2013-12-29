using System;

namespace NiL.JS.Core.BaseTypes
{
    internal class Date
    {
        private readonly static string[] month = new[] { "Jun", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        private DateTime host = DateTime.Now;

        public Date()
        {

        }

        public long valueOf()
        {
            var res = host.Ticks - new DateTime(1970, 1, 1).Ticks;
            return res / 10000;
        }

        public string toString()
        {
            var lt = host.ToLongTimeString();
            var offset = TimeZone.CurrentTimeZone.GetUtcOffset(host);
            var res = host.DayOfWeek.ToString().Substring(0, 3) + " "
                + month[host.Month - 1] + " " + host.Day + " " + host.Year + " " + lt
                + " GMT+" + offset.Hours.ToString("00") + offset.Minutes.ToString("00") + " (" + TimeZone.CurrentTimeZone.DaylightName + ")";
            return res;
        }
    }
}