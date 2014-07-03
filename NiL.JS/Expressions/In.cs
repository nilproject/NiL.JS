using System;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class In : Expression
    {
        public In(CodeNode first, CodeNode second)
            : base(first, second, false)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            var fn = first.Invoke(context);
            var oassc = fn.assignCallback;
            fn.assignCallback = (sender) => { fn = fn.Clone() as JSObject; };
            try
            {
                var source = second.Invoke(context);
                if (source.valueType < JSObjectType.Object)
                    throw new JSException(new TypeError("Right-hand value of instanceof is not object."));
                var t = source.GetMember(fn.ToString());
                return t.isExist ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
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