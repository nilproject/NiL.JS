using System;
using System.Collections.Generic;
using System.Globalization;
using NiL.JS.Core;
using NiL.JS.Core.Modules;

namespace NiL.JS.BaseLibrary
{
#if !PORTABLE
    [Serializable]
#endif
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

        //private static readonly long[,] monthLengths = { 
        //                                       { 31 * _dayMilliseconds, 31 * _dayMilliseconds}, 
        //                                       { 28 * _dayMilliseconds, 29 * _dayMilliseconds}, 
        //                                       { 31 * _dayMilliseconds, 31 * _dayMilliseconds}, 
        //                                       { 30 * _dayMilliseconds, 30 * _dayMilliseconds}, 
        //                                       { 31 * _dayMilliseconds, 31 * _dayMilliseconds},
        //                                       { 30 * _dayMilliseconds, 30 * _dayMilliseconds}, 
        //                                       { 31 * _dayMilliseconds, 31 * _dayMilliseconds}, 
        //                                       { 31 * _dayMilliseconds, 31 * _dayMilliseconds},
        //                                       { 30 * _dayMilliseconds, 30 * _dayMilliseconds},
        //                                       { 31 * _dayMilliseconds, 31 * _dayMilliseconds},
        //                                       { 30 * _dayMilliseconds, 30 * _dayMilliseconds},
        //                                       { 31 * _dayMilliseconds, 31 * _dayMilliseconds} };

        private static readonly long[][] timeToMonthLengths = { 
                                                new[]{ 0 * _dayMilliseconds, 0 * _dayMilliseconds },
                                                new[]{ 31 * _dayMilliseconds, 31 * _dayMilliseconds },
                                                new[]{ 59 * _dayMilliseconds, 60 * _dayMilliseconds },
                                                new[]{ 90 * _dayMilliseconds, 91 * _dayMilliseconds },
                                                new[]{ 120 * _dayMilliseconds, 121 * _dayMilliseconds },
                                                new[]{ 151 * _dayMilliseconds, 152 * _dayMilliseconds },
                                                new[]{ 181 * _dayMilliseconds, 182 * _dayMilliseconds },
                                                new[]{ 212 * _dayMilliseconds, 213 * _dayMilliseconds },
                                                new[]{ 243 * _dayMilliseconds, 244 * _dayMilliseconds },
                                                new[]{ 273 * _dayMilliseconds, 274 * _dayMilliseconds },
                                                new[]{ 304 * _dayMilliseconds, 305 * _dayMilliseconds },
                                                new[]{ 334 * _dayMilliseconds, 335 * _dayMilliseconds },
                                                new[]{ 365 * _dayMilliseconds, 366 * _dayMilliseconds } };

        private static readonly string[] daysOfWeek = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun", };

