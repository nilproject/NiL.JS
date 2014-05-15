using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    [Serializable]
    [Modules.Prototype(typeof(Function))]
    internal class TypeProxyConstructor : Function
    {
        private static readonly new object Object = new object();
        internal readonly TypeProxy proxy;

        private static JSObject empty(Context context, JSObject args)
        {
            return null;
        }

        public TypeProxyConstructor(TypeProxy typeProxy)
        {
            proxy = typeProxy;
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            if (name == "__proto__" && prototype == null)
            {
                prototype = TypeProxy.GetPrototype(typeof(TypeProxyConstructor)).Clone() as JSObject;
                return prototype;
            }
            var res = proxy.GetField(name, true, own);
            if (res == JSObject.undefined)
                return base.GetField(name, fast, own);
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
                    _length = new Number(0) { attributes = JSObjectAttributes.ReadOnly | JSObjectAttributes.DontDelete | JSObjectAttributes.DontEnum };
                if (proxy.hostedType == typeof(Function))
                    _length.iValue = 1;
                else
                    _length.iValue = proxy.hostedType.GetConstructors().Last().GetParameters().Length;
                return _length;
            }
        }

        private ConstructorInfo findConstructor(JSObject argObj, ref object[] args)
        {
            ConstructorInfo constructor = null;
            var len = argObj == null ? 0 : argObj.GetField("length", false, false).iValue;
            if (len == 0)
                constructor = proxy.hostedType.GetConstructor(System.Type.EmptyTypes);
            if (constructor != null && constructor.GetCustomAttributes(typeof(Modules.HiddenAttribute), false).Length == 0)
                return constructor;
            Type[] argtypes = new[] { typeof(JSObject) };
            if (len == 1)
            {
                var val = argObj.GetField("0", true, true).Value;
                if (val == null)
                    argtypes[0] = typeof(object);
                else
                    argtypes[0] = val.GetType();
                if (argtypes[0] != typeof(JSObject) && !argtypes[0].IsSubclassOf(typeof(JSObject)))
                {
                    constructor = proxy.hostedType.GetConstructor(argtypes);
                    if (constructor != null && constructor.GetCustomAttributes(typeof(Modules.HiddenAttribute), false).Length == 0)
                    {
                        args = new object[] { val };
                        return constructor;
                    }
                }
                argtypes[0] = typeof(JSObject);
            }
            constructor = proxy.hostedType.GetConstructor(argtypes);
            if (constructor != null && constructor.GetCustomAttributes(typeof(Modules.HiddenAttribute), false).Length == 0)
            {
                args = new object[] { argObj };
                return constructor;
            }
            argtypes[0] = typeof(JSObject[]);
            constructor = proxy.hostedType.GetConstructor(argtypes);
            if (constructor != null && constructor.GetCustomAttributes(typeof(Modules.HiddenAttribute), false).Length == 0)
            {
                args = new JSObject[len];
                for (int i = 0; i < len; i++)
                    args[i] = argObj.GetField(i.ToString(), true, false);
                args = new[] { args };
                return constructor;
            }
            if (len != 1)
                argtypes = new Type[len];
            for (int i = 0; i < len; i++)
            {
                var a = argObj.GetField(i.ToString(), true, false);
                argtypes[i] = (a.Value ?? Object).GetType();
            }
            constructor = proxy.hostedType.GetConstructor(argtypes);
            if (constructor != null && constructor.GetCustomAttributes(typeof(Modules.HiddenAttribute), false).Length == 0)
            {
                args = new object[len];
                for (int i = 0; i < len; i++)
                    args[i] = argObj.GetField(i.ToString(), true, false).Value;
                return constructor;
            }
            for (int i = 0; i < len; i++)
                argtypes[i] = typeof(JSObject);
            constructor = proxy.hostedType.GetConstructor(argtypes);
            if (constructor != null && constructor.GetCustomAttributes(typeof(Modules.HiddenAttribute), false).Length == 0)
            {
                args = new object[len];
                for (int i = 0; i < len; i++)
                    args[i] = argObj.GetField(i.ToString(), true, false);
                return constructor;
            }
            constructor = proxy.hostedType.GetConstructor(System.Type.EmptyTypes);
            args = null;
            return constructor;
        }

        public override JSObject Invoke(JSObject argsObj)
        {
            context.ValidateThreadID();
            var thisBind = context.thisBind;
            object[] args = null;
            ConstructorInfo constructor = findConstructor(argsObj, ref args);
            if (constructor == null)
                throw new JSException(TypeProxy.Proxy(new BaseTypes.TypeError(proxy.hostedType.Name + " can't be created.")));
            var _this = thisBind;
            bool bynew = false;
            if (_this != null)
            {
                bynew = _this.oValue is Statements.Operators.New;
            }
            try
            {
                var obj = constructor.Invoke(args);
                JSObject res = null;
                if (bynew)
                {
                    _this.oValue = obj;
                    if (obj is BaseTypes.Date)
                        _this.ValueType = JSObjectType.Date;
                    else if (obj is JSObject)
                        _this.ValueType = (JSObjectType)System.Math.Max((int)JSObjectType.Object, (int)(obj as JSObject).ValueType);
                    res = _this;
                }
                else
                {
                    if (proxy.hostedType == typeof(JSObject))
                    {
                        if (((obj as JSObject).oValue is JSObject) && ((obj as JSObject).oValue as JSObject).ValueType >= JSObjectType.Object)
                            return (obj as JSObject).oValue as JSObject;
                    }
                    if (proxy.hostedType == typeof(Date))
                        res = (obj as Date).toString();
                    else
                        res = obj is JSObject ? obj as JSObject : new JSObject(false)
                        {
                            oValue = obj,
                            ValueType = JSObjectType.Object,
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
