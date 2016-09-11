using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class DeleteProperty : Expression
    {
        private JSValue cachedMemberName;

        public Expression Source { get { return first; } }
        public Expression PropertyName { get { return second; } }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        internal DeleteProperty(Expression obj, Expression fieldName)
            : base(obj, fieldName, true)
        {
            if (fieldName is Constant)
                cachedMemberName = fieldName.Evaluate(null);
        }

        public override JSValue Evaluate(Context context)
        {
            JSValue source = null;
            source = first.Evaluate(context);
            if (source._valueType < JSValueType.Object)
                source = source.CloneImpl(false);
            else
                source = source._oValue as JSValue ?? source;

            var res = source.DeleteProperty(cachedMemberName ?? second.Evaluate(context));
            context.objectSource = null;
            if (!res && context.strict)
                ExceptionsHelper.ThrowTypeError("Cannot delete property \"" + first + "\".");
            return res;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            return false;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            var res = first.ToString();
            int i = 0;
            var cn = second as Constant;
            if (second is Constant
                && cn.value.ToString().Length > 0
                && (Parser.ValidateName(cn.value.ToString(), ref i, true)))
                res += "." + cn.value;
            else
                res += "[" + second + "]";
            return "delete " + res;
        }
    }
}