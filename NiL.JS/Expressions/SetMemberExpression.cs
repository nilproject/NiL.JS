using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class SetMemberExpression : Expression
    {
        private JSObject cachedMemberName;
        private Expression value;

        public Expression Source { get { return first; } }
        public Expression FieldName { get { return second; } }
        public Expression Value { get { return value; } }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal SetMemberExpression(Expression obj, Expression fieldName, Expression value)
            : base(obj, fieldName, true)
        {
            if (fieldName is Constant)
                cachedMemberName = fieldName.Evaluate(null);
            this.value = value;
        }

        internal override JSObject Evaluate(Context context)
        {
            JSObject sjso = null;
            JSObject res = null;
            JSObject source = null;
            source = first.Evaluate(context);
            if (source.valueType >= JSObjectType.Object
                && source.oValue != null
                && source.oValue != source
                && (sjso = source.oValue as JSObject) != null
                && sjso.valueType >= JSObjectType.Object)
            {
                source = sjso;
                sjso = null;
            }
            else
            {
                if ((sjso ?? source).fields == null)
                    (sjso ?? source).fields = JSObject.createFields();
                sjso = source;
                tempContainer.Assign(source);
                source = tempContainer;
            }
            source.SetMember(cachedMemberName ?? second.Evaluate(context), res = value.Evaluate(context), context.strict);
            context.objectSource = null;
            return res;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict, CompilerMessageCallback message)
        {
            throw new InvalidOperationException();
        }

        public override string ToString()
        {
            var res = first.ToString();
            int i = 0;
            if (second is Constant
                && (second as Constant).value.ToString().Length > 0
                && (Parser.ValidateName((second as Constant).value.ToString(), ref i, true)))
                res += "." + (second as Constant).value;
            else
                res += "[" + second.ToString() + "]";
            return res + " = " + value;
        }
    }
}