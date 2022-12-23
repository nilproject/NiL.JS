using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;

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
            return $"({Line}:{Column}{(Length != 0 ? "*" + Length : "")})";
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
                if (i >= text.Length)
                {
                    return null;
                }

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

        internal static readonly char[] TrimChars = new[]
        {
            '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u0020', '\u00A0', '\u1680',
            '\u180E', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006',
            '\u2007', '\u2008', '\u2009', '\u200A', '\u2028', '\u2029', '\u202F', '\u205F',
            '\u3000', '\uFEFF'
        };

        internal static readonly char[] NumChars = new[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
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

#if !NET40
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

                        if (Tools.ParseJsNumber(s, ref ix, out x, 0, ParseNumberOptions.AllowFloat | ParseNumberOptions.AllowAutoRadix) && ix < s.Length)
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
                    case JSValueType.Symbol:
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
#if !NET40
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
#if !NET40
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
#if !NET40
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

                    if (!Tools.ParseJsNumber(s, ref ix, out x, 0, ParseNumberOptions.AllowAutoRadix | ParseNumberOptions.AllowFloat) || ix < s.Length)
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
#if !NET40
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
#if !NET40
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
                    if (!ParseJsNumber(s, ref ix, out x, 0, ParseNumberOptions.AllowAutoRadix | ParseNumberOptions.AllowFloat) || ix < s.Length)
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

#if !NET40
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
                    if (!Tools.ParseJsNumber(s, ref ix, out x, ParseNumberOptions.Default & ~ParseNumberOptions.ProcessOctalLiteralsOldSyntax) || ix < s.Length)
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

        public static object ConvertJSValueToType(JSValue jsobj, Type targetType)
            => ConvertJStoObj(jsobj, targetType, true);

        internal static object ConvertJStoObj(JSValue jsobj, Type targetType, bool hightLoyalty)
        {
            if (jsobj == null)
                return null;

            var typeInfo = targetType.GetTypeInfo();
            if (typeInfo.IsInterface)
            {
                if (targetType == typeof(IDictionary))
                    targetType = typeof(Dictionary<string, object>);

                if (targetType.GetTypeInfo().IsGenericType && targetType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    var genericPrms = targetType.GetGenericArguments();
                    targetType = typeof(Dictionary<,>).MakeGenericType(genericPrms);
                }

                if (targetType == typeof(IEnumerable))
                    targetType = typeof(object[]);
            }

            if (typeInfo.IsGenericType
                && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                targetType = targetType.GetGenericArguments()[0];

            if (targetType.IsAssignableFrom(jsobj.GetType()))
                return jsobj;

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

                    if (hightLoyalty)
                        goto case JSValueType.Integer;

                    return null;
                }
                case JSValueType.Double:
                {
                    if (hightLoyalty)
                    {
                        if (targetType == typeof(byte))
                            return (byte)jsobj._dValue;
                        if (targetType == typeof(sbyte))
                            return (sbyte)jsobj._dValue;
                        if (targetType == typeof(ushort))
                            return (ushort)jsobj._dValue;
                        if (targetType == typeof(short))
                            return (short)jsobj._dValue;
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
                            return NumberUtils.DoubleToString(jsobj._dValue);

                        if (targetType.GetTypeInfo().IsEnum)
                            return Enum.ToObject(targetType, (long)jsobj._dValue);
                    }

                    if (targetType == typeof(double))
                        return jsobj._dValue;
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

                        if (targetType == typeof(byte))
                            return (byte)jsobj._iValue;
                        if (targetType == typeof(sbyte))
                            return (sbyte)jsobj._iValue;
                        if (targetType == typeof(ushort))
                            return (ushort)jsobj._iValue;
                        if (targetType == typeof(short))
                            return (short)jsobj._iValue;
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
                            return (byte)JSObjectToInt32(jsobj);
                        if (targetType == typeof(sbyte))
                            return (sbyte)JSObjectToInt32(jsobj);
                        if (targetType == typeof(short))
                            return (short)JSObjectToInt32(jsobj);
                        if (targetType == typeof(ushort))
                            return (ushort)JSObjectToInt32(jsobj);
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

                        if (targetType == typeof(Guid))
                        {
                            return Guid.Parse(jsobj.Value.ToString());
                        }
                    }

                    if (targetType == typeof(string))
                        return jsobj.Value.ToString();

                    if (targetType == typeof(BaseLibrary.String))
                        return new BaseLibrary.String(jsobj.Value.ToString());

                    if (targetType == typeof(Date))
                        return new Date(new Arguments { jsobj.Value.ToString() });

                    if (targetType == typeof(DateTime))
                        return DateTime.TryParse(jsobj.Value.ToString(), out var dateTime) ? dateTime : new Date(new Arguments { jsobj.Value.ToString() }).ToDateTime();

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
                return (value as ConstructorProxy)._staticProxy._hostedType;

            if ((value is BaseLibrary.Array
                 || value is TypedArray
                 || value is ArrayBuffer)
               && typeof(IEnumerable).IsAssignableFrom(targetType))
            {
                Type elementType = typeof(object);

                if (targetType.IsArray && targetType.HasElementType)
                {
                    elementType = targetType.GetElementType();
                }
                else
                {
                    Type iEnumerableInterface = null;
#if PORTABLE || NETCORE
                    iEnumerableInterface = targetType.GetInterface(typeof(IEnumerable<>).Name);
#else
                    iEnumerableInterface = targetType.GetTypeInfo().GetInterface(typeof(IEnumerable<>).Name);
#endif
                    if (iEnumerableInterface != null)
                    {
                        elementType = iEnumerableInterface.GetGenericArguments()[0];
                    }
                }

                if (value is TypedArray typedArray && targetType.IsAssignableFrom(typedArray.ElementType.MakeArrayType()))
                    return typedArray.ToNativeArray();

                if (value is ArrayBuffer arrayBuffer && targetType.IsAssignableFrom(typeof(byte[])))
                    return (value as ArrayBuffer).GetData();

                if (value is BaseLibrary.Array array)
                {
                    if (hightLoyalty || elementType == typeof(JSValue))
                    {
                        if (targetType.IsAssignableFrom(elementType.MakeArrayType()))
                        {
                            return convertCollection(array, elementType, elementType.MakeArrayType(), hightLoyalty);
                        }

                        if (targetType.IsAssignableFrom(typeof(List<>).MakeGenericType(elementType)))
                        {
                            return convertCollection(value as BaseLibrary.Array, elementType, typeof(List<>).MakeGenericType(elementType), hightLoyalty);
                        }
                    }
                }
            }

            if (hightLoyalty)
            {
                if (jsobj._valueType >= JSValueType.Object
                    && !targetType.GetTypeInfo().IsPrimitive
                    && targetType != typeof(string)
                    && !typeof(JSValue).IsAssignableFrom(targetType))
                {
                    var args = new Arguments();
                    var targetInstance = (JSValue.GetConstructor(targetType) as Function)?.Construct(args).Value;

                    if (targetInstance is IDictionary dictionary)
                    {
                        var addMethod = dictionary.GetType().GetMethod("Add");

                        if (addMethod != null)
                        {
                            var parameters = addMethod.GetParameters();
                            var keyType = parameters[0].ParameterType;
                            var valueType = parameters[1].ParameterType;
                            foreach (var kvp in jsobj)
                            {
                                try
                                {
                                    dictionary.Add(
                                        keyType == typeof(string) ? kvp.Key : ConvertJStoObj(kvp.Key, keyType, true),
                                        ConvertJStoObj(kvp.Value, valueType, true));
                                }
                                catch { }
                            }
                        }
                        else
                        {
                            foreach (var kvp in jsobj)
                            {
                                try
                                {
                                    dictionary.Add(kvp.Key, kvp.Value);
                                }
                                catch { }
                            }
                        }
                    }
                    else if (targetInstance is not null)
                    {
                        foreach (var field in targetType.GetFields(BindingFlags.Instance | BindingFlags.Public))
                        {
                            var name = field.GetCustomAttribute<JavaScriptNameAttribute>()?.Name ?? field.Name;
                            var propValue = jsobj[name];
                            if (propValue.Defined)
                                field.SetValue(targetInstance, ConvertJStoObj(propValue, field.FieldType, true));
                        }

                        foreach (var property in targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                        {
                            var name = property.GetCustomAttribute<JavaScriptNameAttribute>()?.Name ?? property.Name;
                            var propValue = jsobj[name];
                            if (propValue.Defined)
                                property.SetValue(targetInstance, ConvertJStoObj(propValue, property.PropertyType, true));
                        }
                    }

                    return targetInstance;
                }

                if (targetType.GetTypeInfo().IsValueType)
                {
                    return Activator.CreateInstance(targetType);
                }
            }

            return null;
        }

        private static IList convertCollection(BaseLibrary.Array array, Type elementType, Type collectionType, bool hightLoyalty)
        {
            if (array == null)
                return null;

            var len = (int)array._data.Length;
            var result = (IList)Activator.CreateInstance(collectionType, new object[] { len });

            for (var j = 0; j < len; j++)
            {
                var temp = (array._data[j] ?? JSValue.undefined);
                var value = ConvertJStoObj(temp, elementType, hightLoyalty);

                if (!hightLoyalty && value == null && (elementType.GetTypeInfo().IsValueType || (!temp.IsNull && !temp.IsUndefined())))
                    return null;

                if (result.Count <= j)
                    result.Add(value);
                else
                    result[j] = value;
            }

            return result;
        }

        internal static void CheckEndOfInput(string code, ref int i)
        {
            if (i >= code.Length)
                ExceptionHelper.ThrowSyntaxError(Strings.UnexpectedEndOfSource, code, i);
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

        public static bool ParseJsNumber(string code, out double value, int radix)
        {
            int index = 0;
            return ParseJsNumber(code, ref index, out value, radix, ParseNumberOptions.Default);
        }

        public static bool ParseJsNumber(string code, out double value, ParseNumberOptions options)
        {
            int index = 0;
            return ParseJsNumber(code, ref index, out value, 0, options);
        }

        public static bool ParseJsNumber(string code, out double value, int radix, ParseNumberOptions options)
        {
            int index = 0;
            return ParseJsNumber(code, ref index, out value, radix, options);
        }

        public static bool ParseJsNumber(string code, ref int index, out double value)
        {
            return ParseJsNumber(code, ref index, out value, 0, ParseNumberOptions.Default);
        }

        public static bool ParseJsNumber(string code, ref int index, out double value, ParseNumberOptions options)
        {
            return ParseJsNumber(code, ref index, out value, 0, options);
        }

        public static bool ParseJsNumber(string code, ref int index, out double value, int radix, ParseNumberOptions options)
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
                if (NumberUtils.IsDigit(code[i + 1]))
                {
                    if (raiseOldOctalLiterals)
                        ExceptionHelper.ThrowSyntaxError("Octal literals not allowed in strict mode", code, i);

                    while ((i + 1 < code.Length) && (code[i + 1] == '0'))
                    {
                        i++;
                    }

                    if (processOldOctals && (i + 1 < code.Length) && NumberUtils.IsDigit(code[i + 1]))
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
                var len = NumberUtils.TryParse(code, i, out value);
                if (len <= 0)
                    return false;

                value *= sign;
                index = i + len;
                return true;
            }
            else
            {
                if (radix == 0)
                    radix = 10;

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

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool IsNegativeZero(double d)
        {
            return (((ulong)BitConverter.DoubleToInt64Bits(d)) & 0x800F_FFFF_FFFF_FFFF) == 0x8000_0000_0000_0000;
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static string Unescape(string code, bool strict)
        {
            return Unescape(code, strict, true, false, true);
        }

#if !NET40
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

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static string UnescapeNextChar(string code, int index, out int processedChars, bool strict)
        {
            return UnescapeNextChar(code, index, out processedChars, strict, true, false, true);
        }

#if !NET40
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
                            if (ushort.TryParse(c, NumberStyles.HexNumber, null, out chc))
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
                    if (i + 1 < str.Length && NumberUtils.IsDigit(str[i + 1]))
                        ccode = ccode * 8 + (str[++i] - '0');
                    if (i + 1 < str.Length && NumberUtils.IsDigit(str[i + 1]))
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

#if !NET40
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

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool IsLineTerminator(char c)
        {
            return (c == '\u000A') || (c == '\u000D') || (c == '\u2028') || (c == '\u2029');
        }

#if !NET40
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
#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal static int hexCharToInt(char p)
        {
            return ((p % 'a' % 'A' + 10) % ('0' + 10));
        }

        internal static long getLengthOfArraylike(JSValue src, bool reassignLen)
        {
            var length = src.GetProperty("length", true, PropertyScope.Common); // тут же проверка на null/undefined с падением если надо

            var result = (uint)JSObjectToInt64(GetPropertyOrValue(length, src).ToPrimitiveValue_Value_String(), 0, false);
            if (reassignLen)
            {
                if (length._valueType == JSValueType.Property)
                    ((length._oValue as PropertyPair).setter ?? Function.Empty).Call(src, new Arguments { result });
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
                if (Tools.ParseJsNumber(i, ref pindex, out dindex)
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static JSValue GetPropertyOrValue(JSValue propertyObject, JSValue target)
        {
            if (propertyObject._valueType != JSValueType.Property)
                return propertyObject;

            var propPair = propertyObject._oValue as PropertyPair;
            if (propPair == null || propPair.getter == null)
                return JSValue.undefined;

            propertyObject = propPair.getter.Call(target, null);
            if (propertyObject._valueType < JSValueType.Undefined)
                propertyObject = JSValue.undefined;

            return propertyObject;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void SetPropertyOrValue(JSValue propertyObject, JSValue target, JSValue newValue)
        {
            if (propertyObject._valueType != JSValueType.Property)
            {
                propertyObject.Assign(newValue);
                return;
            }

            var propPair = propertyObject._oValue as PropertyPair;
            if (propPair == null || propPair.setter == null)
                return;

            propertyObject = propPair.setter.Call(target, new Arguments { newValue });
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

            var step = 13;
            var pos = step;
            var lastSteps = 2;
            while (step > 0)
            {
                if (TrimChars[pos] == p)
                    return true;

                if (TrimChars[pos] > p)
                {
                    pos -= step;
                    if (pos < 0)
                        pos = 0;
                }
                else
                {
                    pos += step;
                    if (pos >= 26)
                        pos = 25;
                }

                step >>= 1;
                if (step == 0 && lastSteps-- > 0)
                    step = 1;
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

            argumentsObject._iValue = targetIndex;
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
                {
                    return "\"" + v.ToString() + "\"";
                }
                case JSValueType.Date:
                {
                    string dstr = v.ToString();
                    if (dstr == "Invalid date")
                        return dstr;
                    return "Date " + dstr;
                }
                case JSValueType.Function:
                {
                    BaseLibrary.Function f = v.Value as BaseLibrary.Function;
                    if (f == null)
                        return v.ToString();
                    if (recursionDepth >= maxRecursionDepth)
                        return f.name + "()";
                    if (recursionDepth == maxRecursionDepth - 1)
                        return f.ToString(true);
                    return f.ToString();
                }
                case JSValueType.Object:
                {
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
                            if (NumberUtils.IsDigit(f[pos + len]))
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
                            else if (!Tools.ParseJsNumber((o ?? "null").ToString(), out d, 0))
                                d = double.NaN;

                            if (double.IsNaN(d) || double.IsInfinity(d))
                                d = 0.0;
                            d = System.Math.Truncate(d);

                            string dstr = NumberUtils.DoubleToString(System.Math.Abs(d));
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
                            else if (!Tools.ParseJsNumber((o ?? "null").ToString(), out d, 0))
                                d = double.NaN;

                            if (para >= 0)
                                d = System.Math.Round(d, System.Math.Min(15, para));
                            s.Append(NumberUtils.DoubleToString(d));

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

        /// <summary>
        /// Determines if the given type matches Task<>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsTaskOfT(Type type)
        {
            var typeInfo = type?.GetTypeInfo();
            if (typeInfo == null)
            {
                return false;
            }

            if (typeInfo.IsGenericType
                && typeInfo.GetGenericTypeDefinition() == typeof(Task<>))
            {
                return true;
            }

            return IsTaskOfT(typeInfo.BaseType);
        }
    }
}
