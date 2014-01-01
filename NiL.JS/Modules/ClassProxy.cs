using NiL.JS.Core;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace NiL.JS.Modules
{
    public sealed class ClassProxy : JSObject
    {
        private static readonly Dictionary<Type, ClassProxy> constructors = new Dictionary<Type, ClassProxy>();

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
            ClassProxy exconst = null;
            if (constructors.TryGetValue(hostedType, out exconst))
            {
                oValue = exconst.oValue;
                proto = DefaultFieldGetter("prototype", false);
                proto.Assign(exconst.GetField("prototype", true));
                base.fieldGetter = getField;
                getItem = exconst.getItem;
                setItem = exconst.setItem;
            }
            else
            {
                oValue = new Statements.ExternalFunction((x, y) =>
                {
                    object[] args = null;
                    var constructor = hostedType.GetConstructor(y != null ? new Type[] { y.GetType() } : Type.EmptyTypes);
                    if (constructor == null)
                    {
                        args = convertArgs(y);
                        constructor = hostedType.GetConstructor(Type.GetTypeArray(y));
                        if (constructor == null)
                        {
                            constructor = hostedType.GetConstructor(new Type[] { typeof(object[]) });
                            if (constructor == null)
                            {
                                constructor = hostedType.GetConstructor(Type.EmptyTypes);
                                args = null;
                            }
                            else
                                args = new object[] { args };
                        }
                    }
                    else
                        args = y != null ?new object[] { y } : null;
                    var res = new ClassProxy(hostedType == typeof(NiL.JS.Core.BaseTypes.Date) ? ObjectValueType.Date : ObjectValueType.Object, hostedType)
                    {
                        oValue = constructor.Invoke(args)
                    };
                    res.prototype = proto;
                    var _this = x.thisBind;
                    if (_this != null && _this.prototype != null && _this.prototype.ValueType == ObjectValueType.Object && _this.prototype.oValue == hostedType as object)
                        _this.Assign(res);
                    var c = res.DefaultFieldGetter("constructor", false);
                    c.Assign(this);
                    c.attributes |= ObjectAttributes.DontEnum;
                    return res;
                });
                constructors[hostedType] = this;
                proto = DefaultFieldGetter("prototype", false);
                proto.Assign(new JSObject() { prototype = Core.BaseTypes.BaseObject.Prototype, ValueType = ObjectValueType.Object, oValue = hostedType });
                base.fieldGetter = getField;
                getItem = hostedType.GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                setItem = hostedType.GetMethod("set_Item", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }
        }

        private static object[] convertArgs(JSObject[] source)
        {
            if (source == null)
                return null;
            object[] res = new object[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == null)
                    continue;
                var obj = source[i];
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

        private static object[] convertArgs(JSObject[] source, ParameterInfo[] targetTypes)
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
                var obj = source[i];
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
            JSObject result = null;
            if (method.ReturnType == typeof(JSObject))
            {
                if ((method.GetParameters().Length == 1) && (method.GetParameters()[0].ParameterType == typeof(JSObject[])))
                {
                    var dinv = (Func<JSObject[], JSObject>)Delegate.CreateDelegate(typeof(Func<JSObject[], JSObject>), ValueType == ObjectValueType.Statement ? null : oValue, method);
                    result = new CallableField((th, args) =>
                    {
                        return dinv(args);
                    });
                    return result;
                }
                else if (method.GetParameters().Length == 0)
                {
                    var dinv = (Func<JSObject>)Delegate.CreateDelegate(typeof(Func<JSObject>), ValueType == ObjectValueType.Statement ? null : oValue, method);
                    result = new CallableField((th, args) =>
                    {
                        return dinv();
                    });
                    return result;
                }
            }
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
            return result;
        }

        private JSObject getField(string name, bool fast)
        {
            JSObject r = DefaultFieldGetter(name, fast);
            if (r.ValueType == ObjectValueType.NotExistInObject || (fast && r == undefined))
            {
                switch (name)
                {
                    case "constructor":
                        {
                            if (ValueType == ObjectValueType.Object)
                            {
                                ClassProxy res = null;
                                if (constructors.TryGetValue(hostedType, out res))
                                    r.Assign(res);
                                else
                                {
                                    constructors[hostedType] = res = new ClassProxy(hostedType);
                                    r.Assign(res);
                                }
                                return r;
                            }
                            break;
                        }
                }
                JSObject result = null;
#if DEBUG
                var members = hostedType.GetMembers(BindingFlags.Public | (ValueType == ObjectValueType.Statement ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic);
#endif
                var m = hostedType.GetMember(name, BindingFlags.Public | (ValueType == ObjectValueType.Statement ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic);
                if (m.Length > 1)
                    throw new InvalidOperationException("Too many fields with name " + name);
                if (m.Length == 0 || m[0].GetCustomAttribute(typeof(HiddenAttribute)) != null)
                {
                    var i = 0;
                    var d = 0;
                    var gprms = getItem != null ? getItem.GetParameters() : null;
                    var sprms = setItem != null ? setItem.GetParameters() : null;
                    if (((sprms ?? gprms) != null) &&
                        ((gprms ?? sprms)[0].ParameterType == typeof(string)
                        || (Parser.ParseNumber(name, ref i, false, out i) && (gprms ?? sprms)[0].ParameterType == typeof(int))
                        || (Parser.ParseNumber(name, ref i, false, out d) && (gprms ?? sprms)[0].ParameterType == typeof(double))
                        || ((gprms ?? sprms)[0].ParameterType == typeof(object))))
                    {
                        var ptype = (sprms ?? gprms)[0].ParameterType;
                        object index = ptype == typeof(double) ? (object)d : (object)(ptype == typeof(int) ? i : (object)name);
                        return new JSObject()
                        {
                            ValueType = ObjectValueType.Property,
                            oValue = new Statement[] 
                            {
                                new NiL.JS.Statements.ExternalFunction(new CallableField((_th, args) =>
                                {
                                    object[] a = null;
                                    if (sprms[1].ParameterType != typeof(JSObject))
                                    {
                                        args = new JSObject[] { null, args[0] };
                                        a = convertArgs(args, sprms);
                                        a[0] = sprms[0].ParameterType == typeof(string) ? name : (object)(sprms[0].ParameterType == typeof(int) ? i : (object)d);
                                    }
                                    else
                                        a = new object[] { index, args[0] };
                                    setItem.Invoke(oValue, a);
                                    return undefined;
                                })),
                                new NiL.JS.Statements.ExternalFunction(new CallableField((_th, args) =>
                                {
                                    var res = getItem.Invoke(oValue, new object[] { index });
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
                    }
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
                                oValue = new Statement[] { 
                                    pinfo.CanWrite ? convert(pinfo.SetMethod).oValue as Statement : null,
                                    pinfo.CanRead ? convert(pinfo.GetMethod).oValue as Statement : null 
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
