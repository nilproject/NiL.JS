using System;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
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

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> vars, bool strict, CompilerMessageCallback message, FunctionStatistic statistic)
        {
            var res = base.Build(ref _this, depth, vars, strict, message, statistic);
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