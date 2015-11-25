using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class AssignmentOverReplace : AssignmentOperator
    {
        public AssignmentOverReplace(Expression first, Expression second)
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

        internal protected override bool Build(ref CodeNode _this, int expressionDepth, List<string> scopeVariables, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics stats, Options opts)
        {
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics stats)
        {
            // do nothing
        }
    }
}