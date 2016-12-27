using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
{
#if !(PORTABLE || NETCORE)
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
            int prevPos = 0;
            while (position < source.Length)
            {
                if (source[position] == '(' && (prevPos == position || source.IndexOf(':', prevPos, position - prevPos) == -1))
                {
                    if (prevPos != position)
                    {
                        yield return source.Substring(prevPos, position - prevPos);
                        prevPos = position;
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
                    prevPos = position;
                    continue;
                }
                if (!Tools.IsWhiteSpace(source[position]))
                {
                    position++;
                    continue;
                }
                if (prevPos != position)
                {
                    yield return source.Substring(prevPos, position - prevPos);
                    prevPos = position;
                }
                else
                    prevPos = ++position;
            }
            if (prevPos != position)
                yield return source.Substring(prevPos);
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
                        else
                            return false;
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

        private static bool parseIso8601(string timeStr, out long time, out long timeZoneOffset)
        {
            const string format = "YYYY|-MM|-DD|THH|:MM|:SS|.SSS";

            var year = 0;
            var month = int.MinValue;
            var day = int.MinValue;
            var hour = 0;
            var minutes = 0;
            var seconds = 0;
            var milliseconds = 0;
            time = 0;
            timeZoneOffset = 0;
            int part = 0; // 0 - дата, 1 - время, 2 - миллисекунды
            int i = 0, j = 0;
            for (; i < format.Length; i++, j++)
            {
                if (timeStr.Length <= j)
                {
                    if (format[i] == '|')
                        break;
                    else
                        return false;
                }

                switch (char.ToLowerInvariant(format[i]))
                {
                    case 'y':
                        {
                            if (part != 0)
                                return false;
                            if (!Tools.IsDigit(timeStr[j]))
                                return false;
                            year = year * 10 + timeStr[j] - '0';
                            break;
                        }
                    case 'm':
                        {
                            if (!Tools.IsDigit(timeStr[j]))
                                return false;
                            switch (part)
                            {
                                case 0:
                                    {
                                        if (month == int.MinValue)
                                            month = 0;
                                        month = month * 10 + timeStr[j] - '0';
                                        break;
                                    }
                                case 1:
                                    {
                                        minutes = minutes * 10 + timeStr[j] - '0';
                                        break;
                                    }
                                default:
                                    return false;
                            }
                            break;
                        }
                    case 'd':
                        {
                            if (part != 0)
                                return false;
                            if (!Tools.IsDigit(timeStr[j]))
                                return false;
                            if (day == int.MinValue)
                                day = 0;
                            day = day * 10 + timeStr[j] - '0';
                            break;
                        }
                    case 'h':
                        {
                            if (part != 1)
                                return false;
                            if (!Tools.IsDigit(timeStr[j]))
                                return false;
                            hour = hour * 10 + timeStr[j] - '0';
                            break;
                        }
                    case 's':
                        {
                            if (part < 1)
                                return false;
                            if (!Tools.IsDigit(timeStr[j]))
                                return false;
                            if (part == 1)
                                seconds = seconds * 10 + timeStr[j] - '0';
                            else
                                milliseconds = milliseconds * 10 + timeStr[j] - '0';
                            break;
                        }
                    case ':':
                        {
                            if (part != 1)
                                return false;
                            if (format[i] != timeStr[j])
                                return false;
                            break;
                        }
                    case '/':
                        {
                            if (part != 0)
                                return false;
                            if (format[i] != timeStr[j])
                                return false;
                            break;
                        }
                    case ' ':
                        {
                            if (format[i] != timeStr[j])
                                return false;
                            while (j < timeStr.Length && Tools.IsWhiteSpace(timeStr[j]))
                                j++;
                            j--;
                            break;
                        }
                    case '-':
                        {
                            if (format[i] != timeStr[j])
                                return false;
                            break;
                        }
                    case 't':
                        {
                            if ('t' != char.ToLowerInvariant(timeStr[j]))
                                return false;
                            if (part == 0)
                                part++;
                            else
                                return false;
                            break;
                        }
                    case '.':
                        {
                            if ('.' != timeStr[j])
                            {
                                if (char.ToLowerInvariant(timeStr[j]) == 'z')
                                {
                                    j = timeStr.Length;
                                    i = format.Length;
                                    break;
                                }
                                return false;
                            }
                            if (part != 1)
                                return false;
                            else
                            {
                                part++;
                                break;
                            }
                        }
                    case '|':
                        {
                            j--;
                            break;
                        }
                    default:
                        return false;
                }
            }
            if (j < timeStr.Length && char.ToLowerInvariant(timeStr[j]) != 'z')
                return false;
            if (month == int.MinValue)
                month = 1;
            if (day == int.MinValue)
                day = 1;
            if (year < 100)
                year += (DateTime.Now.Year / 100) * 100;
            time = dateToMilliseconds(year, month - 1, day, hour, minutes, seconds, milliseconds);
            timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(new DateTime(time * 10000)).Ticks / 10000;
            time += timeZoneOffset;
            return true;
        }

        private static bool tryParse(string timeString, out long time, out long tzo)
        {
            return parseIso8601(timeString, out time, out tzo) || parseSelf(timeString, out time, out tzo);
        }

        private static bool isLeap(int year)
        {
            return (year % 4 == 0 && year % 100 != 0) || year % 400 == 0;
        }

        private long _time;
        private long _timeZoneOffset;

        private bool _error = false;

        [DoNotEnumerate]
        public Date()
        {
            _time = DateTime.Now.Ticks / 10000;
            _timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Ticks / 10000;
        }

        [Hidden]
        public Date(long ticks, long timeZoneOffset)
        {
            _time = ticks / 10000;
            _timeZoneOffset = timeZoneOffset / 10000;
        }

        [DoNotEnumerate]
        [ArgumentsCount(7)]
        public Date(Arguments args)
        {
            if (args.length == 1)
            {
                var arg = args[0];
                if (arg._valueType >= JSValueType.Object)
                    arg = arg.ToPrimitiveValue_Value_String();

                switch (arg._valueType)
                {
                    case JSValueType.Integer:
                    case JSValueType.Boolean:
                    case JSValueType.Double:
                        {
                            var timeValue = Tools.JSObjectToDouble(arg);
                            if (double.IsNaN(timeValue) || double.IsInfinity(timeValue))
                            {
                                _error = true;
                                break;
                            }
                            _time = (long)timeValue + _unixTimeBase;
                            _timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Ticks / 10000;
                            _time += _timeZoneOffset;
                            break;
                        }
                    case JSValueType.String:
                        {
                            _error = !tryParse(arg.ToString(), out _time, out _timeZoneOffset);
                            break;
                        }
                }
            }
            else
            {
                for (var i = 0; i < 9 && !_error; i++)
                {
                    if (args[i].Exists && !args[i].Defined)
                    {
                        _error = true;
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
                    _error = true;
                    return;
                }
                if (y > 9999999
                    || y < -9999999)
                {
                    _error = true;
                    return;
                }
                if (m == long.MaxValue
                    || m == long.MinValue)
                {
                    _error = true;
                    return;
                }
                if (d == long.MaxValue
                    || d == long.MinValue)
                {
                    _error = true;
                    return;
                }
                if (h == long.MaxValue
                    || h == long.MinValue)
                {
                    _error = true;
                    return;
                }
                if (n == long.MaxValue
                    || n == long.MinValue)
                {
                    _error = true;
                    return;
                }
                if (s == long.MaxValue
                    || s == long.MinValue)
                {
                    _error = true;
                    return;
                }
                if (ms == long.MaxValue
                    || ms == long.MinValue)
                {
                    _error = true;
                    return;
                }
                for (var i = 7; i < System.Math.Min(8, args.length); i++)
                {
                    var t = Tools.JSObjectToInt64(args[i], 0, true);
                    if (t == long.MaxValue
                    || t == long.MinValue)
                    {
                        _error = true;
                        return;
                    }
                }
                if (y < 100)
                    y += 1900;
                _time = dateToMilliseconds(y, m, d, h, n, s, ms);
                _timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Ticks / 10000;
            }
        }

        private void offsetTimeValue(JSValue value, long amort, long mul)
        {
            if (value == null
               || !value.Defined
               || (value._valueType == JSValueType.Double && (double.IsNaN(value._dValue) || double.IsInfinity(value._dValue))))
            {
                _error = true;
                _time = 0;
            }
            else
            {
                this._time = this._time + (-amort + Tools.JSObjectToInt64(value)) * mul;
                if (this._time < 5992660800000)
                    _error = true;
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
            if (_error)
                return double.NaN;
            return _time - _timeZoneOffset - _unixTimeBase;
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
            if (_error)
                return Number.NaN;
            var res = -_timeZoneOffset / _minuteMillisecond;
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
            if (_error)
                return Number.NaN;
            return getYearImpl();
        }

        private int getYearImpl()
        {
            var t = _time;
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
            if (_error)
                return Number.NaN;
            return getMonthImpl();
        }

        private int getMonthImpl()
        {
            var t = _time;
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
            if (_error)
                return Number.NaN;
            return getDateImpl();
        }

        private int getDateImpl()
        {
            var t = _time;
            if (t < 0)
                t = t + (1 - _time / (_400yearsMilliseconds * 7)) * (_400yearsMilliseconds * 7);
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
            return (int)((_time / _dayMilliseconds + 1) % 7);
        }

        [DoNotEnumerate]
        public JSValue getUTCDay()
        {
            return (int)((_time / _dayMilliseconds + 1) % 7);
        }

        [DoNotEnumerate]
        public JSValue getHours()
        {
            if (_error)
                return Number.NaN;
            return getHoursImpl();
        }

        private int getHoursImpl()
        {
            var t = System.Math.Abs(_time) % _dayMilliseconds;
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
            if (_error)
                return Number.NaN;
            return getMinutesImpl();
        }

        private int getMinutesImpl()
        {
            var t = System.Math.Abs(_time) % _hourMilliseconds;
            return (int)(t / _minuteMillisecond);
        }

        [DoNotEnumerate]
        public JSValue getUTCMinutes()
        {
            if (_error)
                return Number.NaN;
            return getMinutes();
        }

        [DoNotEnumerate]
        public JSValue getSeconds()
        {
            if (_error)
                return Number.NaN;
            return getSecondsImpl();
        }

        private int getSecondsImpl()
        {
            var t = System.Math.Abs(_time);
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
            if (_error)
                return Number.NaN;
            return getMillisecondsImpl();
        }

        private int getMillisecondsImpl()
        {
            var t = System.Math.Abs(_time);
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
                || !time.Defined
                || (time._valueType == JSValueType.Double && (double.IsNaN(time._dValue) || double.IsInfinity(time._dValue))))
            {
                _error = true;
                this._time = 0;
            }
            else
            {
                this._time = Tools.JSObjectToInt64(time) + _unixTimeBase + _timeZoneOffset;
                _error = this._time < 5992660800000;
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
            if (seconds != null && seconds.Exists)
                offsetTimeValue(seconds, getSecondsImpl(), 1000);
            if (!_error && milliseconds != null && milliseconds.Exists)
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
            if (minutes != null && minutes.Exists)
                offsetTimeValue(minutes, getMinutesImpl(), _minuteMillisecond);
            if (!_error)
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
            if (hours != null && hours.Exists)
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
            if (days != null && days.Exists)
                offsetTimeValue(days, getDateImpl(), _dayMilliseconds);
            return getDate();
        }

        [DoNotEnumerate]
        public JSValue setUTCDate(JSValue days)
        {
            return setDate(days);
        }

        [DoNotEnumerate]
        public JSValue setMonth(JSValue month, JSValue day)
        {
            if (month != null && month.Exists)
            {
                if (!month.Defined
                || (month._valueType == JSValueType.Double && (double.IsNaN(month._dValue) || double.IsInfinity(month._dValue))))
                {
                    _error = true;
                    _time = 0;
                    return Number.NaN;
                }
                var intMonth = Tools.JSObjectToInt64(month);
                if (intMonth < 0 || intMonth > 12)
                {
                    this._time = this._time - timeToMonthLengths[getMonthImpl()][isLeap(getYearImpl()) ? 1 : 0];
                    _time = dateToMilliseconds(getYearImpl(), intMonth, getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
                }
                else
                    this._time = this._time - timeToMonthLengths[getMonthImpl()][isLeap(getYearImpl()) ? 1 : 0] + timeToMonthLengths[intMonth][isLeap(getYearImpl()) ? 1 : 0];
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
            _time = dateToMilliseconds(Tools.JSObjectToInt64(year) + 1900, getMonthImpl(), getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
            return year;
        }

        [DoNotEnumerate]
        public JSValue setUTCYear(JSValue year)
        {
            _time = dateToMilliseconds(Tools.JSObjectToInt64(year) + 1900, getMonthImpl(), getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
            return year;
        }

        [DoNotEnumerate]
        public JSValue setFullYear(JSValue year, JSValue month, JSValue day)
        {
            if (year != null && year.Exists)
            {
                if (!year.Defined
                   || (year._valueType == JSValueType.Double && (double.IsNaN(year._dValue) || double.IsInfinity(year._dValue))))
                {
                    _error = true;
                    _time = 0;
                    return Number.NaN;
                }
                _time = dateToMilliseconds(Tools.JSObjectToInt64(year), getMonthImpl(), getDateImpl(), getHoursImpl(), getMinutesImpl(), getSecondsImpl(), getMillisecondsImpl());
                _error = this._time < 5992660800000;
            }
            if (!_error)
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
            var dt = new DateTime(0, DateTimeKind.Local);
            dt = dt.AddDays(getDateImpl() - 1);
            dt = dt.AddMonths(getMonthImpl());
            dt = dt.AddYears(y - 1);
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
#if !(PORTABLE || NETCORE)
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
                _time -= _timeZoneOffset;
                if (_time > 8702135600400000 || _time < -8577864403200000 || _error)
                    ExceptionHelper.Throw(new RangeError("Invalid time value"));
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
                _time += _timeZoneOffset;
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
            var offset = new TimeSpan(_timeZoneOffset * 10000);
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
                daysOfWeek[(System.Math.Abs(_time) % _weekMilliseconds) / _dayMilliseconds] + " "
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
            dt = dt.AddDays((System.Math.Abs(_time) % _weekMilliseconds) / _dayMilliseconds);
            dt = dt.AddMonths(getMonthImpl());
            dt = dt.AddYears(y);
#if (PORTABLE || NETCORE)
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
            if (_error)
                return "Invalid date";

            var offset = new TimeSpan(_timeZoneOffset * 10000);
            var res =
                daysOfWeek[System.Math.Abs(_time) / _dayMilliseconds % 7] + " "
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
        [ArgumentsCount(7)]
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

        #region Do not remove

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

        #endregion

    }
}