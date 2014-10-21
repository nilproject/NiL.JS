using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class ToBool : Expression
    {
        public ToBool(CodeNode first)
            : base(first, null, false)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            return (bool)first.Evaluate(context);
        }

        public override string ToString()
        {
            return "!!" + first;
        }
    }
}
