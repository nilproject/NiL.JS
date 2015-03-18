using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Or : Expression
    {
        public Or(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            var left = Tools.JSObjectToInt32(first.Evaluate(context));
            tempContainer.iValue = left | Tools.JSObjectToInt32(second.Evaluate(context));
            tempContainer.valueType = JSObjectType.Int;
            return tempContainer;
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            var res = base.Build(ref _this, depth,variables, state, message, statistic, opts);
            if (_this != this)
                return res;
            if ((second is Constant || (second is Expression && ((Expression)second).IsContextIndependent))
                && Tools.JSObjectToInt32(second.Evaluate(null)) == 0)
            {
                _this = new ToInt(first);
                return true;
            }
            if ((first is Constant || (first is Expression && ((Expression)first).IsContextIndependent))
                 && Tools.JSObjectToInt32(first.Evaluate(null)) == 0)
            {
                _this = new ToInt(second);
                return true;
            }
            return res;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " | " + second + ")";
        }
    }
}