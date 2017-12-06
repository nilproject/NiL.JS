using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Property : Expression
    {
        private JSValue cachedMemberName;
        private PropertyScope memberScope;

        public CodeNode Source { get { return _left; } }
        public CodeNode FieldName { get { return _right; } }

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

        internal Property(Expression source, Expression fieldName)
            : base(source, fieldName, false)
        {
        }

        internal protected override JSValue EvaluateForWrite(Context context)
        {
            JSValue res = null;
            JSValue source = null;
            source = _left.Evaluate(context);
            if (source._valueType < JSValueType.Object)
                source = source.Clone() as JSValue;
            else
                source = source._oValue as JSValue ?? source;
            res = source.GetProperty(cachedMemberName ?? _right.Evaluate(context), true, memberScope);
            context._objectSource = source;
            if (res._valueType == JSValueType.NotExists)
                res._valueType = JSValueType.NotExistsInObject;
            return res;
        }

        public override JSValue Evaluate(Context context)
        {
            JSValue res = null;
            JSValue source = null;

            source = _left.Evaluate(context);
            if (source._valueType < JSValueType.Object)
            {
                source = source.CloneImpl(false);
            }
            else if (source != source._oValue)
            {
                res = source._oValue as JSValue;
                if (res != null)
                {
                    source = res;
                }
            }

            res = source.GetProperty(cachedMemberName ?? _right.Evaluate(context), false, memberScope);
            context._objectSource = source;

            if (res == null)
                res = JSValue.undefined;

            if (res._valueType == JSValueType.NotExists)
            {
                res._valueType = JSValueType.NotExistsInObject;
            }
            else if (res._valueType == JSValueType.Property)
            {
                res = Tools.InvokeGetter(res, source);
            }

            return res;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (stats != null)
                stats.UseGetMember = true;
            base.Build(ref _this, expressionDepth, variables, codeContext, message, stats, opts);
            if (_right is Constant)
            {
                cachedMemberName = _right.Evaluate(null);
                if (stats != null && cachedMemberName.ToString() == "arguments")
                    stats.ContainsArguments = true;
            }

            if (_left is Super)
                memberScope = (codeContext & CodeContext.InStaticMember) != 0 ? PropertyScope.Super : PropertyScope.PrototypeOfSuperclass;

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
            if (_right is Constant
                && (_right as Constant).value.ToString().Length > 0
                && (Parser.ValidateName((_right as Constant).value.ToString(), ref i, false, true, true)))
                res += "." + (_right as Constant).value;
            else
                res += "[" + _right + "]";
            return res;
        }
    }
}