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
        public static readonly Expression UndefinedConstant = Expression.Field(null, typeof(JSObject).GetField("undefined", BindingFlags.Static | BindingFlags.NonPublic));
        public static readonly Expression NotExistsConstant = Expression.Field(null, typeof(JSObject).GetField("notExists", BindingFlags.Static | BindingFlags.NonPublic));

        public static readonly MethodInfo JSObjectToBooleanMethod = null;
        public static readonly MethodInfo JSObjectToInt32Method = typeof(Tools).GetMethod("JSObjectToInt32", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(JSObject) }, null);

        static JITHelpers()
        {
            var methods = typeof(JSObject).GetMethods(BindingFlags.Static | BindingFlags.Public);
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

        internal static JSObject wrap<T>(T source, JSObject dest)
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    {
                        dest.iValue = (bool)(object)source ? 1 : 0;
                        dest.valueType = JSObjectType.Bool;
                        break;
                    }
                case TypeCode.Byte:
                    {
                        dest.iValue = (byte)(object)source;
                        dest.valueType = JSObjectType.Int;
                        break;
                    }
                case TypeCode.Char:
                    {
                        dest.oValue = source.ToString();
                        dest.valueType = JSObjectType.String;
                        break;
                    }
                case TypeCode.Decimal:
                    {
                        dest.dValue = (double)(decimal)(object)source;
                        dest.valueType = JSObjectType.Double;
                        break;
                    }
                case TypeCode.Double:
                    {
                        dest.dValue = (double)(object)source;
                        dest.valueType = JSObjectType.Double;
                        break;
                    }
                case TypeCode.Int16:
                    {
                        dest.iValue = (short)(object)source;
                        dest.valueType = JSObjectType.Int;
                        break;
                    }
                case TypeCode.Int32:
                    {
                        dest.iValue = (int)(object)source;
                        dest.valueType = JSObjectType.Int;
                        break;
                    }
                case TypeCode.Int64:
                    {
                        var t = (long)(object)source;
                        if (t > int.MaxValue || t < int.MinValue)
                        {
                            dest.dValue = t;
                            dest.valueType = JSObjectType.Double;
                        }
                        else
                        {
                            dest.iValue = (int)t;
                            dest.valueType = JSObjectType.Int;
                        }
                        break;
                    }
                case TypeCode.SByte:
                    {
                        dest.iValue = (sbyte)(object)source;
                        dest.valueType = JSObjectType.Int;
                        break;
                    }
                case TypeCode.Single:
                    {
                        dest.dValue = (float)(object)source;
                        dest.valueType = JSObjectType.Double;
                        break;
                    }
                case TypeCode.String:
                    {
                        dest.oValue = source.ToString();
                        dest.valueType = JSObjectType.String;
                        break;
                    }
                case TypeCode.UInt16:
                    {
                        dest.iValue = (ushort)(object)source;
                        dest.valueType = JSObjectType.Int;
                        break;
                    }
                case TypeCode.UInt32:
                    {
                        var t = (uint)(object)source;
                        if (t > int.MaxValue)
                        {
                            dest.dValue = t;
                            dest.valueType = JSObjectType.Double;
                        }
                        else
                        {
                            dest.iValue = (int)t;
                            dest.valueType = JSObjectType.Int;
                        }
                        break;
                    }
                case TypeCode.UInt64:
                    {
                        var t = (ulong)(object)source;
                        if (t > int.MaxValue)
                        {
                            dest.dValue = t;
                            dest.valueType = JSObjectType.Double;
                        }
                        else
                        {
                            dest.iValue = (int)t;
                            dest.valueType = JSObjectType.Int;
                        }
                        break;
                    }
                default:
                    {
                        dest.oValue = new ObjectContainer(source);
                        dest.valueType = JSObjectType.Object;
                        break;
                    }
            }
            return dest;
        }

        internal static MethodInfo methodof(Func<JSObject, JSObject, bool, JSObject> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Action<JSObject> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Func<Context, JSObject> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Func<object, bool> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Func<Context, CodeNode[], JSObject> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Func<Context, Exception, Exception> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Action<Context, Exception> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Func<JSObject, object> func)
        {
            return func.Method;
        }

        internal static MethodInfo methodof(Func<Context, JSObject, Context> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Action<string, Exception, Context> method)
        {
            return method.Method;
        }

        internal static MethodInfo methodof(Action<Exception, Context> method)
        {
            return method.Method;
        }
    }
#endif
}