using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.TypeProxing;

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
            return "(" + Line + ": " + Column + "*" + Length + ")";
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
        internal static readonly string[] NumString = new[] 
		{ 
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15"
		};

        //internal static readonly string[] charStrings = (from x in Enumerable.Range(char.MinValue, char.MaxValue) select ((char)x).ToString()).ToArray();

        internal sealed class _ForcedEnumerator<T> : IEnumerator<T>
        {
            private int index;
            private IEnumerable<T> owner;
            private IEnumerator<T> parent;

            private _ForcedEnumerator(IEnumerable<T> owner)
            {
                this.owner = owner;
                this.parent = owner.GetEnumerator();
            }

            public static _ForcedEnumerator<T> create(IEnumerable<T> owner)
            {
                return new _ForcedEnumerator<T>(owner);
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
                    for (int i = 0; i < index && parent.MoveNext(); i++) ;
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
        public static double JSObjectToDouble(JSObject arg)
        {
            do
            {
                if (arg == null)
                    return double.NaN;
                switch (arg.valueType)
                {
                    case JSObjectType.Bool:
                    case JSObjectType.Int:
                        {
                            return arg.iValue;
                        }
                    case JSObjectType.Double:
                        {
                            return arg.dValue;
                        }
                    case JSObjectType.String:
                        {
                            double x = double.NaN;
                            int ix = 0;
                            string s = (arg.oValue.ToString());
                            if (s.Length > 0 && (char.IsWhiteSpace(s[0]) || char.IsWhiteSpace(s[s.Length - 1])))
                                s = s.Trim(Tools.TrimChars);
                            if (Tools.ParseNumber(s, ref ix, out x, 0, ParseNumberOptions.AllowFloat | ParseNumberOptions.AllowAutoRadix) && ix < s.Length)
                                return double.NaN;
                            return x;
                        }
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.Object:
                        {
                            if (arg.oValue == null)
                                return 0;
                            arg = arg.ToPrimitiveValue_Value_String();
                            break;
                            //return JSObjectToDouble(arg);
                        }
                    case JSObjectType.NotExists:
                    case JSObjectType.Undefined:
                    case JSObjectType.NotExistsInObject:
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
        public static int JSObjectToInt32(JSObject arg)
        {
            if (arg.valueType == JSObjectType.Int)
                return arg.iValue;
            return JSObjectToInt32(arg, 0, false);
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
        public static int JSObjectToInt32(JSObject arg, int nullOrUndef)
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
        public static int JSObjectToInt32(JSObject arg, bool alternateInfinity)
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
        public static int JSObjectToInt32(JSObject arg, int nullOrUndef, bool alternateInfinity)
        {
            if (arg == null)
                return nullOrUndef;
            var r = arg;
            switch (r.valueType)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        return r.iValue;
                    }
                case JSObjectType.Double:
                    {
                        if (double.IsNaN(r.dValue))
                            return 0;
                        if (double.IsInfinity(r.dValue))
                            return alternateInfinity ? double.IsPositiveInfinity(r.dValue) ? int.MaxValue : int.MinValue : 0;
                        return (int)(long)r.dValue;
                    }
                case JSObjectType.String:
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
                case JSObjectType.Date:
                case JSObjectType.Function:
                case JSObjectType.Object:
                    {
                        if (r.oValue == null)
                            return nullOrUndef;
                        r = r.ToPrimitiveValue_Value_String();
                        return JSObjectToInt32(r);
                    }
                case JSObjectType.NotExists:
                //throw new JSException((new NiL.JS.BaseLibrary.ReferenceError("Variable not defined.")));
                case JSObjectType.Undefined:
                case JSObjectType.NotExistsInObject:
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
        public static long JSObjectToInt64(JSObject arg)
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
        public static long JSObjectToInt64(JSObject arg, long nullOrUndef)
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
        public static long JSObjectToInt64(JSObject arg, long nullOrUndef, bool alternateInfinity)
        {
            if (arg == null)
                return nullOrUndef;
            var r = arg;
            switch (r.valueType)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        return r.iValue;
                    }
                case JSObjectType.Double:
                    {
                        if (double.IsNaN(r.dValue))
                            return 0;
                        if (double.IsInfinity(r.dValue))
                            return alternateInfinity ? double.IsPositiveInfinity(r.dValue) ? long.MaxValue : long.MinValue : 0;
                        return (long)r.dValue;
                    }
                case JSObjectType.String:
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
                case JSObjectType.Date:
                case JSObjectType.Function:
                case JSObjectType.Object:
                    {
                        if (r.oValue == null)
                            return nullOrUndef;
                        r = r.ToPrimitiveValue_Value_String();
                        return JSObjectToInt32(r);
                    }
                case JSObjectType.NotExists:
                case JSObjectType.Undefined:
                case JSObjectType.NotExistsInObject:
                    return nullOrUndef;
                default:
                    throw new NotImplementedException();
            }
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static JSObject JSObjectToNumber(JSObject arg)
        {
            return JSObjectToNumber(arg, new JSObject());
        }

        internal static JSObject JSObjectToNumber(JSObject arg, JSObject result)
        {
            if (arg == null)
            {
                result.valueType = JSObjectType.Int;
                result.iValue = 0;
                return result;
            }
            switch (arg.valueType)
            {
                case JSObjectType.Bool:
                    {
                        result.valueType = JSObjectType.Int;
                        result.iValue = arg.iValue;
                        return result;
                    }
                case JSObjectType.Int:
                case JSObjectType.Double:
                    return arg;
                case JSObjectType.String:
                    {
                        double x = 0;
                        int ix = 0;
                        string s = (arg.oValue.ToString()).Trim(TrimChars);
                        if (!Tools.ParseNumber(s, ref ix, out x) || ix < s.Length)
                            return Number.NaN;
                        result.valueType = JSObjectType.Double;
                        result.dValue = x;
                        return result;
                    }
                case JSObjectType.Date:
                case JSObjectType.Function:
                case JSObjectType.Object:
                    {
                        if (arg.oValue == null)
                        {
                            result.valueType = JSObjectType.Int;
                            result.iValue = 0;
                            return result;
                        }
                        arg = arg.ToPrimitiveValue_Value_String();
                        return JSObjectToNumber(arg);
                    }
                case JSObjectType.NotExists:
                case JSObjectType.Undefined:
                case JSObjectType.NotExistsInObject:
                    {
                        result.valueType = JSObjectType.Double;
                        result.dValue = double.NaN;
                        return result;
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object convertJStoObj(JSObject jsobj, Type targetType)
        {
            if (jsobj == null)
                return null;
            if (targetType.IsAssignableFrom(jsobj.GetType()))
                return jsobj;
            object value = null;
            switch (jsobj.valueType)
            {
                case JSObjectType.Bool:
                    {
                        if (targetType == typeof(bool))
                            return jsobj.iValue != 0;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        if (targetType == typeof(double)) return (double)jsobj.dValue;
                        if (targetType == typeof(float)) return (float)jsobj.dValue;
                        break;
                    }
                case JSObjectType.Int:
                    {
                        if (targetType == typeof(int)) return (int)jsobj.iValue;

                        //if (targetType == typeof(byte)) return (byte)jsobj.iValue;
                        //if (targetType == typeof(sbyte)) return (sbyte)jsobj.iValue;
                        //if (targetType == typeof(short)) return (short)jsobj.iValue;
                        //if (targetType == typeof(ushort)) return (ushort)jsobj.iValue;
                        if (targetType == typeof(uint)) return (uint)jsobj.iValue;
                        if (targetType == typeof(long)) return (long)jsobj.iValue;
                        if (targetType == typeof(ulong)) return (ulong)jsobj.iValue;
                        if (targetType == typeof(double)) return (double)jsobj.iValue;
                        if (targetType == typeof(float)) return (float)jsobj.iValue;
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
                if (jsobj != null && jsobj.GetType() == typeof(ObjectContainer))
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
            for (var i = 8; i-- > 0; )
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
                if (!char.IsDigit(res[res.Length - 1]))
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
                        for (var i = -count - ts.Length; i-- > 0; )
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
                    for (var i = res.Length; i-- > 0; )
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

        private static readonly IntStringCacheItem[] intStringCache = new IntStringCacheItem[8]; // Обрати внимание на константы внизу
        private static int intStrCacheIndex = -1;

        public static string Int32ToString(int value)
        {
            for (var i = 8; i-- > 0; )
            {
                if (intStringCache[i].key == value)
                    return intStringCache[i].value;
            }
            return (intStringCache[intStrCacheIndex = (intStrCacheIndex + 1) & 7] = new IntStringCacheItem { key = value, value = value.ToString(CultureInfo.InvariantCulture) }).value;
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

        public static bool isDigit(char c)
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
            while (i < code.Length && char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i]))
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
                    else if (radix == 0 && isDigit(code[i + 1]))
                    {
                        if (raiseOctal)
                            throw new JSException((new SyntaxError("Octal literals not allowed in strict mode")));
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
                    if (!isDigit(code[i]))
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
                        if (!isDigit(code[i]))
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
                        if (!isDigit(code[i]))
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
                    throw new JSException((new SyntaxError("Octal literals not allowed in strict mode")));
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
                                        throw new JSException((new SyntaxError("Invalid escape code (\"" + code + "\")")));
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
                                        throw new JSException((new SyntaxError("Invalid escape sequence '\\" + code[i] + c + "'")));
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
                                if (isDigit(code[i]) && !processRegexComp)
                                {
                                    if (strict)
                                        throw new JSException((new SyntaxError("Octal literals are not allowed in strict mode.")));
                                    var ccode = code[i] - '0';
                                    if (i + 1 < code.Length && isDigit(code[i + 1]))
                                        ccode = ccode * 10 + (code[++i] - '0');
                                    if (i + 1 < code.Length && isDigit(code[i + 1]))
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
        internal static bool isLineTerminator(char c)
        {
            return (c == '\u000A') || (c == '\u000D') || (c == '\u2028') || (c == '\u2029');
        }

        internal static void skipComment(string code, ref int index, bool skipSpaces)
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
                                while (index < code.Length && !Tools.isLineTerminator(code[index]))
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
                                    throw new JSException(new SyntaxError("Unexpected end of source."));
                                index += 2;
                                work = true;
                                break;
                            }
                    }
                }
            } while (work);
            if (skipSpaces)
                while ((index < code.Length) && (char.IsWhiteSpace(code[index])))
                    index++;
        }

        internal static string RemoveComments(string code, int startPosition)
        {
            StringBuilder res = null;// new StringBuilder(code.Length);
            for (int i = startPosition; i < code.Length; )
            {
                while (i < code.Length && char.IsWhiteSpace(code[i]))
                {
                    if (res != null)
                        res.Append(code[i++]);
                    else
                        i++;
                }
                var s = i;
                skipComment(code, ref i, false);
                if (s != i && res == null)
                {
                    res = new StringBuilder(code.Length);
                    for (var j = 0; j < s; j++)
                        res.Append(code[j]);
                }
                for (; s < i; s++)
                {
                    if (char.IsWhiteSpace(code[s]))
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
        internal static JSObject RaiseIfNotExist(JSObject obj, object name)
        {
            if (obj.valueType == JSObjectType.NotExists)
                throw new JSException((new NiL.JS.BaseLibrary.ReferenceError("Variable \"" + name + "\" is not defined.")));
            return obj;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool isHex(char p)
        {
            if (p < '0')
                return false;
            if (p > 'f')
                return false;
            var c = anum(p);
            return c >= 0 && c < 16;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal static int anum(char p)
        {
            return ((p % 'a' % 'A' + 10) % ('0' + 10));
        }
        internal static long getLengthOfIterably(JSObject src, bool reassignLen)
        {
            var len = src.GetMember("length", true, false); // тут же проверка на null/undefined с падением если надо
            if (len.valueType == JSObjectType.Property)
            {
                if (reassignLen && (len.attributes & JSObjectAttributesInternal.ReadOnly) == 0)
                {
                    len.valueType = JSObjectType.Undefined;
                    len.Assign(((len.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(src, null));
                }
                else
                    len = ((len.oValue as PropertyPair).get ?? Function.emptyFunction).Invoke(src, null);
            }
            uint res;
            if (len.valueType >= JSObjectType.Object)
                res = (uint)Tools.JSObjectToInt64(len.ToPrimitiveValue_Value_String(), 0, false);
            else
                res = (uint)Tools.JSObjectToInt64(len, 0, false);
            if (reassignLen)
            {
                if (len.valueType == JSObjectType.Property)
                    ((len.oValue as PropertyPair).set ?? Function.emptyFunction).Invoke(src, new Arguments() { a0 = res, length = 1 });
                else
                    len.Assign(res);
            }
            return res;
        }

        internal static BaseLibrary.Array iterableToArray(JSObject src, bool evalProps, bool clone, bool reassignLen, long _length)
        {
            var temp = new BaseLibrary.Array();
            HashSet<string> processedKeys = null;
            bool goDeep = true;
            for (; goDeep; )
            {
                goDeep = false;
                if (src.GetType() == typeof(BaseLibrary.Array))
                {
                    if (_length == -1)
                        _length = (src as NiL.JS.BaseLibrary.Array).data.Length;
                    long prew = -1;
                    foreach (var element in ((src as BaseLibrary.Array).data as IEnumerable<KeyValuePair<int, JSObject>>))
                    {
                        if (element.Key >= _length) // эээ...
                            break;
                        var value = element.Value;
                        if (value == null || !value.IsExist)
                            continue;
                        if (!goDeep && System.Math.Abs(prew - element.Key) > 1)
                            goDeep = true;
                        if (evalProps && value.valueType == JSObjectType.Property)
                            value = (value.oValue as PropertyPair).get == null ? JSObject.undefined : (value.oValue as PropertyPair).get.Invoke(src, null).CloneImpl();
                        else if (clone)
                            value = value.CloneImpl();
                        if (processedKeys != null)
                        {
                            var sk = element.Key.ToString();
                            if (processedKeys.Contains(sk))
                                continue;
                            processedKeys.Add(sk);
                        }
                        temp.data[element.Key] = value;
                    }
                    goDeep |= System.Math.Abs(prew - _length) > 1;
                }
                else
                {
                    if (_length == -1)
                    {
                        _length = getLengthOfIterably(src, reassignLen);
                        if (_length == 0)
                            return temp;
                    }
                    long prew = -1;
                    var tjo = new JSObject() { valueType = JSObjectType.String };
                    foreach (var index in iterablyEnum(_length, src))
                    {
                        tjo.oValue = index.Value;
                        var value = src.GetMember(tjo, false, false);
                        if (!value.IsExist)
                            continue;
                        if (evalProps && value.valueType == JSObjectType.Property)
                            value = (value.oValue as PropertyPair).get == null ? JSObject.undefined : (value.oValue as PropertyPair).get.Invoke(src, null).CloneImpl();
                        else if (clone)
                            value = value.CloneImpl();
                        if (!goDeep && System.Math.Abs(prew - index.Key) > 1)
                            goDeep = true;
                        if (processedKeys != null)
                        {
                            var sk = index.Value;
                            if (processedKeys.Contains(sk))
                                continue;
                            processedKeys.Add(sk);
                        }
                        temp.data[(int)(uint)index.Key] = value;
                    }
                    goDeep |= System.Math.Abs(prew - _length) > 1;
                }
                var crnt = src;
                if (src.__proto__ == JSObject.Null)
                    break;
                src = src.__proto__.oValue as JSObject ?? src.__proto__;
                if (src == null || (src.valueType >= JSObjectType.String && src.oValue == null))
                    break;
                if (processedKeys == null)
                {
                    processedKeys = new HashSet<string>();
                    for (var @enum = crnt.GetEnumeratorImpl(false); @enum.MoveNext(); )
                        processedKeys.Add(@enum.Current);
                }
            }
            temp.data[(int)(_length - 1)] = temp.data[(int)(_length - 1)];
            return temp;
        }

        internal static IEnumerable<KeyValuePair<long, string>> iterablyEnum(long length, JSObject src)
        {
            var res = new List<KeyValuePair<long, string>>();
            var @enum = src.GetEnumerator(false);
            while (@enum.MoveNext())
            {
                var i = @enum.Current;
                var pindex = 0;
                var dindex = 0.0;
                long lindex = 0;
                if (Tools.ParseNumber(i, ref pindex, out dindex)
                    && (pindex == i.Length)
                    && dindex < length
                    && (lindex = (long)dindex) == dindex)
                {
                    res.Add(new KeyValuePair<long, string>(lindex, i));
                }
            }
            res.Sort(new Comparison<KeyValuePair<long, string>>((x, y) => System.Math.Sign(x.Key - y.Key)));
            return res;
        }

        public static int CompareWithMask(Enum x, Enum y, Enum mask)
        {
            return ((int)(ValueType)x & (int)(ValueType)mask) - ((int)(ValueType)y & (int)(ValueType)mask);
        }

        public static bool IsEqual(Enum x, Enum y, Enum mask)
        {
            return ((int)(ValueType)x & (int)(ValueType)mask) == ((int)(ValueType)y & (int)(ValueType)mask);
        }
    }
}