        private readonly static string[] months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

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
            time += timeToMonthLengths[month][isLeap];
            time += day * _dayMilliseconds;
            time += hour * _hourMilliseconds;
            time += minute * _minuteMillisecond;
            time += second * 1000;
            time += millisecond;
            return time;
        }

        private static IEnumerable<string> tokensOf(string source)
        {
            int position = 0;
            int prewPos = 0;
            while (position < source.Length)
            {
                if (source[position] == '(' && (prewPos == position || source.IndexOf(':', prewPos, position - prewPos) == -1))
                {
                    if (prewPos != position)
                    {
                        yield return source.Substring(prewPos, position - prewPos);
                        prewPos = position;
                    }
                    int depth = 1;
                    position++;
                    while (depth > 0 && position < source.Length)
                    {
                        switch (source[position++])
                        {
                            case '(':
                                depth++;
                                break;
                            case ')':
                                depth--;
                                break;
                        }
                    }
                    prewPos = position;
                    continue;
                }
                if (!char.IsWhiteSpace(source[position]))
                {
                    position++;
                    continue;
                }
                if (prewPos != position)
                {
                    yield return source.Substring(prewPos, position - prewPos);
                    prewPos = position;
                }
                else
                    prewPos = ++position;
            }
            if (prewPos != position)
                yield return source.Substring(prewPos);
        }

        private static int indexOf(IList<string> list, string value, bool ignoreCase)
        {
            for (var i = 0; i < list.Count; i++)
                if (string.Compare(list[i], value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
                    return i;
            return -1;
        }

        private static bool parseSelf(string timeStr, out long time, out long timeZoneOffset)
        {
            timeStr = timeStr.Trim(Tools.TrimChars);
            if (string.IsNullOrEmpty(timeStr))
            {
                time = 0;
                timeZoneOffset = 0;
                return false;
            }
            time = 0;
            timeZoneOffset = 0;
            bool wasForceMonth = false;
            bool wasMonth = false;
            bool wasDay = false;
            bool wasYear = false;
            bool wasTZ = false;
            bool wasTzo = false;
            int month = 0;
            int year = 0;
            int day = 1;
            string[] timeTokens = null;
            int tzoH = 0;
            int tzoM = 0;
            int temp = 0;
            bool pm = false;
            foreach (var token in tokensOf(timeStr))
            {
                if (indexOf(daysOfWeek, token, true) != -1)
                    continue;
                var index = indexOf(months, token, true);
                if (index != -1)
                {
                    if (wasMonth)
                    {
                        if (wasForceMonth
                            || (wasDay && wasYear))
                            return false;
                        if (!wasDay)
                        {
                            day = month;
                            wasDay = true;
                        }
                        else if (!wasYear)
                        {
                            year = month;
                            wasYear = true;
                        }
                        else return false;
                    }
                    wasForceMonth = true;
                    wasMonth = true;
                    month = index + 1;
                    continue;
                }
                if (int.TryParse(token, out index))
                {
                    if (!wasMonth && index <= 12 && index > 0)
                    {
                        month = index;
                        wasMonth = true;
                        continue;
                    }
                    if (!wasDay && index > 0 && index <= 31)
                    {
                        day = index;
                        wasDay = true;
                        continue;
                    }
                    if (!wasYear)
                    {
                        if ((wasDay || wasMonth)
                            && (!wasDay || !wasMonth))
                            return false;
                        year = index;
                        wasYear = true;
                        continue;
                    }
                    return false;
                }
                if (token.IndexOf(':') != -1)
                {
                    if (timeTokens != null)
                        return false;
                    timeTokens = token.Split(':');
                    continue;
                }
                if (token.StartsWith("gmt", StringComparison.OrdinalIgnoreCase)
                    || (token.StartsWith("ut", StringComparison.OrdinalIgnoreCase) && (token.Length == 2 || token[2] == 'c' || token[2] == 'C'))
                    || token.StartsWith("pst", StringComparison.OrdinalIgnoreCase)
                    || token.StartsWith("pdt", StringComparison.OrdinalIgnoreCase))
                {
                    if (wasTZ)
                        return false;
                    if (token.Length <= 3)
                    {
                        //var tzo = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                        //tzoH += tzo.Hours;
                        //tzoM += tzo.Minutes;
                    }
                    else
                    {
                        if (wasTzo)
                            return false;
                        if (!int.TryParse(token.Substring(3), out temp))
                            return false;
                        tzoM += temp % 100;
                        tzoH += temp / 100;
                    }
                    if (token.StartsWith("pst", StringComparison.OrdinalIgnoreCase))
                        tzoH -= 8;
                    if (token.StartsWith("pdt", StringComparison.OrdinalIgnoreCase))
                        tzoH -= 7;
                    wasTZ = true;
                    continue;
                }
                if (!wasTzo && (token[0] == '+' || token[0] == '-') && int.TryParse(token.Substring(3), out temp))
                {
                    tzoM += temp % 100;
                    tzoH += temp / 100;
                    wasTzo = true;
                    wasTZ = true;
                    continue;
                }
                if (string.Compare("am", token, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    continue;
                }
                if (string.Compare("pm", token, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    pm = true;
                    continue;
                }
                if (wasDay)
                    return false;
            }
            try
            {
                if ((wasDay || wasMonth || wasYear)
                    && (!wasDay || !wasMonth || !wasYear))
                    return false;
                if (year < 100)
                    year += (DateTime.Now.Year / 100) * 100;
                time = dateToMilliseconds(year, month - 1, day,
                    timeTokens != null && timeTokens.Length > 0 ? (long)double.Parse(timeTokens[0]) - tzoH : -tzoH,
                    timeTokens != null && timeTokens.Length > 1 ? (long)double.Parse(timeTokens[1]) - tzoM : -tzoM,
                    timeTokens != null && timeTokens.Length > 2 ? (long)double.Parse(timeTokens[2]) : 0,
                    timeTokens != null && timeTokens.Length > 3 ? (long)double.Parse(timeTokens[3]) : 0
                    );
                if (pm)
                    time += _hourMilliseconds * 12;
                timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Ticks / 10000;
                if (wasTZ)
                {
                    time += timeZoneOffset;
                }
            }
            catch
            {
                return false;
            }
            return true;
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

        private static bool tryParse(string timeString, out long time, out long tzo)
        {
            return (parseByFormat("YYYY-MM-DDTHH:MM:SS.SSS", timeString, out time, out tzo)
             || parseByFormat("YYYY-MM-DDTHH:MM:SS", timeString, out time, out tzo)
             || parseByFormat("YYYY-MM-DDTHH:MM", timeString, out time, out tzo)
             || parseByFormat("YYYY-MM-DDTHH", timeString, out time, out tzo)
             || parseByFormat("YYYY-MM-DD", timeString, out time, out tzo)
             || parseByFormat("YYYY-MM", timeString, out time, out tzo)
             || parseByFormat("YYYY", timeString, out time, out tzo)
             || parseSelf(timeString, out time, out tzo));
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
            timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Ticks / 10000;
        }

        [DoNotEnumerate]
        [ArgumentsLength(7)]
        public Date(Arguments args)
        {
            if (args.length == 1)
            {
                var arg = args[0];
                if (arg.valueType >= JSValueType.Object)
                    arg = arg.ToPrimitiveValue_Value_String();
                switch (arg.valueType)
                {
                    case JSValueType.Int:
                    case JSValueType.Bool:
                    case JSValueType.Double:
                        {
                            var timeValue = Tools.JSObjectToDouble(arg);
                            if (double.IsNaN(timeValue) || double.IsInfinity(timeValue))
                            {
                                error = true;
                                break;
                            }
                            time = (long)timeValue + _unixTimeBase;
                            timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Ticks / 10000;
                            time += timeZoneOffset;
                            break;
                        }
                    case JSValueType.String:
                        {
                            error = !tryParse(args.a0.ToString(), out time, out timeZoneOffset);
                            break;
                        }
                }
            }
            else
            {
                for (var i = 0; i < 9 && !error; i++)
                {
                    if (args[i].IsExist && !args[i].IsDefinded)
                    {
                        error = true;
                        return;
                    }
                }
                long y = Tools.JSObjectToInt64(args[0], 1, true);
                long m = Tools.JSObjectToInt64(args[1], 0, true);
                long d = Tools.JSObjectToInt64(args[2], 1, true);
                long h = Tools.JSObjectToInt64(args[3], 0, true);
                long n = Tools.JSObjectToInt64(args[4], 0, true);
                long s = Tools.JSObjectToInt64(args[5], 0, true);
                long ms = Tools.JSObjectToInt64(args[6], 0, true);
                if (y == long.MaxValue
                    || y == long.MinValue)
                {
                    error = true;
                    return;
                }
                if (y > 9999999
                    || y < -9999999)
                {
                    error = true;
                    return;
                }
                if (m == long.MaxValue
                    || m == long.MinValue)
                {
                    error = true;
                    return;
                }
                if (d == long.MaxValue
                    || d == long.MinValue)
                {
                    error = true;
                    return;
                }
                if (h == long.MaxValue
                    || h == long.MinValue)
                {
                    error = true;
                    return;
                }
                if (n == long.MaxValue
                    || n == long.MinValue)
                {
                    error = true;
                    return;
                }
                if (s == long.MaxValue
                    || s == long.MinValue)
                {
                    error = true;
                    return;
                }
                if (ms == long.MaxValue
                    || ms == long.MinValue)
                {
                    error = true;
                    return;
                }
                for (var i = 7; i < System.Math.Min(8, args.length); i++)
                {
                    var t = Tools.JSObjectToInt64(args[i], 0, true);
                    if (t == long.MaxValue
                    || t == long.MinValue)
                    {
                        error = true;
                        return;
                    }
                }
                if (y < 100)
                    y += 1900;
                time = dateToMilliseconds(y, m, d, h, n, s, ms);
                timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Ticks / 10000;
                //time += timeZoneOffset;
            }
        }

        private void offsetTimeValue(JSValue value, long amort, long mul)
        {
            if (value == null
               || !value.IsDefinded
               || (value.valueType == JSValueType.Double && (double.IsNaN(value.dValue) || double.IsInfinity(value.dValue))))
            {
                error = true;
                time = 0;
            }
            else
            {
                this.time = this.time + (-amort + Tools.JSObjectToInt64(value)) * mul;
                if (this.time < 5992660800000)
                    error = true;
            }
        }

        [DoNotEnumerate]
        public JSValue valueOf()
        {
            return getTime();
        }

        [DoNotEnumerate]
        public JSValue getTime()
        {
            if (error)
                return double.NaN;
            return time - timeZoneOffset - _unixTimeBase;
        }

        [DoNotEnumerate]
        public static JSValue now()
        {
            var time = DateTime.Now.Ticks / 10000;
            var timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Ticks / 10000;
            return time + timeZoneOffset - _unixTimeBase;
        }

        [DoNotEnumerate]
        public JSValue getTimezoneOffset()
        {
            if (error)
                return Number.NaN;
            var res = -timeZoneOffset / _minuteMillisecond;
            return (int)res;
        }

        [DoNotEnumerate]
        public JSValue getYear()
        {
            return getFullYear();
        }

        [DoNotEnumerate]
        public JSValue getFullYear()
        {
            if (error)
                return Number.NaN;
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
        public JSValue getUTCFullYear()
        {
            return getFullYear();
        }

        [DoNotEnumerate]
        public JSValue getMonth()
        {
            if (error)
                return Number.NaN;
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
            while (timeToMonthLengths[m][isLeap] <= t)
                m++;
            return m - 1;
        }

        [DoNotEnumerate]
        public JSValue getUTCMonth()
        {
            return getMonth();
        }

        [DoNotEnumerate]
        public JSValue getDate()
        {
            if (error)
                return Number.NaN;
            return getDateImpl();
        }

        private int getDateImpl()
        {
            var t = time;
            if (t < 0)
                t = t + (1 - time / (_400yearsMilliseconds * 7)) * (_400yearsMilliseconds * 7);
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
            while (timeToMonthLengths[m][isLeap] <= t)
                m++;
            if (m > 0)
                t -= timeToMonthLengths[m - 1][isLeap];
            return (int)(t / _dayMilliseconds + 1);
        }

        [DoNotEnumerate]
        public JSValue getUTCDate()
        {
            return getDate();
        }

        [DoNotEnumerate]
        public JSValue getDay()
        {
            return (int)((time / _dayMilliseconds + 1) % 7);
        }

        [DoNotEnumerate]
        public JSValue getUTCDay()
        {
            return (int)((time / _dayMilliseconds + 1) % 7);
        }

        [DoNotEnumerate]
        public JSValue getHours()
        {
            if (error)
                return Number.NaN;
            return getHoursImpl();
        }

        private int getHoursImpl()
        {
            var t = System.Math.Abs(time) % _dayMilliseconds;
            return (int)(t / _hourMilliseconds);
        }

        [DoNotEnumerate]
        public JSValue getUTCHours()
        {
            return getHours();
        }

        [DoNotEnumerate]
        public JSValue getMinutes()
        {
            if (error)
                return Number.NaN;
            return getMinutesImpl();
        }

        private int getMinutesImpl()
        {
            var t = System.Math.Abs(time) % _hourMilliseconds;
            return (int)(t / _minuteMillisecond);
        }

        [DoNotEnumerate]
        public JSValue getUTCMinutes()
        {
            if (error)
                return Number.NaN;
            return getMinutes();
        }

        [DoNotEnumerate]
        public JSValue getSeconds()
        {
            if (error)
                return Number.NaN;
            return getSecondsImpl();
        }

        private int getSecondsImpl()
        {
            var t = System.Math.Abs(time);
            t %= _minuteMillisecond;
            return (int)(t / 1000);
        }

        [DoNotEnumerate]
        public JSValue getUTCSeconds()
        {
            return getSeconds();
        }

        [DoNotEnumerate]
        public JSValue getMilliseconds()
        {
            if (error)
                return Number.NaN;
            return getMillisecondsImpl();
        }

        private int getMillisecondsImpl()
        {
            var t = System.Math.Abs(time);
            t %= _minuteMillisecond;
            return (int)(t % 1000);
        }

        [DoNotEnumerate]
        public JSValue getUTCMilliseconds()
        {
            return getMilliseconds();
        }

        [DoNotEnumerate]
        public JSValue setTime(JSValue time)
        {
            if (time == null
                || !time.IsDefinded
                || (time.valueType == JSValueType.Double && (double.IsNaN(time.dValue) || double.IsInfinity(time.dValue))))
            {
                error = true;
                time = 0;
            }
            else
            {
                this.time = Tools.JSObjectToInt64(time) + _unixTimeBase + timeZoneOffset;
                error = this.time < 5992660800000;
            }
            return getTime();
        }

        [DoNotEnumerate]
        public JSValue setMilliseconds(JSValue milliseconds)
        {
            offsetTimeValue(milliseconds, getMillisecondsImpl(), 1);
            return getMilliseconds();
        }

        [DoNotEnumerate]
        public JSValue setUTCMilliseconds(JSValue milliseconds)
        {
            return setMilliseconds(milliseconds);
        }

        [DoNotEnumerate]
        public JSValue setSeconds(JSValue seconds, JSValue milliseconds)
        {
            if (seconds != null && seconds.IsExist)
                offsetTimeValue(seconds, getSecondsImpl(), 1000);
            if (!error && milliseconds != null && milliseconds.IsExist)
                setMilliseconds(milliseconds);
            return getSeconds();
        }

        [DoNotEnumerate]
        public JSValue setUTCSeconds(JSValue seconds, JSValue milliseconds)
        {
            return setSeconds(seconds, milliseconds);
        }

        [DoNotEnumerate]
        public JSValue setMinutes(JSValue minutes, JSValue seconds, JSValue milliseconds)
        {
            if (minutes != null && minutes.IsExist)
                offsetTimeValue(minutes, getMinutesImpl(), _minuteMillisecond);
            if (!error)
                setSeconds(seconds, milliseconds);
            return getMinutes();
        }

        [DoNotEnumerate]
        public JSValue setUTCMinutes(JSValue minutes, JSValue seconds, JSValue milliseconds)
        {
            return setMinutes(minutes, seconds, milliseconds);
        }

        [DoNotEnumerate]
        public JSValue setHours(JSValue hours, JSValue minutes, JSValue seconds, JSValue milliseconds)
        {
            if (hours != null && hours.IsExist)
                offsetTimeValue(hours, getHoursImpl(), _hourMilliseconds);
            setMinutes(minutes, seconds, milliseconds);
            return getHours();
        }

        [DoNotEnumerate]
        public JSValue setUTCHours(JSValue hours, JSValue minutes, JSValue seconds, JSValue milliseconds)
        {
            return setHours(hours, minutes, seconds, milliseconds);
        }

        [DoNotEnumerate]
        public JSValue setDate(JSValue days)
        {
            if (days != null && days.IsExist)
                offsetTimeValue(days, getDateImpl(), _dayMilliseconds);
            return getDate();
        }

        [DoNotEnumerate]
        public JSValue setUTCDate(JSValue days)
        {
            return setDate(days);
        }

        [DoNotEnumerate]
        public JSValue setMonth(JSValue monthO, JSValue day)
        {
            if (monthO != null)
            {
                if (!monthO.IsDefinded
                || (monthO.valueType == JSValueType.Double && (double.IsNaN(monthO.dValue) || double.IsInfinity(monthO.dValue))))
                {
                    error = true;
                    time = 0;
                    return Number.NaN;
                }
                var month = Tools.JSObjectToInt64(monthO);
                if (month < 0 || month > 12)
                {
                    this.time = this.time - timeToMonthLengths[getMonthImpl()][isLeap(getYearImpl()) ? 1 : 0];
                    time = dateToMilliseconds(getYearImpl(), month, getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
                }
                else
                    this.time = this.time - timeToMonthLengths[getMonthImpl()][isLeap(getYearImpl()) ? 1 : 0] + timeToMonthLengths[month][isLeap(getYearImpl()) ? 1 : 0];
            }
            if (day != null)
                setDate(day);
            return getMonth();
        }

        [DoNotEnumerate]
        public JSValue setUTCMonth(JSValue monthO, JSValue day)
        {
            return setMonth(monthO, day);
        }

        [DoNotEnumerate]
        public JSValue setYear(JSValue year)
        {
            time = dateToMilliseconds(Tools.JSObjectToInt64(year) + 1900, getMonthImpl(), getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
            return year;
        }

        [DoNotEnumerate]
        public JSValue setUTCYear(JSValue year)
        {
            time = dateToMilliseconds(Tools.JSObjectToInt64(year) + 1900, getMonthImpl(), getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
            return year;
        }

        [DoNotEnumerate]
        public JSValue setFullYear(JSValue year, JSValue month, JSValue day)
        {
            if (year != null && year.IsExist)
            {
                if (!year.IsDefinded
                   || (year.valueType == JSValueType.Double && (double.IsNaN(year.dValue) || double.IsInfinity(year.dValue))))
                {
                    error = true;
                    time = 0;
                    return Number.NaN;
                }
                time = dateToMilliseconds(Tools.JSObjectToInt64(year), getMonthImpl(), getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
                error = this.time < 5992660800000;
            }
            if (!error)
                setMonth(month, day);
            return getFullYear();
        }

        [DoNotEnumerate]
        public JSValue setUTCFullYear(JSValue year, JSValue month, JSValue day)
        {
            return setFullYear(year, month, day);
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public JSValue toString()
        {
            return ToString();
        }

        [Hidden]
        public DateTime ToDateTime()
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
            dt = dt.AddHours(getHoursImpl());
            dt = dt.AddMinutes(getMinutesImpl());
            dt = dt.AddSeconds(getSecondsImpl());
            dt = dt.AddMilliseconds(getMillisecondsImpl());
            return dt;
        }

        [DoNotEnumerate]
        public JSValue toLocaleString()
        {
            var dt = ToDateTime();
#if !PORTABLE
            return dt.ToLongDateString() + " " + dt.ToLongTimeString();
#else
            return dt.ToString();
            /*
            var res =
                dt.ToString("dddd MMMM")
                + " " + getDateImpl() + " "
                + getYearImpl() + " "
                + getHoursImpl().ToString("00:")
                + getMinutesImpl().ToString("00:")
                + getSecondsImpl().ToString("00")
                + " GMT" + (timeZoneOffset.Ticks > 0 ? "+" : "") + (timeZoneOffset.Hours * 100 + timeZoneOffset.Minutes).ToString("0000") + " (" + TimeZoneInfo.Local.DaylightName + ")";
            return res;
            */
#endif
        }

        [DoNotEnumerate]
        public JSValue toLocaleTimeString()
        {
            var res =
                getHoursImpl().ToString("00:")
                + getMinutesImpl().ToString("00:")
                + getSecondsImpl().ToString("00");
            return res;
        }

        [DoNotEnumerate]
        public JSValue toISOString()
        {
            try
            {
                time -= timeZoneOffset;
                if (time > 8702135600400000 || time < -8577864403200000 || error)
                    throw new JSException(new RangeError("Invalid time value"));
                var y = getYearImpl();

                return y +
                        "-" + (this.getMonthImpl() + 1).ToString("00") +
                        "-" + this.getDateImpl().ToString("00") +
                        "T" + this.getHoursImpl().ToString("00") +
                        ":" + this.getMinutesImpl().ToString("00") +
                        ":" + this.getSecondsImpl().ToString("00") +
                        "." + (this.getMillisecondsImpl() / 1000.0).ToString(".000", System.Globalization.CultureInfo.InvariantCulture).Substring(1) +
                        "Z";
            }
            finally
            {
                time += timeZoneOffset;
            }
        }

        [DoNotEnumerate]
        public JSValue toJSON(JSValue obj)
        {
            return toISOString();
        }

        [DoNotEnumerate]
        public JSValue toUTCString()
        {
            return ToString();
        }

        [DoNotEnumerate]
        public JSValue toGMTString()
        {
            return ToString();
        }

        [DoNotEnumerate]
        public JSValue toTimeString()
        {
            var offset = new TimeSpan(timeZoneOffset * 10000);
            var res =
                getHoursImpl().ToString("00:")
                + getMinutesImpl().ToString("00:")
                + getSecondsImpl().ToString("00")
                + " GMT" + (offset.Ticks > 0 ? "+" : "") + (offset.Hours * 100 + offset.Minutes).ToString("0000") + " (" + TimeZoneInfo.Local.DaylightName + ")";
            return res;
        }

        [DoNotEnumerate]
        public JSValue toDateString()
        {
            var res =
                daysOfWeek[(System.Math.Abs(time) % _weekMilliseconds) / _dayMilliseconds] + " "
                + months[getMonthImpl()]
                + " " + getDateImpl().ToString("00") + " "
                + getYearImpl();
            return res;
        }

        [DoNotEnumerate]
        public JSValue toLocaleDateString()
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
#if PORTABLE
            return dt.ToString();
#else
            return dt.ToLongDateString();
#endif
            //var res =
            //    dt.ToString("dddd, MMMM")
            //    + " " + getDateImpl() + ", "
            //    + getYearImpl();
            //return res;
        }

        [Hidden]
        public override string ToString()
        {
            if (error)
                return "Invalid date";
            var offset = new TimeSpan(timeZoneOffset * 10000);
            var res =
                daysOfWeek[System.Math.Abs(time) / _dayMilliseconds % 7] + " "
                + months[getMonthImpl()]
                + " " + getDateImpl().ToString("00") + " "
                + getYearImpl() + " "
                + getHoursImpl().ToString("00:")
                + getMinutesImpl().ToString("00:")
                + getSecondsImpl().ToString("00")
                + " GMT" + (offset.Ticks > 0 ? "+" : "") + (offset.Hours * 100 + offset.Minutes).ToString("0000") + " (" + TimeZoneInfo.Local.DaylightName + ")";
            return res;
        }

        [DoNotEnumerate]
        public static JSValue parse(string dateTime)
        {
            var time = 0L;
            var tzo = 0L;
            if (tryParse(dateTime, out time, out tzo))
                return time - tzo - _unixTimeBase;
            return double.NaN;
        }

        [DoNotEnumerate]
        [ArgumentsLength(7)]
        public static JSValue UTC(Arguments dateTime)
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

        [Hidden]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [Hidden]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}