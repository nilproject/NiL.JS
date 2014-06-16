using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class None : Operator
    {
        public None(Statement first, Statement second)
            : base(first, second, false)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            JSObject temp = null;
            temp = first.Invoke(context);
            if (second != null)
            {
                if (context != null)
                    context.objectSource = null;
                temp = second.Invoke(context);
            }
            if (context != null)
                context.objectSource = null;
            return temp;
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            if (second == null)
            {
                _this = first;
                return true;
            }
            Parser.Optimize(ref first, depth, vars, strict);
            Parser.Optimize(ref second, depth, vars, strict);
            return false;
        }

        public override string ToString()
        {
            return "(" + first + (second != null ? ", " + second : "") + ")";
        }
    }
}