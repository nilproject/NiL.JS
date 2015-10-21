using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class AssignOverReplace : AssignmentOperator
    {
        public AssignOverReplace(Expression first, Expression second)
            : base(first, second)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            var oldContainer = second.tempContainer;
            second.tempContainer = first.EvaluateForWrite(context);
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

        internal protected override bool Build<T>(ref T _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            return false;
        }

        internal protected override void Optimize<T>(ref T _this, FunctionNotation owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            // do nothing
        }
    }
}