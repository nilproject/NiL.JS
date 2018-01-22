using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    public sealed class NewTarget : Expression
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

        protected internal override bool LValueModifier
        {
            get
            {
                return false;
            }
        }

        public NewTarget()
        {

        }

        public override JSValue Evaluate(Context context)
        {
            if (context._thisBind != null && (context._thisBind._attributes & JSValueAttributesInternal.ConstructingObject) != 0)
            {
                var stack = Context.GetCurrectContextStack();

                var i = 2;
                while (stack.Count >= i && stack[stack.Count - i]._thisBind == context._thisBind)
                {
                    context = stack[stack.Count - i];
                    i++;
                }

                return context._owner;
            }

            return JSValue.undefined;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            return false;
        }

        public override void Optimize(ref Core.CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {

        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "new.target";
        }
    }
}
