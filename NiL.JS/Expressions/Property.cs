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

        public CodeNode Source { get { return first; } }
        public CodeNode FieldName { get { return second; } }

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
            source = first.Evaluate(context);
            if (source._valueType < JSValueType.Object)
                source = source.Clone() as JSValue;
            else
                source = source._oValue as JSValue ?? source;
            res = source.GetProperty(cachedMemberName ?? second.Evaluate(context), true, memberScope);
            context.objectSource = source;
            if (res._valueType == JSValueType.NotExists)
                res._valueType = JSValueType.NotExistsInObject;
            return res;
        }

        public override JSValue Evaluate(Context context)
        {
            JSValue res = null;
            JSValue source = null;
            source = first.Evaluate(context);
            if (source._valueType < JSValueType.Object)
                source = source.CloneImpl(false);
            else if (source != source._oValue)
            {
                res = source._oValue as JSValue;
                if (res != null)
                {
                    source = res;
                    res = null;
                }
            }
            res = source.GetProperty(cachedMemberName ?? second.Evaluate(context), false, memberScope);
            context.objectSource = source;
            if (res._valueType == JSValueType.NotExists)
                res._valueType = JSValueType.NotExistsInObject;
            else if (res._valueType == JSValueType.Property)
                res = Tools.InvokeGetter(res, source);
            return res;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (stats != null)
                stats.UseGetMember = true;
            base.Build(ref _this, expressionDepth, variables, codeContext, message, stats, opts);
            if (second is Constant)
            {
                cachedMemberName = second.Evaluate(null);
                if (stats != null && cachedMemberName.ToString() == "arguments")
                    stats.ContainsArguments = true;
            }
            if (first is Super)
                memberScope = (codeContext & CodeContext.InStaticMember) != 0 ? PropertyScope.Super : PropertyScope.SuperProto;
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
            if (second is Constant
                && (second as Constant).value.ToString().Length > 0
                && (Parser.ValidateName((second as Constant).value.ToString(), ref i, false, true, true)))
                res += "." + (second as Constant).value;
            else
                res += "[" + second + "]";
            return res;
        }
    }
}