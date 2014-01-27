using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    public class Date
    {
        [Hidden]
        private readonly static String tempSResult = "";

        [Hidden]
        private readonly static long UTCBase = new DateTime(1970, 1, 1).Ticks;

        [Hidden]
        private readonly static string[] month = new[] { "Jun", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        [Hidden]
        private DateTime host;
        [Hidden]
        private bool error = false;

        public Date()
        {
            host = DateTime.Now;
        }

        public Date(JSObject args)
        {
            try
            {
                host = new DateTime((long)Tools.JSObjectToDouble(args.GetField("0", true, false)) + UTCBase);
            }
            catch
            {
                error = true;
            }
        }

        public double valueOf()
        {
            if (error)
                return double.NaN;
            var res = host.Ticks - UTCBase;
            return res;
        }

        public JSObject toString()
        {
            tempSResult.oValue = ToString();
            return tempSResult;
        }

        public override string ToString()
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