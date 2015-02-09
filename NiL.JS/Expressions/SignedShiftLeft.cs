using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class SignedShiftLeft : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Int;
            }
        }

        public SignedShiftLeft(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
                var left = Tools.JSObjectToInt32(first.Evaluate(context));
                tempContainer.iValue = (int)(left << Tools.JSObjectToInt32(second.Evaluate(context)));
                tempContainer.valueType = JSObjectType.Int;
                return tempContainer;
            }
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> vars, bool strict, CompilerMessageCallback message, FunctionStatistic statistic, OptimizationOptions opts)
        {
            var res = base.Build(ref _this, depth, vars, strict, message, statistic, opts);
            if (!res && _this == this)
            {
                try
                {
                    if ((first is Expression)
                        && (first).IsContextIndependent
                        && Tools.JSObjectToInt32((first).Evaluate(null)) == 0)
                        _this = new Constant(0);
                    else if ((second is Expression)
                            && (second).IsContextIndependent
                            && Tools.JSObjectToInt32((second).Evaluate(null)) == 0)
                        _this = new ToInt(first);
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
            return "(" + first + " << " + second + ")";
        }
    }
}