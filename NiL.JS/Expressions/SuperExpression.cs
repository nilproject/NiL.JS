using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    public sealed class SuperExpression : GetVariableExpression
    {
        public bool ctorMode;

        internal SuperExpression(int functionDepth)
            : base("super", functionDepth)
        {

        }

        protected internal override Core.JSValue EvaluateForWrite(Core.Context context)
        {
            ExceptionsHelper.ThrowReferenceError(Strings.InvalidLefthandSideInAssignment);
            return null;
        }

        public override JSValue Evaluate(Context context)
        {
            if (ctorMode)
            {
                context.objectSource = context.thisBind;
                return context.owner.__proto__;
            }
            else
            {
                return context.thisBind;
            }
        }

        protected internal override bool Build(ref CodeNode _this, int expressionDepth, List<string> scopeVariables, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics stats, Options opts)
        {
            return false;
        }

        protected internal override void Optimize(ref Core.CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, Core.FunctionStatistics stats)
        {

        }
    }
}
