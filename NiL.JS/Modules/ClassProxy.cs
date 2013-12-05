using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core;
using System.Reflection;

namespace NiL.JS.Modules
{
    public sealed class ClassProxy : JSObject
    {
        private readonly Type hostedType;
        private object instance;

        public ClassProxy(Type host)
        {
            hostedType = host;
            instance = null;
            ValueType = ObjectValueType.Object;
            oValue = "Module";
            base.fieldGetter = getField;
        }

        private JSObject getField(string name, bool fast)
        {
            JSObject r = DefaultFieldGetter(name, false);
            if (r.ValueType == ObjectValueType.NoExistInObject)
            {
                bool protect = false;
                JSObject result = null;
                var m = hostedType.GetMember(name);
                if (m.Length > 1)
                    throw new InvalidOperationException("Too many fields with name " + name);
                if (m.Length == 0)
                    return undefined;
                switch (m[0].MemberType)
                {
                    case System.Reflection.MemberTypes.Method:
                        {
                            MethodInfo method = m[0] as MethodInfo;
                            if (instance == null)
                                instance = hostedType.GetConstructor(System.Type.EmptyTypes).Invoke(System.Type.EmptyTypes);
                            var dinv = (Func<IContextStatement[], JSObject>)Delegate.CreateDelegate(typeof(Func<IContextStatement[], JSObject>), instance, method);
                            result = new CallableField((th, args) =>
                            {
                                return dinv(args);
                            });
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
                            else result = new JSObject() { oValue = res, ValueType = ObjectValueType.Object };
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
