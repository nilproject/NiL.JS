using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class SetProperty : Expression
    {
        private JSValue tempContainer1;
        private JSValue tempContainer2;
        private JSValue cachedMemberName;
        private Expression value;

        public Expression Source { get { return _left; } }
        public Expression FieldName { get { return _right; } }
        public Expression Value { get { return value; } }

        protected internal override bool ContextIndependent
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

        internal SetProperty(Expression obj, Expression fieldName, Expression value)
            : base(obj, fieldName, true)
        {
            if (fieldName is Constant)
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
                source = _left.Evaluate(context);
                if (source._valueType >= JSValueType.Object
                    && source._oValue != null
                    && source._oValue != source
                    && (sjso = source._oValue as JSValue) != null
                    && sjso._valueType >= JSValueType.Object)
                {
                    source = sjso;
                    sjso = null;
                }
                else
                {
                    tempContainer2.Assign(source);
                    source = tempContainer2;
                }

                source.SetProperty(
                    cachedMemberName ?? safeGet(tempContainer1, _right, context),
                    safeGet(_tempContainer, value, context),
                    context._strict);

                context._objectSource = null;
                return _tempContainer;
            }
        }

        private static JSValue safeGet(JSValue temp, CodeNode source, Context context)
        {
            temp.Assign(source.Evaluate(context));
            return temp;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            this._codeContext = codeContext;
            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            var cn = value as CodeNode;
            value.Optimize(ref cn, owner, message, opts, stats);
            value = cn as Expression;
            base.Optimize(ref _this, owner, message, opts, stats);
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            base.RebuildScope(functionInfo, transferedVariables, scopeBias);

            value.RebuildScope(functionInfo, transferedVariables, scopeBias);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            return new CodeNode[] { _left, _right, value };
        }

        public override string ToString()
        {
            var res = _left.ToString();
            int i = 0;
            var cn = _right as Constant;
            if (_right is Constant
                && cn.value.ToString().Length > 0
                && (Parser.ValidateName(cn.value.ToString(), ref i, true)))
                res += "." + cn.value;
            else
                res += "[" + _right + "]";
            return res + " = " + value;
        }
    }
}