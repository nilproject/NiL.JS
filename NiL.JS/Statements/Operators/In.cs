using System;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class In : Operator
    {
        public In(Statement first, Statement second)
            : base(first, second, false)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            var fn = Tools.RaiseIfNotExist(first.Invoke(context));
            var oassc = fn.assignCallback;
            fn.assignCallback = (sender) => { fn = fn.Clone() as JSObject; };
            try
            {
                var source = Tools.RaiseIfNotExist(second.Invoke(context));
                if (source.valueType < JSObjectType.Object)
                    throw new JSException(TypeProxy.Proxy(new TypeError("Right-hand value of instanceof is not object.")));
                var t = source.GetMember(fn.ToString());
                return t.valueType >= JSObjectType.Undefined ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
            }
            finally
            {
                fn.assignCallback = oassc;
            }
        }

        public override string ToString()
        {
            return "(" + first + " in " + second + ")";
        }
    }
}