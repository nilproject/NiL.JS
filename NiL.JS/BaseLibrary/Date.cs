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
        [Obsolete("Use GlobalContext.CurrentTimeZone instead")]
        [Hidden]
        public static TimeZoneInfo CurrentTimeZone => Context.CurrentGlobalContext.CurrentTimeZone;

        private const long _timeAccuracy = TimeSpan.TicksPerMillisecond;
        private const long _unixTimeBase = 62135596800000;
        private const long _minuteMillisecond = 60 * 1000;
        private const long _hourMilliseconds = 60 * _minuteMillisecond;
        private const long _dayMilliseconds = 24 * _hourMilliseconds;
        private const long _weekMilliseconds = 7 * _dayMilliseconds;
        private const long _400yearsMilliseconds = (365 * 400 + 100 - 3) * _dayMilliseconds;
        private const long _100yearsMilliseconds = (365 * 100 + 25 - 1) * _dayMilliseconds;
        private const long _4yearsMilliseconds = (365 * 4 + 1) * _dayMilliseconds;
        private const long _yearMilliseconds = 365 * _dayMilliseconds;

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
                                                    new[]{ 365 * _dayMilliseconds, 366 * _dayMilliseconds }
                                                };

        private static readonly string[] daysOfWeek = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        private static readonly string[] months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        private long _time;
        private long _timeZoneOffset;
        private bool _error;

        [DoNotEnumerate]
        public Date()
        {
            var now = getNow();
            _time = now.Ticks / 10000;
            _timeZoneOffset = Context.CurrentGlobalContext.CurrentTimeZone.GetUtcOffset(now).Ticks / 10000;
            _time -= _timeZoneOffset;
        }

        [DoNotEnumerate]
        public Date(DateTime dateTime)
        {
            _time = dateTime.Ticks / 10000;
            _timeZoneOffset = Context.CurrentGlobalContext.CurrentTimeZone.GetUtcOffset(dateTime).Ticks / 10000;
            if (dateTime.Kind == DateTimeKind.Local)
                _time -= _timeZoneOffset;
        }

        [DoNotEnumerate]
        public Date(DateTimeOffset dateTimeOffset)
        {
            _time = dateTimeOffset.UtcTicks / 10000;
            _timeZoneOffset = dateTimeOffset.Offset.Ticks / 10000;
        }

        [DoNotEnumerate]
        [ArgumentsCount(7)]
        public Date(Arguments args)
        {
            if (args._iValue == 1)
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
                        _timeZoneOffset = getTimeZoneOffset(_time);
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
                for (var i = 7; i < System.Math.Min(8, args._iValue); i++)
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
                _timeZoneOffset = getTimeZoneOffset(_time);
                _time -= _timeZoneOffset;

                if (_time - _unixTimeBase > 8640000000000000)
                    _error = true;
            }
        }

        private static long getTimeZoneOffset(long time)
        {
            var dateTime = new DateTime(System.Math.Min(System.Math.Max(time * _timeAccuracy, DateTime.MinValue.Ticks), DateTime.MaxValue.Ticks), DateTimeKind.Utc);
            var offset = Context.CurrentGlobalContext.CurrentTimeZone.GetUtcOffset(dateTime).Ticks / _timeAccuracy;
            return offset;
        }

        private void offsetTimeValue(JSValue value, long amort, long mul, bool correctTimeWithTimezone = true)
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
                _time = _time + (-amort + Tools.JSObjectToInt64(value)) * mul;
                _error = isIncorrectTimeRange(_time);

                var oldTzo = _timeZoneOffset;
                _timeZoneOffset = getTimeZoneOffset(_time);
                if (correctTimeWithTimezone)
                {
                    _time -= _timeZoneOffset - oldTzo;
                }
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

            return _time - _unixTimeBase;
        }

        [DoNotEnumerate]
        public static JSValue now()
        {
            var now = getNow();
            var time = now.Ticks / 10000;
            var timeZoneOffset = Context.CurrentGlobalContext.CurrentTimeZone.GetUtcOffset(now).Ticks / 10000;
            return time - timeZoneOffset - _unixTimeBase;
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
            var jsYear = getFullYear();
            if (jsYear._valueType == JSValueType.Integer)
            {
                jsYear._iValue -= 1900;
            }
            else if (jsYear._valueType == JSValueType.Double)
            {
                jsYear._dValue -= 1900;
            }

            return jsYear;
        }

        [DoNotEnumerate]
        public JSValue getFullYear()
        {
            if (_error)
                return Number.NaN;
            return getYearImpl(true);
        }

        private int getYearImpl(bool withTzo)
        {
            var t = _time;
            if (withTzo)
                t += _timeZoneOffset;

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
            if (_error)
                return Number.NaN;

            return getYearImpl(false);
        }

        [DoNotEnumerate]
        public JSValue getMonth()
        {
            if (_error)
                return Number.NaN;

            return getMonthImpl(true);
        }

        private int getMonthImpl(bool withTzo)
        {
            var t = _time;
            if (withTzo)
                t += _timeZoneOffset;

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

            return getDateImpl(true);
        }

        private int getDateImpl(bool withTzo)
        {
            var t = _time;
            if (withTzo)
                t += _timeZoneOffset;

            if (t < 0)
                t = t + (1 - t / (_400yearsMilliseconds * 7)) * (_400yearsMilliseconds * 7);

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
            return getDateImpl(false);
        }

        [DoNotEnumerate]
        public JSValue getDay()
        {
            return getDayImpl(true);
        }

        [DoNotEnumerate]
        public JSValue getUTCDay()
        {
            return getDayImpl(false);
        }

        private int getDayImpl(bool withTzo)
        {
            var t = System.Math.Abs(_time + (withTzo ? _timeZoneOffset : 0));
            return (int)((t / _dayMilliseconds + 1) % 7);
        }

        [DoNotEnumerate]
        public JSValue getHours()
        {
            if (_error)
                return Number.NaN;

            return getHoursImpl(true);
        }

        private int getHoursImpl(bool withTzo)
        {
            var t = System.Math.Abs(_time + (withTzo ? _timeZoneOffset : 0)) % _dayMilliseconds;
            return (int)(t / _hourMilliseconds);
        }

        [DoNotEnumerate]
        public JSValue getUTCHours()
        {
            if (_error)
                return Number.NaN;

            return getHoursImpl(false);
        }

        [DoNotEnumerate]
        public JSValue getMinutes()
        {
            if (_error)
                return Number.NaN;

            return getMinutesImpl(true);
        }

        private int getMinutesImpl(bool withTzo)
        {
            var t = System.Math.Abs(_time + (withTzo ? _timeZoneOffset : 0)) % _hourMilliseconds;
            return (int)(t / _minuteMillisecond);
        }

        [DoNotEnumerate]
        public JSValue getUTCMinutes()
        {
            if (_error)
                return Number.NaN;

            return getMinutesImpl(false);
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
            if (_error)
                return Number.NaN;

            return getMillisecondsImpl();
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
                this._timeZoneOffset = 0;
            }
            else
            {
                this.offsetTimeValue(time, _time - _unixTimeBase, 1, false);
            }

            return valueOf();
        }

        [DoNotEnumerate]
        public JSValue setMilliseconds(JSValue milliseconds)
        {
            offsetTimeValue(milliseconds, getMillisecondsImpl(), 1);
            return valueOf();
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

            return valueOf();
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
                offsetTimeValue(minutes, getMinutesImpl(true), _minuteMillisecond);

            if (!_error)
                setSeconds(seconds, milliseconds);

            return valueOf();
        }

        [DoNotEnumerate]
        public JSValue setUTCMinutes(JSValue minutes, JSValue seconds, JSValue milliseconds)
        {
            if (minutes != null && minutes.Exists)
                offsetTimeValue(minutes, getMinutesImpl(false), _minuteMillisecond);

            if (!_error)
                setUTCSeconds(seconds, milliseconds);

            return valueOf();
        }

        [DoNotEnumerate]
        public JSValue setHours(JSValue hours, JSValue minutes, JSValue seconds, JSValue milliseconds)
        {
            if (hours != null && hours.Exists)
                offsetTimeValue(hours, getHoursImpl(true), _hourMilliseconds);

            setMinutes(minutes, seconds, milliseconds);

            return valueOf();
        }

        [DoNotEnumerate]
        public JSValue setUTCHours(JSValue hours, JSValue minutes, JSValue seconds, JSValue milliseconds)
        {
            if (hours != null && hours.Exists)
                offsetTimeValue(hours, getHoursImpl(false), _hourMilliseconds);

            setUTCMinutes(minutes, seconds, milliseconds);

            return valueOf();
        }

        [DoNotEnumerate]
        public JSValue setDate(JSValue days)
        {
            if (days != null && days.Exists)
                offsetTimeValue(days, getDateImpl(true), _dayMilliseconds);

            return valueOf();
        }

        [DoNotEnumerate]
        public JSValue setUTCDate(JSValue days)
        {
            if (days != null && days.Exists)
                offsetTimeValue(days, getDateImpl(false), _dayMilliseconds);

            return valueOf();
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
                    this._time = this._time - timeToMonthLengths[getMonthImpl(true)][isLeap(getYearImpl(true)) ? 1 : 0];
                    _time = dateToMilliseconds(getYearImpl(true), intMonth, getDateImpl(true), getHoursImpl(true), getMinutesImpl(true), getSecondsImpl(), getMillisecondsImpl());
                }
                else
                    this._time = this._time - timeToMonthLengths[getMonthImpl(true)][isLeap(getYearImpl(true)) ? 1 : 0] + timeToMonthLengths[intMonth][isLeap(getYearImpl(true)) ? 1 : 0];
            }

            if (day != null)
                setDate(day);

            return valueOf();
        }

        [DoNotEnumerate]
        public JSValue setUTCMonth(JSValue monthO, JSValue day)
        {
            return setMonth(monthO, day);
        }

        [DoNotEnumerate]
        public JSValue setYear(JSValue year)
        {
            _time = dateToMilliseconds(Tools.JSObjectToInt64(year) + 1900, getMonthImpl(true), getDateImpl(true), getHoursImpl(true), getMinutesImpl(true), getSecondsImpl(), getMillisecondsImpl());
            return year;
        }

        [DoNotEnumerate]
        public JSValue setUTCYear(JSValue year)
        {
            _time = dateToMilliseconds(Tools.JSObjectToInt64(year) + 1900, getMonthImpl(false), getDateImpl(false), getHoursImpl(false), getMinutesImpl(false), getSecondsImpl(), getMillisecondsImpl());
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
                _time = dateToMilliseconds(Tools.JSObjectToInt64(year), getMonthImpl(false), getDateImpl(false), getHoursImpl(false), getMinutesImpl(false), getSecondsImpl(), getMillisecondsImpl());
                _error = this._time < 5992660800000;
            }

            if (!_error)
                setMonth(month, day);

            return valueOf();
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
            var dt = new DateTime(_time * _timeAccuracy, DateTimeKind.Utc);
            dt = dt.ToLocalTime();
            return dt;
        }

        [DoNotEnumerate]
        public JSValue toLocaleString()
        {
            return stringifyDate(true, false) + " " + stringifyTime(true, false);
        }

        [DoNotEnumerate]
        public JSValue toLocaleTimeString()
        {
            return stringifyTime(true, false);
        }

        [DoNotEnumerate]
        public JSValue toISOString()
        {
            return toIsoString();
        }

        private JSValue toIsoString()
        {
            if (_error || isIncorrectTimeRange(_time))
                ExceptionHelper.Throw(new RangeError("Invalid time value"));

            return getYearImpl(false).ToString("0000") +
            "-" + (this.getMonthImpl(false) + 1).ToString("00") +
            "-" + this.getDateImpl(false).ToString("00") +
            "T" + this.getHoursImpl(false).ToString("00") +
            ":" + this.getMinutesImpl(false).ToString("00") +
            ":" + this.getSecondsImpl().ToString("00") +
            "." + (this.getMillisecondsImpl() / 1000.0).ToString(".000", System.Globalization.CultureInfo.InvariantCulture).Substring(1) +
            "Z";
        }

        private static bool isIncorrectTimeRange(long time)
        {
            return time > 8702135604000000 || time <= -8577864403199999;
        }

        private string stringify(bool withTzo, bool rfc1123)
        {
            if (_error)
                return "Invalid date";

            return stringifyDate(withTzo, rfc1123) + " " + stringifyTime(withTzo, rfc1123);
        }

        private string stringifyDate(bool withTzo, bool rfc1123)
        {
            if (withTzo && rfc1123)
                throw new ArgumentException();

            if (_error)
                return "Invalid date";

            var res =
                daysOfWeek[(getDayImpl(withTzo) + 6) % 7] + (rfc1123 ? ", " : " ")
                + months[getMonthImpl(withTzo)]
                + " " + getDateImpl(withTzo).ToString("00") + " "
                + getYearImpl(withTzo);
            return res;
        }

        private string stringifyTime(bool withTzo, bool rfc1123)
        {
            if (withTzo && rfc1123)
                throw new ArgumentException();

            if (_error)
                return "Invalid date";

            var offset = new TimeSpan(_timeZoneOffset * _timeAccuracy);
            var timeName = Context.CurrentGlobalContext.CurrentTimeZone.IsDaylightSavingTime(new DateTimeOffset(_time * _timeAccuracy, offset)) ? Context.CurrentGlobalContext.CurrentTimeZone.DaylightName : Context.CurrentGlobalContext.CurrentTimeZone.StandardName;
            var res =
                getHoursImpl(withTzo).ToString("00:")
                + getMinutesImpl(withTzo).ToString("00:")
                + getSecondsImpl().ToString("00")
                + " GMT" + (withTzo ? (offset.Ticks > 0 ? "+" : "") + (offset.Hours * 100 + offset.Minutes).ToString("0000") + " (" + timeName + ")" : "");
            return res;
        }

        [Hidden]
        public override string ToString()
        {
            return stringify(true, false);
        }

        [DoNotEnumerate]
        [ArgumentsCount(1)]
        public JSValue toJSON()
        {
            return toISOString();
        }

        [DoNotEnumerate]
        public JSValue toUTCString()
        {
            return stringify(false, true);
        }

        [DoNotEnumerate]
        public JSValue toGMTString()
        {
            return stringify(false, true);
        }

        [DoNotEnumerate]
        public JSValue toTimeString()
        {
            return stringifyTime(true, false);
        }

        [DoNotEnumerate]
        public JSValue toDateString()
        {
            return stringifyDate(true, false);
        }

        [DoNotEnumerate]
        public JSValue toLocaleDateString()
        {
            return stringifyDate(true, false);
        }

        [DoNotEnumerate]
        public static JSValue parse(string dateTime)
        {
            var time = 0L;
            var tzo = 0L;
            if (tryParse(dateTime, out time, out tzo))
                return time - _unixTimeBase;
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
            var allowSlash = true;
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

                if (!Tools.IsWhiteSpace(source[position]) && (source[position] != '/' || !allowSlash))
                {
                    position++;
                    continue;
                }
                else
                {
                    allowSlash &= source[position] == '/';
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
            var tokens = tokensOf(timeStr);
            foreach (var token in tokens)
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
                    || token.StartsWith("ut", StringComparison.OrdinalIgnoreCase)
                    || token.StartsWith("utc", StringComparison.OrdinalIgnoreCase)
                    || token.StartsWith("pst", StringComparison.OrdinalIgnoreCase)
                    || token.StartsWith("pdt", StringComparison.OrdinalIgnoreCase))
                {
                    if (wasTZ)
                        return false;

                    if (token.Length <= 3)
                    {
                        // time zone offset
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
                    continue;

                if (string.Compare("pm", token, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    pm = true;
                    continue;
                }

                return false;
            }

            try
            {
                var now = getNow();
                if (!wasDay && !wasMonth && !wasYear && timeTokens == null)
                    return false;

                if ((wasDay || wasMonth || wasYear)
                    && (!wasDay || !wasMonth || !wasYear))
                    return false;

                if (!wasYear)
                {
                    year = now.Year;
                }
                else
                {
                    if (year < 100)
                        year += (now.Year / 100) * 100;
                }

                time = dateToMilliseconds(year, month - 1, day,
                    timeTokens != null && timeTokens.Length > 0 ? (long)double.Parse(timeTokens[0]) - tzoH : -tzoH,
                    timeTokens != null && timeTokens.Length > 1 ? (long)double.Parse(timeTokens[1]) - tzoM : -tzoM,
                    timeTokens != null && timeTokens.Length > 2 ? (long)double.Parse(timeTokens[2]) : 0,
                    timeTokens != null && timeTokens.Length > 3 ? (long)double.Parse(timeTokens[3]) : 0);

                if (pm)
                    time += _hourMilliseconds * 12;

                timeZoneOffset = Context.CurrentGlobalContext.CurrentTimeZone.GetUtcOffset(new DateTime(time * _timeAccuracy)).Ticks / 10000;

                if (!wasTZ)
                    time -= timeZoneOffset;
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static bool parseIso8601(string timeStr, out long time, out long timeZoneOffset)
        {
            const string format = "YYYY|-MM|-DD|T|HH|:MM|:SS|.S*";

            time = 0;
            timeZoneOffset = 0;

            var year = 0;
            var month = int.MinValue;
            var day = int.MinValue;
            var hour = 0;
            var minutes = 0;
            var seconds = 0;
            var milliseconds = 0;
            var computeTzo = false;
            var part = 0; // 0 - дата, 1 - время, 2 - миллисекунды
            var inManyLoop = false;
            var i = 0;
            var j = 0;
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

                        if (!NumberUtils.IsDigit(timeStr[j]))
                            return false;

                        year = year * 10 + timeStr[j] - '0';
                        break;
                    }

                    case 'm':
                    {
                        if (!NumberUtils.IsDigit(timeStr[j]))
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

                        if (!NumberUtils.IsDigit(timeStr[j]))
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

                        if (!NumberUtils.IsDigit(timeStr[j]))
                            return false;

                        hour = hour * 10 + timeStr[j] - '0';
                        break;
                    }

                    case 's':
                    {
                        if (part < 1)
                            return false;

                        if (!NumberUtils.IsDigit(timeStr[j]))
                        {
                            if (inManyLoop)
                            {
                                inManyLoop = false;
                                i++;
                                j--;
                                continue;
                            }

                            return false;
                        }

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
                        if (part != 0)
                            return false;

                        if (char.ToLowerInvariant(timeStr[j]) != 't' && !char.IsWhiteSpace(timeStr[j]))
                            return false;

                        computeTzo = char.ToLowerInvariant(timeStr[j]) == 't';

                        part++;
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

                    case '*':
                    {
                        i -= 2;
                        j--;
                        inManyLoop = true;
                        continue;
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
                year += (getNow().Year / 100) * 100;

            time = dateToMilliseconds(year, month - 1, day, hour, minutes, seconds, milliseconds);

            if (computeTzo)
            {
                timeZoneOffset = Context.CurrentGlobalContext.CurrentTimeZone.GetUtcOffset(new DateTime(time * _timeAccuracy)).Ticks / 10000;
                if (j >= timeStr.Length)
                    time -= timeZoneOffset;
            }

            return true;
        }

        private static bool parseDateTime(string timeString, out long time, out long tzo)
        {
            try
            {
                var dateTime = DateTime.Parse(timeString);
                time = dateTime.Ticks / _timeAccuracy;
                tzo = Context.CurrentGlobalContext.CurrentTimeZone.GetUtcOffset(dateTime).Ticks / _timeAccuracy;
                if (dateTime.Kind == DateTimeKind.Local)
                {
                    time += tzo;
                }
                return true;
            }
            catch (FormatException)
            {
                time = 0;
                tzo = 0;
                return false;
            }
        }

        private static bool tryParse(string timeString, out long time, out long tzo)
        {
            return parseIso8601(timeString, out time, out tzo) || parseSelf(timeString, out time, out tzo) || parseDateTime(timeString, out time, out tzo);
        }

        private static bool isLeap(int year)
        {
            return (year % 4 == 0 && year % 100 != 0) || year % 400 == 0;
        }

        private static DateTime getNow()
            => TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, Context.CurrentGlobalContext.CurrentTimeZone);

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