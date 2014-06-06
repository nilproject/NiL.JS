using NiL.JS.Core;
using System;
using System.Collections.Generic;

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

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VaribleDescriptor> vars)
        {
            if (second == null)
            {
                _this = first;
                return true;
            }
            Parser.Optimize(ref first, depth, vars);
            Parser.Optimize(ref second, depth, vars);
            return false;
        }

        public override string ToString()
        {
            return "(" + first + (second != null ? ", " + second : "") + ")";
        }
    }
}