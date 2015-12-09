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
#if !PORTABLE
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
#if PORTABLE
                return System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(del).Name;
#else
                return del.Method.Name;
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

        private readonly ExternalFunctionDelegate del;

        [Hidden]
        public ExternalFunctionDelegate Delegate { get { return del; } }

        public ExternalFunction(ExternalFunctionDelegate del)
        {
            if (_length == null)
                _length = new Number(0) { attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate };

#if PORTABLE
            var paramCountAttrbt = del.GetMethodInfo().GetCustomAttributes(typeof(ArgumentsLengthAttribute), false).ToArray();
#else
            var paramCountAttrbt = del.Method.GetCustomAttributes(typeof(ArgumentsLengthAttribute), false);
#endif
            _length.iValue = paramCountAttrbt.Length > 0 ? ((ArgumentsLengthAttribute)paramCountAttrbt[0]).Count : 1;
            _prototype = undefined;
            if (del == null)
                throw new ArgumentNullException();
            this.del = del;
            RequireNewKeywordLevel = BaseLibrary.RequireNewKeywordLevel.WithoutNewOnly;
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments, Function newTarget)
        {
            var res = del(targetObject, arguments);
            if (res == null)
                return JSValue.NotExists;
            return res;
        }
    }
}