using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    [Serializable]
    [Prototype(typeof(Function))]
    internal class TypeProxyConstructor : Function
    {
        [Hidden]
        private static readonly object _object = new object();
        [Hidden]
        private static readonly object[] _objectA = new object[0];
        [Hidden]
        internal readonly TypeProxy proxy;
        [Hidden]
        private MethodProxy[] constructors;

        [Hidden]
        public override string Name
        {
            [Hidden]
            get
            {
                return proxy.hostedType.Name;
            }
        }

        [Hidden]
        public override FunctionType Type
        {
            [Hidden]
            get
            {
                return FunctionType.Function;
            }
        }

        [Hidden]
        public TypeProxyConstructor(TypeProxy typeProxy)
        {
            proxy = typeProxy;
            var ctors = typeProxy.hostedType.GetConstructors();
            List<MethodProxy> ctorsL = new List<MethodProxy>(ctors.Length);
            for (int i = 0; i < ctors.Length; i++)
            {
                if (ctors[i].GetCustomAttributes(typeof(HiddenAttribute), false).Length == 0)
                    ctorsL.Add(new MethodProxy(ctors[i]));
            }
            ctorsL.Sort((x, y) => x.Parameters.Length - y.Parameters.Length);
            constructors = ctorsL.ToArray();
        }

        [Hidden]
        internal protected override JSObject GetMember(string name, bool create, bool own)
        {
            if (name == "__proto__" && prototype == null)
            {
                prototype = TypeProxy.GetPrototype(typeof(TypeProxyConstructor));
                if (create && prototype.GetType() != typeof(JSObject))
                    prototype = prototype.Clone() as JSObject;
                return prototype;
            }
            var res = proxy.GetMember(name, false, own);
            if (res == JSObject.notExist)
                return base.GetMember(name, create, own);
            return res;
        }

        [Hidden]
        public override JSObject Invoke(JSObject thisOverride, JSObject argsObj)
        {
            if (proxy.hostedType.ContainsGenericParameters)
                throw new JSException(TypeProxy.Proxy(new BaseTypes.TypeError(proxy.hostedType.Name + " can't be created because it's generic type.")));
            var _this = thisOverride;
            object[] args = null;
            MethodProxy constructor = findConstructor(argsObj, ref args);
            if (constructor == null)
                throw new JSException(TypeProxy.Proxy(new BaseTypes.TypeError(proxy.hostedType.Name + " can't be created.")));
            bool bynew = false;
            if (_this != null)
            {
                bynew = _this.oValue is Statements.Operators.New;
            }
            try
            {
                var obj = constructor.InvokeImpl(null, args, argsObj);
                JSObject res = null;
                if (bynew)
                {
                    _this.oValue = obj;
                    if (obj is BaseTypes.Date)
                        _this.valueType = JSObjectType.Date;
                    else if (obj is JSObject)
                        _this.valueType = (JSObjectType)System.Math.Max((int)JSObjectType.Object, (int)(obj as JSObject).valueType);
                    res = _this;
                }
                else
                {
                    if (proxy.hostedType == typeof(JSObject))
                    {
                        if (((obj as JSObject).oValue is JSObject) && ((obj as JSObject).oValue as JSObject).valueType >= JSObjectType.Object)
                            return (obj as JSObject).oValue as JSObject;
                    }
                    if (proxy.hostedType == typeof(Date))
                        res = (obj as Date).toString();
                    else
                        res = obj is JSObject ? obj as JSObject : new JSObject(false)
                        {
                            oValue = obj,
                            valueType = JSObjectType.Object,
                            prototype = TypeProxy.GetPrototype(proxy.hostedType)
                        };
                }
                return res;
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        [DoNotEnumerate]
        [DoNotDelete]
        public override JSObject length
        {
            [Hidden]
            get
            {
                if (_length == null)
                    _length = new Number(0) { attributes = JSObjectAttributes.ReadOnly | JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum };
                if (proxy.hostedType == typeof(Function))
                    _length.iValue = 1;
                else
                    _length.iValue = proxy.hostedType.GetConstructors().Last().GetParameters().Length;
                return _length;
            }
        }

        [Hidden]
        private MethodProxy findConstructor(JSObject argObj, ref object[] args)
        {
            args = null;
            var len = argObj == null ? 0 : argObj.GetMember("length").iValue;
            for (int i = 0; i < constructors.Length; i++)
            {
                if (constructors[i].Parameters.Length == len
                    || (constructors[i].Parameters.Length == 1 && (constructors[i].Parameters[0].ParameterType == typeof(JSObject)
                                                                   || constructors[i].Parameters[0].ParameterType == typeof(JSObject[])
                                                                   || constructors[i].Parameters[0].ParameterType == typeof(object[]))))
                {
                    if (len == 0)
                        args = _objectA;
                    else
                    {
                        args = constructors[i].ConvertArgs(argObj);
                        for (var j = args.Length; j-- > 0; )
                        {
                            if (!constructors[i].Parameters[j].ParameterType.IsAssignableFrom(args[j] != null ? args[j].GetType() : typeof(object)))
                            {
                                j = 0;
                                args = null;
                            }
                        }
                        if (args == null)
                            continue;
                    }
                    return constructors[i];
                }
            }
            return null;
        }

        [Hidden]
        protected internal override IEnumerator<string> GetEnumeratorImpl(bool pdef)
        {
            var e = (prototype ?? GetMember("__proto__")).GetEnumeratorImpl(pdef);
            while (e.MoveNext())
                yield return e.Current;
            e = proxy.GetEnumeratorImpl(pdef);
            while (e.MoveNext())
                yield return e.Current;
        }

        [Hidden]
        public override string ToString()
        {
            return "function " + proxy.hostedType.Name + "() { [native code] }";
        }

        [Hidden]
        public override JSObject toString(JSObject args)
        {
            return base.toString(args);
        }
    }
}
