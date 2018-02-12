using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;

#if NET40 || NETCORE
using NiL.JS.Backward;
#endif

using JSBool = NiL.JS.BaseLibrary.Boolean;

namespace NiL.JS.Core
{
    [Flags]
    public enum ParseNumberOptions
    {
        None = 0,
        RaiseIfOctal = 1,
        ProcessOctalLiteralsOldSyntax = 2,
        AllowFloat = 4,
        AllowAutoRadix = 8,
        Default = 2 + 4 + 8
    }

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
            return $"({ Line }:{ Column }{ (Length != 0 ? "*" + Length : "") })";
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
                    if (text.Length > i + 1 && text[i + 1] == '\r')
                        i++;
                }
                else if (text[i] == '\r')
                {
                    column = 0;
                    line++;
                    if (text.Length > i + 1 && text[i + 1] == '\n')
                        i++;
                }

                column++;
            }

            return new CodeCoordinates(line, column, length);
        }
    }

    public static class Tools
    {
        private static readonly Type[] intTypeWithinArray = new[] { typeof(int) };

        internal static readonly char[] TrimChars = new[] { '\u0020', '\u0009', '\u000A', '\u000D', '\u000B', '\u000C', '\u00A0', '\u1680', '\u180E', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007', '\u2008', '\u2009', '\u200A', '\u2028', '\u2029', '\u202F', '\u205F', '\u3000', '\uFEFF' };

        internal static readonly char[] NumChars = new[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        private struct DoubleStringCacheItem
        {
            public double key;
            public string value;
        }

        private static readonly DoubleStringCacheItem[] cachedDoubleString = new DoubleStringCacheItem[8];
        private static int cachedDoubleStringsIndex = 0;
        private static readonly string[] divFormats =
            {
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

        private static readonly decimal[] powersOf10 = new[]
            {
                1e-18M, 1e-17M, 1e-16M, 1e-15M, 1e-14M, 1e-13M, 1e-12M, 1e-11M,
                1e-10M, 1e-9M, 1e-8M, 1e-7M, 1e-6M, 1e-5M, 1e-4M, 1e-3M, 1e-2M,
                1e-1M, 1e+0M, 1e+1M, 1e+2M, 1e+3M, 1e+4M, 1e+5M, 1e+6M, 1e+7M,
                1e+8M, 1e+9M, 1e+10M, 1e+11M, 1e+12M, 1e+13M, 1e+14M, 1e+15M,
                1e+16M, 1e+17M, 1e+18M
            };

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

                switch (arg._valueType)
                {
                    case JSValueType.Boolean:
                    case JSValueType.Integer:
                        {
                            return arg._iValue;
                        }
                    case JSValueType.Double:
                        {
                            return arg._dValue;
                        }
                    case JSValueType.String:
                        {
                            double x = double.NaN;
                            int ix = 0;
                            string s = (arg._oValue.ToString());
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
                            if (arg._oValue == null)
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
            if (arg._valueType == JSValueType.Integer)
                return arg._iValue;
            return JSObjectToInt32(arg, 0, 0, false);
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
        /// <param name="nullOrUndefinedOrNan">Значение, которое будет возвращено, если значение arg null или undefined.</param>
        /// <returns>Целочисленное значение, представленное в объекте arg.</returns>
#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static int JSObjectToInt32(JSValue arg, int nullOrUndefinedOrNan)
        {
            return JSObjectToInt32(arg, nullOrUndefinedOrNan, 0, false);
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
            return JSObjectToInt32(arg, 0, 0, alternateInfinity);
        }

        public static int JSObjectToInt32(JSValue arg, int nullOrUndefined, bool alternateInfinity)
        {
            return JSObjectToInt32(arg, nullOrUndefined, 0, alternateInfinity);
        }

        public static int JSObjectToInt32(JSValue arg, int nullOrUndefined, int nan, bool alternateInfinity)
        {
            return JSObjectToInt32(arg, nullOrUndefined, nullOrUndefined, nan, alternateInfinity);
        }

        public static int JSObjectToInt32(JSValue value, int @null, int undefined, int nan, bool alternateInfinity)
        {
            if (value == null)
                return @null;
            
            switch (value._valueType)
            {
                case JSValueType.Boolean:
                case JSValueType.Integer:
                    {
                        return value._iValue;
                    }
                case JSValueType.Double:
                    {
                        if (double.IsNaN(value._dValue))
                            return nan;

                        if (double.IsInfinity(value._dValue))
                            return alternateInfinity ? double.IsPositiveInfinity(value._dValue) ? int.MaxValue : int.MinValue : 0;

                        return (int)(long)value._dValue;
                    }
                case JSValueType.String:
                    {
                        double x = 0;
                        int ix = 0;
                        string s = (value._oValue.ToString()).Trim();

                        if (!Tools.ParseNumber(s, ref ix, out x, 0, ParseNumberOptions.AllowAutoRadix | ParseNumberOptions.AllowFloat) || ix < s.Length)
                            return 0;

                        if (double.IsNaN(x))
                            return nan;

                        if (double.IsInfinity(x))
                            return alternateInfinity ? double.IsPositiveInfinity(x) ? int.MaxValue : int.MinValue : 0;

                        return (int)x;
                    }
                case JSValueType.Date:
                case JSValueType.Function:
                case JSValueType.Object:
                    {
                        if (value._oValue == null)
                            return @null;

                        value = value.ToPrimitiveValue_Value_String();
                        return JSObjectToInt32(value, 0, 0, 0, true);
                    }
                case JSValueType.NotExists:
                case JSValueType.Undefined:
                case JSValueType.NotExistsInObject:
                    return undefined;
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
        /// <param name="nullOrUndefinedOrNan">Значение, которое будет возвращено, если значение arg null или undefined.</param>
        /// <returns>Целочисленное значение, представленное в объекте arg.</returns>
#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static long JSObjectToInt64(JSValue arg, long nullOrUndefinedOrNan)
        {
            return JSObjectToInt64(arg, nullOrUndefinedOrNan, false);
        }

        /// <summary>
        /// Преобразует JSObject в значение типа Int64.
        /// </summary>
        /// <param name="arg">JSObject, значение которого нужно преобразовать.</param>
        /// <param name="nullOrUndefined">Значение, которое будет возвращено, если значение arg null или undefined.</param>
        /// <param name="alternateInfinity">Если истина, для значений +Infinity и -Infinity будут возвращены значения int.MaxValue и int.MinValue соответственно.</param>
        /// <returns>Целочисленное значение, представленное в объекте arg.</returns>
        public static long JSObjectToInt64(JSValue arg, long nullOrUndefined, bool alternateInfinity)
        {
            if (arg == null)
                return nullOrUndefined;

            var r = arg;
            switch (r._valueType)
            {
                case JSValueType.Boolean:
                case JSValueType.Integer:
                    {
                        return r._iValue;
                    }
                case JSValueType.Double:
                    {
                        if (double.IsNaN(r._dValue))
                            return 0;
                        if (double.IsInfinity(r._dValue))
                            return alternateInfinity ? double.IsPositiveInfinity(r._dValue) ? long.MaxValue : long.MinValue : 0;
                        return (long)r._dValue;
                    }
                case JSValueType.String:
                    {
                        double x = 0;
                        int ix = 0;
                        string s = (r._oValue.ToString()).Trim();
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
                        if (r._oValue == null)
                            return nullOrUndefined;
                        r = r.ToPrimitiveValue_Value_String();
                        return JSObjectToInt64(r, nullOrUndefined, alternateInfinity);
                    }
                case JSValueType.NotExists:
                case JSValueType.Undefined:
                case JSValueType.NotExistsInObject:
                    return nullOrUndefined;
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
                result._valueType = JSValueType.Integer;
                result._iValue = 0;
                return result;
            }
            switch (arg._valueType)
            {
                case JSValueType.Boolean:
                    {
                        result._valueType = JSValueType.Integer;
                        result._iValue = arg._iValue;
                        return result;
                    }
                case JSValueType.Integer:
                case JSValueType.Double:
                    return arg;
                case JSValueType.String:
                    {
                        double x = 0;
                        int ix = 0;
                        string s = (arg._oValue.ToString()).Trim(TrimChars);
                        if (!Tools.ParseNumber(s, ref ix, out x) || ix < s.Length)
                            x = double.NaN;
                        result._valueType = JSValueType.Double;
                        result._dValue = x;
                        return result;
                    }
                case JSValueType.Date:
                case JSValueType.Function:
                case JSValueType.Object:
                    {
                        if (arg._oValue == null)
                        {
                            result._valueType = JSValueType.Integer;
                            result._iValue = 0;
                            return result;
                        }
                        arg = arg.ToPrimitiveValue_Value_String();
                        return JSObjectToNumber(arg);
                    }
                case JSValueType.NotExists:
                case JSValueType.Undefined:
                case JSValueType.NotExistsInObject:
                    {
                        result._valueType = JSValueType.Double;
                        result._dValue = double.NaN;
                        return result;
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object convertJStoObj(JSValue jsobj, Type targetType, bool hightLoyalty)
        {
            if (jsobj == null)
                return null;

            if (targetType.IsAssignableFrom(jsobj.GetType()))
                return jsobj;

            if (targetType.GetTypeInfo().IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                targetType = targetType.GetGenericArguments()[0];

            object value = null;
            switch (jsobj._valueType)
            {
                case JSValueType.Boolean:
                    {
                        if (hightLoyalty)
                        {
                            if (targetType == typeof(string))
                                return jsobj._iValue != 0 ? JSBool.TrueString : JSBool.FalseString;

                        }

                        if (targetType == typeof(bool))
                            return jsobj._iValue != 0;

                        if (targetType == typeof(JSBool))
                            return new JSBool(jsobj._iValue != 0);

                        return null;
                    }
                case JSValueType.Double:
                    {
                        if (hightLoyalty)
                        {
                            if (targetType == typeof(int))
                                return (int)jsobj._dValue;
                            if (targetType == typeof(uint))
                                return (uint)jsobj._dValue;
                            if (targetType == typeof(long))
                                return (long)jsobj._dValue;
                            if (targetType == typeof(ulong))
                                return (ulong)jsobj._dValue;
                            if (targetType == typeof(decimal))
                                return (decimal)jsobj._dValue;
                            if (targetType == typeof(string))
                                return DoubleToString(jsobj._dValue);

                            if (targetType.GetTypeInfo().IsEnum)
                                return Enum.ToObject(targetType, (long)jsobj._dValue);
                        }

                        if (targetType == typeof(double))
                            return (double)jsobj._dValue;
                        if (targetType == typeof(float))
                            return (float)jsobj._dValue;

                        if (targetType == typeof(Number))
                            return new Number(jsobj._dValue);

                        return null;
                    }
                case JSValueType.Integer:
                    {
                        if (hightLoyalty)
                        {
                            if (targetType == typeof(string))
                                return Int32ToString(jsobj._iValue);
                        }

                        if (targetType == typeof(int))
                            return (int)jsobj._iValue;

                        if (targetType == typeof(uint))
                            return (uint)jsobj._iValue;
                        if (targetType == typeof(long))
                            return (long)jsobj._iValue;
                        if (targetType == typeof(ulong))
                            return (ulong)jsobj._iValue;
                        if (targetType == typeof(double))
                            return (double)jsobj._iValue;
                        if (targetType == typeof(float))
                            return (float)jsobj._iValue;
                        if (targetType == typeof(decimal))
                            return (decimal)jsobj._iValue;

                        if (targetType == typeof(Number))
                            return new Number(jsobj._iValue);

                        if (targetType.GetTypeInfo().IsEnum)
                            return Enum.ToObject(targetType, jsobj._iValue);

                        return null;
                    }
                case JSValueType.String:
                    {
                        if (hightLoyalty)
                        {
                            if (targetType == typeof(byte))
                                return JSObjectToInt32(jsobj);
                            if (targetType == typeof(sbyte))
                                return JSObjectToInt32(jsobj);
                            if (targetType == typeof(short))
                                return JSObjectToInt32(jsobj);
                            if (targetType == typeof(ushort))
                                return JSObjectToInt32(jsobj);
                            if (targetType == typeof(int))
                                return JSObjectToInt32(jsobj);
                            if (targetType == typeof(uint))
                                return (uint)JSObjectToInt64(jsobj);
                            if (targetType == typeof(long))
                                return JSObjectToInt64(jsobj);
                            if (targetType == typeof(ulong))
                                return (ulong)JSObjectToInt64(jsobj);

                            if (targetType == typeof(double))
                            {
                                if (jsobj.Value.ToString() == "NaN")
                                    return double.NaN;

                                var r = JSObjectToDouble(jsobj);
                                if (!double.IsNaN(r))
                                    return r;

                                return null;
                            }

                            if (targetType == typeof(float))
                            {
                                var r = JSObjectToDouble(jsobj);
                                if (!double.IsNaN(r))
                                    return (float)r;

                                return null;
                            }

                            if (targetType == typeof(decimal))
                            {
                                var r = JSObjectToDouble(jsobj);
                                if (!double.IsNaN(r))
                                    return (decimal)r;

                                return null;
                            }

                            if (targetType.GetTypeInfo().IsEnum)
                            {
                                try
                                {
                                    return Enum.Parse(targetType, jsobj.Value.ToString());
                                }
                                catch
                                {
                                    return null;
                                }
                            }
                        }

                        if (targetType == typeof(string))
                            return jsobj.Value.ToString();

                        if (targetType == typeof(BaseLibrary.String))
                            return new BaseLibrary.String(jsobj.Value.ToString());

                        return null;
                    }
                case JSValueType.Symbol:
                    {
                        if (hightLoyalty)
                        {
                            if (targetType == typeof(string))
                                return jsobj.Value.ToString();
                        }

                        if (targetType == typeof(Symbol))
                            return jsobj.Value;

                        return null;
                    }
                case JSValueType.Function:
                    {
                        if (hightLoyalty)
                        {
                            if (targetType == typeof(string))
                                return jsobj.Value.ToString();
                        }

                        if (!targetType.GetTypeInfo().IsAbstract && targetType.GetTypeInfo().IsSubclassOf(typeof(Delegate)))
                            return (jsobj.Value as Function).MakeDelegate(targetType);

                        goto default;
                    }
                default:
                    {
                        value = jsobj.Value;

                        if (value == null)
                            return null;

                        break;
                    }
            }

            if (targetType.IsAssignableFrom(value.GetType()))
                return value;
#if (PORTABLE || NETCORE)
            if (IntrospectionExtensions.GetTypeInfo(targetType).IsEnum && Enum.IsDefined(targetType, value))
                return value;
#else
            if (targetType.IsEnum && Enum.IsDefined(targetType, value))
                return value;
#endif
            var tpres = value as Proxy;
            if (tpres != null && targetType.IsAssignableFrom(tpres._hostedType))
            {
                jsobj = tpres.PrototypeInstance;
                if (jsobj is ObjectWrapper)
                    return jsobj.Value;

                return jsobj;
            }

            if (value is ConstructorProxy && typeof(Type).IsAssignableFrom(targetType))
            {
                return (value as ConstructorProxy)._staticProxy._hostedType;
            }

            if ((value is BaseLibrary.Array || value is TypedArray || value is ArrayBuffer)
                && typeof(IEnumerable).IsAssignableFrom(targetType))
            {
                Type @interface = null;
                Type elementType = null;

                if ((targetType.IsArray && (elementType = targetType.GetElementType()) != null)
#if PORTABLE || NETCORE
                || ((@interface = targetType.GetInterface(typeof(IEnumerable<>).Name)) != null
#else
                || ((@interface = targetType.GetTypeInfo().GetInterface(typeof(IEnumerable<>).Name)) != null
#endif
                     && targetType.IsAssignableFrom((elementType = @interface.GetGenericArguments()[0]).MakeArrayType())))
                {
                    if (elementType.GetTypeInfo().IsPrimitive)
                    {
                        if (elementType == typeof(byte) && value is ArrayBuffer)
                            return (value as ArrayBuffer).GetData();

                        var ta = value as TypedArray;
                        if (ta != null && ta.ElementType == elementType)
                            return ta.ToNativeArray();
                    }

                    return convertArray(value as BaseLibrary.Array, elementType, hightLoyalty);
                }
                else if (targetType.IsAssignableFrom(typeof(object[])))
                {
                    return convertArray(value as BaseLibrary.Array, typeof(object), hightLoyalty);
                }
            }

            return null;
        }

        private static object convertArray(BaseLibrary.Array array, Type elementType, bool hightLoyalty)
        {
            if (array == null)
                return null;

            var result = (IList)Activator.CreateInstance(elementType.MakeArrayType(), new object[] { (int)array._data.Length });

            for (var j = result.Count; j-- > 0;)
            {
                var temp = (array._data[j] ?? JSValue.undefined);
                var value = convertJStoObj(temp, elementType, hightLoyalty);

                if (!hightLoyalty && value == null && (elementType.GetTypeInfo().IsValueType || (!temp.IsNull && !temp.IsUndefined())))
                    return null;

                result[j] = value;
            }

            return result;
        }

        public static string DoubleToString(double d)
        {
            if (d == 0.0)
                return "0";
            if (double.IsPositiveInfinity(d))
                return "Infinity";
            if (double.IsNegativeInfinity(d))
                return "-Infinity";
            if (double.IsNaN(d))
                return "NaN";

            string res = null;
            lock (cachedDoubleString)
            {
                for (var i = 8; i-- > 0;)
                {
                    if (cachedDoubleString[i].key == d)
                        return cachedDoubleString[i].value;
                }

                var abs = System.Math.Abs(d);
                if (abs < 0.000001)
                    res = d.ToString("0.####e-0", CultureInfo.InvariantCulture);
                else if (abs >= 1e+21)
                    res = d.ToString("0.####e+0", CultureInfo.InvariantCulture);
                else
                {
                    int neg = (d < 0.0 || (d == -0.0 && double.IsNegativeInfinity(1.0 / d))) ? 1 : 0;

                    if (abs >= 1e+18)
                    {
                        res = ((ulong)(abs / 1000.0)).ToString(CultureInfo.InvariantCulture) + "000";
                    }
                    else
                    {
                        ulong absIntPart = (abs < 1.0) ? 0L : (ulong)(abs);
                        res = (absIntPart == 0 ? "0" : absIntPart.ToString(CultureInfo.InvariantCulture));

                        abs %= 1.0;
                        if (abs != 0 && res.Length <= 15)
                        {
                            string fracPart = abs.ToString(divFormats[15 - res.Length], CultureInfo.InvariantCulture);
                            if (fracPart == "1")
                                res = (absIntPart + 1).ToString(CultureInfo.InvariantCulture);
                            else
                                res += fracPart;
                        }
                    }

                    if (neg == 1)
                        res = "-" + res;
                }

                cachedDoubleString[cachedDoubleStringsIndex].key = d;
                cachedDoubleString[cachedDoubleStringsIndex].value = res;
                cachedDoubleStringsIndex = (cachedDoubleStringsIndex + 1) & 7;
            }

            return res;
        }

        internal static void CheckEndOfInput(string code, ref int i)
        {
            if (i >= code.Length)
                ExceptionHelper.ThrowSyntaxError("Unexpected end of line", code, i);
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

            intStrCacheIndex = (intStrCacheIndex + 1) & (cacheSize - 1);

            var cacheItem = new IntStringCacheItem
            {
                key = value,
                value = value.ToString(CultureInfo.InvariantCulture)
            };

            intStringCache[intStrCacheIndex] = cacheItem;
            return cacheItem.value;
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
            if (code == null)
                throw new ArgumentNullException("code");

            if (code.Length == 0)
            {
                value = 0;
                return true;
            }

            if (radix != 0 && (radix < 2 || radix > 36))
            {
                value = double.NaN;
                return false;
            }

            bool raiseOldOctalLiterals = (options & ParseNumberOptions.RaiseIfOctal) != 0;
            bool processOldOctals = (options & ParseNumberOptions.ProcessOctalLiteralsOldSyntax) != 0;
            bool allowRadixDetection = (options & ParseNumberOptions.AllowAutoRadix) != 0;
            bool allowFloat = (options & ParseNumberOptions.AllowFloat) != 0;

            int i = index;
            while (i < code.Length && IsWhiteSpace(code[i]) && !IsLineTerminator(code[i]))
                i++;

            if (i >= code.Length)
            {
                value = 0.0;
                return true;
            }

            const string NaN = "NaN";
            if (code.Length - i >= NaN.Length && code.IndexOf(NaN, i, NaN.Length, StringComparison.Ordinal) == i)
            {
                index = i + NaN.Length;
                value = double.NaN;
                return true;
            }

            int sign = 1;
            if (code[i] == '-' || code[i] == '+')
                sign = 44 - code[i++];

            const string Infinity = "Infinity";
            if (code.Length - i >= Infinity.Length && code.IndexOf(Infinity, i, Infinity.Length, StringComparison.Ordinal) == i)
            {
                index = i + Infinity.Length;
                value = sign * double.PositiveInfinity;
                return true;
            }

            bool result = false;
            if (allowRadixDetection
                && (code[i] == '0')
                && (i + 1 < code.Length))
            {
                if (IsDigit(code[i + 1]))
                {
                    if (raiseOldOctalLiterals)
                        ExceptionHelper.ThrowSyntaxError("Octal literals not allowed in strict mode", code, i);

                    while ((i + 1 < code.Length) && (code[i + 1] == '0'))
                    {
                        i++;
                    }

                    if (processOldOctals && (i + 1 < code.Length) && IsDigit(code[i + 1]))
                        radix = 8;
                }
                else
                {
                    if ((radix == 0 || radix == 16)
                     && (code[i + 1] == 'x' || code[i + 1] == 'X'))
                    {
                        i += 2;
                        radix = 16;
                    }
                    else if ((radix == 0 || radix == 8)
                     && (code[i + 1] == 'o' || code[i + 1] == 'O'))
                    {
                        i += 2;
                        radix = 8;
                    }
                    else if ((radix == 0 || radix == 8)
                     && (code[i + 1] == 'b' || code[i + 1] == 'B'))
                    {
                        i += 2;
                        radix = 2;
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
                    if (IsDigit(code[i]))
                    {
                        if (scount <= 18)
                        {
                            temp = temp * 10 + (ulong)(code[i++] - '0');
                        }
                        else
                        {
                            deg++;
                            i++;
                        }

                        scount++;
                        result = true;
                    }
                    else
                    {
                        break;
                    }
                }

                if (!result && (i >= code.Length || code[i] != '.'))
                {
                    value = double.NaN;
                    return false;
                }

                if (i < code.Length && code[i] == '.')
                {
                    i++;
                    while (i < code.Length)
                    {
                        if (IsDigit(code[i]))
                        {
                            if (scount <= 18 || ((temp * 10) / 10 == temp))
                            {
                                temp = temp * 10 + (ulong)(code[i++] - '0');
                                deg--;
                            }
                            else
                            {
                                i++;
                            }

                            scount++;
                            result = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (!result)
                {
                    value = double.NaN;
                    return false;
                }

                if (i < code.Length && (code[i] == 'e' || code[i] == 'E'))
                {
                    i++;
                    int td = 0;
                    int esign = code[i] == 43 || code[i] == 45 ? 44 - code[i++] : 1;
                    if (!IsDigit(code[i]))
                    {
                        i--;
                        if (code[i] == 'e' || code[i] == 'E')
                            i--;
                    }
                    else
                    {
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
                }

                if (deg != 0)
                {
                    if (deg < 0)
                    {
                        if (temp != 0)
                        {
                            while ((temp % 10) == 0 && deg < 0)
                            {
                                deg++;
                                temp /= 10;
                            }
                        }

                        if (temp == 0)
                        {
                            value = 0;
                            deg = 0;
                        }
                        else if (deg < -18)
                        {
                            var tail = temp % 1000000;
                            temp /= 1000000;

                            value = (double)(temp * 1e-12M + tail * 1e-18M);
                            deg += 18;
                        }
                        else
                        {
                            var frac = temp;
                            temp = temp / (ulong)powersOf10[-deg + 18];
                            frac -= (temp * (ulong)powersOf10[-deg + 18]);
                            var mask = (ulong)powersOf10[-deg + 18];

                            int e = 0;

                            if (frac != 0)
                            {
                                while (frac != 0 && (temp >> 52) == 0)
                                {
                                    e++;
                                    temp <<= 1;
                                    frac <<= 1;
                                    if (frac >= mask)
                                    {
                                        temp |= 1;
                                        frac -= mask;
                                    }
                                }

                                if (frac >= mask >> 1)
                                {
                                    temp++;
                                }

                                while (temp < (1UL << 52))
                                {
                                    e++;
                                    temp <<= 1;
                                }
                            }
                            else if (temp != 0)
                            {
                                while ((temp >> 52) == 0)
                                {
                                    temp <<= 1;
                                    e++;
                                }

                                while (temp > ((1UL << 53) - 1))
                                {
                                    temp >>= 1;
                                    e--;
                                }
                            }

                            temp = (temp & ((1UL << 52) - 1));
                            e = 1023 - e + 52;
                            temp |= ((ulong)e << 52);
                            value = BitConverter.Int64BitsToDouble((long)temp);

                            deg = 0;
                        }
                    }
                    else
                    {
                        if (deg > 10)
                        {
                            value = temp * 1e+10;
                            deg -= 10;
                        }
                        else
                        {
                            var tail = temp % 10000;
                            deg += 4;
                            temp /= 10000;

                            value = (double)(temp * powersOf10[deg + 18]) + (double)(tail * powersOf10[deg + 18 - 4]);
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

                value *= sign;
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
                    var degSign = hexCharToInt(code[i]);
                    if (degSign >= radix || (NumChars[degSign] != code[i] && (NumChars[degSign] + ('a' - 'A')) != code[i]))
                    {
                        break;
                    }
                    else
                    {
                        if (extended)
                        {
                            doubleTemp = doubleTemp * radix + degSign;
                        }
                        else
                        {
                            temp = temp * (ulong)radix + (ulong)degSign;
                            if ((temp & 0xFE00000000000000) != 0)
                            {
                                extended = true;
                                doubleTemp = temp;
                            }
                        }
                        result = true;
                    }
                    i++;
                }
                if (!result)
                {
                    value = double.NaN;
                    return false;
                }
                value = extended ? doubleTemp : temp;
                value *= sign;
                index = i;
                return true;
            }
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static string Unescape(string code, bool strict)
        {
            return Unescape(code, strict, true, false, true);
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static string Unescape(string code, bool strict, bool processUnknown, bool processRegexComp)
        {
            return Unescape(code, strict, processUnknown, processRegexComp, true);
        }

        public static string Unescape(string code, bool strict, bool processUnknown, bool processRegexComp, bool fullUnicode)
        {
            if (code == null)
                throw new ArgumentNullException("code");
            if (code.Length == 0)
                return code;

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
                                    ExceptionHelper.ThrowSyntaxError("Invalid escape code (\"" + code + "\")");
                            }

                            if (fullUnicode && code[i] == 'u' && code[i + 1] == '{')
                            {
                                // look here in section 3.7 Surrogates for more information.
                                // http://unicode.org/versions/Unicode3.0.0/ch03.pdf

                                int closingBracket = code.IndexOf('}', i + 2);
                                if (closingBracket == -1)
                                    ExceptionHelper.Throw(new SyntaxError("Invalid escape sequence"));

                                string c = code.Substring(i + 2, closingBracket - i - 2);
                                uint ucs = 0;
                                if (uint.TryParse(c, NumberStyles.HexNumber, null, out ucs))
                                {
                                    if (ucs <= 0xFFFF)
                                        res.Append((char)ucs);
                                    else if (ucs <= 0x10FFFF)
                                    {
                                        ucs -= 0x10000;
                                        char h = (char)((ucs >> 10) + 0xD800);
                                        char l = (char)((ucs % 0x400) + 0xDC00);
                                        res.Append(h).Append(l);
                                    }
                                    else
                                        ExceptionHelper.Throw(new SyntaxError("Invalid escape sequence '\\u{" + c + "}'"));
                                    i += c.Length + 2;
                                }
                                else
                                {
                                    if (processRegexComp)
                                        res.Append(code[i]);
                                    else
                                        ExceptionHelper.Throw(new SyntaxError("Invalid escape sequence '\\u{" + c + "}'"));
                                }
                            }
                            else
                            {
                                string c = code.Substring(i + 1, code[i] == 'u' ? 4 : 2);
                                ushort chc = 0;
                                if (ushort.TryParse(c, NumberStyles.HexNumber, null, out chc))
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
                                        ExceptionHelper.Throw(new SyntaxError("Invalid escape sequence '\\" + code[i] + c + "'"));
                                }
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
                        case '\n':
                        {
                            break;
                        }
                        case '\r':
                        {
                            if (code.Length > i + 1 && code[i + 1] == '\n')
                                i++;
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
                            if (!processRegexComp && code[i] >= '0' && code[i] <= '7')
                            {
                                if (strict && (code[i] != '0' || (code.Length > i + 1 && code[i + 1] >= '0' && code[i + 1] <= '7')))
                                    ExceptionHelper.Throw(new SyntaxError("Octal literals are not allowed in strict mode."));

                                var ccode = code[i] - '0';
                                if (i + 1 < code.Length && code[i + 1] >= '0' && code[i + 1] <= '7')
                                    ccode = ccode * 8 + (code[++i] - '0');
                                if (i + 1 < code.Length && code[i + 1] >= '0' && code[i + 1] <= '7')
                                    ccode = ccode * 8 + (code[++i] - '0');
                                res.Append((char)ccode);
                            }
                            else if (processUnknown)
                            {
                                res.Append(code[i]);
                            }
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
        public static string UnescapeNextChar(string code, int index, out int processedChars, bool strict)
        {
            return UnescapeNextChar(code, index, out processedChars, strict, true, false, true);
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static string UnescapeNextChar(string code, int index, out int processedChars, bool strict, bool processUnknown, bool processRegexComp)
        {
            return UnescapeNextChar(code, index, out processedChars, strict, processUnknown, processRegexComp, true);
        }

        public static string UnescapeNextChar(string code, int index, out int processedChars, bool strict, bool processUnknown, bool processRegexComp, bool fullUnicode)
        {
            processedChars = 0;

            if (code == null)
                throw new ArgumentNullException("code");
            if (code.Length == 0)
                return code;
            if (index >= code.Length)
                return "";


            int i = index;
            if (code[i] == '\\' && i + 1 < code.Length)
            {
                i++;
                processedChars = 2; // most cases
                switch (code[i])
                {
                    case 'x':
                    case 'u':
                        {
                            if (i + (code[i] == 'u' ? 5 : 3) > code.Length)
                            {
                                if (processRegexComp)
                                    return code[i].ToString();
                                else
                                    ExceptionHelper.Throw(new SyntaxError("Invalid escape code (\"" + code + "\")"));
                            }

                            if (code[i] == 'u' && code[i + 1] == '{')
                            {
                                if (fullUnicode)
                                {
                                    // look here in section 3.7 Surrogates for more information.
                                    // http://unicode.org/versions/Unicode3.0.0/ch03.pdf

                                    int closingBracket = code.IndexOf('}', i + 2);
                                    if (closingBracket == -1)
                                        ExceptionHelper.Throw(new SyntaxError("Invalid escape sequence"));

                                    string c = code.Substring(i + 2, closingBracket - i - 2);
                                    uint ucs = 0;
                                    if (uint.TryParse(c, NumberStyles.HexNumber, null, out ucs))
                                    {
                                        processedChars += c.Length + 2;
                                        if (ucs <= 0xFFFF)
                                            return ((char)ucs).ToString();
                                        else if (ucs <= 0x10FFFF)
                                        {
                                            ucs -= 0x10000;
                                            char h = (char)((ucs >> 10) + 0xD800);
                                            char l = (char)((ucs % 0x400) + 0xDC00);
                                            return h.ToString() + l.ToString();
                                        }
                                        else
                                            ExceptionHelper.Throw(new SyntaxError("Invalid escape sequence '\\u{" + c + "}'"));
                                    }
                                    else
                                    {
                                        if (processRegexComp)
                                            return code[i].ToString();
                                        else
                                            ExceptionHelper.Throw(new SyntaxError("Invalid escape sequence '\\u{" + c + "}'"));
                                    }
                                }
                                else
                                    return code[i].ToString();
                            }
                            else
                            {
                                string c = code.Substring(i + 1, code[i] == 'u' ? 4 : 2);
                                ushort chc = 0;
                                if (ushort.TryParse(c, System.Globalization.NumberStyles.HexNumber, null, out chc))
                                {
                                    processedChars += c.Length;
                                    char ch = (char)chc;
                                    return ch.ToString();
                                }
                                else
                                {
                                    if (processRegexComp)
                                        return code[i].ToString();
                                    else
                                        ExceptionHelper.Throw(new SyntaxError("Invalid escape sequence '\\" + code[i] + c + "'"));
                                }
                            }
                            return code[i].ToString(); // this will never be reached
                        }
                    case 't':
                        return (processRegexComp ? "\\t" : "\t");
                    case 'f':
                        return (processRegexComp ? "\\f" : "\f");
                    case 'v':
                        return (processRegexComp ? "\\v" : "\v");
                    case 'b':
                        return (processRegexComp ? "\\b" : "\b");
                    case 'n':
                        return (processRegexComp ? "\\n" : "\n");
                    case 'r':
                        return (processRegexComp ? "\\r" : "\r");
                    case '\n':
                        {
                            return "";
                        }
                    case '\r':
                        {
                            if (code.Length > i + 1 && code[i] == '\n')
                                processedChars = 3;
                            return "";
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
                                    processedChars++;
                                    return "\\c" + ch.ToString();
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
                            return @"\b\B";
                        }
                    default:
                        {
                            if (code[i] >= '0' && code[i] <= '7' && !processRegexComp)
                            {
                                if (strict)
                                    ExceptionHelper.Throw(new SyntaxError("Octal literals are not allowed in strict mode."));

                                var ccode = code[i] - '0';
                                if (i + 1 < code.Length && code[i + 1] >= '0' && code[i + 1] <= '7')
                                {
                                    ccode = ccode * 8 + (code[++i] - '0');
                                    processedChars++;
                                }
                                if (i + 1 < code.Length && code[i + 1] >= '0' && code[i + 1] <= '7')
                                {
                                    ccode = ccode * 8 + (code[++i] - '0');
                                    processedChars++;
                                }
                                return ((char)ccode).ToString();
                            }
                            else
                            {
                                if (!processUnknown)
                                    return "\\" + code[i].ToString();
                                return code[i].ToString();
                            }
                        }
                }
            }

            processedChars = 1;
            return code[i].ToString();
        }

        internal static int NextCodePoint(string str, ref int i)
        {
            if (str[i] >= '\uD800' && str[i] <= '\uDBFF' && i + 1 < str.Length && str[i + 1] >= '\uDC00' && str[i + 1] <= '\uDFFF')
                return ((str[i] - 0xD800) * 0x400) + (str[++i] - 0xDC00) + 0x10000;
            return str[i];
        }
        internal static int NextCodePoint(string str, ref int i, bool regexp)
        {
            if (str[i] >= '\uD800' && str[i] <= '\uDBFF' && i + 1 < str.Length && str[i + 1] >= '\uDC00' && str[i + 1] <= '\uDFFF')
                return ((str[i] - 0xD800) * 0x400) + (str[++i] - 0xDC00) + 0x10000;

            if (regexp && str[i] == '\\' && i + 1 < str.Length)
            {
                i++;
                if (i + 1 < str.Length && str[i] == 'c' && str[i + 1] >= 'A' && str[i + 1] <= 'Z')
                {
                    i++;
                    return str[i] - '@';
                }
                
                if (str[i] >= '0' && str[i] <= '7')
                {
                    var ccode = str[i] - '0';
                    if (i + 1 < str.Length && IsDigit(str[i + 1]))
                        ccode = ccode * 8 + (str[++i] - '0');
                    if (i + 1 < str.Length && IsDigit(str[i + 1]))
                        ccode = ccode * 8 + (str[++i] - '0');
                    return ccode;
                }

                if (str[i] == 't')
                    return '\t';
                if (str[i] == 'f')
                    return '\f';
                if (str[i] == 'v')
                    return '\v';
                if (str[i] == 'b')
                    return '\b';
                if (str[i] == 'n')
                    return '\n';
                if (str[i] == 'r')
                    return '\r';

                return NextCodePoint(str, ref i);
            }

            return str[i];
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool IsSurrogatePair(string str, int i)
        {
            return (i >= 0 && i + 1 < str.Length && str[i] >= '\uD800' && str[i] <= '\uDBFF' && str[i + 1] >= '\uDC00' && str[i + 1] <= '\uDFFF');
        }

        internal static string CodePointToString(int codePoint)
        {
            if (codePoint < 0 || codePoint > 0x10FFFF)
                ExceptionHelper.Throw(new RangeError("Invalid code point " + codePoint));

            if (codePoint <= 0xFFFF)
                return ((char)codePoint).ToString();

            codePoint -= 0x10000;
            char h = (char)((codePoint >> 10) + 0xD800);
            char l = (char)((codePoint % 0x400) + 0xDC00);
            return h.ToString() + l.ToString();
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool IsLineTerminator(char c)
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
                                while (index < code.Length && !IsLineTerminator(code[index]))
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
                                    ExceptionHelper.Throw(new SyntaxError("Unexpected end of source."));
                                index += 2;
                                work = true;
                                break;
                            }
                    }
                }

                if (Parser.Validate(code, "<!--", index))
                {
                    while (index < code.Length && !IsLineTerminator(code[index]))
                        index++;

                    work = true;
                }
            }
            while (work);

            if (skipSpaces)
            {
                while ((index < code.Length) && (IsWhiteSpace(code[index])))
                    index++;
            }
        }

        internal static string removeComments(string code, int startPosition)
        {
            StringBuilder res = null;
            for (var i = startPosition; i < code.Length;)
            {
                while (i < code.Length && IsWhiteSpace(code[i]))
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
                    if (IsWhiteSpace(code[s]))
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

                if (Parser.ValidateString(code, ref i, false))
                {
                    if (res != null)
                    {
                        for (; s < i; s++)
                            res.Append(code[s]);
                    }
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
            var c = hexCharToInt(p);
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
        internal static int hexCharToInt(char p)
        {
            return ((p % 'a' % 'A' + 10) % ('0' + 10));
        }

        internal static long getLengthOfArraylike(JSValue src, bool reassignLen)
        {
            var length = src.GetProperty("length", true, PropertyScope.Common); // тут же проверка на null/undefined с падением если надо

            var result = (uint)JSObjectToInt64(InvokeGetter(length, src).ToPrimitiveValue_Value_String(), 0, false);
            if (reassignLen)
            {
                if (length._valueType == JSValueType.Property)
                    ((length._oValue as PropertyPair).setter ?? Function.Empty).Call(src, new Arguments() { result });
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
                        _length = srca._data.Length;
                    long prew = -1;
                    foreach (var element in srca._data.DirectOrder)
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
                        if (evalProps && value._valueType == JSValueType.Property)
                            value = (value._oValue as PropertyPair).getter == null ? JSValue.undefined : (value._oValue as PropertyPair).getter.Call(src, null).CloneImpl(false);
                        else if (clone)
                            value = value.CloneImpl(false);
                        if (temp._data[element.Key] == null)
                            temp._data[element.Key] = value;
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
                        if (evalProps && value._valueType == JSValueType.Property)
                            value = (value._oValue as PropertyPair).getter == null ? JSValue.undefined : (value._oValue as PropertyPair).getter.Call(src, null).CloneImpl(false);
                        else if (clone)
                            value = value.CloneImpl(false);
                        if (!goDeep && System.Math.Abs(prew - index.Key) > 1)
                        {
                            goDeep = true;
                        }
                        if (temp._data[(int)(uint)index.Key] == null)
                            temp._data[(int)(uint)index.Key] = value;
                    }
                    goDeep |= System.Math.Abs(prew - _length) > 1;
                }
                if (src.__proto__ == JSValue.@null)
                    break;
                src = src.__proto__._oValue as JSValue ?? src.__proto__;
                if (src == null || (src._valueType >= JSValueType.String && src._oValue == null))
                    break;
            }
            temp._data[(int)(_length - 1)] = temp._data[(int)(_length - 1)];
            return temp;
        }

        internal static IEnumerable<KeyValuePair<uint, JSValue>> EnumerateArraylike(long length, JSValue src)
        {
            if (src._valueType == JSValueType.Object && src.Value is BaseLibrary.Array)
            {
                foreach (var item in (src.Value as BaseLibrary.Array)._data.DirectOrder)
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

        internal static int CompareWithMask(Enum x, Enum y, Enum mask)
        {
            return ((int)(ValueType)x & (int)(ValueType)mask) - ((int)(ValueType)y & (int)(ValueType)mask);
        }

        internal static bool IsEqual(Enum x, Enum y, Enum mask)
        {
            return ((int)(ValueType)x & (int)(ValueType)mask) == ((int)(ValueType)y & (int)(ValueType)mask);
        }

        internal static JSValue InvokeGetter(JSValue property, JSValue target)
        {
            if (property._valueType != JSValueType.Property)
                return property;

            var getter = property._oValue as PropertyPair;
            if (getter == null || getter.getter == null)
                return JSValue.undefined;

            property = getter.getter.Call(target, null);
            if (property._valueType < JSValueType.Undefined)
                property = JSValue.undefined;

            return property;
        }

        internal static JSValue EvalExpressionSafe(Context context, Expressions.Expression source)
        {
            var a = source.Evaluate(context);
            if (a == null)
                return JSValue.undefined;

            if (a._valueType != JSValueType.SpreadOperatorResult)
            {
                a = a.CloneImpl(false, JSValueAttributesInternal.ReadOnly
                    | JSValueAttributesInternal.SystemObject
                    | JSValueAttributesInternal.Temporary
                    | JSValueAttributesInternal.Reassign
                    | JSValueAttributesInternal.ProxyPrototype
                    | JSValueAttributesInternal.DoNotEnumerate
                    | JSValueAttributesInternal.NonConfigurable
                    | JSValueAttributesInternal.DoNotDelete);
                a._attributes |= JSValueAttributesInternal.Cloned;
            }

            return a;
        }

        internal static bool IsWhiteSpace(char p)
        {
            var fb = p >> 8;
            if (fb != 0x0 && fb != 0x16 && fb != 0x18 && fb != 0x20 && fb != 0x30 && fb != 0xFE)
                return false;

            for (var i = 0; i < TrimChars.Length; i++)
            {
                if (p == TrimChars[i])
                    return true;
            }

            return false;
        }

        internal static Arguments CreateArguments(Expressions.Expression[] arguments, Context initiator)
        {
            Arguments argumentsObject = new Arguments(initiator);
            IList<JSValue> spreadSource = null;

            int targetIndex = 0;
            int sourceIndex = 0;
            int spreadIndex = 0;

            while (sourceIndex < arguments.Length)
            {
                if (spreadSource != null)
                {
                    if (spreadIndex < spreadSource.Count)
                    {
                        argumentsObject[targetIndex++] = spreadSource[spreadIndex];
                        spreadIndex++;
                    }
                    if (spreadIndex == spreadSource.Count)
                    {
                        spreadSource = null;
                        sourceIndex++;
                    }
                }
                else
                {
                    var value = EvalExpressionSafe(initiator, arguments[sourceIndex]);
                    if (value._valueType == JSValueType.SpreadOperatorResult)
                    {
                        spreadIndex = 0;
                        spreadSource = value._oValue as IList<JSValue>;
                        continue;
                    }
                    else
                    {
                        sourceIndex++;
                        argumentsObject[targetIndex] = value;
                    }
                    targetIndex++;
                }
            }

            argumentsObject.length = targetIndex;
            return argumentsObject;
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
#if (PORTABLE || NETCORE)
                    if (argument.Type.GetTypeInfo().IsValueType)
#else
                    if (argument.Type.IsValueType)
#endif
                    {
                        argument = Expression.Convert(argument, typeof(object));
                    }

                    var currentBaseContext = Context.CurrentGlobalContext;

                    expressions.Add(Expression.Call(
                        argumentsParameter,
                        typeof(Arguments).GetRuntimeMethod("Add", new[] { typeof(JSValue) }),
                        Expression.Call(
                            Expression.Constant(currentBaseContext),
                            methodof<object, JSValue>(currentBaseContext.ProxyValue),
                            argument)));
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


        public static string GetTypeName(JSValue v)
        {
            if (v == null)
                return "null";

            switch (v._valueType)
            {
                case JSValueType.NotExists:
                case JSValueType.NotExistsInObject:
                case JSValueType.Undefined:
                    return "undefined";
                case JSValueType.Boolean:
                    return "Boolean";
                case JSValueType.Integer:
                case JSValueType.Double:
                    return "Number";
                case JSValueType.String:
                    return "String";
                case JSValueType.Symbol:
                    return "Symbol";
                case JSValueType.Object:
                    {
                        var o = v as ObjectWrapper;
                        if (o == null)
                            o = v._oValue as ObjectWrapper;
                        if (o != null)
                            return o.Value.GetType().Name;

                        if (v._oValue == null)
                            return "null";

                        if (v._oValue is GlobalObject)
                            return "global";

                        if (v._oValue is Proxy)
                        {
                            var hostedType = (v._oValue as Proxy)._hostedType;
                            if (hostedType == typeof(JSObject))
                                return "Object";
                            return hostedType.Name;
                        }

                        if (v.Value.GetType() == typeof(JSObject))
                            return "Object";
                        return v.Value.GetType().Name;
                    }
                case JSValueType.Function:
                    return "Function";
                case JSValueType.Date:
                    return "Date";
                case JSValueType.Property:
                    {
                        var prop = v._oValue as PropertyPair;
                        if (prop != null)
                        {
                            var tempStr = "";
                            if (prop.getter != null)
                                tempStr += "Getter";
                            if (prop.setter != null)
                                tempStr += ((tempStr.Length > 0) ? "/" : "") + "Setter";
                            if (tempStr.Length == 0)
                                tempStr = "Invalid";
                            return tempStr + " Property";
                        }
                        return "Property";
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        public static string JSValueToObjectString(JSValue v)
        {
            return Tools.JSValueToObjectString(v, 1, 0);
        }

        public static string JSValueToObjectString(JSValue v, int maxRecursionDepth)
        {
            return Tools.JSValueToObjectString(v, maxRecursionDepth, 0);
        }

        internal static string JSValueToObjectString(JSValue v, int maxRecursionDepth, int recursionDepth = 0)
        {
            if (v == null)
                return "null";

            switch (v.ValueType)
            {
                case JSValueType.String:
                    return "\"" + v.ToString() + "\"";
                case JSValueType.Date:
                    string dstr = v.ToString();
                    if (dstr == "Invalid date")
                        return dstr;
                    return "Date " + dstr;
                case JSValueType.Function:
                    BaseLibrary.Function f = v.Value as BaseLibrary.Function;
                    if (f == null)
                        return v.ToString();
                    if (recursionDepth >= maxRecursionDepth)
                        return f.name + "()";
                    if (recursionDepth == maxRecursionDepth - 1)
                        return f.ToString(true);
                    return f.ToString();
                case JSValueType.Object:
                    if (v._oValue == null)
                        return "null";

                    if (v.Value is RegExp)
                        return v.ToString();

                    if (v.Value is BaseLibrary.Array
                        || v.Value == Context.CurrentGlobalContext.GetPrototype(typeof(BaseLibrary.Array))
                        || v == Context.CurrentGlobalContext.GetPrototype(typeof(BaseLibrary.Array)))
                    {
                        BaseLibrary.Array a = v.Value as BaseLibrary.Array;
                        StringBuilder s;

                        if (a == null) // v == Array.prototype
                        {
                            s = new StringBuilder("Array [ ");
                            int j = 0;
                            for (var e = v.GetEnumerator(true, EnumerationMode.RequireValues); e.MoveNext();)
                            {
                                if (j++ > 0)
                                    s.Append(", ");
                                s.Append(e.Current.Key).Append(": ");
                                s.Append(Tools.JSValueToObjectString(e.Current.Value, maxRecursionDepth, recursionDepth + 1));
                            }
                            s.Append(" ]");
                            return s.ToString();
                        }

                        long len = (long)a.length;

                        if (recursionDepth >= maxRecursionDepth)
                            return $"Array[{len}]";

                        s = new StringBuilder($"Array ({len}) [ ");
                        int i = 0;
                        int undefs = 0;
                        for (i = 0; i < len; i++)
                        {
                            var val = a[i];
                            if (undefs > 1)
                            {
                                if (val != null && val._valueType <= JSValueType.Undefined)
                                    undefs++;
                                else
                                {
                                    s.Append(" x ").Append(undefs);
                                    s.Append(", ");
                                    s.Append(Tools.JSValueToObjectString(val, maxRecursionDepth, recursionDepth + 1));
                                    undefs = 0;
                                }
                            }
                            else
                            {
                                if (val != null && val._valueType <= JSValueType.Undefined)
                                {
                                    if (++undefs > 1)
                                        continue;
                                }
                                else
                                    undefs = 0;
                                if (i > 0)
                                    s.Append(", ");
                                s.Append(Tools.JSValueToObjectString(val, maxRecursionDepth, recursionDepth + 1));
                            }
                        }
                        if (undefs > 0)
                            s.Append(" x ").Append(undefs);

                        if (a._fields != null)
                        {
                            for (var e = a._fields.GetEnumerator(); e.MoveNext();)
                            {
                                if (i++ > 0)
                                    s.Append(", ");
                                s.Append(e.Current.Key).Append(": ");
                                s.Append(Tools.JSValueToObjectString(e.Current.Value, maxRecursionDepth, recursionDepth + 1));
                            }
                        }
                        s.Append(" ]");

                        return s.ToString();
                    }
                    else
                    {
                        string typeName = Tools.GetTypeName(v);
                        if (recursionDepth >= maxRecursionDepth)
                            return typeName;

                        StringBuilder s = new StringBuilder(typeName);
                        s.Append(" { ");

                        JSObject o = v as JSObject;
                        if (o == null)
                            o = v._oValue as JSObject;
                        if (o == null)
                            return v.ToString();


                        int i = 0;
                        for (var e = o.GetEnumerator(true, EnumerationMode.RequireValues); e.MoveNext();)
                        {
                            if (i++ > 0)
                                s.Append(", ");
                            s.Append(e.Current.Key).Append(": ");
                            s.Append(Tools.JSValueToObjectString(e.Current.Value, maxRecursionDepth, recursionDepth + 1));
                        }

                        s.Append(" }");

                        return s.ToString();
                    }
            }
            return v.ToString();
        }

        internal static string JSValueToString(JSValue v)
        {
            if (v == null)
                return "null";

            if (v.ValueType == JSValueType.Object)
            {
                if (v._oValue == null)
                    return "null";

                var o = v as ObjectWrapper;
                if (o == null)
                    o = v._oValue as ObjectWrapper;
                if (o != null)
                    return o.Value.ToString();
            }

            return v.ToString();
        }


        internal static string FormatArgs(IEnumerable args)
        {
            if (args == null)
                return null;

            IEnumerable ie = args;
            if (args is IEnumerable<KeyValuePair<string, JSValue>>)
                ie = (args as IEnumerable<KeyValuePair<string, JSValue>>).Select((x) => x.Value);

            var e = ie.GetEnumerator();
            if (!e.MoveNext())
                return null;

            object o = e.Current;
            JSValue v = o as JSValue;
            string f = null;

            if (f == null && o != null && (o is string))
                f = v.ToString();
            if (f == null && v != null && v.ValueType == JSValueType.String)
                f = v.ToString();

            bool hasNext = e.MoveNext();

            var s = new StringBuilder();

            if (f != null && hasNext)
            {
                int pos = 0;
                while (pos < f.Length && hasNext)
                {
                    v = (o = e.Current) as JSValue;
                    bool usedArg = false;

                    int next = f.IndexOf('%', pos);
                    if (next < 0 || next == f.Length - 1)
                        break;
                    if (next > 0)
                        s.Append(f.Substring(pos, next - pos));
                    pos = next; // now: f[pos] == '%'

                    bool include = false;
                    int len = 2;

                    char c = f[pos + 1];

                    int para = -1; // the int parameter after "%."
                    if (c == '.') // expected format: "%.0000f"
                    {
                        while (pos + len < f.Length)
                        {
                            if (Tools.IsDigit(f[pos + len]))
                                len++;
                            else
                                break;
                        }
                        if (pos + len == f.Length) // invalid: "... %.000"
                            break;

                        if (len > 12) // >10 digits  ->  >2^32
                            para = int.MaxValue;
                        else
                        {
                            long res = -1;
                            if (len > 2 && long.TryParse(f.Substring(pos + 2, len - 2), out res))
                                para = (int)System.Math.Min(res, int.MaxValue);
                        }
                        if (len > 2)
                            c = f[pos + len++];
                    }

                    double d;
                    switch (c)
                    {
                        case 's':
                            if (v != null)
                                s.Append(Tools.JSValueToString(v));
                            else
                                s.Append((o ?? "null").ToString());

                            usedArg = true;
                            break;
                        case 'o':
                        case 'O':
                            int maxRec = (c == 'o') ? 1 : 2;
                            if (v != null)
                                s.Append(Tools.JSValueToObjectString(v, maxRec));
                            else if (o == null)
                                s.Append("null");
                            else if (o is string || o is char || o is StringBuilder)
                                s.Append('"').Append(o.ToString()).Append('"');
                            else
                                s.Append((o ?? "null").ToString());

                            usedArg = true;
                            break;
                        case 'i':
                        case 'd':
                            d = double.NaN;
                            if (v != null)
                                d = (double)Tools.JSObjectToNumber(v);
                            else if (!Tools.ParseNumber((o ?? "null").ToString(), out d, 0))
                                d = double.NaN;

                            if (double.IsNaN(d) || double.IsInfinity(d))
                                d = 0.0;
                            d = System.Math.Truncate(d);

                            string dstr = Tools.DoubleToString(System.Math.Abs(d));
                            if (d < 0)
                                s.Append('-');
                            if (dstr.Length < para)
                                s.Append(new string('0', para - dstr.Length));
                            s.Append(dstr);

                            usedArg = true;
                            break;
                        case 'f':
                            d = double.NaN;
                            if (v != null)
                                d = (double)Tools.JSObjectToNumber(v);
                            else if (!Tools.ParseNumber((o ?? "null").ToString(), out d, 0))
                                d = double.NaN;

                            if (para >= 0)
                                d = System.Math.Round(d, System.Math.Min(15, para));
                            s.Append(Tools.DoubleToString(d));

                            usedArg = true;
                            break;
                        case '%':
                            if (len == 2)
                                s.Append('%');
                            else
                                include = true;
                            break;
                        default:
                            include = true;
                            break;
                    }

                    if (include)
                        s.Append(f.Substring(pos, len));
                    pos += len;

                    if (usedArg)
                        hasNext = e.MoveNext();
                }
                if (pos < f.Length)
                    s.Append(f.Substring(pos).Replace("%%", "%")); // out of arguments? -> still unescape %%

                while (hasNext)
                {
                    v = (o = e.Current) as JSValue;
                    s.Append(' ');
                    if (v != null)
                    {
                        if (v.ValueType == JSValueType.Object)
                            s.Append(Tools.JSValueToObjectString(v));
                        else
                            s.Append(v.ToString());
                    }
                    else
                        s.Append((o ?? "null").ToString());
                    hasNext = e.MoveNext();
                }
            }
            else
            {
                if (v != null)
                {
                    if (v.ValueType == JSValueType.Object)
                        s.Append(Tools.JSValueToObjectString(v));
                    else
                        s.Append(v.ToString());
                }
                else
                    s.Append((o ?? "null").ToString());

                while (hasNext)
                {
                    v = (o = e.Current) as JSValue;
                    s.Append(' ');
                    if (v != null)
                    {
                        if (v.ValueType == JSValueType.Object)
                            s.Append(Tools.JSValueToObjectString(v));
                        else
                            s.Append(v.ToString());
                    }
                    else
                        s.Append((o ?? "null").ToString());
                    hasNext = e.MoveNext();
                }
            }

            return s.ToString();
        }

    }
}
