using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class AssignmentOverReplace : Assignment
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

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            // do nothing
        }
    }
}