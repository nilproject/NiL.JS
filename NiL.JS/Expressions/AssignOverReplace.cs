using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class AssignOverReplace : Assign
    {
        public AssignOverReplace(Expression first, Expression second)
            : base(first, second)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            var oldContainer = second.tempContainer;
            second.tempContainer = first.EvaluateForAssing(context);
            var res = second.tempContainer;
            try
            {
                second.Evaluate(context);
            }
            finally
            {
                second.tempContainer = oldContainer;
            }
            return res;
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            // do nothing
        }
    }
}