using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.JIT;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class GetMemberStatement : CodeNode
    {
        private JSObject cachedMemberName;
        private CodeNode objStatement;
        private CodeNode memberNameStatement;

        public CodeNode Source { get { return objStatement; } }
        public CodeNode FieldName { get { return memberNameStatement; } }

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

        internal GetMemberStatement(CodeNode obj, CodeNode fieldName)
        {
            objStatement = obj;
            memberNameStatement = fieldName;
        }

        internal override JSObject EvaluateForAssing(Context context)
        {
            JSObject res = null;
            JSObject source = null;
            source = objStatement.Evaluate(context);
            if (source.valueType >= JSObjectType.Object
                && source.oValue != null
                && source.oValue != source
                && source.oValue is JSObject
                && (source.oValue as JSObject).valueType >= JSObjectType.Object)
                source = source.oValue as JSObject;
            else
                source = source.CloneImpl();
            res = source.GetMember(cachedMemberName ?? memberNameStatement.Evaluate(context), true, false);
            context.objectSource = source;
            if (res.valueType == JSObjectType.NotExists)
                res.valueType = JSObjectType.NotExistsInObject;
            return res;
        }

        internal override JSObject Evaluate(Context context)
        {
            JSObject res = null;
            JSObject source = null;
            source = objStatement.Evaluate(context);
            if (source.valueType >= JSObjectType.Object
                && source.oValue != null
                && source.oValue != source
                && source.oValue is JSObject
                && (source.oValue as JSObject).valueType >= JSObjectType.Object)
                source = source.oValue as JSObject;
            else
                source = source.CloneImpl();
            res = source.GetMember(cachedMemberName ?? memberNameStatement.Evaluate(context), false, false);
            context.objectSource = source;
            if (res.valueType == JSObjectType.NotExists)
                res.valueType = JSObjectType.NotExistsInObject;
            else if (res.valueType == JSObjectType.Property)
                res = (res.oValue as PropertyPair).get != null ? (res.oValue as PropertyPair).get.Invoke(source, null) : JSObject.notExists;
            return res;
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                objStatement,
                memberNameStatement
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            Parser.Build(ref objStatement, depth + 1, variables, strict);
            Parser.Build(ref memberNameStatement, depth + 1, variables, strict);
            if (memberNameStatement is Constant)
                cachedMemberName = memberNameStatement.Evaluate(null);
            return false;
        }

        public override string ToString()
        {
            var res = objStatement.ToString();
            int i = 0;
            if (memberNameStatement is Constant
                && (memberNameStatement as Constant).value.ToString().Length > 0
                && (Parser.ValidateName((memberNameStatement as Constant).value.ToString(), ref i, true)))
                res += "." + (memberNameStatement as Constant).value;
            else
                res += "[" + memberNameStatement.ToString() + "]";
            return res;
        }
    }
}