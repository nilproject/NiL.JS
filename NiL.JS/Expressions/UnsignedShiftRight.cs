using System;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class UnsignedShiftRight : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Number;
            }
        }

        public UnsignedShiftRight(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            var left = (uint)Tools.JSObjectToInt32(first.Evaluate(context));
            var t = left >> Tools.JSObjectToInt32(second.Evaluate(context));
            if (t <= int.MaxValue)
            {
                tempContainer.iValue = (int)t;
                tempContainer.valueType = JSObjectType.Int;
            }
            else
            {
                tempContainer.dValue = (double)t;
                tempContainer.valueType = JSObjectType.Double;
            }
            return tempContainer;
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> vars, bool strict, CompilerMessageCallback message, FunctionStatistic statistic)
        {
            var res = base.Build(ref _this, depth, vars, strict, message, statistic);
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
                        _this = new ToUInt(first);
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
            return "(" + first + " >>> " + second + ")";
        }
    }
}