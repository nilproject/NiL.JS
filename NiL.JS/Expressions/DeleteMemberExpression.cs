using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.JIT;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class DeleteMemberExpression : Expression
    {
        private JSObject cachedMemberName;

        public Expression Source { get { return first; } }
        public Expression FieldName { get { return second; } }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal DeleteMemberExpression(Expression obj, Expression fieldName)
            : base(obj, fieldName, true)
        {
        }

        internal override JSObject Evaluate(Context context)
        {
            JSObject sjso = null;
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
            var res = source.DeleteMember(cachedMemberName ?? second.Evaluate(context));
            context.objectSource = null;
            if (!res && context.strict)
                throw new JSException(new TypeError("Can not delete property \"" + first + "\"."));
            return res;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
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
            return "delete " + res;
        }
    }
}