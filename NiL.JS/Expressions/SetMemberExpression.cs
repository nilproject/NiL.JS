using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class SetMemberExpression : Expression
    {
        private JSObject tempContainer1;
        private JSObject tempContainer2;
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

        protected internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        internal SetMemberExpression(Expression obj, Expression fieldName, Expression value)
            : base(obj, fieldName, true)
        {
            if (fieldName is Constant)
                cachedMemberName = fieldName.Evaluate(null);
            else
                tempContainer1 = new JSObject();
            this.value = value;
            tempContainer2 = new JSObject();
        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
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
                    //if ((sjso ?? source).fields == null)
                    //    (sjso ?? source).fields = JSObject.createFields();
                    tempContainer2.Assign(source);
                    source = tempContainer2;
                }
                source.SetMember(
                    cachedMemberName ?? safeGet(tempContainer1, second, context),
                    safeGet(tempContainer, value, context),
                    context.strict);
                context.objectSource = null;
                return tempContainer;
            }
        }

        private static JSObject safeGet(JSObject temp, CodeNode source, Context context)
        {
            temp.Assign(source.Evaluate(context));
            return temp;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            codeContext = state;

            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            var cn = value as CodeNode;
            value.Optimize(ref cn, owner, message, opts, statistic);
            value = cn as Expression;
            base.Optimize(ref _this, owner, message, opts, statistic);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        protected override CodeNode[] getChildsImpl()
        {
            return new CodeNode[] { first, second, value };
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