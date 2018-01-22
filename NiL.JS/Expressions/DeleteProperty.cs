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

        public Expression Source { get { return _left; } }
        public Expression PropertyName { get { return _right; } }

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
            source = _left.Evaluate(context);
            if (source._valueType < JSValueType.Object)
                source = source.CloneImpl(false);
            else
                source = source._oValue as JSValue ?? source;

            var res = source.DeleteProperty(cachedMemberName ?? _right.Evaluate(context));
            context._objectSource = null;
            if (!res && context._strict)
                ExceptionHelper.ThrowTypeError("Cannot delete property \"" + _left + "\".");
            return res;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            return false;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
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
            return "delete " + res;
        }
    }
}