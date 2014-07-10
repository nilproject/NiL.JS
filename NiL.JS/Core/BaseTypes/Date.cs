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

        private static bool parseByFormat(string format, string timeStr, out long time, out long timeZoneOffset)
        {
            var year = 0;
            var month = int.MinValue;
            var day = int.MinValue;
            var hour = 0;
            var minutes = 0;
            var seconds = 0;
            var milliseconds = 0;
            format = format.ToLower();
            time = 0;
            timeZoneOffset = 0;
            int part = 0; // 0 - дата, 1 - время, 2 - смещение
            int i = 0, j = 0;
            for (; i < format.Length; i++, j++)
            {
                if (timeStr.Length <= j)
                    return false;
                switch (format[i])
                {
                    case 'y':
                        {
                            if (!Tools.isDigit(timeStr[j]))
                                return false;
                            year = year * 10 + timeStr[j] - '0';
                            break;
                        }
                    case 'm':
                        {
                            if (!Tools.isDigit(timeStr[j]))
                                return false;
                            switch (part)
                            {
                                case 0:
                                    {
                                        month = month * 10 + timeStr[j] - '0';
                                        break;
                                    }
                                case 1:
                                    {
                                        minutes = minutes * 10 + timeStr[j] - '0';
                                        break;
                                    }
                            }
                            break;
                        }
                    case 'd':
                        {
                            if (!Tools.isDigit(timeStr[j]))
                                return false;
                            day = day * 10 + timeStr[j] - '0';
                            break;
                        }
                    case 'h':
                        {
                            if (!Tools.isDigit(timeStr[j]))
                                return false;
                            hour = hour * 10 + timeStr[j] - '0';
                            break;
                        }
                    case ':':
                        {
                            if (format[i] != timeStr[j])
                                return false;
                            part++;
                            break;
                        }
                    case '/':
                        {
                            if (format[i] != timeStr[j])
                                return false;
                            break;
                        }
                    case ' ':
                        {
                            if (format[i] != timeStr[j])
                                return false;
                            while (j < timeStr.Length && char.IsWhiteSpace(timeStr[j]))
                                j++;
                            j--;
                            break;
                        }
                    case '-':
                        {
                            if (format[i] != timeStr[j])
                                return false;
                            month = 0;
                            break;
                        }
                    default: return false;
                }
            }
            if (j < timeStr.Length)
                return false;
            if (month == int.MinValue)
                month = 1;
            if (day == int.MinValue)
                day = 1;
            if (year < 100)
                year += (DateTime.Now.Year / 100) * 100;
            time = dateToMilliseconds(year, month - 1, day, hour, minutes, seconds, milliseconds);
            timeZoneOffset = 0;
            return true;
        }

        private static bool isLeap(int year)
        {
            return (year % 4 == 0 && year % 100 != 0) || year % 400 == 0;
        }

        private long time;
        private long timeZoneOffset;

        private bool error = false;

        [DoNotEnumerate]
        public Date()
        {
            time = DateTime.Now.Ticks / 10000;
        }

        [DoNotEnumerate]
        [ParamCount(7)]
        public Date(Arguments args)
        {
            if (args.length == 1)
            {
                var arg = args[0];
                switch (arg.valueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                    case JSObjectType.Double:
                        {
                            var timeValue = Tools.JSObjectToDouble(arg);
                            if (double.IsNaN(timeValue) || double.IsInfinity(timeValue))
                            {
                                error = true;
                                break;
                            }
                            timeZoneOffset = TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime((((long)timeValue) % _400yearsMilliseconds + _unixTimeBase) * 10000)).Ticks / 10000;
                            time = (long)timeValue + _unixTimeBase + timeZoneOffset;
                            break;
                        }
                    case JSObjectType.String:
                        {
                            long tzo = 0;
                            var tstr = arg.ToString();
                            /*if (!parseByFormat("YYYY-MM-DDTHH:MM:SS.SSS", tstr, out time, out tzo)
                                //&& !parseByFormat("YYYY-MM-DDTHH:MM:SS", tstr, out time, out tzo)
                                //&& !parseByFormat("YYYY-MM-DDTHH:MM", tstr, out time, out tzo)
                                //&& !parseByFormat("YYYY-MM-DDTHH", tstr, out time, out tzo)
                                //&& !parseByFormat("YYYY-MM-DD", tstr, out time, out tzo)
                                //&& !parseByFormat("YYYY-MM", tstr, out time, out tzo)
                                && !parseByFormat("YYYY", tstr, out time, out tzo))
                            {
                                if (!parseByFormat("YYYY MM DD HH:MM:SS +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY MM DD HH:MM +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY MM DD HH: +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY MM DD +OOOO", tstr, out time, out tzo)

                                    && !parseByFormat("YYYY MM HH:MM:SS +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY MM HH:MM +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY MM HH: +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY MM +OOOO", tstr, out time, out tzo)

                                    && !parseByFormat("YYYY HH:MM:SS +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY HH:MM +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY HH: +OOOO", tstr, out time, out tzo)

                                    && !parseByFormat("MM DD YYYY HH:MM:SS +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("MM DD YYYY HH:MM +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("MM DD YYYY HH: +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("MM DD YYYY +OOOO", tstr, out time, out tzo)

                                    && !parseByFormat("MM DD YY HH:MM:SS +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("MM DD YY HH:MM +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("MM DD YY HH: +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("MM DD YY +OOOO", tstr, out time, out tzo)

                                    && !parseByFormat("YYYY/MM/DD HH:MM:SS +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY/MM/DD HH:MM +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY/MM/DD HH: +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY/MM/DD +OOOO", tstr, out time, out tzo)

                                    && !parseByFormat("YYYY/MM HH:MM:SS +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY/MM HH:MM +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY/MM HH: +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY/MM +OOOO", tstr, out time, out tzo)

                                    && !parseByFormat("YYYY HH:MM:SS +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY HH:MM +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY HH: +OOOO", tstr, out time, out tzo)

                                    && !parseByFormat("MM/DD/YYYY HH:MM:SS  +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("MM/DD/YYYY HH:MM  +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("MM/DD/YYYY HH:  +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("MM/DD/YYYY  +OOOO", tstr, out time, out tzo)

                                    && !parseByFormat("MM/DD/YY HH:MM:SS +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("MM/DD/YY HH:MM +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("MM/DD/YY HH: +OOOO", tstr, out time, out tzo)
                                    && !parseByFormat("MM/DD/YY +OOOO", tstr, out time, out tzo)

                                    && !parseByFormat("YYYY MM DD HH:MM:SS", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY MM DD HH:MM", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY MM DD HH:", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY MM DD", tstr, out time, out tzo)

                                    && !parseByFormat("YYYY MM HH:MM:SS", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY MM HH:MM", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY MM HH:", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY MM", tstr, out time, out tzo)

                                    && !parseByFormat("YYYY HH:MM:SS", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY HH:MM", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY HH:", tstr, out time, out tzo)

                                    && !parseByFormat("MM DD YYYY HH:MM:SS", tstr, out time, out tzo)
                                    && !parseByFormat("MM DD YYYY HH:MM", tstr, out time, out tzo)
                                    && !parseByFormat("MM DD YYYY HH:", tstr, out time, out tzo)
                                    && !parseByFormat("MM DD YYYY", tstr, out time, out tzo)

                                    && !parseByFormat("MM DD YY HH:MM:SS", tstr, out time, out tzo)
                                    && !parseByFormat("MM DD YY HH:MM", tstr, out time, out tzo)
                                    && !parseByFormat("MM DD YY HH:", tstr, out time, out tzo)
                                    && !parseByFormat("MM DD YY", tstr, out time, out tzo)

                                    && !parseByFormat("YYYY/MM/DD HH:MM:SS", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY/MM/DD HH:MM", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY/MM/DD HH:", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY/MM/DD", tstr, out time, out tzo)

                                    && !parseByFormat("YYYY/MM HH:MM:SS", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY/MM HH:MM", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY/MM HH:", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY/MM", tstr, out time, out tzo)

                                    && !parseByFormat("YYYY HH:MM:SS", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY HH:MM", tstr, out time, out tzo)
                                    && !parseByFormat("YYYY HH:", tstr, out time, out tzo)

                                    && !parseByFormat("MM/DD/YYYY HH:MM:SS", tstr, out time, out tzo)
                                    && !parseByFormat("MM/DD/YYYY HH:MM", tstr, out time, out tzo)
                                    && !parseByFormat("MM/DD/YYYY HH:", tstr, out time, out tzo)
                                    && !parseByFormat("MM/DD/YYYY", tstr, out time, out tzo)

                                    && !parseByFormat("MM/DD/YY HH:MM:SS", tstr, out time, out tzo)
                                    && !parseByFormat("MM/DD/YY HH:MM", tstr, out time, out tzo)
                                    && !parseByFormat("MM/DD/YY HH:", tstr, out time, out tzo)
                                    && !parseByFormat("MM/DD/YY", tstr, out time, out tzo))
                                    error = true;
                            }
                            
                            timeZoneOffset = TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime((((long)time) % _400yearsMilliseconds + _unixTimeBase) * 10000)).Ticks / 10000;*/
                            DateTime dt;
                            if (DateTime.TryParse(tstr, out dt))
                            {
                                time = dt.Ticks / 10000;
                                timeZoneOffset = TimeZone.CurrentTimeZone.GetUtcOffset(dt).Ticks / 10000;
                            }
                            else if (parseByFormat("YYYY MM DD HH:MM:SS", tstr, out time, out tzo)
                                    || parseByFormat("YYYY MM DD HH:MM", tstr, out time, out tzo)
                                    || parseByFormat("YYYY MM DD HH:", tstr, out time, out tzo)
                                    || parseByFormat("YYYY MM DD", tstr, out time, out tzo)

                                    || parseByFormat("YYYY MM HH:MM:SS", tstr, out time, out tzo)
                                    || parseByFormat("YYYY MM HH:MM", tstr, out time, out tzo)
                                    || parseByFormat("YYYY MM HH:", tstr, out time, out tzo)
                                    || parseByFormat("YYYY MM", tstr, out time, out tzo)

                                    || parseByFormat("YYYY HH:MM:SS", tstr, out time, out tzo)
                                    || parseByFormat("YYYY HH:MM", tstr, out time, out tzo)
                                    || parseByFormat("YYYY HH:", tstr, out time, out tzo)

                                    || parseByFormat("MM DD YYYY HH:MM:SS", tstr, out time, out tzo)
                                    || parseByFormat("MM DD YYYY HH:MM", tstr, out time, out tzo)
                                    || parseByFormat("MM DD YYYY HH:", tstr, out time, out tzo)
                                    || parseByFormat("MM DD YYYY", tstr, out time, out tzo)

                                    || parseByFormat("MM DD YY HH:MM:SS", tstr, out time, out tzo)
                                    || parseByFormat("MM DD YY HH:MM", tstr, out time, out tzo)
                                    || parseByFormat("MM DD YY HH:", tstr, out time, out tzo)
                                    || parseByFormat("MM DD YY", tstr, out time, out tzo)

                                    || parseByFormat("YYYY/MM/DD HH:MM:SS", tstr, out time, out tzo)
                                    || parseByFormat("YYYY/MM/DD HH:MM", tstr, out time, out tzo)
                                    || parseByFormat("YYYY/MM/DD HH:", tstr, out time, out tzo)
                                    || parseByFormat("YYYY/MM/DD", tstr, out time, out tzo)

                                    || parseByFormat("YYYY/MM HH:MM:SS", tstr, out time, out tzo)
                                    || parseByFormat("YYYY/MM HH:MM", tstr, out time, out tzo)
                                    || parseByFormat("YYYY/MM HH:", tstr, out time, out tzo)
                                    || parseByFormat("YYYY/MM", tstr, out time, out tzo)

                                    || parseByFormat("YYYY HH:MM:SS", tstr, out time, out tzo)
                                    || parseByFormat("YYYY HH:MM", tstr, out time, out tzo)
                                    || parseByFormat("YYYY HH:", tstr, out time, out tzo)
                                    || parseByFormat("YYYY", tstr, out time, out tzo)

                                    || parseByFormat("MM/DD/YYYY HH:MM:SS", tstr, out time, out tzo)
                                    || parseByFormat("MM/DD/YYYY HH:MM", tstr, out time, out tzo)
                                    || parseByFormat("MM/DD/YYYY HH:", tstr, out time, out tzo)
                                    || parseByFormat("MM/DD/YYYY", tstr, out time, out tzo)

                                    || parseByFormat("MM/DD/YY HH:MM:SS", tstr, out time, out tzo)
                                    || parseByFormat("MM/DD/YY HH:MM", tstr, out time, out tzo)
                                    || parseByFormat("MM/DD/YY HH:", tstr, out time, out tzo)
                                    || parseByFormat("MM/DD/YY", tstr, out time, out tzo))
                            {
                                timeZoneOffset = TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime((((long)time) % _400yearsMilliseconds + _unixTimeBase) * 10000)).Ticks / 10000;
                                time += timeZoneOffset;
                            }
                            else
                                error = true;
                            break;
                        }
                }
            }
            else
            {
                for (var i = 0; i < 7 && !error; i++)
                {
                    if (args[i].isExist && !args[i].isDefinded)
                    {
                        error = true;
                        return;
                    }
                }
                long y = Tools.JSObjectToInt64(args[0], 1, true);
                if (y == long.MinValue)
                    y >>= 6;
                if (y == long.MaxValue)
                    y >>= 6;
                long m = Tools.JSObjectToInt64(args[1]);
                long d = Tools.JSObjectToInt64(args[2], 1);
                long h = Tools.JSObjectToInt64(args[3]);
                long n = Tools.JSObjectToInt64(args[4]);
                long s = Tools.JSObjectToInt64(args[5]);
                long ms = Tools.JSObjectToInt64(args[6]);
                time = dateToMilliseconds(y, m, d, h, n, s, ms);
                timeZoneOffset = TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime((((long)time) % _400yearsMilliseconds + _unixTimeBase) * 10000)).Ticks / 10000;
                //time -= timeZoneOffset;
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
            return time - _unixTimeBase - timeZoneOffset;
        }

        [DoNotEnumerate]
        public static JSObject now()
        {
            return DateTime.Now.Ticks / 10000 - _unixTimeBase;
        }

        [DoNotEnumerate]
        public JSObject getTimezoneOffset()
        {
            if (error)
                return Number.NaN;
            var res = -timeZoneOffset / _minuteMillisecond;
            return (int)res;
        }

        [DoNotEnumerate]
        public JSObject getYear()
        {
            var t = getFullYear();
            t.iValue -= 1900;
            return t;
        }

        [DoNotEnumerate]
        public JSObject getFullYear()
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
        public JSObject getUTCFullYear()
        {
            return getFullYear();
        }

        [DoNotEnumerate]
        public JSObject getMonth()
        {
            return getMonthImpl();
        }

        private int getMonthImpl()
        {
            var t = time;
            while (t < 0)
                t += _400yearsMilliseconds * 7;
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
            while (timeToMonthLengths[m, isLeap] <= t)
                m++;
            return m - 1;
        }

        [DoNotEnumerate]
        public JSObject getUTCMonth()
        {
            return getMonth();
        }

        [DoNotEnumerate]
        public JSObject getDate()
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
            while (timeToMonthLengths[m, isLeap] <= t)
                m++;
            if (m > 0)
                t -= timeToMonthLengths[m - 1, isLeap];
            return (int)(t / _dayMilliseconds + 1);
        }

        [DoNotEnumerate]
        public JSObject getUTCDate()
        {
            return getDate();
        }

        [DoNotEnumerate]
        public JSObject getDay()
        {
            return (int)((time / _dayMilliseconds + 1) % 7);
        }

        [DoNotEnumerate]
        public JSObject getUTCDay()
        {
            return (int)((time / _dayMilliseconds + 1) % 7);
        }

        [DoNotEnumerate]
        public JSObject getHours()
        {
            return getHoursImpl();
        }

        private int getHoursImpl()
        {
            var t = time % _dayMilliseconds;
            return (int)(t / _hourMilliseconds);
        }

        [DoNotEnumerate]
        public JSObject getUTCHours()
        {
            return getHours();
        }

        [DoNotEnumerate]
        public JSObject getMinutes()
        {
            return getMinutesImpl();
        }

        private int getMinutesImpl()
        {
            var t = time;
            t %= _hourMilliseconds;
            return (int)(t / _minuteMillisecond);
        }

        [DoNotEnumerate]
        public JSObject getUTCMinutes()
        {
            return getMinutes();
        }

        [DoNotEnumerate]
        public JSObject getSeconds()
        {
            return getSecondsImpl();
        }

        private int getSecondsImpl()
        {
            var t = time;
            t %= _minuteMillisecond;
            return (int)(t / 1000);
        }

        [DoNotEnumerate]
        public JSObject getUTCSeconds()
        {
            return getSeconds();
        }

        [DoNotEnumerate]
        public JSObject getMilliseconds()
        {
            return getMillisecondsImpl();
        }

        private int getMillisecondsImpl()
        {
            var t = time;
            t %= _minuteMillisecond;
            return (int)(t % 1000);
        }

        [DoNotEnumerate]
        public JSObject getUTCMilliseconds()
        {
            return getMilliseconds();
        }

        [DoNotEnumerate]
        public JSObject setTime(JSObject time)
        {
            this.time = Tools.JSObjectToInt64(time);
            return time;
        }

        [DoNotEnumerate]
        public JSObject setMilliseconds(JSObject milliseconds)
        {
            this.time = this.time - getMillisecondsImpl() + Tools.JSObjectToInt64(milliseconds);
            return milliseconds;
        }

        [DoNotEnumerate]
        public JSObject setUTCMilliseconds(JSObject milliseconds)
        {
            this.time = this.time - getMillisecondsImpl() + Tools.JSObjectToInt64(milliseconds);
            return milliseconds;
        }

        [DoNotEnumerate]
        public JSObject setSeconds(JSObject seconds, JSObject milliseconds)
        {
            if (seconds != null && seconds.isExist)
                this.time = this.time + (-getSecondsImpl() + Tools.JSObjectToInt64(seconds)) * 1000;
            if (milliseconds != null && milliseconds.isExist)
                time = (time % 1000) + (Tools.JSObjectToInt64(milliseconds) % 1000);
            return seconds;
        }

        [DoNotEnumerate]
        public JSObject setUTCSeconds(JSObject seconds, JSObject milliseconds)
        {
            if (seconds != null && seconds.isExist)
                this.time = this.time + (-getSecondsImpl() + Tools.JSObjectToInt64(seconds)) * 1000;
            if (milliseconds != null && milliseconds.isExist)
                time = (time % 1000) + (Tools.JSObjectToInt64(milliseconds) % 1000);
            return seconds;
        }

        [DoNotEnumerate]
        public JSObject setMinutes(JSObject minutes, JSObject seconds, JSObject milliseconds)
        {
            if (minutes != null && minutes.isExist)
                this.time = this.time + (-getMinutesImpl() + Tools.JSObjectToInt64(minutes)) * _minuteMillisecond;
            setSeconds(seconds, milliseconds);
            return minutes;
        }

        [DoNotEnumerate]
        public JSObject setUTCMinutes(JSObject minutes, JSObject seconds, JSObject milliseconds)
        {
            if (minutes != null && minutes.isExist)
                this.time = this.time + (-getMinutesImpl() + Tools.JSObjectToInt64(minutes)) * _minuteMillisecond;
            setSeconds(seconds, milliseconds);
            return minutes;
        }

        [DoNotEnumerate]
        public JSObject setHours(JSObject hours, JSObject minutes, JSObject seconds, JSObject milliseconds)
        {
            if (hours != null && hours.isExist)
                this.time = this.time + (-getHoursImpl() + Tools.JSObjectToInt64(hours)) * _hourMilliseconds;
            setMinutes(minutes, seconds, milliseconds);
            return hours;
        }

        [DoNotEnumerate]
        public JSObject setUTCHours(JSObject hours, JSObject minutes, JSObject seconds, JSObject milliseconds)
        {
            if (hours != null && hours.isExist)
                this.time = this.time + (-getHoursImpl() + Tools.JSObjectToInt64(hours)) * _hourMilliseconds;
            setMinutes(minutes, seconds, milliseconds);
            return hours;
        }

        [DoNotEnumerate]
        public JSObject setDate(JSObject days)
        {
            if (days != null && days.isExist)
                this.time = this.time + (-getDateImpl() + Tools.JSObjectToInt64(days)) * _dayMilliseconds;
            return days;
        }

        [DoNotEnumerate]
        public JSObject setUTCDate(JSObject days)
        {
            this.time = this.time + (-getDateImpl() + Tools.JSObjectToInt64(days)) * _dayMilliseconds;
            return days;
        }

        [DoNotEnumerate]
        public JSObject setMonth(JSObject monthO, JSObject day)
        {
            try
            {
                var month = Tools.JSObjectToInt64(monthO);
                if (month < 0 || month > 12)
                {
                    this.time = this.time - timeToMonthLengths[getMonthImpl(), isLeap(getYearImpl()) ? 1 : 0];
                    time = dateToMilliseconds(getYearImpl(), month, getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
                    return getMonthImpl();
                }
                else
                {
                    this.time = this.time - timeToMonthLengths[getMonthImpl(), isLeap(getYearImpl()) ? 1 : 0] + timeToMonthLengths[month, isLeap(getYearImpl()) ? 1 : 0];
                    return monthO;
                }
            }
            finally
            {
                setDate(day);
            }
        }

        [DoNotEnumerate]
        public JSObject setUTCMonth(JSObject monthO, JSObject day)
        {
            try
            {
                var month = Tools.JSObjectToInt64(monthO);
                if (month < 0 || month > 12)
                {
                    this.time = this.time - timeToMonthLengths[getMonthImpl(), isLeap(getYearImpl()) ? 1 : 0];
                    time = dateToMilliseconds(getYearImpl(), month, getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
                    return getMonthImpl();
                }
                else
                {
                    this.time = this.time - timeToMonthLengths[getMonthImpl(), isLeap(getYearImpl()) ? 1 : 0] + timeToMonthLengths[month, isLeap(getYearImpl()) ? 1 : 0];
                    return monthO;
                }
            }
            finally
            {
                setDate(day);
            }
        }

        [DoNotEnumerate]
        public JSObject setYear(JSObject year)
        {
            time = dateToMilliseconds(Tools.JSObjectToInt64(year) + 1900, getMonthImpl(), getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
            return year;
        }

        [DoNotEnumerate]
        public JSObject setUTCYear(JSObject year)
        {
            time = dateToMilliseconds(Tools.JSObjectToInt64(year) + 1900, getMonthImpl(), getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
            return year;
        }

        [DoNotEnumerate]
        public JSObject setFullYear(JSObject year, JSObject month, JSObject day)
        {
            time = dateToMilliseconds(Tools.JSObjectToInt64(year), getMonthImpl(), getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
            setMonth(month, day);
            return year;
        }

        [DoNotEnumerate]
        public JSObject setUTCFullYear(JSObject year, JSObject month, JSObject day)
        {
            time = dateToMilliseconds(Tools.JSObjectToInt64(year), getMonthImpl(), getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
            setMonth(month, day);
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
            var offset = new TimeSpan(timeZoneOffset * 10000);
            var y = getYearImpl();
            while (y > 2800)
                y -= 2800;
            while (y < 0)
                y += 2800;
            var dt = new DateTime(0);
            dt = dt.AddDays((System.Math.Abs(time) % _weekMilliseconds) / _dayMilliseconds);
            dt = dt.AddMonths(getMonthImpl());
            dt = dt.AddYears(y);
            var res =
                dt.ToString("dddd MMMM")
                + " " + getDateImpl() + " "
                + getYearImpl() + " "
                + getHoursImpl().ToString("00:")
                + getMinutesImpl().ToString("00:")
                + getSecondsImpl().ToString("00")
                + " GMT" + (offset.Ticks > 0 ? "+" : "") + (offset.Hours * 100 + offset.Minutes).ToString("0000 (") + TimeZone.CurrentTimeZone.DaylightName + ")";
            return res;
        }

        [DoNotEnumerate]
        public JSObject toLocaleTimeString()
        {
            var res =
                getHoursImpl().ToString("00:")
                + getMinutesImpl().ToString("00:")
                + getSecondsImpl().ToString("00");
            return res;
        }

        [DoNotEnumerate]
        public JSObject toISOString()
        {
            if (time > 8702135596800000 || time < -8577864399600000)
                throw new JSException(new RangeError("Invalid time value"));
            try
            {
                time -= timeZoneOffset;
                var y = getYearImpl();

                return y +
                        "-" + (this.getMonthImpl() + 1).ToString("00") +
                        "-" + this.getDateImpl().ToString("00") +
                        "T" + this.getHoursImpl().ToString("00") +
                        ":" + this.getMinutesImpl().ToString("00") +
                        ":" + this.getSecondsImpl().ToString("00") +
                        "." + (this.getMillisecondsImpl() / 1000.0).ToString(".000").Substring(1) +
                        "Z";
            }
            finally
            {
                time += timeZoneOffset;
            }
        }

        [DoNotEnumerate]
        public JSObject toJSON(JSObject obj)
        {
            return toISOString();
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

        [DoNotEnumerate]
        public JSObject toTimeString()
        {
            var offset = new TimeSpan(timeZoneOffset * 10000);
            var res =
                getHoursImpl().ToString("00:")
                + getMinutesImpl().ToString("00:")
                + getSecondsImpl().ToString("00")
                + " GMT" + (offset.Ticks > 0 ? "+" : "") + (offset.Hours * 100 + offset.Minutes).ToString("0000 (") + TimeZone.CurrentTimeZone.DaylightName + ")";
            return res;
        }

        [DoNotEnumerate]
        public JSObject toDateString()
        {
            var offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            var res =
                daysOfWeekNames[(System.Math.Abs(time) % _weekMilliseconds) / _dayMilliseconds] + " "
                + month[getMonthImpl()]
                + " " + getDateImpl() + " "
                + getYearImpl();
            return res;
        }

        [DoNotEnumerate]
        public JSObject toLocaleDateString()
        {
            var y = getYearImpl();
            while (y > 2800)
                y -= 2800;
            while (y < 0)
                y += 2800;
            var dt = new DateTime(0);
            dt = dt.AddDays((System.Math.Abs(time) % _weekMilliseconds) / _dayMilliseconds);
            dt = dt.AddMonths(getMonthImpl());
            dt = dt.AddYears(y);
            var res =
                dt.ToString("dddd MMMM")
                + " " + getDateImpl() + " "
                + getYearImpl();
            return res;
        }

        [Hidden]
        public override string ToString()
        {
            var offset = new TimeSpan(timeZoneOffset * 10000);
            var res =
                daysOfWeekNames[System.Math.Abs(time) / _dayMilliseconds % 7] + " "
                + month[getMonthImpl()]
                + " " + getDateImpl() + " "
                + getYearImpl() + " "
                + getHoursImpl().ToString("00:")
                + getMinutesImpl().ToString("00:")
                + getSecondsImpl().ToString("00")
                + " GMT" + (offset.Ticks > 0 ? "+" : "") + (offset.Hours * 100 + offset.Minutes).ToString("0000 (") + TimeZone.CurrentTimeZone.DaylightName + ")";
            return res;
        }

        [DoNotEnumerate]
        public static JSObject parse(string dateTime)
        {
            System.DateTime res;
            if (System.DateTime.TryParse(dateTime, CultureInfo.CurrentCulture, DateTimeStyles.None, out res))
                return res.Ticks / 10000 - _unixTimeBase;
            return double.NaN;
        }

        [DoNotEnumerate]
        [ParamCount(7)]
        public static JSObject UTC(Arguments dateTime)
        {
            try
            {
                return dateToMilliseconds(
                    Tools.JSObjectToInt64(dateTime[0], 1),
                    Tools.JSObjectToInt64(dateTime[1]),
                    Tools.JSObjectToInt64(dateTime[2], 1),
                    Tools.JSObjectToInt64(dateTime[3]),
                    Tools.JSObjectToInt64(dateTime[4]),
                    Tools.JSObjectToInt64(dateTime[5]),
                    Tools.JSObjectToInt64(dateTime[6])) - _unixTimeBase;
            }
            catch
            {
                return double.NaN;
            }
        }
    }
}