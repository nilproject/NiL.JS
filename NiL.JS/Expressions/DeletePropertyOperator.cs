using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class DeletePropertyOperator : Expression
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

        internal DeletePropertyOperator(Expression obj, Expression fieldName)
            : base(obj, fieldName, true)
        {
            if (fieldName is ConstantDefinition)
                cachedMemberName = fieldName.Evaluate(null);
        }

        public override JSValue Evaluate(Context context)
        {
            JSValue source = null;
            source = first.Evaluate(context);
            if (source.valueType < JSValueType.Object)
                source = source.Clone() as JSValue;
            else
                source = source.oValue as JSValue ?? source;
            var res = source.DeleteProperty(cachedMemberName ?? second.Evaluate(context));
            context.objectSource = null;
            if (!res && context.strict)
                ExceptionsHelper.ThrowTypeError("Cannot delete property \"" + first + "\".");
            return res;
        }

        internal protected override bool Build(ref CodeNode _this, int expressionDepth, List<string> scopeVariables, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics stats, Options opts)
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
            var cn = second as ConstantDefinition;
            if (second is ConstantDefinition
                && cn.value.ToString().Length > 0
                && (Parser.ValidateName(cn.value.ToString(), ref i, true)))
                res += "." + cn.value;
            else
                res += "[" + second + "]";
            return "delete " + res;
        }
    }
}