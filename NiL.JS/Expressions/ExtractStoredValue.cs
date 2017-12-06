using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    public sealed class ExtractStoredValue : Expression
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

        public ExtractStoredValue(Expression source)
            : base(source, null, false)
        {

        }

        protected internal override JSValue EvaluateForWrite(Context context)
        {
            return Evaluate(context);
        }

        public override JSValue Evaluate(Context context)
        {
            return (JSValue)context.SuspendData[_left];
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            return false;
        }

        public override string ToString()
        {
            return _left.ToString();
        }

        public override void Decompose(ref Expression self, IList<CodeNode> result)
        {

        }
    }
}
