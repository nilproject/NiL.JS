using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class LessOrEqual : More
    {
        public LessOrEqual(CodeNode first, CodeNode second)
            : base(first, second)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            return base.Invoke(context).iValue == 0;
        }

        public override string ToString()
        {
            return "(" + first + " <= " + second + ")";
        }
    }
}