using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class DeleteMemberExpression : Expression
    {
        private JSValue cachedMemberName;

        public Expression Source { get { return first; } }
        public Expression FieldName { get { return second; } }

        public override bool IsContextIndependent
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

        internal DeleteMemberExpression(Expression obj, Expression fieldName)
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
            var res = source.DeleteMember(cachedMemberName ?? second.Evaluate(context));
            context.objectSource = null;
            if (!res && context.strict)
                ExceptionsHelper.Throw(new TypeError("Can not delete property \"" + first + "\"."));
            return res;
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
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