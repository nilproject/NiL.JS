using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class SignedShiftRightOperator : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Int;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public SignedShiftRightOperator(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            tempContainer.iValue = Tools.JSObjectToInt32(first.Evaluate(context)) >> Tools.JSObjectToInt32(second.Evaluate(context));
            tempContainer.valueType = JSValueType.Int;
            return tempContainer;
        }

        internal protected override bool Build<T>(ref T _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            var res = base.Build(ref _this, depth, variables, state, message, statistic, opts);
            if (!res && _this == this)
            {
                try
                {
                    if (first.IsContextIndependent && Tools.JSObjectToInt32((first).Evaluate(null)) == 0)
                        _this = new ConstantNotation(0) as T;
                    else if (second.IsContextIndependent && Tools.JSObjectToInt32((second).Evaluate(null)) == 0)
                        _this = new ToIntegerOperator(first) as T;
                }
                catch
                {

                }
            }
            return res;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " >> " + second + ")";
        }
    }
}