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
        private JSValue tempContainer1;
        private JSValue tempContainer2;
        private JSValue cachedMemberName;
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

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        internal SetMemberExpression(Expression obj, Expression fieldName, Expression value)
            : base(obj, fieldName, true)
        {
            if (fieldName is ConstantDefinition)
                cachedMemberName = fieldName.Evaluate(null);
            else
                tempContainer1 = new JSValue();
            this.value = value;
            tempContainer2 = new JSValue();
        }

        public override JSValue Evaluate(Context context)
        {
            lock (this)
            {
                JSValue sjso = null;
                JSValue source = null;
                source = first.Evaluate(context);
                if (source.valueType >= JSValueType.Object
                    && source.oValue != null
                    && source.oValue != source
                    && (sjso = source.oValue as JSValue) != null
                    && sjso.valueType >= JSValueType.Object)
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

        private static JSValue safeGet(JSValue temp, CodeNode source, Context context)
        {
            temp.Assign(source.Evaluate(context));
            return temp;
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            codeContext = state;
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
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
            var cn = second as ConstantDefinition;
            if (second is ConstantDefinition
                && cn.value.ToString().Length > 0
                && (Parser.ValidateName(cn.value.ToString(), ref i, true)))
                res += "." + cn.value;
            else
                res += "[" + second + "]";
            return res + " = " + value;
        }
    }
}