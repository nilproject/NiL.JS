using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;

#if NET40
using NiL.JS.Backward;
#endif

namespace NiL.JS.Core
{
    public sealed class CodeCoordinates
    {
        public int Line { get; private set; }
        public int Column { get; private set; }
        public int Length { get; private set; }

        public CodeCoordinates(int line, int column, int length)
        {
            Line = line;
            Column = column;
            Length = length;
        }

        public override string ToString()
        {
            return "(" + Line + ":" + Column + (Length != 0 ? "*" + Length : "") + ")";
        }

        public static CodeCoordinates FromTextPosition(string text, int position, int length)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (position < 0)
                throw new ArgumentOutOfRangeException("position");
            int line = 1;
            int column = 1;
            for (int i = 0; i < position; i++)
            {
                if (text[i] == '\n')
                {
                    column = 0;
                    line++;
                    if (text[i + 1] == '\r')
                        i++;
                }
                else if (text[i] == '\r')
                {
                    column = 0;
                    line++;
                    if (text[i + 1] == '\n')
                        i++;
                }
                column++;
            }
            return new CodeCoordinates(line, column, length);
        }
    }
    /// <summary>
    /// Содержит функции, используемые на разных этапах выполнения скрипта.
    /// </summary>
    public static class Tools
    {
        [Flags]
        public enum ParseNumberOptions
        {
            None = 0,
            RaiseIfOctal = 1,
            ProcessOctal = 2,
            AllowFloat = 4,
            AllowAutoRadix = 8,
            Default = 2 + 4 + 8
        }

        internal static readonly char[] TrimChars = new[] { '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u0020', '\u00A0', '\u1680', '\u180E', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007', '\u2008', '\u2009', '\u200A', '\u2028', '\u2029', '\u202F', '\u205F', '\u3000', '\uFEFF' };

        internal static readonly char[] NumChars = new[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        //internal static readonly string[] charStrings = (from x in Enumerable.Range(char.MinValue, char.MaxValue) select ((char)x).ToString()).ToArray();

        internal sealed class _ForcedEnumerator<T> : IEnumerator<T>
        {
            private int index;
            private IEnumerable<T> owner;
            private IEnumerator<T> parent;

            public _ForcedEnumerator(IEnumerable<T> owner)
            {
                this.owner = owner;
                this.parent = owner.GetEnumerator();
            }

            #region Члены IEnumerator<T>

            public T Current
            {
                get { return parent.Current; }
            }

            #endregion

            #region Члены IDisposable

            public void Dispose()
            {
                parent.Dispose();
            }

            #endregion

            #region Члены IEnumerator

            object System.Collections.IEnumerator.Current
            {
                get { return parent.Current; }
            }

            public bool MoveNext()
            {
                try
                {
                    var res = parent.MoveNext();
                    if (res)
                        index++;
                    return res;
                }
                catch
                {
                    parent = owner.GetEnumerator();
                    for (int i = 0; i < index && parent.MoveNext(); i++)
                        ;
                    return MoveNext();
                }
            }

            public void Reset()
            {
                parent.Reset();
            }

            #endregion
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static double JSObjectToDouble(JSValue arg)
        {
            do
            {
                if (arg == null)
                    return double.NaN;
                switch (arg.valueType)
                {
                    case JSValueType.Boolean:
                    case JSValueType.Integer:
                        {
                            return arg.iValue;
                        }
                    case JSValueType.Double:
                        {
                            return arg.dValue;
                        }
                    case JSValueType.String:
                        {
                            double x = double.NaN;
                            int ix = 0;
                            string s = (arg.oValue.ToString());
                            if (s.Length > 0 && (Tools.IsWhiteSpace(s[0]) || Tools.IsWhiteSpace(s[s.Length - 1])))
                                s = s.Trim(Tools.TrimChars);
                            if (Tools.ParseNumber(s, ref ix, out x, 0, ParseNumberOptions.AllowFloat | ParseNumberOptions.AllowAutoRadix) && ix < s.Length)
                                return double.NaN;
                            return x;
                        }
                    case JSValueType.Date:
                    case JSValueType.Function:
                    case JSValueType.Object:
                        {
                            if (arg.oValue == null)
                                return 0;
                            arg = arg.ToPrimitiveValue_Value_String();
                            break;
                            //return JSObjectToDouble(arg);
                        }
                    case JSValueType.NotExists:
                    case JSValueType.Undefined:
                    case JSValueType.NotExistsInObject:
                        return double.NaN;
                    default:
                        throw new NotImplementedException();
                }
            } while (true);
        }

        /// <summary>
        /// Преобразует JSObject в значение типа integer.
        /// </summary>
        /// <param name="arg">JSObject, значение которого нужно преобразовать.</param>
        /// <returns>Целочисленное значение, представленное в объекте arg.</returns>
#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static int JSObjectToInt32(JSValue arg)
        {
            if (arg.valueType == JSValueType.Integer)
                return arg.iValue;
            return JSObjectToInt32(arg, 0, false);
        }

        internal static void SkipSpaces(string code, ref int i)
        {
            while (i < code.Length && IsWhiteSpace(code[i]))
                i++;
        }

        /// <summary>
        /// Преобразует JSObject в значение типа integer.
        /// </summary>
        /// <param name="arg">JSObject, значение которого нужно преобразовать.</param>
        /// <param name="nullOrUndef">Значение, которое будет возвращено, если значение arg null или undefined.</param>
        /// <returns>Целочисленное значение, представленное в объекте arg.</returns>
#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static int JSObjectToInt32(JSValue arg, int nullOrUndef)
        {
            return JSObjectToInt32(arg, nullOrUndef, false);
        }

        /// <summary>
        /// Преобразует JSObject в значение типа integer.
        /// </summary>
        /// <param name="arg">JSObject, значение которого нужно преобразовать.</param>
        /// <param name="alternateInfinity">Если истина, для значений +Infinity и -Infinity будут возвращены значения int.MaxValue и int.MinValue соответственно.</param>
        /// <returns>Целочисленное значение, представленное в объекте arg.</returns>
#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static int JSObjectToInt32(JSValue arg, bool alternateInfinity)
        {
            return JSObjectToInt32(arg, 0, alternateInfinity);
        }

        /// <summary>
        /// Преобразует JSObject в значение типа Int32.
        /// </summary>
        /// <param name="arg">JSObject, значение которого нужно преобразовать.</param>
        /// <param name="nullOrUndef">Значение, которое будет возвращено, если значение arg null или undefined.</param>
        /// <param name="alternateInfinity">Если истина, для значений +Infinity и -Infinity будут возвращены значения int.MaxValue и int.MinValue соответственно.</param>
        /// <returns>Целочисленное значение, представленное в объекте arg.</returns>
        public static int JSObjectToInt32(JSValue arg, int nullOrUndef, bool alternateInfinity)
        {
            if (arg == null)
                return nullOrUndef;
            var r = arg;
            switch (r.valueType)
            {
                case JSValueType.Boolean:
                case JSValueType.Integer:
                    {
                        return r.iValue;
                    }
                case JSValueType.Double:
                    {
                        if (double.IsNaN(r.dValue))
                            return 0;
                        if (double.IsInfinity(r.dValue))
                            return alternateInfinity ? double.IsPositiveInfinity(r.dValue) ? int.MaxValue : int.MinValue : 0;
                        return (int)(long)r.dValue;
                    }
                case JSValueType.String:
                    {
                        double x = 0;
                        int ix = 0;
                        string s = (r.oValue.ToString()).Trim();
                        if (!Tools.ParseNumber(s, ref ix, out x, 0, ParseNumberOptions.AllowAutoRadix | ParseNumberOptions.AllowFloat) || ix < s.Length)
                            return 0;
                        if (double.IsNaN(x))
                            return 0;
                        if (double.IsInfinity(x))
                            return alternateInfinity ? double.IsPositiveInfinity(x) ? int.MaxValue : int.MinValue : 0;
                        return (int)x;
                    }
                case JSValueType.Date:
                case JSValueType.Function:
                case JSValueType.Object:
                    {
                        if (r.oValue == null)
                            return nullOrUndef;
                        r = r.ToPrimitiveValue_Value_String();
                        return JSObjectToInt32(r);
                    }
                case JSValueType.NotExists:
                //ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.ReferenceError("Variable not defined.")));
                case JSValueType.Undefined:
                case JSValueType.NotExistsInObject:
                    return nullOrUndef;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Преобразует JSObject в значение типа integer.
        /// </summary>
        /// <param name="arg">JSObject, значение которого нужно преобразовать.</param>
        /// <param name="nullOrUndef">Значение, которое будет возвращено, если значение arg null или undefined.</param>
        /// <returns>Целочисленное значение, представленное в объекте arg.</returns>
#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static long JSObjectToInt64(JSValue arg)
        {
            return JSObjectToInt64(arg, 0, false);
        }

        /// <summary>
        /// Преобразует JSObject в значение типа integer.
        /// </summary>
        /// <param name="arg">JSObject, значение которого нужно преобразовать.</param>
        /// <param name="nullOrUndef">Значение, которое будет возвращено, если значение arg null или undefined.</param>
        /// <returns>Целочисленное значение, представленное в объекте arg.</returns>
#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static long JSObjectToInt64(JSValue arg, long nullOrUndef)
        {
            return JSObjectToInt64(arg, nullOrUndef, false);
        }

        /// <summary>
        /// Преобразует JSObject в значение типа Int64.
        /// </summary>
        /// <param name="arg">JSObject, значение которого нужно преобразовать.</param>
        /// <param name="nullOrUndef">Значение, которое будет возвращено, если значение arg null или undefined.</param>
        /// <param name="alternateInfinity">Если истина, для значений +Infinity и -Infinity будут возвращены значения int.MaxValue и int.MinValue соответственно.</param>
        /// <returns>Целочисленное значение, представленное в объекте arg.</returns>
        public static long JSObjectToInt64(JSValue arg, long nullOrUndef, bool alternateInfinity)
        {
            if (arg == null)
                return nullOrUndef;
            var r = arg;
            switch (r.valueType)
            {
                case JSValueType.Boolean:
                case JSValueType.Integer:
                    {
                        return r.iValue;
                    }
                case JSValueType.Double:
                    {
                        if (double.IsNaN(r.dValue))
                            return 0;
                        if (double.IsInfinity(r.dValue))
                            return alternateInfinity ? double.IsPositiveInfinity(r.dValue) ? long.MaxValue : long.MinValue : 0;
                        return (long)r.dValue;
                    }
                case JSValueType.String:
                    {
                        double x = 0;
                        int ix = 0;
                        string s = (r.oValue.ToString()).Trim();
                        if (!Tools.ParseNumber(s, ref ix, out x, 0, ParseNumberOptions.AllowAutoRadix | ParseNumberOptions.AllowFloat) || ix < s.Length)
                            return 0;
                        if (double.IsNaN(x))
                            return 0;
                        if (double.IsInfinity(x))
                            return alternateInfinity ? double.IsPositiveInfinity(x) ? long.MaxValue : long.MinValue : 0;
                        return (long)x;
                    }
                case JSValueType.Date:
                case JSValueType.Function:
                case JSValueType.Object:
                    {
                        if (r.oValue == null)
                            return nullOrUndef;
                        r = r.ToPrimitiveValue_Value_String();
                        return JSObjectToInt64(r);
                    }
                case JSValueType.NotExists:
                case JSValueType.Undefined:
                case JSValueType.NotExistsInObject:
                    return nullOrUndef;
                default:
                    throw new NotImplementedException();
            }
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static JSValue JSObjectToNumber(JSValue arg)
        {
            return JSObjectToNumber(arg, new JSValue());
        }

        internal static JSValue JSObjectToNumber(JSValue arg, JSValue result)
        {
            if (arg == null)
            {
                result.valueType = JSValueType.Integer;
                result.iValue = 0;
                return result;
            }
            switch (arg.valueType)
            {
                case JSValueType.Boolean:
                    {
                        result.valueType = JSValueType.Integer;
                        result.iValue = arg.iValue;
                        return result;
                    }
                case JSValueType.Integer:
                case JSValueType.Double:
                    return arg;
                case JSValueType.String:
                    {
                        double x = 0;
                        int ix = 0;
                        string s = (arg.oValue.ToString()).Trim(TrimChars);
                        if (!Tools.ParseNumber(s, ref ix, out x) || ix < s.Length)
                            x = double.NaN;
                        result.valueType = JSValueType.Double;
                        result.dValue = x;
                        return result;
                    }
                case JSValueType.Date:
                case JSValueType.Function:
                case JSValueType.Object:
                    {
                        if (arg.oValue == null)
                        {
                            result.valueType = JSValueType.Integer;
                            result.iValue = 0;
                            return result;
                        }
                        arg = arg.ToPrimitiveValue_Value_String();
                        return JSObjectToNumber(arg);
                    }
                case JSValueType.NotExists:
                case JSValueType.Undefined:
                case JSValueType.NotExistsInObject:
                    {
                        result.valueType = JSValueType.Double;
                        result.dValue = double.NaN;
                        return result;
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object convertJStoObj(JSValue jsobj, Type targetType)
        {
            if (jsobj == null)
                return null;
            if (targetType.IsAssignableFrom(jsobj.GetType()))
                return jsobj;
            object value = null;
            switch (jsobj.valueType)
            {
                case JSValueType.Boolean:
                    {
                        if (targetType == typeof(bool))
                            return jsobj.iValue != 0;
                        break;
                    }
                case JSValueType.Double:
                    {
                        if (targetType == typeof(double))
                            return (double)jsobj.dValue;
                        if (targetType == typeof(float))
                            return (float)jsobj.dValue;
                        break;
                    }
                case JSValueType.Integer:
                    {
                        if (targetType == typeof(int))
                            return (int)jsobj.iValue;

                        //if (targetType == typeof(byte)) return (byte)jsobj.iValue;
                        //if (targetType == typeof(sbyte)) return (sbyte)jsobj.iValue;
                        //if (targetType == typeof(short)) return (short)jsobj.iValue;
                        //if (targetType == typeof(ushort)) return (ushort)jsobj.iValue;
                        if (targetType == typeof(uint))
                            return (uint)jsobj.iValue;
                        if (targetType == typeof(long))
                            return (long)jsobj.iValue;
                        if (targetType == typeof(ulong))
                            return (ulong)jsobj.iValue;
                        if (targetType == typeof(double))
                            return (double)jsobj.iValue;
                        if (targetType == typeof(float))
                            return (float)jsobj.iValue;
                        if (targetType == typeof(decimal))
                            return (float)jsobj.iValue;
                        break;
                    }
                default:
                    value = jsobj.Value;
                    break;
            }
            if (value == null)
                return null;
            if (targetType.IsAssignableFrom(value.GetType()))
                return value;
#if PORTABLE
            if (System.Reflection.IntrospectionExtensions.GetTypeInfo(targetType).IsEnum && Enum.IsDefined(targetType, value))
                return value;
#else
            if (targetType.IsEnum && Enum.IsDefined(targetType, value))
                return value;
#endif
            var tpres = value as TypeProxy;
            if (tpres != null && targetType.IsAssignableFrom(tpres.hostedType))
            {
                jsobj = tpres.prototypeInstance;
                if (jsobj is ObjectWrapper)
                    return jsobj.Value;
                return jsobj;
            }
            return null;
        }

        private struct DoubleStringCacheItem
        {
            public double key;
            public string value;
        }

        private static readonly DoubleStringCacheItem[] cachedDoubleString = new DoubleStringCacheItem[8];
        private static int cachedDoubleStringsIndex = 0;
        private static string[] divFormats = {
                                                 ".#",
                                                 ".##",
                                                 ".###",
                                                 ".####",
                                                 ".#####",
                                                 ".######",
                                                 ".#######",
                                                 ".########",
                                                 ".#########",
                                                 ".##########",
                                                 ".###########",
                                                 ".############",
                                                 ".#############",
                                                 ".##############",
                                                 ".###############"
                                             };

        internal static string DoubleToString(double d)
        {
            if (d == 0.0)
                return "0";
            if (double.IsPositiveInfinity(d))
                return "Infinity";
            if (double.IsNegativeInfinity(d))
                return "-Infinity";
            if (double.IsNaN(d))
                return "NaN";
            for (var i = 8; i-- > 0;)
            {
                if (cachedDoubleString[i].key == d)
                    return cachedDoubleString[i].value;
            }
            //return dtoString(d);
            var abs = System.Math.Abs(d);
            string res = null;
            if (abs < 1.0)
            {
                if (d == (d % 0.000001))
                    return res = d.ToString("0.####e-0", System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (abs >= 1e+21)
                return res = d.ToString("0.####e+0", System.Globalization.CultureInfo.InvariantCulture);
            int neg = (d < 0 || (d == -0.0 && double.IsNegativeInfinity(1.0 / d))) ? 1 : 0;
            if (d == 100000000000000000000d)
                res = "100000000000000000000";
            else if (d == -100000000000000000000d)
                res = "-100000000000000000000";
            else
                res = abs < 1.0 ? neg == 1 ? "-0" : "0" : ((d < 0 ? "-" : "") + ((ulong)(System.Math.Abs(d))).ToString(System.Globalization.CultureInfo.InvariantCulture));
            abs %= 1.0;
            if (abs != 0 && res.Length < (15 + neg))
                res += abs.ToString(divFormats[15 - res.Length + neg], System.Globalization.CultureInfo.InvariantCulture);
            cachedDoubleString[cachedDoubleStringsIndex].key = d;
            cachedDoubleString[cachedDoubleStringsIndex++].value = res;
            cachedDoubleStringsIndex &= 7;
            return res;
        }

        private static string dtoString(double a)
        {
            var b = (ulong)BitConverter.DoubleToInt64Bits(a);
            ulong m0 = (b & ((1UL << 52) - 1)) | (1UL << 52);
            ulong m1 = m0 & uint.MaxValue;
            m0 &= ~(ulong)uint.MaxValue;
            m1 <<= 21;
            int e = 0;
            var s = (ulong)(b >> 63) | 1;
            e |= (int)(b >> 52);
            e = 52 - e + 1023;

            const int estep = 3;
            const int dstep = 10;
            const int estepCor0 = 3;
            const int estepCor1 = 107;// 321;
            int count = 0;
            int ec = 0;
            if (e < 0)
            {
                while (e <= -(estep + (count % estepCor0 == 0 && count % estepCor1 != 0 ? 1 : 0)))
                {
                    e += estep + (count % estepCor0 == 0 && count % estepCor1 != 0 ? 1 : 0);
                    m0 <<= estep + (count % estepCor0 == 0 && count % estepCor1 != 0 ? 1 : 0);
                    m0 /= dstep;
                    m1 <<= estep + (count % estepCor0 == 0 && count % estepCor1 != 0 ? 1 : 0);
                    m1 /= dstep;
                    count++;
                    if (count % estepCor1 == 0)
                        ec++;
                }
                m0 <<= -e;
                m1 <<= -e;
            }
            else
            {
                while (e >= estep + (count % estepCor0 == 0 && count % estepCor1 != 0 ? 1 : 0))
                {
                    e -= estep + (count % estepCor0 == 0 && count % estepCor1 != 0 ? 1 : 0);
                    m0 *= dstep;
                    m0 >>= estep + (count % estepCor0 == 0 && count % estepCor1 != 0 ? 1 : 0);
                    m1 *= dstep;
                    m1 >>= estep + (count % estepCor0 == 0 && count % estepCor1 != 0 ? 1 : 0);
                    if (count % estepCor1 == 0)
                        ec--;
                    count--;
                }
                count += ec / 3;
                m0 >>= e;
                m1 >>= e;
            }
            m0 += m1 >> 21;

            if (m0 == 0)
                return s > 0 ? "0" : "-0";
            if (count >= 0)
            {
                while (m0 >= (ulong)1e17)
                {
                    var mod = m0 % 10;
                    m0 /= 10;
                    if (mod >= 5)
                        m0++;
                    count++;
                }
                var ts = m0.ToString();
                var res = new StringBuilder();
                if (ts.Length + count < 22)
                {
                    res.Append(ts);
                    for (var i = 0; i < count; i++)
                        res.Append("0");
                    return res.ToString();
                }
                for (var i = 0; i < System.Math.Min(17, ts.Length); i++)
                {
                    if (i == 1)
                        res.Append('.');
                    res.Append(ts[i]);
                }
                while (res[res.Length - 1] == '0')
                    res.Length--;
                if (!Tools.IsDigit(res[res.Length - 1]))
                    res.Length--;
                res.Append("e+").Append(ts.Length - 1 + count);
                return res.ToString();
            }
            else
            {
                while (m0 >= (ulong)1e17)
                {
                    var mod = m0 % 10;
                    m0 /= 10;
                    if (mod >= 5)
                        m0++;
                    count++;
                }
                var ts = m0.ToString();
                var res = new StringBuilder();
                if (count + ts.Length <= 0)
                {
                    if (count + ts.Length <= -7)
                    {
                        for (var i = 0; i < ts.Length; i++)
                        {
                            if (i == 1)
                                res.Append('.');
                            res.Append(ts[i]);
                        }
                        res.Append("e").Append(count + ts.Length);
                    }
                    else
                    {
                        res.Append("0.");
                        for (var i = -count - ts.Length; i-- > 0;)
                            res.Append('0');
                        res.Append(ts);
                    }
                }
                else
                {
                    for (var i = 0; i < ts.Length; i++)
                    {
                        if (count + ts.Length == i)
                            res.Append('.');
                        res.Append(ts[i]);
                    }
                    for (var i = res.Length; i-- > 0;)
                    {
                        if (res[i] == '0')
                            res.Length--;
                        else
                            break;
                    }
                    if (res[res.Length - 1] == '.')
                        res.Length--;
                }
                return res.ToString();
            }
        }

        private struct IntStringCacheItem
        {
            public int key;
            public string value;
        }

        private const int cacheSize = 16;
        private static readonly IntStringCacheItem[] intStringCache = new IntStringCacheItem[cacheSize]; // Обрати внимание на константы внизу
        private static int intStrCacheIndex = -1;

        public static string Int32ToString(int value)
        {
            if (value == 0)
                return "0";
            for (var i = cacheSize; i-- > 0;)
            {
                if (intStringCache[i].key == value)
                    return intStringCache[i].value;
            }
            return (intStringCache[intStrCacheIndex = (intStrCacheIndex + 1) & (cacheSize - 1)] = new IntStringCacheItem { key = value, value = value.ToString(CultureInfo.InvariantCulture) }).value;
        }

        public static bool ParseNumber(string code, out double value, int radix)
        {
            int index = 0;
            return ParseNumber(code, ref index, out value, radix, ParseNumberOptions.Default);
        }

        public static bool ParseNumber(string code, out double value, ParseNumberOptions options)
        {
            int index = 0;
            return ParseNumber(code, ref index, out value, 0, options);
        }

        public static bool ParseNumber(string code, out double value, int radix, ParseNumberOptions options)
        {
            int index = 0;
            return ParseNumber(code, ref index, out value, radix, options);
        }

        public static bool ParseNumber(string code, ref int index, out double value)
        {
            return ParseNumber(code, ref index, out value, 0, ParseNumberOptions.Default);
        }

        public static bool ParseNumber(string code, int index, out double value, ParseNumberOptions options)
        {
            return ParseNumber(code, ref index, out value, 0, options);
        }

        /// <summary>
        /// Проверяет символ на принадлежность диапазону цифр
        /// </summary>
        /// <param name="c"></param>
        /// <remarks>Использовать вместо этой функции char.IsDigit не получится. Версия char учитывает региональные особенности, что не нужно</remarks>
        /// <returns></returns>
        public static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        public static bool ParseNumber(string code, ref int index, out double value, int radix, ParseNumberOptions options)
        {
            bool raiseOctal = (options & ParseNumberOptions.RaiseIfOctal) != 0;
            bool processOctal = (options & ParseNumberOptions.ProcessOctal) != 0;
            bool allowAutoRadix = (options & ParseNumberOptions.AllowAutoRadix) != 0;
            bool allowFloat = (options & ParseNumberOptions.AllowFloat) != 0;
            if (code == null)
                throw new ArgumentNullException("code");
            value = double.NaN;
            if (radix != 0 && (radix < 2 || radix > 36))
                return false;
            if (code.Length == 0)
            {
                value = 0;
                return true;
            }
            int i = index;
            while (i < code.Length && Tools.IsWhiteSpace(code[i]) && !Tools.IsLineTerminator(code[i]))
                i++;
            if (i >= code.Length)
            {
                value = 0.0;
                return true;
            }
            const string nan = "NaN";
            for (int j = i; ((j - i) < nan.Length) && (j < code.Length); j++)
            {
                if (code[j] != nan[j - i])
                    break;
                else if (j > i && code[j] == 'N')
                {
                    index = j + 1;
                    value = double.NaN;
                    return true;
                }
            }
            int sig = 1;
            if (code[i] == '-' || code[i] == '+')
                sig = 44 - code[i++];
            const string infinity = "Infinity";
            for (int j = i; ((j - i) < infinity.Length) && (j < code.Length); j++)
            {
                if (code[j] != infinity[j - i])
                    break;
                else if (code[j] == 'y')
                {
                    index = j + 1;
                    value = sig * double.PositiveInfinity;
                    return true;
                }
            }
            bool res = false;
            bool skiped = false;
            if (allowAutoRadix && i + 1 < code.Length)
            {
                while (code[i] == '0' && i + 1 < code.Length && code[i + 1] == '0')
                {
                    skiped = true;
                    i++;
                }
                if ((i + 1 < code.Length) && (code[i] == '0'))
                {
                    if ((radix == 0 || radix == 16)
                        && !skiped
                        && (code[i + 1] == 'x' || code[i + 1] == 'X'))
                    {
                        i += 2;
                        radix = 16;
                    }
                    else if (radix == 0 && IsDigit(code[i + 1]))
                    {
                        if (raiseOctal)
                            ExceptionsHelper.Throw((new SyntaxError("Octal literals not allowed in strict mode")));
                        i += 1;
                        if (processOctal)
                            radix = 8;
                        res = true;
                    }
                }
            }
            if (allowFloat && radix == 0)
            {
                ulong temp = 0;
                int scount = 0;
                int deg = 0;
                while (i < code.Length)
                {
                    if (!IsDigit(code[i]))
                        break;
                    else
                    {
                        if (scount <= 18)
                        {
                            temp = temp * 10 + (ulong)(code[i++] - '0');
                            scount++;
                        }
                        else
                        {
                            deg++;
                            i++;
                        }
                        res = true;
                    }
                }
                if (!res && (i >= code.Length || code[i] != '.'))
                    return false;
                if (i < code.Length && code[i] == '.')
                {
                    i++;
                    while (i < code.Length)
                    {
                        if (!IsDigit(code[i]))
                            break;
                        else
                        {
                            if (scount <= 18)
                            {
                                temp = temp * 10 + (ulong)(code[i++] - '0');
                                scount++;
                                deg--;
                            }
                            else
                                i++;
                            res = true;
                        }
                    }
                }
                if (!res)
                    return false;
                if (i < code.Length && (code[i] == 'e' || code[i] == 'E'))
                {
                    i++;
                    int td = 0;
                    int esign = code[i] >= 43 && code[i] <= 45 ? 44 - code[i++] : 1;
                    scount = 0;
                    while (i < code.Length)
                    {
                        if (!IsDigit(code[i]))
                            break;
                        else
                        {
                            if (scount <= 6)
                                td = td * 10 + (code[i++] - '0');
                            else
                                i++;
                        }
                    }
                    deg += td * esign;
                }
                if (deg != 0)
                {
                    if (deg < 0)
                    {
                        if (deg < -18)
                        {
                            value = temp * 1e-18;
                            deg += 18;
                        }
                        else
                        {
                            value = (double)((decimal)temp * (decimal)System.Math.Pow(10.0, deg));
                            deg = 0;
                        }
                    }
                    else
                    {
                        if (deg > 10)
                        {
                            value = (double)((decimal)temp * 10000000000M);
                            deg -= 10;
                        }
                        else
                        {
                            value = (double)((decimal)temp * (decimal)System.Math.Pow(10.0, deg));
                            deg = 0;
                        }
                    }
                    if (deg != 0)
                    {
                        if (deg == -324)
                        {
                            value *= 1e-323;
                            value *= 0.1;
                        }
                        else
                        {
                            var exp = System.Math.Pow(10.0, deg);
                            value *= exp;
                        }
                    }
                }
                else
                    value = temp;
                if (value == 0 && skiped && raiseOctal)
                    ExceptionsHelper.Throw((new SyntaxError("Octal literals not allowed in strict mode")));
                value *= sig;
                index = i;
                return true;
            }
            else
            {
                if (radix == 0)
                    radix = 10;
                value = 0;
                bool extended = false;
                double doubleTemp = 0.0;
                ulong temp = 0;
                while (i < code.Length)
                {
                    var sign = anum(code[i]);
                    if (sign >= radix || (NumChars[sign] != code[i] && (NumChars[sign] + ('a' - 'A')) != code[i]))
                    {
                        break;
                    }
                    else
                    {
                        if (extended)
                            doubleTemp = doubleTemp * radix + sign;
                        else
                        {
                            temp = temp * (uint)radix + (uint)sign;
                            if ((temp & 0xFE00000000000000) != 0)
                            {
                                extended = true;
                                doubleTemp = temp;
                            }
                        }
                        res = true;
                    }
                    i++;
                }
                if (!res)
                {
                    value = double.NaN;
                    return false;
                }
                value = extended ? doubleTemp : temp;
                value *= sig;
                index = i;
                return true;
            }
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static string Unescape(string code, bool strict)
        {
            return Unescape(code, strict, true, false);
        }

        public static string Unescape(string code, bool strict, bool processUnknown, bool processRegexComp)
        {
            if (code == null)
                throw new ArgumentNullException("code");
            StringBuilder res = null;
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] == '\\' && i + 1 < code.Length)
                {
                    if (res == null)
                    {
                        res = new StringBuilder(code.Length);
                        for (var j = 0; j < i; j++)
                            res.Append(code[j]);
                    }
                    i++;
                    switch (code[i])
                    {
                        case 'x':
                        case 'u':
                            {
                                if (i + (code[i] == 'u' ? 5 : 3) > code.Length)
                                {
                                    if (processRegexComp)
                                    {
                                        res.Append(code[i]);
                                        break;
                                    }
                                    else
                                        ExceptionsHelper.Throw((new SyntaxError("Invalid escape code (\"" + code + "\")")));
                                }
                                string c = code.Substring(i + 1, code[i] == 'u' ? 4 : 2);
                                ushort chc = 0;
                                if (ushort.TryParse(c, System.Globalization.NumberStyles.HexNumber, null, out chc))
                                {
                                    char ch = (char)chc;
                                    res.Append(ch);
                                    i += c.Length;
                                }
                                else
                                {
                                    if (processRegexComp)
                                        res.Append(code[i]);
                                    else
                                        ExceptionsHelper.Throw((new SyntaxError("Invalid escape sequence '\\" + code[i] + c + "'")));
                                }
                                break;
                            }
                        case 't':
                            {
                                res.Append(processRegexComp ? "\\t" : "\t");
                                break;
                            }
                        case 'f':
                            {
                                res.Append(processRegexComp ? "\\f" : "\f");
                                break;
                            }
                        case 'v':
                            {
                                res.Append(processRegexComp ? "\\v" : "\v");
                                break;
                            }
                        case 'b':
                            {
                                res.Append(processRegexComp ? "\\b" : "\b");
                                break;
                            }
                        case 'n':
                            {
                                res.Append(processRegexComp ? "\\n" : "\n");
                                break;
                            }
                        case 'r':
                            {
                                res.Append(processRegexComp ? "\\r" : "\r");
                                break;
                            }
                        case 'c':
                        case 'C':
                            {
                                if (!processRegexComp)
                                    goto default;

                                if (i + 1 < code.Length)
                                {
                                    char ch = code[i + 1];
                                    // convert a -> A
                                    if (ch >= 'a' && ch <= 'z')
                                        ch = (char)(ch - ('a' - 'A'));
                                    if ((char)(ch - '@') < ' ')
                                    {
                                        res.Append("\\c");
                                        res.Append(ch);
                                        ++i;
                                        break;
                                    }
                                }

                                // invalid control character
                                goto case 'p';
                            }
                        // not supported in standard
                        case 'P':
                        case 'p':
                        case 'k':
                        case 'K':
                            {
                                if (!processRegexComp)
                                    goto default;

                                // regex that does not match anything
                                res.Append(@"\b\B");
                                break;
                            }
                        default:
                            {
                                if (IsDigit(code[i]) && !processRegexComp)
                                {
                                    if (strict)
                                        ExceptionsHelper.Throw((new SyntaxError("Octal literals are not allowed in strict mode.")));
                                    var ccode = code[i] - '0';
                                    if (i + 1 < code.Length && IsDigit(code[i + 1]))
                                        ccode = ccode * 10 + (code[++i] - '0');
                                    if (i + 1 < code.Length && IsDigit(code[i + 1]))
                                        ccode = ccode * 10 + (code[++i] - '0');
                                    res.Append((char)ccode);
                                }
                                else if (processUnknown)
                                    res.Append(code[i]);
                                else
                                {
                                    res.Append('\\');
                                    res.Append(code[i]);
                                }
                                break;
                            }
                    }
                }
                else if (res != null)
                    res.Append(code[i]);
            }
            return (res as object ?? code).ToString();
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool IsLineTerminator(char c)
        {
            return (c == '\u000A') || (c == '\u000D') || (c == '\u2028') || (c == '\u2029');
        }

        internal static void SkipComment(string code, ref int index, bool skipSpaces)
        {
            bool work;
            do
            {
                if (code.Length <= index)
                    return;
                work = false;
                if (code[index] == '/' && index + 1 < code.Length)
                {
                    switch (code[index + 1])
                    {
                        case '/':
                            {
                                index += 2;
                                while (index < code.Length && !Tools.IsLineTerminator(code[index]))
                                    index++;
                                work = true;
                                break;
                            }
                        case '*':
                            {
                                index += 2;
                                while (index + 1 < code.Length && (code[index] != '*' || code[index + 1] != '/'))
                                    index++;
                                if (index + 1 >= code.Length)
                                    ExceptionsHelper.Throw(new SyntaxError("Unexpected end of source."));
                                index += 2;
                                work = true;
                                break;
                            }
                    }
                }
            } while (work);
            if (skipSpaces)
                while ((index < code.Length) && (Tools.IsWhiteSpace(code[index])))
                    index++;
        }

        internal static string RemoveComments(string code, int startPosition)
        {
            StringBuilder res = null;// new StringBuilder(code.Length);
            for (int i = startPosition; i < code.Length;)
            {
                while (i < code.Length && Tools.IsWhiteSpace(code[i]))
                {
                    if (res != null)
                        res.Append(code[i++]);
                    else
                        i++;
                }
                var s = i;
                SkipComment(code, ref i, false);
                if (s != i && res == null)
                {
                    res = new StringBuilder(code.Length);
                    for (var j = 0; j < s; j++)
                        res.Append(code[j]);
                }
                for (; s < i; s++)
                {
                    if (Tools.IsWhiteSpace(code[s]))
                        res.Append(code[s]);
                    else
                        res.Append(' ');
                }
                if (i >= code.Length)
                    continue;

                if (Parser.ValidateRegex(code, ref i, false)) // оно путает деление с комментарием в конце строки и regexp.
                // Посему делаем так: если встретили что-то похожее на regexp - останавливаемся. 
                // Остальные комментарии удалим когда, в процессе разбора, поймём, что же это на самом деле
                {
                    if (res != null)
                        for (; s <= i; s++)
                            res.Append(code[s]);
                    break;
                }

                if (//Parser.ValidateName(code, ref i, false) ||
                    //Parser.ValidateNumber(code, ref i) ||
                    //Parser.ValidateRegex(code, ref i, false) ||
                    Parser.ValidateString(code, ref i, false))
                {
                    if (res != null)
                        for (; s < i; s++)
                            res.Append(code[s]);
                }
                else if (res != null)
                    res.Append(code[i++]);
                else
                    i++;
            }
            return (res as object ?? code).ToString();
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool isHex(char p)
        {
            if (p < '0' || p > 'f')
                return false;
            var c = anum(p);
            return c >= 0 && c < 16;
        }

        /// <summary>
        /// Переводит число из системы исчисления с основанием 36 (0-9 A-Z без учёта регистра) в десятичную.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal static int anum(char p)
        {
            return ((p % 'a' % 'A' + 10) % ('0' + 10));
        }

        internal static long getLengthOfArraylike(JSValue src, bool reassignLen)
        {
            var length = src.GetProperty("length", true, PropertyScope.Сommon); // тут же проверка на null/undefined с падением если надо
            
            var result = (uint)JSObjectToInt64(InvokeGetter(length, src).ToPrimitiveValue_Value_String(), 0, false);
            if (reassignLen)
            {
                if (length.valueType == JSValueType.Property)
                    ((length.oValue as GsPropertyPair).set ?? Function.emptyFunction).Call(src, new Arguments() { result });
                else
                    length.Assign(result);
            }

            return result;
        }

        internal static BaseLibrary.Array arraylikeToArray(JSValue src, bool evalProps, bool clone, bool reassignLen, long _length)
        {
            var temp = new BaseLibrary.Array();
            bool goDeep = true;
            for (; goDeep;)
            {
                goDeep = false;
                var srca = src as BaseLibrary.Array;
                if (srca != null)
                {
                    if (_length == -1)
                        _length = srca.data.Length;
                    long prew = -1;
                    foreach (var element in srca.data.DirectOrder)
                    {
                        if (element.Key >= _length) // эээ...
                            break;
                        var value = element.Value;
                        if (value == null || !value.Exists)
                            continue;
                        if (!goDeep && System.Math.Abs(prew - element.Key) > 1)
                        {
                            goDeep = true;
                        }
                        if (evalProps && value.valueType == JSValueType.Property)
                            value = (value.oValue as GsPropertyPair).get == null ? JSValue.undefined : (value.oValue as GsPropertyPair).get.Call(src, null).CloneImpl(false);
                        else if (clone)
                            value = value.CloneImpl(false);
                        if (temp.data[element.Key] == null)
                            temp.data[element.Key] = value;
                    }
                    goDeep |= System.Math.Abs(prew - _length) > 1;
                }
                else
                {
                    if (_length == -1)
                    {
                        _length = getLengthOfArraylike(src, reassignLen);
                        if (_length == 0)
                            return temp;
                    }
                    long prew = -1;
                    foreach (var index in EnumerateArraylike(_length, src))
                    {
                        var value = index.Value;
                        if (!value.Exists)
                            continue;
                        if (evalProps && value.valueType == JSValueType.Property)
                            value = (value.oValue as GsPropertyPair).get == null ? JSValue.undefined : (value.oValue as GsPropertyPair).get.Call(src, null).CloneImpl(false);
                        else if (clone)
                            value = value.CloneImpl(false);
                        if (!goDeep && System.Math.Abs(prew - index.Key) > 1)
                        {
                            goDeep = true;
                        }
                        if (temp.data[(int)(uint)index.Key] == null)
                            temp.data[(int)(uint)index.Key] = value;
                    }
                    goDeep |= System.Math.Abs(prew - _length) > 1;
                }
                if (src.__proto__ == JSValue.@null)
                    break;
                src = src.__proto__.oValue as JSValue ?? src.__proto__;
                if (src == null || (src.valueType >= JSValueType.String && src.oValue == null))
                    break;
            }
            temp.data[(int)(_length - 1)] = temp.data[(int)(_length - 1)];
            return temp;
        }

        internal static IEnumerable<KeyValuePair<uint, JSValue>> EnumerateArraylike(long length, JSValue src)
        {
            if (src.valueType == JSValueType.Object && src.Value is BaseLibrary.Array)
            {
                foreach (var item in (src.Value as BaseLibrary.Array).data.DirectOrder)
                {
                    yield return new KeyValuePair<uint, JSValue>((uint)item.Key, item.Value);
                }
            }
            var @enum = src.GetEnumerator(false, EnumerationMode.RequireValues);
            while (@enum.MoveNext())
            {
                var i = @enum.Current.Key;
                var pindex = 0;
                var dindex = 0.0;
                var lindex = 0U;
                if (Tools.ParseNumber(i, ref pindex, out dindex)
                    && (pindex == i.Length)
                    && dindex < length
                    && (lindex = (uint)dindex) == dindex)
                {
                    yield return new KeyValuePair<uint, JSValue>(lindex, @enum.Current.Value);
                }
            }
        }

        public static int CompareWithMask(Enum x, Enum y, Enum mask)
        {
            return ((int)(ValueType)x & (int)(ValueType)mask) - ((int)(ValueType)y & (int)(ValueType)mask);
        }

        public static bool IsEqual(Enum x, Enum y, Enum mask)
        {
            return ((int)(ValueType)x & (int)(ValueType)mask) == ((int)(ValueType)y & (int)(ValueType)mask);
        }

        internal static JSValue InvokeGetter(JSValue property, JSValue target)
        {
            if (property.valueType != JSValueType.Property)
                return property;
            var getter = property.oValue as GsPropertyPair;
            if (getter == null || getter.get == null)
                return JSValue.undefined;
            property = getter.get.Call(target, null);
            if (property.valueType < JSValueType.Undefined)
                property = JSValue.undefined;
            return property;
        }

        internal static JSValue PrepareArg(Context context, CodeNode source)
        {
            var a = source.Evaluate(context);
            if (a == null)
                return JSValue.undefined;
            if (a.valueType != JSValueType.SpreadOperatorResult)
            {
                a = a.CloneImpl(false);
                a.attributes |= JSValueAttributesInternal.Cloned;
            }
            return a;
        }

        internal static bool IsWhiteSpace(char p)
        {
            var fb = p >> 8;
            if (fb != 0x0 && fb != 0x16 && fb != 0x18 && fb != 0x20 && fb != 0x30 && fb != 0xFE)
                return false;
            for (var i = TrimChars.Length; i-- > 0;)
                if (p == TrimChars[i])
                    return true;
            return false;
        }

        internal static LambdaExpression BuildJsCallTree(string name, Expression functionGetter, ParameterExpression thisParameter, MethodInfo method, Type delegateType)
        {
            var prms = method.GetParameters();

            var handlerArgumentsParameters = new ParameterExpression[prms.Length + (thisParameter != null ? 1 : 0)];
            {
                var i = 0;

                if (thisParameter != null)
                    handlerArgumentsParameters[i++] = thisParameter;

                for (; i < prms.Length; i++)
                {
                    handlerArgumentsParameters[i] = Expression.Parameter(prms[i].ParameterType, prms[i].Name);
                }
            }

            var argumentsParameter = Expression.Parameter(typeof(Arguments), "arguments");
            var expressions = new List<Expression>();

            if (prms.Length != 0)
            {
                expressions.Add(Expression.Assign(argumentsParameter, Expression.New(typeof(Arguments))));
                for (var i = 0; i < handlerArgumentsParameters.Length; i++)
                {
                    Expression argument = handlerArgumentsParameters[i];
                    if (argument.Type.IsValueType)
                    {
                        argument = Expression.Convert(argument, typeof(object));
                    }

                    expressions.Add(Expression.Call(
                        argumentsParameter,
                        typeof(Arguments).GetRuntimeMethod("Add", new[] { typeof(JSValue) }),
                        Expression.Call(Tools.methodof<object, JSValue>(TypeProxy.Proxy), argument)));
                }
            }

            var callTree = Expression.Call(functionGetter, typeof(Function).GetRuntimeMethod(nameof(Function.Call), new[] { typeof(Arguments) }), argumentsParameter);

            expressions.Add(callTree);
            if (method.ReturnParameter.ParameterType != typeof(void)
                && method.ReturnParameter.ParameterType != typeof(object)
                && !typeof(JSValue).IsAssignableFrom(method.ReturnParameter.ParameterType))
            {
                var asMethod = typeof(JSValueExtensions).GetRuntimeMethods().First(x => x.Name == "As").MakeGenericMethod(method.ReturnParameter.ParameterType);
                expressions[expressions.Count - 1] = Expression.Call(asMethod, callTree);
            }

            var result = Expression.Block(new[] { argumentsParameter }, expressions);

            if (delegateType != null)
                return Expression.Lambda(delegateType, result, name, handlerArgumentsParameters);
            else
                return Expression.Lambda(result, name, handlerArgumentsParameters);
        }

        internal static MethodInfo methodof<T0, T1, T2, T3>(Action<T0, T1, T2, T3> method)
        {
            return method.GetMethodInfo();
        }

        internal static MethodInfo methodof<T0, T1>(Func<T0, T1> method)
        {
            return method.GetMethodInfo();
        }
    }
}
