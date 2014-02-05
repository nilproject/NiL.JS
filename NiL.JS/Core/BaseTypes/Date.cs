using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    public class Date
    {
        [Hidden]
        private readonly String tempSResult = "";

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
                if (args.GetField("length", true, false).iValue == 1)
                {
                    var arg = args.GetField("0", true, false);
                    switch (arg.ValueType)
                    {
                        case JSObjectType.Int:
                        case JSObjectType.Bool:
                        case JSObjectType.Double:
                            {
                                host = new DateTime((long)Tools.JSObjectToDouble(arg) + UTCBase);
                                break;
                            }
                        case JSObjectType.String:
                            {
                                host = DateTime.Parse(arg.ToString());
                                break;
                            }
                    }
                }
                else
                {
                    int y = Tools.JSObjectToInt(args.GetField("0", true, false), 1);
                    int m = Tools.JSObjectToInt(args.GetField("1", true, false), 0) + 1;
                    int d = Tools.JSObjectToInt(args.GetField("2", true, false), 1);
                    int h = Tools.JSObjectToInt(args.GetField("3", true, false), 0);
                    int n = Tools.JSObjectToInt(args.GetField("4", true, false), 0);
                    int s = Tools.JSObjectToInt(args.GetField("5", true, false), 0);
                    host = new System.DateTime();
                    host = host.AddYears(y - host.Year);
                    host = host.AddMonths(m - host.Month);
                    host = host.AddDays(d - host.Day);
                    host = host.AddHours(h - host.Hour);
                    host = host.AddMinutes(n - host.Minute);
                    host = host.AddSeconds(s - host.Second);
                    string test = host.ToLongDateString();
                }
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

        public double getTime()
        {
            if (error)
                return double.NaN;
            var res = host.Ticks - UTCBase;
            return res;
        }

        public int getYear()
        {
            return host.Year - 1900;
        }

        public int getFullYear()
        {
            return host.Year;
        }

        public int getUTCFullYear()
        {
            return host.Year;
        }

        public int getMonth()
        {
            return host.Month - 1;
        }

        public int getUTCMonth()
        {
            return host.Month - 1;
        }

        public int getDate()
        {
            return host.Day;
        }

        public int getUTCDate()
        {
            return host.Day;
        }

        public int getDay()
        {
            return (int)host.DayOfWeek;
        }

        public int getUTCDay()
        {
            return (int)host.DayOfWeek;
        }

        public int getHours()
        {
            return host.Hour;
        }

        public int getUTCHours()
        {
            return host.Hour;
        }

        public int getMinutes()
        {
            return host.Minute;
        }

        public int getUTCMinutes()
        {
            return host.Minute;
        }

        public int getSeconds()
        {
            return host.Second;
        }

        public int getUTCSeconds()
        {
            return host.Second;
        }

        public int getMilliseconds()
        {
            return host.Millisecond;
        }

        public int getUTCMilliseconds()
        {
            return host.Millisecond;
        }

        public int setTime(int time)
        {
            host = new DateTime(time + UTCBase);
            return time;
        }

        public int setMilliseconds(int time)
        {
            host = host.AddMilliseconds(time - host.Millisecond);
            return time;
        }

        public int setUTCMilliseconds(int time)
        {
            host = host.AddMilliseconds(time - host.Millisecond);
            return time;
        }

        public int setSeconds(int time)
        {
            host = host.AddSeconds(time - host.Second);
            return time;
        }

        public int setUTCSeconds(int time)
        {
            host = host.AddSeconds(time - host.Second);
            return time;
        }

        public int setMinutes(int time)
        {
            host = host.AddMinutes(time - host.Minute);
            return time;
        }

        public int setUTCMinutes(int time)
        {
            host = host.AddMinutes(time - host.Minute);
            return time;
        }

        public int setHours(int time)
        {
            host = host.AddHours(time - host.Hour);
            return time;
        }

        public int setUTCHours(int time)
        {
            host = host.AddHours(time - host.Hour);
            return time;
        }

        public int setDate(int time)
        {
            host = host.AddDays(time - host.Day);
            return time;
        }

        public int setUTCDate(int time)
        {
            host = host.AddDays(time - host.Day);
            return time;
        }

        public int setMonth(int time)
        {
            host = host.AddMonths(time - host.Month);
            return time;
        }

        public int setUTCMonth(int time)
        {
            host = host = host.AddMonths(time - host.Month);
            return time;
        }

        public int setYear(int time)
        {
            host = host.AddYears(time - host.Year);
            return time;
        }

        public int setUTCYear(int time)
        {
            host = host.AddYears(time - host.Year);
            return time;
        }

        public int setFullYear(int time)
        {
            host = host.AddYears(time - host.Year);
            return time;
        }

        public int setUTCFullYear(int time)
        {
            host = host.AddYears(time - host.Year);
            return time;
        }

        public JSObject toString()
        {
            tempSResult.oValue = ToString();
            return tempSResult;
        }

        public JSObject toLocaleString()
        {
            tempSResult.oValue = ToString();
            return tempSResult;
        }

        public JSObject toUTCString()
        {
            tempSResult.oValue = ToString();
            return tempSResult;
        }

        public JSObject toGMTString()
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

        public static double parse(string dateTime)
        {
            try
            {
                return System.DateTime.Parse(dateTime).Ticks - UTCBase;
            }
            catch
            {
                return double.NaN;
            }
        }

        public static double UTC(JSObject dateTime)
        {
            try
            {
                return new System.DateTime(
                    Tools.JSObjectToInt(dateTime.GetField("0", true, false)),
                    Tools.JSObjectToInt(dateTime.GetField("1", true, false)),
                    Tools.JSObjectToInt(dateTime.GetField("2", true, false)),
                    Tools.JSObjectToInt(dateTime.GetField("3", true, false)),
                    Tools.JSObjectToInt(dateTime.GetField("4", true, false)),
                    Tools.JSObjectToInt(dateTime.GetField("5", true, false))).Ticks - UTCBase;
            }
            catch
            {
                return double.NaN;
            }
        }
    }
}