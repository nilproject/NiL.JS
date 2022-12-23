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
        private JSValue _cachedMemberName;
        private PropertyScope _memberScope;

        public CodeNode Source => _left;
        public CodeNode FieldName => _right;

        protected internal override bool ContextIndependent => false;

        internal override bool ResultInTempContainer => false;

        public bool OptionalChaining { get; }

        public Property(Expression source, Expression fieldName)
            : this(source, fieldName, false)
        {

        }

        public Property(Expression source, Expression fieldName, bool optionalChaining)
            : base(source, fieldName, false)
        {
            OptionalChaining = optionalChaining;
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
            res = source.GetProperty(_cachedMemberName ?? _right.Evaluate(context), true, _memberScope);
            context._objectSource = source;
            if (res._valueType == JSValueType.NotExists)
                res._valueType = JSValueType.NotExistsInObject;
            return res;
        }

        public override JSValue Evaluate(Context context)
        {
            JSValue res;

            var source = _left.Evaluate(context);

            if (source._valueType <= JSValueType.Undefined
                || (source._valueType >= JSValueType.Object && source._oValue == null))
            {
                if (OptionalChaining)
                    return JSValue.undefined;

                ExceptionHelper.ThrowTypeError(
                    string.Format(Strings.TryingToGetProperty, _right, source.Defined ? "null" : "undefined"));
            }

            var oldTempContainer = _tempContainer;
            _tempContainer = null;
            if (_cachedMemberName is null)
            {
                if (source._valueType < JSValueType.Object)
                {
                    if (oldTempContainer == null)
                        oldTempContainer = new JSValue();

                    oldTempContainer.Assign(source);
                    source = oldTempContainer;
                }
                else if (source != source._oValue)
                {
                    source = source._oValue as JSValue ?? source;
                }
            }

            var key = _cachedMemberName ?? _right.Evaluate(context);

            res = source.GetProperty(key, false, _memberScope);
            context._objectSource = source;

            if (res == null)
                res = JSValue.undefined;
            else
            {
                if (res._valueType == JSValueType.NotExists)
                {
                    res._valueType = JSValueType.NotExistsInObject;
                }
                else if (res._valueType == JSValueType.Property)
                {
                    res = Tools.GetPropertyOrValue(res, source);
                }
            }

            _tempContainer = oldTempContainer;
            return res;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (stats != null)
                stats.UseGetMember = true;

            base.Build(ref _this, expressionDepth, variables, codeContext, message, stats, opts);

            if (_right is Constant)
            {
                _cachedMemberName = _right.Evaluate(null);
                if (stats != null && _cachedMemberName.ToString() == "arguments")
                    stats.ContainsArguments = true;
            }

            if (_left is Super)
                _memberScope = (codeContext & CodeContext.InStaticMember) != 0 ? PropertyScope.Super : PropertyScope.PrototypeOfSuperClass;

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