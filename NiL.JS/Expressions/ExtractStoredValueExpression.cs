using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    public sealed class ExtractStoredValueExpression : Expression
    {
        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override bool NeedDecompose
        {
            get
            {
                return false;
            }
        }

        public ExtractStoredValueExpression(Expression source)
            : base(source, null, false)
        {

        }

        public override Core.JSValue Evaluate(Core.Context context)
        {
            return (JSValue)context.SuspendData[first];
        }

        protected internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            return false;
        }

        public override string ToString()
        {
            return first.ToString();
        }

        protected internal override void Decompose(ref Expression self, IList<CodeNode> result)
        {

        }
    }
}
