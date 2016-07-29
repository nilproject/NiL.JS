using System;
using System.Linq;
using System.Reflection;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core.Functions
{
    /// <summary>
    /// Представляет функцию платформы с фиксированной сигнатурой.
    /// </summary>
    [Prototype(typeof(Function))]
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class ExternalFunction : Function
    {
        [Hidden]
        public override string name
        {
            [Hidden]
            get
            {
#if (PORTABLE || NETCORE)
                return System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(_delegate).Name;
#else
                return _delegate.Method.Name;
#endif
            }
        }
        
        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public override JSValue prototype
        {
            [Hidden]
            get
            {
                return null;
            }
            [Hidden]
            set
            {
            }
        }

        private readonly ExternalFunctionDelegate _delegate;

        [Hidden]
        public ExternalFunctionDelegate Delegate { get { return _delegate; } }

        public ExternalFunction(ExternalFunctionDelegate @delegate)
        {
            if (_length == null)
                _length = new Number(0) { attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate };

#if (PORTABLE || NETCORE)
            var paramCountAttrbt = @delegate.GetMethodInfo().GetCustomAttributes(typeof(ArgumentsLengthAttribute), false).ToArray();
#else
            var paramCountAttrbt = @delegate.Method.GetCustomAttributes(typeof(ArgumentsLengthAttribute), false);
#endif
            _length.iValue = paramCountAttrbt.Length > 0 ? ((ArgumentsLengthAttribute)paramCountAttrbt[0]).Count : 1;
            _prototype = undefined;
            if (@delegate == null)
                throw new ArgumentNullException();
            _delegate = @delegate;
            RequireNewKeywordLevel = BaseLibrary.RequireNewKeywordLevel.WithoutNewOnly;
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
        {
            var res = _delegate(targetObject, arguments);
            if (res == null)
                return JSValue.NotExists;
            return res;
        }
        
        [Hidden]
        public override string ToString(bool headerOnly)
        {
            var result = "function " + name + "()";

            if (!headerOnly)
            {
                result += " { [native code] }";
            }

            return result;
        }
    }
}