using System;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Or : Expression
    {
        public Or(CodeNode first, CodeNode second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
                var left = Tools.JSObjectToInt32(first.Evaluate(context));
                tempContainer.iValue = left | Tools.JSObjectToInt32(second.Evaluate(context));
                tempContainer.valueType = JSObjectType.Int;
                return tempContainer;
            }
        }

        internal override bool Optimize(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            var res = base.Optimize(ref _this, depth, vars, strict);
            if (_this != this)
                return res;
            if ((second is ImmidateValueStatement || (second is Expression && (second as Expression).IsContextIndependent))
                && Tools.JSObjectToInt32(second.Evaluate(null)) == 0)
            {
                _this = new ToInt(first);
                return true;
            }
            if ((first is ImmidateValueStatement || (first is Expression && (first as Expression).IsContextIndependent))
                 && Tools.JSObjectToInt32(first.Evaluate(null)) == 0)
            {
                _this = new ToInt(second);
                return true;
            }
            return res;
        }

        public override string ToString()
        {
            return "(" + first + " | " + second + ")";
        }
    }
}