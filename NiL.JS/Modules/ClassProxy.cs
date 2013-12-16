using NiL.JS.Core;
using System;
using System.Reflection;

namespace NiL.JS.Modules
{
    public sealed class ClassProxy : JSObject
    {
        private Type hostedType;

        private ClassProxy()
        {
            ValueType = ObjectValueType.Object;
            base.fieldGetter = getField;
        }

        public ClassProxy(Type host)
        {
            hostedType = host;
            oValue = new Statements.ExternalFunction((x, y) => { return new ClassProxy() { hostedType = hostedType, oValue = hostedType.GetConstructor(Type.EmptyTypes).Invoke(null) }; });
            ValueType = ObjectValueType.Statement;
            base.fieldGetter = getField;
        }

        private static object[] convertArgs(IContextStatement[] source, int targetCount)
        {
            if (source == null)
                return null;
            object[] res = new object[targetCount];
            targetCount = System.Math.Min(targetCount, source.Length);
            for (int i = 0; i < targetCount; i++)
                res[i] = source[i].Invoke().Value;
            return res;
        }

        private JSObject getField(string name, bool fast)
        {
            JSObject r = DefaultFieldGetter(name, false);
            if (r.ValueType == ObjectValueType.NoExistInObject)
            {
                bool protect = false;
                JSObject result = null;
                var m = hostedType.GetMember(name, BindingFlags.Public | (oValue == null ? BindingFlags.Static : 0) | BindingFlags.NonPublic | BindingFlags.Instance);
                if (m.Length > 1)
                    throw new InvalidOperationException("Too many fields with name " + name);
                if (m.Length == 0)
                    return undefined;
                switch (m[0].MemberType)
                {
                    case MemberTypes.Constructor:
                        {
                            var method = (ConstructorInfo)m[0];
                            result = new CallableField((th, args) =>
                            {
                                var res = method.Invoke(args);
                                if (res is int)
                                    return (int)res;
                                else if (res is double)
                                    return (double)res;
                                else if (res is string)
                                    return (string)res;
                                else if (res is bool)
                                    return (bool)res;
                                else if (res is ContextStatement)
                                    return (JSObject)(ContextStatement)res;
                                else return new ClassProxy(res.GetType()) { oValue = res };
                            });
                            protect = method.GetCustomAttributes(typeof(ProtectedAttribute), false).Length != 0;
                            break;
                        }
                    case MemberTypes.Method:
                        {
                            var method = (MethodInfo)m[0];
                            //if (oValue == null)
                            //    oValue = hostedType.GetConstructor(System.Type.EmptyTypes).Invoke(System.Type.EmptyTypes);
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
                                    var res = method.Invoke(oValue, convertArgs(args, method.GetParameters().Length));
                                    if (res == null)
                                        return null;
                                    else if (res is int)
                                        return (int)res;
                                    else if (res is double)
                                        return (double)res;
                                    else if (res is string)
                                        return (string)res;
                                    else if (res is bool)
                                        return (bool)res;
                                    else if (res is ContextStatement)
                                        return (JSObject)(ContextStatement)res;
                                    else return new ClassProxy(res.GetType()) { oValue = res };
                                });
                            }
                            protect = method.GetCustomAttributes(typeof(ProtectedAttribute), false).Length != 0;
                            break;
                        }
                    case MemberTypes.Field:
                        {
                            var field = (m[0] as FieldInfo);
                            object res = field.GetValue(this);
                            if (res is int)
                                result = (int)res;
                            else if (res is double)
                                result = (double)res;
                            else if (res is string)
                                result = (string)res;
                            else if (res is bool)
                                result = (bool)res;
                            else if (res is ContextStatement)
                                result = (JSObject)(ContextStatement)res;
                            else return new ClassProxy(res.GetType()) { oValue = res };
                            protect = field.GetCustomAttributes(typeof(ProtectedAttribute), false).Length != 0;
                            break;
                        }
                    default: throw new NotImplementedException("Convertion from " + m[0].MemberType + " not implemented");
                }
                r.Assign(result);
                if (protect)
                    r.Protect();
            }
            return r;
        }
    }
}
