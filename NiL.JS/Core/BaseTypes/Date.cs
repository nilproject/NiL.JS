using NiL.JS.Core.Modules;
using System;
using System.Globalization;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public sealed class Date
    {
        private const long _unixTimeBase = 62135596800000;
        private const long _minuteMillisecond = 60 * 1000;
        private const long _hourMilliseconds = 60 * _minuteMillisecond;
        private const long _dayMilliseconds = 24 * _hourMilliseconds;
        private const long _weekMilliseconds = 7 * _dayMilliseconds;
        private const long _400yearsMilliseconds = (365 * 400 + 100 - 3) * _dayMilliseconds;
        private const long _100yearsMilliseconds = (365 * 100 + 25 - 1) * _dayMilliseconds;
        private const long _4yearsMilliseconds = (365 * 4 + 1) * _dayMilliseconds;
        private const long _yearMilliseconds = 365 * _dayMilliseconds;

        private static readonly long[,] monthLengths = { 
                                               { 31 * _dayMilliseconds, 31 * _dayMilliseconds}, 
                                               { 28 * _dayMilliseconds, 29 * _dayMilliseconds}, 
                                               { 31 * _dayMilliseconds, 31 * _dayMilliseconds}, 
                                               { 30 * _dayMilliseconds, 30 * _dayMilliseconds}, 
                                               { 31 * _dayMilliseconds, 31 * _dayMilliseconds},
                                               { 30 * _dayMilliseconds, 30 * _dayMilliseconds}, 
                                               { 31 * _dayMilliseconds, 31 * _dayMilliseconds}, 
                                               { 31 * _dayMilliseconds, 31 * _dayMilliseconds},
                                               { 30 * _dayMilliseconds, 30 * _dayMilliseconds},
                                               { 31 * _dayMilliseconds, 31 * _dayMilliseconds},
                                               { 30 * _dayMilliseconds, 30 * _dayMilliseconds},
                                               { 31 * _dayMilliseconds, 31 * _dayMilliseconds} };

        private static readonly long[,] timeToMonthLengths = { 
                                                { 0 * _dayMilliseconds, 0 * _dayMilliseconds },
                                                { 31 * _dayMilliseconds, 31 * _dayMilliseconds },
                                                { 59 * _dayMilliseconds, 60 * _dayMilliseconds },
                                                { 90 * _dayMilliseconds, 91 * _dayMilliseconds },
                                                { 120 * _dayMilliseconds, 121 * _dayMilliseconds },
                                                { 151 * _dayMilliseconds, 152 * _dayMilliseconds },
                                                { 181 * _dayMilliseconds, 182 * _dayMilliseconds },
                                                { 212 * _dayMilliseconds, 213 * _dayMilliseconds },
                                                { 243 * _dayMilliseconds, 244 * _dayMilliseconds },
                                                { 273 * _dayMilliseconds, 274 * _dayMilliseconds },
                                                { 304 * _dayMilliseconds, 305 * _dayMilliseconds },
                                                { 334 * _dayMilliseconds, 335 * _dayMilliseconds },
                                                { 365 * _dayMilliseconds, 366 * _dayMilliseconds } };

        private static readonly string[] daysOfWeekNames = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun", };

        private readonly static string[] month = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        private static long dateToMilliseconds(long year, long month, long day, long hour, long minute, long second, long millisecond)
        {
            while (month < 0)
            {
                year--;
                month += 12;
            }
            year += month / 12;
            month %= 12;
            var isLeap = (year % 4 == 0 && year % 100 != 0) || year % 400 == 0 ? 1 : 0;
            year--;
            //month--; // В JS mode надо закомментировать
            day--;
            var time = (year / 400) * _400yearsMilliseconds;
            year %= 400;
            time += (year / 100) * _100yearsMilliseconds;
            year %= 100;
            time += (year / 4) * _4yearsMilliseconds;
            year %= 4;
            time += 365 * year * _dayMilliseconds;
            time += timeToMonthLengths[month, isLeap];
            time += day * _dayMilliseconds;
            time += hour * _hourMilliseconds;
            time += minute * _minuteMillisecond;
            time += second * 1000;
            time += millisecond;
            return time;
        }

        private static bool isLeap(int year)
        {
            return (year % 4 == 0 && year % 100 != 0) || year % 400 == 0;
        }

        private long time;

        private bool error = false;

        [DoNotEnumerate]
        public Date()
        {
            time = DateTime.Now.Ticks / 10000;
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
                                time = (long)d + _unixTimeBase + TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Ticks / 10000;
                                break;
                            }
                        case JSObjectType.String:
                            {
                                time = DateTime.Parse(arg.ToString(), CultureInfo.CurrentCulture).Ticks / 10000;
                                break;
                            }
                    }
                }
                else
                {
                    long y = Tools.JSObjectToInt64(args.GetMember("0"), 1);
                    long m = Tools.JSObjectToInt64(args.GetMember("1"));
                    long d = Tools.JSObjectToInt64(args.GetMember("2"), 1);
                    long h = Tools.JSObjectToInt64(args.GetMember("3"));
                    long n = Tools.JSObjectToInt64(args.GetMember("4"));
                    long s = Tools.JSObjectToInt64(args.GetMember("5"));
                    long ms = Tools.JSObjectToInt64(args.GetMember("6"));
                    time = dateToMilliseconds(y, m, d, h, n, s, ms);
                }
            }
            catch
            {
                error = true;
            }
        }

        [DoNotEnumerate]
        public JSObject valueOf()
        {
            return getTime();
        }

        [DoNotEnumerate]
        public JSObject getTime()
        {
            if (error)
                return double.NaN;
            return time - _unixTimeBase - TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Ticks / 10000;
        }

        [DoNotEnumerate]
        public JSObject getTimezoneOffset()
        {
            if (error)
                return Number.NaN;
            var res = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Ticks / 10000;
            return (int)res;
        }

        [DoNotEnumerate]
        public Number getYear()
        {
            var t = getFullYear();
            t.iValue -= 1900;
            return t;
        }

        [DoNotEnumerate]
        public Number getFullYear()
        {
            return getYearImpl();
        }

        private int getYearImpl()
        {
            var t = time;
            var y = (t / _400yearsMilliseconds) * 400;
            t %= _400yearsMilliseconds;
            y += System.Math.Min(3, t / _100yearsMilliseconds) * 100;
            t -= System.Math.Min(3, t / _100yearsMilliseconds) * _100yearsMilliseconds;
            y += (t / _4yearsMilliseconds) * 4; // 25 никогда не будет, так как 25 * _4yearsMilliseconds > _100yearsMilliseconds
            t %= _4yearsMilliseconds;
            y += System.Math.Min(3, t / _yearMilliseconds) + 1;
            return (int)y; // base date: 0001-01-01
        }

        [DoNotEnumerate]
        public Number getUTCFullYear()
        {
            return getFullYear();
        }

        [DoNotEnumerate]
        public Number getMonth()
        {
            return getMonthImpl();
        }

        private int getMonthImpl()
        {
            var t = time;
            var y = (t / _400yearsMilliseconds) * 400;
            t %= _400yearsMilliseconds;
            y += System.Math.Min(3, t / _100yearsMilliseconds) * 100; // 4 быть не должно, ведь мы уже проверили делимость на 400
            t -= System.Math.Min(3, t / _100yearsMilliseconds) * _100yearsMilliseconds;
            y += (t / _4yearsMilliseconds) * 4;
            t %= _4yearsMilliseconds;
            y += System.Math.Min(3, t / _yearMilliseconds) + 1;
            int isLeap = (y % 4 == 0 && y % 100 != 0) || y % 400 == 0 ? 1 : 0;
            t -= System.Math.Min(3, t / _yearMilliseconds) * _yearMilliseconds;
            var m = 0;
            while (timeToMonthLengths[m, isLeap] <= t) m++;
            return m - 1;
        }

        [DoNotEnumerate]
        public Number getUTCMonth()
        {
            return getMonth();
        }

        [DoNotEnumerate]
        public Number getDate()
        {
            return getDateImpl();
        }

        private int getDateImpl()
        {
            var t = time;
            var y = (t / _400yearsMilliseconds) * 400;
            t %= _400yearsMilliseconds;
            y += System.Math.Min(3, t / _100yearsMilliseconds) * 100;
            t -= System.Math.Min(3, t / _100yearsMilliseconds) * _100yearsMilliseconds;
            y += (t / _4yearsMilliseconds) * 4;
            t %= _4yearsMilliseconds;
            y += System.Math.Min(3, t / _yearMilliseconds) + 1;
            int isLeap = (y % 4 == 0 && y % 100 != 0) || y % 400 == 0 ? 1 : 0;
            t -= System.Math.Min(3, t / _yearMilliseconds) * _yearMilliseconds;
            var m = 0;
            while (timeToMonthLengths[m, isLeap] <= t) m++;
            if (m > 0)
                t -= timeToMonthLengths[m - 1, isLeap];
            return (int)(t / _dayMilliseconds + 1);
        }

        [DoNotEnumerate]
        public Number getUTCDate()
        {
            return getDate();
        }

        [DoNotEnumerate]
        public Number getDay()
        {
            return (int)(time % _weekMilliseconds);
        }

        [DoNotEnumerate]
        public Number getUTCDay()
        {
            return (int)(time % _weekMilliseconds);
        }

        [DoNotEnumerate]
        public Number getHours()
        {
            return getHoursImpl();
        }

        private int getHoursImpl()
        {
            var t = time;
            var y = (t / _400yearsMilliseconds) * 400;
            t %= _400yearsMilliseconds;
            y += System.Math.Min(3, t / _100yearsMilliseconds) * 100;
            t -= System.Math.Min(3, t / _100yearsMilliseconds) * _100yearsMilliseconds;
            y += (t / _4yearsMilliseconds) * 4;
            t %= _4yearsMilliseconds;
            y += System.Math.Min(3, t / _yearMilliseconds) + 1;
            int isLeap = (y % 4 == 0 && y % 100 != 0) || y % 400 == 0 ? 1 : 0;
            t -= System.Math.Min(3, t / _yearMilliseconds) * _yearMilliseconds;
            var m = 0;
            while (timeToMonthLengths[m, isLeap] <= t) m++;
            if (m > 0)
                t -= timeToMonthLengths[m - 1, isLeap];
            t %= _dayMilliseconds;
            return (int)(t / _hourMilliseconds);
        }

        [DoNotEnumerate]
        public Number getUTCHours()
        {
            return getHours();
        }

        [DoNotEnumerate]
        public Number getMinutes()
        {
            return getMinutesImpl();
        }

        private int getMinutesImpl()
        {
            var t = time;
            var y = (t / _400yearsMilliseconds) * 400;
            t %= _400yearsMilliseconds;
            y += System.Math.Min(3, t / _100yearsMilliseconds) * 100;
            t -= System.Math.Min(3, t / _100yearsMilliseconds) * _100yearsMilliseconds;
            y += (t / _4yearsMilliseconds) * 4;
            t %= _4yearsMilliseconds;
            y += System.Math.Min(3, t / _yearMilliseconds) + 1;
            int isLeap = (y % 4 == 0 && y % 100 != 0) || y % 400 == 0 ? 1 : 0;
            t -= System.Math.Min(3, t / _yearMilliseconds) * _yearMilliseconds;
            var m = 0;
            while (timeToMonthLengths[m, isLeap] <= t) m++;
            if (m > 0)
                t -= timeToMonthLengths[m - 1, isLeap];
            t %= _dayMilliseconds;
            t %= _hourMilliseconds;
            return (int)(t / _minuteMillisecond);
        }

        [DoNotEnumerate]
        public Number getUTCMinutes()
        {
            return getMinutes();
        }

        [DoNotEnumerate]
        public Number getSeconds()
        {
            return getSecondsImpl();
        }

        private int getSecondsImpl()
        {
            var t = time;
            var y = (t / _400yearsMilliseconds) * 400;
            t %= _400yearsMilliseconds;
            y += System.Math.Min(3, t / _100yearsMilliseconds) * 100;
            t -= System.Math.Min(3, t / _100yearsMilliseconds) * _100yearsMilliseconds;
            y += (t / _4yearsMilliseconds) * 4;
            t %= _4yearsMilliseconds;
            y += System.Math.Min(3, t / _yearMilliseconds) + 1;
            int isLeap = (y % 4 == 0 && y % 100 != 0) || y % 400 == 0 ? 1 : 0;
            t -= System.Math.Min(3, t / _yearMilliseconds) * _yearMilliseconds;
            var m = 0;
            while (timeToMonthLengths[m, isLeap] <= t) m++;
            if (m > 0)
                t -= timeToMonthLengths[m - 1, isLeap];
            t %= _dayMilliseconds;
            t %= _hourMilliseconds;
            t %= _minuteMillisecond;
            return (int)(t / 1000);
        }

        [DoNotEnumerate]
        public Number getUTCSeconds()
        {
            return getSeconds();
        }

        [DoNotEnumerate]
        public Number getMilliseconds()
        {
            return getMillisecondsImpl();
        }

        private int getMillisecondsImpl()
        {
            var t = time;
            var y = (t / _400yearsMilliseconds) * 400;
            t %= _400yearsMilliseconds;
            y += System.Math.Min(3, t / _100yearsMilliseconds) * 100;
            t -= System.Math.Min(3, t / _100yearsMilliseconds) * _100yearsMilliseconds;
            y += (t / _4yearsMilliseconds) * 4;
            t %= _4yearsMilliseconds;
            y += System.Math.Min(3, t / _yearMilliseconds) + 1;
            int isLeap = (y % 4 == 0 && y % 100 != 0) || y % 400 == 0 ? 1 : 0;
            t -= System.Math.Min(3, t / _yearMilliseconds) * _yearMilliseconds;
            var m = 0;
            while (timeToMonthLengths[m, isLeap] <= t) m++;
            if (m > 0)
                t -= timeToMonthLengths[m - 1, isLeap];
            t %= _dayMilliseconds;
            t %= _hourMilliseconds;
            t %= _minuteMillisecond;
            return (int)(t % 1000);
        }

        [DoNotEnumerate]
        public Number getUTCMilliseconds()
        {
            return getMilliseconds();
        }

        [DoNotEnumerate]
        public Number setTime(int time)
        {
            this.time = time;
            return time;
        }

        [DoNotEnumerate]
        public Number setMilliseconds(int milliseconds)
        {
            this.time = this.time - getMillisecondsImpl() + milliseconds;
            return milliseconds;
        }

        [DoNotEnumerate]
        public Number setUTCMilliseconds(int milliseconds)
        {
            this.time = this.time - getMillisecondsImpl() + milliseconds;
            return milliseconds;
        }

        [DoNotEnumerate]
        public Number setSeconds(int seconds)
        {
            this.time = this.time + (-getSecondsImpl() + seconds) * 1000;
            return seconds;
        }

        [DoNotEnumerate]
        public Number setUTCSeconds(int seconds)
        {
            this.time = this.time + (-getSecondsImpl() + seconds) * 1000;
            return seconds;
        }

        [DoNotEnumerate]
        public Number setMinutes(int minutes)
        {
            this.time = this.time + (-getMinutesImpl() + minutes) * _minuteMillisecond;
            return minutes;
        }

        [DoNotEnumerate]
        public Number setUTCMinutes(int minutes)
        {
            this.time = this.time + (-getMinutesImpl() + minutes) * _minuteMillisecond;
            return minutes;
        }

        [DoNotEnumerate]
        public Number setHours(int hours)
        {
            this.time = this.time + (-getHoursImpl() + hours) * _hourMilliseconds;
            return hours;
        }

        [DoNotEnumerate]
        public Number setUTCHours(int hours)
        {
            this.time = this.time + (-getHoursImpl() + hours) * _hourMilliseconds;
            return hours;
        }

        [DoNotEnumerate]
        public Number setDate(int days)
        {
            this.time = this.time + (-getDateImpl() + days) * _dayMilliseconds;
            return days;
        }

        [DoNotEnumerate]
        public Number setUTCDate(int days)
        {
            this.time = this.time + (-getDateImpl() + days) * _dayMilliseconds;
            return days;
        }

        [DoNotEnumerate]
        public Number setMonth(int month)
        {
            if (month < 0 || month > 12)
            {
                this.time = this.time - timeToMonthLengths[getMonthImpl(), isLeap(getYearImpl()) ? 1 : 0];
                time = dateToMilliseconds(getYearImpl(), month, getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
                return getMonthImpl();
            }
            else
            {
                this.time = this.time - timeToMonthLengths[getMonthImpl(), isLeap(getYearImpl()) ? 1 : 0] + timeToMonthLengths[month, isLeap(getYearImpl()) ? 1 : 0];
                return month;
            }
        }

        [DoNotEnumerate]
        public Number setUTCMonth(int month)
        {
            this.time = this.time - timeToMonthLengths[getMonthImpl(), isLeap(getYearImpl()) ? 1 : 0] + timeToMonthLengths[month, isLeap(getYearImpl()) ? 1 : 0];
            return month;
        }

        [DoNotEnumerate]
        public Number setYear(int year)
        {
            time = dateToMilliseconds(year + 1900, getMonthImpl(), getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
            return year;
        }

        [DoNotEnumerate]
        public Number setUTCYear(int year)
        {
            time = dateToMilliseconds(year + 1900, getMonthImpl(), getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
            return year;
        }

        [DoNotEnumerate]
        public Number setFullYear(int year)
        {
            time = dateToMilliseconds(year, getMonthImpl(), getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
            return year;
        }

        [DoNotEnumerate]
        public Number setUTCFullYear(int year)
        {
            time = dateToMilliseconds(year, getMonthImpl(), getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
            return year;
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
            return this.getYearImpl() +
                    '-' + (this.getMonthImpl() + 1) +
                    '-' + this.getDateImpl() +
                    'T' + this.getHoursImpl() +
                    ':' + this.getMinutesImpl() +
                    ':' + this.getSecondsImpl() +
                    '.' + (this.getMillisecondsImpl() / 1000).ToString(".000").Substring(1) +
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
            var offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            var res =
                daysOfWeekNames[(time % _weekMilliseconds) / _dayMilliseconds] + " "
                + month[getMonthImpl()]
                + " " + getDateImpl() + " "
                + getYearImpl() + " "
                + getHoursImpl().ToString("00:")
                + getMinutesImpl().ToString("00:")
                + getSecondsImpl().ToString("00")
                + " GMT+" + (offset.Hours * 100 + offset.Minutes).ToString("0000 (") + TimeZone.CurrentTimeZone.DaylightName + ")";
            return res;
        }

        [DoNotEnumerate]
        public static Number parse(string dateTime)
        {
            System.DateTime res;
            if (System.DateTime.TryParse(dateTime, CultureInfo.CurrentCulture, DateTimeStyles.None, out res))
                return res.Ticks / 10000 - _unixTimeBase;
            return double.NaN;
        }

        [DoNotEnumerate]
        public static Number UTC(JSObject dateTime)
        {
            try
            {
                return dateToMilliseconds(
                    Tools.JSObjectToInt(dateTime.GetMember("0")),
                    Tools.JSObjectToInt(dateTime.GetMember("1")),
                    Tools.JSObjectToInt(dateTime.GetMember("2")),
                    Tools.JSObjectToInt(dateTime.GetMember("3")),
                    Tools.JSObjectToInt(dateTime.GetMember("4")),
                    Tools.JSObjectToInt(dateTime.GetMember("5")),
                    Tools.JSObjectToInt(dateTime.GetMember("6"))) - _unixTimeBase;
            }
            catch
            {
                return double.NaN;
            }
        }
    }
}