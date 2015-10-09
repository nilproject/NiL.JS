using System;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class InstanceOfOperator : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public InstanceOfOperator(Expression first, Expression second)
            : base(first, second, true)
        {
        }

        internal protected override JSValue Evaluate(Context context)
        {
            var a = tempContainer ?? new JSValue { attributes = JSValueAttributesInternal.Temporary };
            tempContainer = null;
            a.Assign(first.Evaluate(context));
            var c = second.Evaluate(context);
            tempContainer = a;
            if (c.valueType != JSValueType.Function)
                ExceptionsHelper.Throw(new NiL.JS.BaseLibrary.TypeError("Right-hand value of instanceof is not a function."));
            var p = (c.oValue as Function).prototype;
            if (p.valueType < JSValueType.Object)
                ExceptionsHelper.Throw(new TypeError("Property \"prototype\" of function not represent object."));
            if (p.oValue != null)
            {
                while (a != null && a.valueType >= JSValueType.Object && a.oValue != null)
                {
                    if (a.oValue == p.oValue)
                        return true;
                    a = a.__proto__;
                }
            }
            return false;
        }

        internal protected override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            var res = base.Build(ref _this, depth,variables, state, message, statistic, opts);
            if (!res)
            {
                if (first is ConstantNotation)
                {
                    // TODO
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
            return "(" + first + " instanceof " + second + ")";
        }
    }
}