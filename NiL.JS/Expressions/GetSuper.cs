using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    public sealed class GetSuper : Expression
    {
        public bool ctorMode;

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

        internal GetSuper()
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

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            return false;
        }

        public override void Optimize(ref Core.CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {

        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "super";
        }
    }
}
