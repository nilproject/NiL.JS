using NiL.JS.Core;
using System;
using System.Reflection;

namespace NiL.JS.Modules
{
    public sealed class ClassProxy : JSObject
    {
        private readonly Type hostedType;
        private MethodInfo getItem;
        private MethodInfo setItem;

        private ClassProxy(ObjectValueType valueType, Type hostedType)
        {
            ValueType = valueType;
            base.fieldGetter = getField;
            this.hostedType = hostedType;
            getItem = hostedType.GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            setItem = hostedType.GetMethod("set_Item", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public ClassProxy(object obj)
        {
            ValueType = ObjectValueType.Object;
            oValue = obj;
            hostedType = obj.GetType();
            base.fieldGetter = getField;
            getItem = hostedType.GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            setItem = hostedType.GetMethod("set_Item", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public ClassProxy(Type host)
        {
            hostedType = host;
            ValueType = ObjectValueType.Statement;
            JSObject proto = null;
            oValue = new Statements.ExternalFunction((x, y) =>
            {
                var args = convertArgs(y);
                var constructor = hostedType.GetConstructor(Type.GetTypeArray(args));
                if (constructor == null)
                {
                    constructor = hostedType.GetConstructor(new Type[] { typeof(object[]) });
                    args = new object[] { args };
                }
                var res = new ClassProxy(hostedType == typeof(NiL.JS.Core.BaseTypes.Date) ? ObjectValueType.Date : ObjectValueType.Object, hostedType)
                {
                    oValue = constructor.Invoke(args)
                };
                res.prototype = proto;
                if (x.prototype != null && x.prototype.ValueType == ObjectValueType.Object && x.prototype.oValue == hostedType as object)
                    x.Assign(res);
                return res;
            });
            proto = DefaultFieldGetter("prototype", false);
            proto.Assign(new JSObject() { prototype = Core.BaseTypes.BaseObject.Prototype, ValueType = ObjectValueType.Object, oValue = hostedType });
            base.fieldGetter = getField;
            getItem = hostedType.GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            setItem = hostedType.GetMethod("set_Item", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private static object[] convertArgs(IContextStatement[] source)
        {
            if (source == null)
                return null;
            object[] res = new object[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == null)
                    continue;
                var obj = source[i].Invoke();
                var v = obj.Value;
                if (v is Core.BaseTypes.Array)
                {
                    var arr = v as Core.BaseTypes.Array;
                    var arg = new object[arr.length];
                    for (var j = 0; j < arg.Length; j++)
                        arg[j] = arr[j].Value;
                    res[i] = arg;
                }
                else
                    res[i] = v;
            }
            return res;
        }

        private static object[] convertArgs(IContextStatement[] source, ParameterInfo[] targetTypes)
        {
            if (source == null)
                return null;
            int targetCount = targetTypes.Length;
            object[] res = new object[targetCount];
            targetCount = System.Math.Min(targetCount, source.Length);
            for (int i = 0; i < targetCount; i++)
            {
                if (source[i] == null)
                    continue;
                var obj = source[i].Invoke();
                if (targetTypes[i].ParameterType == typeof(JSObject))
                    res[i] = obj;
                else
                {
                    var v = obj.Value;
                    if (v is Core.BaseTypes.Array)
                    {
                        var arr = v as Core.BaseTypes.Array;
                        var arg = new object[arr.length];
                        for (var j = 0; j < arg.Length; j++)
                            arg[j] = arr[j].Value;
                        res[i] = arg;
                    }
                    else
                        res[i] = v;
                }
            }
            return res;
        }

        private JSObject convert(MethodInfo method)
        {
            JSObject result;
            if (method.ReturnType == typeof(JSObject) && (method.GetParameters().Length == 1) && (method.GetParameters()[0].ParameterType == typeof(IContextStatement[])))
            {
                var dinv = (Func<IContextStatement[], JSObject>)Delegate.CreateDelegate(typeof(Func<IContextStatement[], JSObject>), ValueType == ObjectValueType.Statement ? null : oValue, method);
                result = new CallableField((th, args) =>
                {
                    return dinv(args);
                });
            }
            else
            {
                result = new CallableField((th, args) =>
                {
                    var res = method.Invoke(ValueType == ObjectValueType.Statement ? null : oValue, convertArgs(args, method.GetParameters()));
                    if (res == null)
                        return null;
                    else if (res is JSObject)
                        return res as JSObject;
                    else if (res is int)
                        return (int)res;
                    else if (res is long)
                        return (long)res;
                    else if (res is double)
                        return (double)res;
                    else if (res is string)
                        return (string)res;
                    else if (res is bool)
                        return (bool)res;
                    else if (res is ContextStatement)
                        return (JSObject)(ContextStatement)res;
                    else return new ClassProxy(res);
                });
            }
            return result;
        }

        private JSObject getField(string name, bool fast)
        {
            JSObject r = DefaultFieldGetter(name, fast);
            if (r.ValueType == ObjectValueType.NoExistInObject || (fast && r == undefined))
            {
                JSObject result = null;
#if DEBUG
                var members = hostedType.GetMembers(BindingFlags.Public | (ValueType == ObjectValueType.Statement ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic);
#endif
                var m = hostedType.GetMember(name, BindingFlags.Public | (ValueType == ObjectValueType.Statement ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic);
                if (m.Length > 1)
                    throw new InvalidOperationException("Too many fields with name " + name);
                if (m.Length == 0 || m[0].GetCustomAttribute(typeof(InvisibleAttribute)) != null)
                {
                    var i = 0;
                    var d = 0;
                    var gprms = getItem != null ? getItem.GetParameters() : null;
                    var sprms = getItem != null ? setItem.GetParameters() : null;
                    if (((sprms ?? gprms) != null) && ((gprms ?? sprms)[0].ParameterType == typeof(string)
                        || (Parser.ParseNumber(name, ref i, false, out i) && (gprms ?? sprms)[0].ParameterType == typeof(int))
                        || (Parser.ParseNumber(name, ref i, false, out d) && (gprms ?? sprms)[0].ParameterType == typeof(double))))
                        return new JSObject()
                        {
                            ValueType = ObjectValueType.Property,
                            oValue = new IContextStatement[] 
                            {
                                new NiL.JS.Statements.ExternalFunction(new CallableField((_th, args) =>
                                {
                                    object[] a = null;
                                    if (sprms[1].ParameterType != typeof(JSObject))
                                    {
                                        args = new IContextStatement[] { null, args[0] };
                                        a = convertArgs(args, sprms);
                                        a[0] = sprms[0].ParameterType == typeof(string) ? name : (object)(sprms[0].ParameterType == typeof(int) ? i : (object)d);
                                    }
                                    else
                                        a = new object[] { sprms[0].ParameterType == typeof(string) ? name : (object)(sprms[0].ParameterType == typeof(int) ? i : (object)d), args[0].Invoke() };
                                    setItem.Invoke(oValue, a);
                                    return undefined;
                                })),
                                new NiL.JS.Statements.ExternalFunction(new CallableField((_th, args) =>
                                {
                                    var res = getItem.Invoke(oValue, new object[] { sprms[0].ParameterType == typeof(string) ? name : (object)(sprms[0].ParameterType == typeof(int) ? i : (object)d) });
                                    if (res is JSObject)
                                        return res as JSObject;
                                    else if (res is int)
                                        return (int)res;
                                    else if (res is double || res is long)
                                        return (double)res;
                                    else if (res is string)
                                        return (string)res;
                                    else if (res is bool)
                                        return (bool)res;
                                    else if (res is ContextStatement)
                                        return (JSObject)(ContextStatement)res;
                                    else return new ClassProxy(res.GetType() == typeof(NiL.JS.Core.BaseTypes.Date) ? ObjectValueType.Date : ObjectValueType.Object, res.GetType())
                                    {
                                        oValue = res
                                    };
                                })) 
                            }
                        };
                    return r;
                }
                switch (m[0].MemberType)
                {
                    case MemberTypes.Constructor:
                        {
                            var method = (ConstructorInfo)m[0];
                            result = new CallableField((th, args) =>
                            {
                                var res = method.Invoke(args);
                                if (res is JSObject)
                                    return res as JSObject;
                                else if (res is int)
                                    return (int)res;
                                else if (res is double || res is long)
                                    return (double)res;
                                else if (res is string)
                                    return (string)res;
                                else if (res is bool)
                                    return (bool)res;
                                else if (res is ContextStatement)
                                    return (JSObject)(ContextStatement)res;
                                else return new ClassProxy(res.GetType() == typeof(NiL.JS.Core.BaseTypes.Date) ? ObjectValueType.Date : ObjectValueType.Object, res.GetType())
                                {
                                    oValue = res
                                };
                            });
                            break;
                        }
                    case MemberTypes.Method:
                        {
                            var method = (MethodInfo)m[0];
                            result = convert(method);
                            break;
                        }
                    case MemberTypes.Field:
                        {
                            var field = (m[0] as FieldInfo);
                            object res = field.GetValue(this);
                            if (res is JSObject)
                                return res as JSObject;
                            else if (res is int)
                                result = (int)res;
                            else if (res is double || res is long)
                                result = (double)res;
                            else if (res is string)
                                result = (string)res;
                            else if (res is bool)
                                result = (bool)res;
                            else if (res is ContextStatement)
                                result = (JSObject)(ContextStatement)res;
                            else return new ClassProxy(res);
                            break;
                        }
                    case MemberTypes.Property:
                        {
                            var pinfo = (PropertyInfo)m[0];
                            result = new JSObject()
                            {
                                ValueType = ObjectValueType.Property,
                                oValue = new IContextStatement[] { 
                                    pinfo.CanWrite ? convert(pinfo.SetMethod).oValue as IContextStatement : null,
                                    pinfo.CanRead ? convert(pinfo.GetMethod).oValue as IContextStatement : null 
                                }
                            };
                            break;
                        }
                    default: throw new NotImplementedException("Convertion from " + m[0].MemberType + " not implemented");
                }
                if (fast)
                    return result;
                r.Assign(result);
                if (m[0].GetCustomAttributes(typeof(ProtectedAttribute), false).Length != 0)
                    r.Protect();
            }
            return r;
        }
    }
}
