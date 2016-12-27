using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NiL.JS.Statements;
using NiL.JS.Backward;

namespace NiL.JS.Core.JIT
{
#if !NET35
    internal static class JITHelpers
    {
        public static readonly FieldInfo _items = typeof(List<CodeNode>).GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly ParameterExpression DynamicValuesParameter = Expression.Parameter(typeof(CodeNode[]), "dv");
        public static readonly ParameterExpression ContextParameter = Expression.Parameter(typeof(Context), "context");
        public static readonly Expression UndefinedConstant = Expression.Field(null, typeof(JSValue).GetField("undefined", BindingFlags.Static | BindingFlags.NonPublic));
        public static readonly Expression NotExistsConstant = Expression.Field(null, typeof(JSValue).GetField("notExists", BindingFlags.Static | BindingFlags.NonPublic));

        public static readonly MethodInfo JSObjectToBooleanMethod = null;
        public static readonly MethodInfo JSObjectToInt32Method = typeof(Tools).GetMethod("JSObjectToInt32", new[] { typeof(JSValue) });

        internal static readonly MethodInfo EvaluateForWriteMethod = typeof(CodeNode).GetMethod("EvaluateForWrite", new[] { typeof(Context) });
        internal static readonly MethodInfo EvaluateMethod = typeof(CodeNode).GetMethod("Evaluate", new[] { typeof(Context) });

        static JITHelpers()
        {
            var methods = typeof(JSValue).GetMethods(BindingFlags.Static | BindingFlags.Public);
            for (var i = 0; i < methods.Length; i++)
            {
                if (methods[i].Name == "op_Explicite" && methods[i].ReturnType == typeof(bool))
                {
                    JSObjectToBooleanMethod = methods[i];
                    break;
                }
            }
        }

        internal static Expression cnst(object obj)
        {
            return Expression.Constant(obj);
        }

        internal static JSValue wrap<T>(T source, JSValue dest)
        {
#if NETCORE || PORTABLE
            switch (typeof(T).GetTypeCode())
#else
            switch (Type.GetTypeCode(typeof(T)))
#endif
            {
                case TypeCode.Boolean:
                    {
                        dest._iValue = (bool)(object)source ? 1 : 0;
                        dest._valueType = JSValueType.Boolean;
                        break;
                    }
                case TypeCode.Byte:
                    {
                        dest._iValue = (byte)(object)source;
                        dest._valueType = JSValueType.Integer;
                        break;
                    }
                case TypeCode.Char:
                    {
                        dest._oValue = source.ToString();
                        dest._valueType = JSValueType.String;
                        break;
                    }
                case TypeCode.Decimal:
                    {
                        dest._dValue = (double)(decimal)(object)source;
                        dest._valueType = JSValueType.Double;
                        break;
                    }
                case TypeCode.Double:
                    {
                        dest._dValue = (double)(object)source;
                        dest._valueType = JSValueType.Double;
                        break;
                    }
                case TypeCode.Int16:
                    {
                        dest._iValue = (short)(object)source;
                        dest._valueType = JSValueType.Integer;
                        break;
                    }
                case TypeCode.Int32:
                    {
                        dest._iValue = (int)(object)source;
                        dest._valueType = JSValueType.Integer;
                        break;
                    }
                case TypeCode.Int64:
                    {
                        var t = (long)(object)source;
                        if (t > int.MaxValue || t < int.MinValue)
                        {
                            dest._dValue = t;
                            dest._valueType = JSValueType.Double;
                        }
                        else
                        {
                            dest._iValue = (int)t;
                            dest._valueType = JSValueType.Integer;
                        }
                        break;
                    }
                case TypeCode.SByte:
                    {
                        dest._iValue = (sbyte)(object)source;
                        dest._valueType = JSValueType.Integer;
                        break;
                    }
                case TypeCode.Single:
                    {
                        dest._dValue = (float)(object)source;
                        dest._valueType = JSValueType.Double;
                        break;
                    }
                case TypeCode.String:
                    {
                        dest._oValue = source.ToString();
                        dest._valueType = JSValueType.String;
                        break;
                    }
                case TypeCode.UInt16:
                    {
                        dest._iValue = (ushort)(object)source;
                        dest._valueType = JSValueType.Integer;
                        break;
                    }
                case TypeCode.UInt32:
                    {
                        var t = (uint)(object)source;
                        if (t > int.MaxValue)
                        {
                            dest._dValue = t;
                            dest._valueType = JSValueType.Double;
                        }
                        else
                        {
                            dest._iValue = (int)t;
                            dest._valueType = JSValueType.Integer;
                        }
                        break;
                    }
                case TypeCode.UInt64:
                    {
                        var t = (ulong)(object)source;
                        if (t > int.MaxValue)
                        {
                            dest._dValue = t;
                            dest._valueType = JSValueType.Double;
                        }
                        else
                        {
                            dest._iValue = (int)t;
                            dest._valueType = JSValueType.Integer;
                        }
                        break;
                    }
                default:
                    {
                        dest._oValue = new ObjectWrapper(source);
                        dest._valueType = JSValueType.Object;
                        break;
                    }
            }
            return dest;
        }
    }
#endif
}