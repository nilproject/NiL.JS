using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class None : Expression
    {
        public None(CodeNode first, CodeNode second)
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
#if DEBUG
            else if (!(first is FunctionStatement))
            { }
#endif
            if (context != null)
                context.objectSource = null;
            return temp;
        }

        internal override bool Optimize(ref CodeNode _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            if (second == null && (depth > 2 || first is Expression || first is ExpressionStatement))
            {
                _this = first;
                return true;
            }
            Parser.Optimize(ref first, depth + 1, fdepth, vars, strict);
            Parser.Optimize(ref second, depth + 1, fdepth, vars, strict);
            return false;
        }

        public override string ToString()
        {
            return "(" + first + (second != null ? ", " + second : "") + ")";
        }
    }
}