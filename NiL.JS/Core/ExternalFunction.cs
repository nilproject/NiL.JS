using NiL.JS.Core.BaseTypes;
using NiL.JS.Core;
using System;

namespace NiL.JS.Core
{
    /// <summary>
    /// Представляет функцию платформы с фиксированной сигнатурой.
    /// </summary>
    [Modules.Prototype(typeof(Function))]
    [Serializable]
    public sealed class ExternalFunction : Function
    {
        private readonly ExternalFunctionDelegate del;

        public ExternalFunctionDelegate Delegate { get { return del; } }

        public ExternalFunction(ExternalFunctionDelegate del)
            : base(Context.globalContext, null, null, del.Method.Name, FunctionType.Function)
        {
            this.del = del;
        }

        public override JSObject Invoke(JSObject thisBind, JSObject args)
        {
            var res = del(thisBind, args);
            if (res == null)
                return JSObject.Null;
            return res;
        }

        [Modules.DoNotDelete]
        public override JSObject length
        {
            get
            {
                if (_length == null)
                    _length = new Number(0) { attributes = JSObjectAttributes.ReadOnly | JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum };
                _length.iValue = 1;
                return _length;
            }
        }
    }
}