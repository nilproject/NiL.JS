using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Core
{
    [Modules.Prototype(typeof(BaseTypes.Function))]
    internal class TypeProxyConstructor : BaseTypes.Function
    {
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
            var res = proxy.GetField(name, true, false);
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
            var oldContext = context;
            try
            {
                context = new Context(context);
                context.thisBind = thisOverride;
                return Invoke(args);
            }
            finally
            {
                context = oldContext;
            }
        }

        public override JSObject length
        {
            get
            {
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
                constructor = proxy.hostedType.GetConstructor(Type.EmptyTypes);
            if (constructor != null && constructor.GetCustomAttributes(typeof(Modules.HiddenAttribute), false).Length == 0)
                return constructor;

            Type[] argtypes = null;
            argtypes = new[] { typeof(JSObject) };
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
            argtypes[0] = typeof(object[]);
            constructor = proxy.hostedType.GetConstructor(argtypes);
            if (constructor != null && constructor.GetCustomAttributes(typeof(Modules.HiddenAttribute), false).Length == 0)
            {
                args = new object[len];
                for (int i = 0; i < len; i++)
                    args[i] = argObj.GetField(i.ToString(), true, false);
                args = new[] { args };
                return constructor;
            }
            if (len != 1)
            {
                argtypes = new Type[len];
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
            }
            for (int i = 0; i < len; i++)
                argtypes[i] = argObj.GetField(i.ToString(), true, false).Value.GetType();
            constructor = proxy.hostedType.GetConstructor(argtypes);
            if (constructor != null && constructor.GetCustomAttributes(typeof(Modules.HiddenAttribute), false).Length == 0)
            {
                args = new object[len];
                for (int i = 0; i < len; i++)
                    args[i] = argObj.GetField(i.ToString(), true, false).Value;
                return constructor;
            }
            for (int i = 0; i < len; i++)
                argtypes[i] = typeof(object);
            constructor = proxy.hostedType.GetConstructor(argtypes);
            if (constructor != null && constructor.GetCustomAttributes(typeof(Modules.HiddenAttribute), false).Length == 0)
            {
                args = new object[len];
                for (int i = 0; i < len; i++)
                    args[i] = argObj.GetField(i.ToString(), true, false).Value;
                return constructor;
            }
            constructor = proxy.hostedType.GetConstructor(Type.EmptyTypes);
            args = null;
            return constructor;
        }

        public override JSObject Invoke(JSObject argsObj)
        {
            var thisBind = context.thisBind;
            object[] args = null;
            ConstructorInfo constructor = findConstructor(argsObj, ref args);
            if (constructor == null)
                throw new JSException(TypeProxy.Proxy(new BaseTypes.TypeError(proxy.hostedType.Name + " can't be created.")));
            var _this = thisBind;
            JSObject thproto = null;
            bool bynew = false;
            if (_this != null)
            {
                thproto = _this.GetField("__proto__", true, true);
                if (thproto.oValue is TypeProxy)
                    bynew = (thproto.oValue as TypeProxy).hostedType == proxy.hostedType;
            }
            try
            {
                var obj = constructor.Invoke(args);
                JSObject res = null;
                if (bynew)
                {
                    if (obj is EmbeddedType)
                        _this.oValue = (obj as JSObject).oValue;
                    else
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

        public override string ToString()
        {
            return "function " + proxy.hostedType.Name + "() { [native code] }";
        }
    }
}
