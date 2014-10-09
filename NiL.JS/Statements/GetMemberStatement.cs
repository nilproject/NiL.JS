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

        internal override System.Linq.Expressions.Expression BuildTree(NiL.JS.Core.JIT.TreeBuildingState state)
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
            var source = objStatement.Evaluate(context);
            var n = cachedMemberName ?? memberNameStatement.Evaluate(context);
            context.objectSource = source;
            var res = source.GetMember(n, true, false);
            if (res.valueType == JSObjectType.NotExists)
                res.valueType = JSObjectType.NotExistsInObject;
            return res;
        }

        internal override JSObject Evaluate(Context context)
        {
            var source = objStatement.Evaluate(context);
            var res = source.GetMember(cachedMemberName ?? memberNameStatement.Evaluate(context), false, false);
            context.objectSource = source;
            if (res.valueType == JSObjectType.NotExists)
                res.valueType = JSObjectType.NotExistsInObject;
            else if (res.valueType == JSObjectType.Property)
            {
                var f = (res.oValue as Function[])[1];
                if (f == null)
                    res = JSObject.notExists;
                else
                    res = f.Invoke(source, null);
            }
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
            Parser.Optimize(ref objStatement, depth + 1, variables, strict);
            Parser.Optimize(ref memberNameStatement, depth + 1, variables, strict);
            if (memberNameStatement is ImmidateValueStatement)
                cachedMemberName = memberNameStatement.Evaluate(null);
            return false;
        }

        public override string ToString()
        {
            var res = objStatement.ToString();
            int i = 0;
            if (memberNameStatement is ImmidateValueStatement
                && (memberNameStatement as ImmidateValueStatement).value.ToString().Length > 0
                && (Parser.ValidateName((memberNameStatement as ImmidateValueStatement).value.ToString(), ref i, true)))
                res += "." + (memberNameStatement as ImmidateValueStatement).value;
            else
                res += "[" + memberNameStatement.ToString() + "]";
            return res;
        }
    }
}