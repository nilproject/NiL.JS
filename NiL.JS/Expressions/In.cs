using System;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class In : Expression
    {
        public In(CodeNode first, CodeNode second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
                tempContainer.Assign(first.Evaluate(context));
                var source = second.Evaluate(context);
                if (source.valueType < JSObjectType.Object)
                    throw new JSException(new TypeError("Right-hand value of instanceof is not object."));
                var t = source.GetMember(tempContainer.ToString());
                return t.isExist ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
            }
        }

        public override string ToString()
        {
            return "(" + first + " in " + second + ")";
        }
    }
}