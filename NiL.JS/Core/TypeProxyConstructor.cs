using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;
using System.Collections;

namespace NiL.JS.Core
{
    [Serializable]
    [Modules.Prototype(typeof(Function))]
    internal class TypeProxyConstructor : Function
    {
        private static readonly object _object = new object();
        private static readonly object[] _objectA = new object[0];
        internal readonly TypeProxy proxy;
        private MethodProxy[] constructors;

        public TypeProxyConstructor(TypeProxy typeProxy)
        {
            proxy = typeProxy;
            var ctors = typeProxy.hostedType.GetConstructors();
            List<MethodProxy> ctorsL = new List<MethodProxy>(ctors.Length);
            for (int i = 0; i < ctors.Length; i++)
            {
                if (ctors[i].GetCustomAttributes(typeof(Modules.HiddenAttribute), false).Length == 0)
                    ctorsL.Add(new MethodProxy(ctors[i]));
            }
            ctorsL.Sort((x, y) => x.Parameters.Length - y.Parameters.Length);
            constructors = ctorsL.ToArray();
        }

        internal protected override JSObject GetMember(string name, bool create, bool own)
        {
            if (name == "__proto__" && prototype == null)
            {
                prototype = TypeProxy.GetPrototype(typeof(TypeProxyConstructor)).Clone() as JSObject;
                return prototype;
            }
            var res = proxy.GetMember(name, false, own);
            if (res == JSObject.undefined)
                return base.GetMember(name, create, own);
            return res;
        }

        public override JSObject Invoke(Context contextOverride, JSObject args)
        {
            var oldContext = context;
            context = contextOverride;
            try
            {
                return Invoke(args);
            }
            finally
            {
                context = oldContext;
            }
        }

        public override JSObject Invoke(Context contextOverride, JSObject thisOverride, JSObject args)
        {
            var oldContext = context;
            if (contextOverride == null || oldContext == contextOverride)
                return Invoke(thisOverride, args);
            context = contextOverride;
            try
            {
                return Invoke(thisOverride, args);
            }
            finally
            {
                context = oldContext;
            }
        }

        public override JSObject Invoke(JSObject thisOverride, JSObject args)
        {
            if (thisOverride == null)
                return Invoke(args);
            var oldThis = context.thisBind;
            try
            {
                context.thisBind = thisOverride;
                return Invoke(args);
            }
            finally
            {
                context.thisBind = oldThis;
            }
        }

        [Modules.DoNotDelete]
        public override JSObject length
        {
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

        private MethodProxy findConstructor(JSObject argObj, ref object[] args)
        {
            args = null;
            var len = argObj == null ? 0 : argObj.GetMember("length", false, false).iValue;
            for (int i = 0; i < constructors.Length; i++)
            {
                if (constructors[i].Parameters.Length == len
                    || (constructors[i].Parameters.Length == 1
                        && (constructors[i].Parameters[0].ParameterType == typeof(JSObject) 
                            || typeof(ICollection).IsAssignableFrom(constructors[i].Parameters[0].ParameterType))))
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

        public override JSObject Invoke(JSObject argsObj)
        {
            context.ValidateThreadID();
            if (proxy.hostedType.ContainsGenericParameters)
                throw new JSException(TypeProxy.Proxy(new BaseTypes.TypeError(proxy.hostedType.Name + " can't be created because it's generic type.")));
            var _this = context.thisBind;
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
                var obj = constructor.InvokeRaw(context, null, args);
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

        public override IEnumerator<string> GetEnumerator()
        {
            return proxy.GetEnumerator();
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
