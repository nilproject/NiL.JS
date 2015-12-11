using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NiL.JS.Statements;

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
        public static readonly MethodInfo JSObjectToInt32Method = typeof(Tools).GetMethod("JSObjectToInt32", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(JSValue) }, null);

        internal static readonly MethodInfo EvaluateForWriteMethod = typeof(CodeNode).GetMethod("EvaluateForWrite", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(Context) }, null);
        internal static readonly MethodInfo EvaluateMethod = typeof(CodeNode).GetMethod("Evaluate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(Context) }, null);

        static JITHelpers()
        {
            var methods = typeof(JSValue).GetMethods(BindingFlags.Static | BindingFlags.Public);
            for (var i = 0; i < methods.Length; i++)
                if (methods[i].Name == "op_Explicite" && methods[i].ReturnType == typeof(bool))
                {
                    JSObjectToBooleanMethod = methods[i];
                    break;
                }
        }

        internal static Expression cnst(object obj)
        {
            return Expression.Constant(obj);
        }

        internal static JSValue wrap<T>(T source, JSValue dest)
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    {
                        dest.iValue = (bool)(object)source ? 1 : 0;
                        dest.valueType = JSValueType.Boolean;
                        break;
                    }
                case TypeCode.Byte:
                    {
                        dest.iValue = (byte)(object)source;
                        dest.valueType = JSValueType.Integer;
                        break;
                    }
                case TypeCode.Char:
                    {
                        dest.oValue = source.ToString();
                        dest.valueType = JSValueType.String;
                        break;
                    }
                case TypeCode.Decimal:
                    {
                        dest.dValue = (double)(decimal)(object)source;
                        dest.valueType = JSValueType.Double;
                        break;
                    }
                case TypeCode.Double:
                    {
                        dest.dValue = (double)(object)source;
                        dest.valueType = JSValueType.Double;
                        break;
                    }
                case TypeCode.Int16:
                    {
                        dest.iValue = (short)(object)source;
                        dest.valueType = JSValueType.Integer;
                        break;
                    }
                case TypeCode.Int32:
                    {
                        dest.iValue = (int)(object)source;
                        dest.valueType = JSValueType.Integer;
                        break;
                    }
                case TypeCode.Int64:
                    {
                        var t = (long)(object)source;
                        if (t > int.MaxValue || t < int.MinValue)
                        {
                            dest.dValue = t;
                            dest.valueType = JSValueType.Double;
                        }
                        else
                        {
                            dest.iValue = (int)t;
                            dest.valueType = JSValueType.Integer;
                        }
                        break;
                    }
                case TypeCode.SByte:
                    {
                        dest.iValue = (sbyte)(object)source;
                        dest.valueType = JSValueType.Integer;
                        break;
                    }
                case TypeCode.Single:
                    {
                        dest.dValue = (float)(object)source;
                        dest.valueType = JSValueType.Double;
                        break;
                    }
                case TypeCode.String:
                    {
                        dest.oValue = source.ToString();
                        dest.valueType = JSValueType.String;
                        break;
                    }
                case TypeCode.UInt16:
                    {
                        dest.iValue = (ushort)(object)source;
                        dest.valueType = JSValueType.Integer;
                        break;
                    }
                case TypeCode.UInt32:
                    {
                        var t = (uint)(object)source;
                        if (t > int.MaxValue)
                        {
                            dest.dValue = t;
                            dest.valueType = JSValueType.Double;
                        }
                        else
                        {
                            dest.iValue = (int)t;
                            dest.valueType = JSValueType.Integer;
                        }
                        break;
                    }
                case TypeCode.UInt64:
                    {
                        var t = (ulong)(object)source;
                        if (t > int.MaxValue)
                        {
                            dest.dValue = t;
                            dest.valueType = JSValueType.Double;
                        }
                        else
                        {
                            dest.iValue = (int)t;
                            dest.valueType = JSValueType.Integer;
                        }
                        break;
                    }
                default:
                    {
                        dest.oValue = new ObjectWrapper(source);
                        dest.valueType = JSValueType.Object;
                        break;
                    }
            }
            return dest;
        }
    }
#endif
}