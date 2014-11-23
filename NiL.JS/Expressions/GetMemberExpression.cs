using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.JIT;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class GetMemberExpression : Expression
    {
        private JSObject cachedMemberName;

        public CodeNode Source { get { return first; } }
        public CodeNode FieldName { get { return second; } }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            return System.Linq.Expressions.Expression.Call(
                       System.Linq.Expressions.Expression.Constant(this),
                       JITHelpers.methodof(Evaluate),
                       JITHelpers.ContextParameter
                       );
        }

#endif

        internal GetMemberExpression(Expression obj, Expression fieldName)
            : base(obj, fieldName, true)
        {
        }

        internal override JSObject EvaluateForAssing(Context context)
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
            res = source.GetMember(cachedMemberName ?? second.Evaluate(context), true, false);
            if (sjso != null)
            {
                if (sjso.oValue == tempContainer.oValue)
                    source = sjso;
                else
                    source = tempContainer.CloneImpl();
                tempContainer.fields = null;
                tempContainer.oValue = null;
            }
            context.objectSource = source;
            if (res.valueType == JSObjectType.NotExists)
                res.valueType = JSObjectType.NotExistsInObject;
            return res;
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
                if ((sjso ?? source).valueType >= JSObjectType.Object
                    && (((sjso ?? source).attributes & JSObjectAttributesInternal.Immutable) == 0)
                    && (sjso ?? source).fields == null)
                    (sjso ?? source).fields = JSObject.createFields();
                sjso = source;
                tempContainer.Assign(source);
                source = tempContainer;
            }
            res = source.GetMember(cachedMemberName ?? second.Evaluate(context), false, false);
            if (sjso != null)
            {
                if (sjso.oValue == tempContainer.oValue)
                    source = sjso;
                else
                    source = tempContainer.CloneImpl();
                tempContainer.fields = null;
                tempContainer.oValue = null;
            }
            context.objectSource = source;
            if (res.valueType == JSObjectType.NotExists)
                res.valueType = JSObjectType.NotExistsInObject;
            else if (res.valueType == JSObjectType.Property)
                res = (res.oValue as PropertyPair).get != null ? (res.oValue as PropertyPair).get.Invoke(source, null) : JSObject.notExists;
            return res;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            base.Build(ref _this, depth, variables, strict);
            if (second is Constant)
                cachedMemberName = second.Evaluate(null);
            return false;
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
            return res;
        }
    }
}