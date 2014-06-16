using System;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    /// <summary>
    /// Представляет функцию платформы с фиксированной сигнатурой.
    /// </summary>
    [Modules.Prototype(typeof(Function))]
    [Serializable]
    public sealed class ExternalFunction : Function
    {
        public override string Name
        {
            get
            {
                return del.Method.Name;
            }
        }

        public override FunctionType Type
        {
            get
            {
                return FunctionType.Function;
            }
        }

        private readonly ExternalFunctionDelegate del;

        public ExternalFunctionDelegate Delegate { get { return del; } }

        public ExternalFunction(ExternalFunctionDelegate del)
            : base(Context.globalContext, null)
        {
            this.del = del;
            var pc = del.Method.GetCustomAttributes(typeof(ParametersCountAttribute), false);
            if (pc != null && pc.Length != 0)
                _length = (pc[0] as ParametersCountAttribute).Count;
            else
                _length = 0;
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