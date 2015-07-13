using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class BitwiseDisjunctionOperator : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Int;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public BitwiseDisjunctionOperator(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        internal override JSValue Evaluate(Context context)
        {
            var left = Tools.JSObjectToInt32(first.Evaluate(context));
            tempContainer.iValue = left | Tools.JSObjectToInt32(second.Evaluate(context));
            tempContainer.valueType = JSValueType.Int;
            return tempContainer;
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            var res = base.Build(ref _this, depth,variables, state, message, statistic, opts);
            if (_this != this)
                return res;
            if ((second is ConstantNotation || (second is Expression && ((Expression)second).IsContextIndependent))
                && Tools.JSObjectToInt32(second.Evaluate(null)) == 0)
            {
                _this = new ToIntegerOperator(first);
                return true;
            }
            if ((first is ConstantNotation || (first is Expression && ((Expression)first).IsContextIndependent))
                 && Tools.JSObjectToInt32(first.Evaluate(null)) == 0)
            {
                _this = new ToIntegerOperator(second);
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