using System;
using System.Globalization;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public sealed class Date
    {
        [Hidden]
        private readonly static long UTCBase = new DateTime(1970, 1, 1).Ticks;

        [Hidden]
        private readonly static string[] month = new[] { "Jun", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        [Hidden]
        private DateTime host;
        [Hidden]
        private bool error = false;

        [DoNotEnumerate]
        public Date()
        {
            host = DateTime.Now;
        }

        [DoNotEnumerate]
        public Date(JSObject args)
        {
            try
            {
                if (args.GetMember("length").iValue == 1)
                {
                    var arg = args.GetMember("0");
                    switch (arg.valueType)
                    {
                        case JSObjectType.Int:
                        case JSObjectType.Bool:
                        case JSObjectType.Double:
                            {
                                var d = Tools.JSObjectToDouble(arg);
                                if (double.IsNaN(d) || double.IsInfinity(d))
                                {
                                    error = true;
                                    break;
                                }
                                host = new DateTime((long)d * 10000 + UTCBase + TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Ticks);
                                break;
                            }
                        case JSObjectType.String:
                            {
                                host = DateTime.Parse(arg.ToString(), CultureInfo.CurrentCulture);
                                break;
                            }
                    }
                }
                else
                {
                    int y = Tools.JSObjectToInt(args.GetMember("0"), 1);
                    int m = Tools.JSObjectToInt(args.GetMember("1"), 0) + 1;
                    int d = Tools.JSObjectToInt(args.GetMember("2"), 1);
                    int h = Tools.JSObjectToInt(args.GetMember("3"), 0);
                    int n = Tools.JSObjectToInt(args.GetMember("4"), 0);
                    int s = Tools.JSObjectToInt(args.GetMember("5"), 0);
                    host = new System.DateTime(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Ticks);
                    host = host.AddYears(y - host.Year);
                    host = host.AddMonths(m - host.Month);
                    host = host.AddDays(d - host.Day);
                    host = host.AddHours(h - host.Hour);
                    host = host.AddMinutes(n - host.Minute);
                    host = host.AddSeconds(s - host.Second);
                }
            }
            catch
            {
                error = true;
            }
        }

        [DoNotEnumerate]
        public Number valueOf()
        {
            if (error)
                return double.NaN;
            var res = getTime();
            return res;
        }

        [DoNotEnumerate]
        public Number getTime()
        {
            if (error)
                return double.NaN;
            var res = (host.Ticks - UTCBase - TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Ticks) / 10000;
            return res;
        }

        [DoNotEnumerate]
        public Number getTimezoneOffset()
        {
            if (error)
                return double.NaN;
            var res = (TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Ticks) / 10000;
            return res;
        }

        [DoNotEnumerate]
        public Number getYear()
        {
            return host.Year - 1900;
        }

        [DoNotEnumerate]
        public Number getFullYear()
        {
            return host.Year;
        }

        [DoNotEnumerate]
        public Number getUTCFullYear()
        {
            return host.Year;
        }

        [DoNotEnumerate]
        public Number getMonth()
        {
            return host.Month - 1;
        }

        [DoNotEnumerate]
        public Number getUTCMonth()
        {
            return host.Month - 1;
        }

        [DoNotEnumerate]
        public Number getDate()
        {
            return host.Day;
        }

        [DoNotEnumerate]
        public Number getUTCDate()
        {
            return host.Day;
        }

        [DoNotEnumerate]
        public Number getDay()
        {
            return (int)host.DayOfWeek;
        }

        [DoNotEnumerate]
        public Number getUTCDay()
        {
            return (int)host.DayOfWeek;
        }

        [DoNotEnumerate]
        public Number getHours()
        {
            return host.Hour;
        }

        [DoNotEnumerate]
        public Number getUTCHours()
        {
            return host.Hour;
        }

        [DoNotEnumerate]
        public Number getMinutes()
        {
            return host.Minute;
        }

        [DoNotEnumerate]
        public Number getUTCMinutes()
        {
            return host.Minute;
        }

        [DoNotEnumerate]
        public Number getSeconds()
        {
            return host.Second;
        }

        [DoNotEnumerate]
        public Number getUTCSeconds()
        {
            return host.Second;
        }

        [DoNotEnumerate]
        public Number getMilliseconds()
        {
            return host.Millisecond;
        }

        [DoNotEnumerate]
        public Number getUTCMilliseconds()
        {
            return host.Millisecond;
        }

        [DoNotEnumerate]
        public Number setTime(int time)
        {
            host = new DateTime(time * 10000 + UTCBase);
            return time;
        }

        [DoNotEnumerate]
        public Number setMilliseconds(int time)
        {
            host = host.AddMilliseconds(time - host.Millisecond);
            return time;
        }

        [DoNotEnumerate]
        public Number setUTCMilliseconds(int time)
        {
            host = host.AddMilliseconds(time - host.Millisecond);
            return time;
        }

        [DoNotEnumerate]
        public Number setSeconds(int time)
        {
            host = host.AddSeconds(time - host.Second);
            return time;
        }

        [DoNotEnumerate]
        public Number setUTCSeconds(int time)
        {
            host = host.AddSeconds(time - host.Second);
            return time;
        }

        [DoNotEnumerate]
        public Number setMinutes(int time)
        {
            host = host.AddMinutes(time - host.Minute);
            return time;
        }

        [DoNotEnumerate]
        public Number setUTCMinutes(int time)
        {
            host = host.AddMinutes(time - host.Minute);
            return time;
        }

        [DoNotEnumerate]
        public Number setHours(int time)
        {
            host = host.AddHours(time - host.Hour);
            return time;
        }

        [DoNotEnumerate]
        public Number setUTCHours(int time)
        {
            host = host.AddHours(time - host.Hour);
            return time;
        }

        [DoNotEnumerate]
        public Number setDate(int time)
        {
            host = host.AddDays(time - host.Day);
            return time;
        }

        [DoNotEnumerate]
        public Number setUTCDate(int time)
        {
            host = host.AddDays(time - host.Day);
            return time;
        }

        [DoNotEnumerate]
        public Number setMonth(int time)
        {
            host = host.AddMonths(time - host.Month);
            return time;
        }

        [DoNotEnumerate]
        public Number setUTCMonth(int time)
        {
            host = host = host.AddMonths(time - host.Month);
            return time;
        }

        [DoNotEnumerate]
        public Number setYear(int time)
        {
            host = host.AddYears(time - host.Year);
            return time;
        }

        [DoNotEnumerate]
        public Number setUTCYear(int time)
        {
            host = host.AddYears(time - host.Year);
            return time;
        }

        [DoNotEnumerate]
        public Number setFullYear(int time)
        {
            host = host.AddYears(time - host.Year);
            return time;
        }

        [DoNotEnumerate]
        public Number setUTCFullYear(int time)
        {
            host = host.AddYears(time - host.Year);
            return time;
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public JSObject toString()
        {
            return ToString();
        }

        [DoNotEnumerate]
        public JSObject toLocaleString()
        {
            return ToString();
        }

        [DoNotEnumerate]
        public JSObject toISOString()
        {
            return this.getUTCFullYear() +
                    '-' + (this.getUTCMonth() + 1) +
                    '-' + this.getUTCDate() +
                    'T' + this.getUTCHours() +
                    ':' + this.getUTCMinutes() +
                    ':' + this.getUTCSeconds() +
                    '.' + (this.getUTCMilliseconds() / 1000).ToString(".000").Substring(1) +
                    'Z';
        }

        [DoNotEnumerate]
        public JSObject toUTCString()
        {
            return ToString();
        }

        [DoNotEnumerate]
        public JSObject toGMTString()
        {
            return ToString();
        }

        [Hidden]
        public override string ToString()
        {
            var lt = host.ToLongTimeString();
            var offset = TimeZone.CurrentTimeZone.GetUtcOffset(host);
            var res = host.DayOfWeek.ToString().Substring(0, 3) + " "
                + month[host.Month - 1] + " " + host.Day + " " + host.Year + " " + lt
                + " GMT+" + (offset.Hours * 100 + offset.Minutes).ToString("0000 (") + TimeZone.CurrentTimeZone.DaylightName + ")";
            return res;
        }

        [DoNotEnumerate]
        public static Number parse(string dateTime)
        {
            System.DateTime res;
            if (System.DateTime.TryParse(dateTime, CultureInfo.CurrentCulture, DateTimeStyles.None, out res))
                return res.Ticks - UTCBase;
            return double.NaN;
        }

        [DoNotEnumerate]
        public static Number UTC(JSObject dateTime)
        {
            try
            {
                return new System.DateTime(
                    Tools.JSObjectToInt(dateTime.GetMember("0")),
                    Tools.JSObjectToInt(dateTime.GetMember("1")),
                    Tools.JSObjectToInt(dateTime.GetMember("2")),
                    Tools.JSObjectToInt(dateTime.GetMember("3")),
                    Tools.JSObjectToInt(dateTime.GetMember("4")),
                    Tools.JSObjectToInt(dateTime.GetMember("5"))).Ticks - UTCBase;
            }
            catch
            {
                return double.NaN;
            }
        }
    }
}