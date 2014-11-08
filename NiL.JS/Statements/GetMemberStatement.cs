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
        private bool triedToAGO;
        private bool ago;
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
            var source = objStatement.Evaluate(context);
            var n = cachedMemberName ?? memberNameStatement.Evaluate(context);
            context.objectSource = source;
            res = source.GetMember(n, true, false);
            if (res.valueType == JSObjectType.NotExists)
                res.valueType = JSObjectType.NotExistsInObject;
            return res;
        }

        internal override JSObject Evaluate(Context context)
        {
            JSObject res = null;
            JSObject source = null;
            if (!triedToAGO)
            {
                triedToAGO = true;
                if (context.caller != null
                    && context.caller.creator.containsArguments
                    && !context.caller.creator.containsEval
                    && !context.caller.creator.containsWith
                    && context.caller._arguments is Arguments)
                {
                    var src = objStatement as VariableReference;
                    if (src != null
                        && src.Name == "arguments")
                    {
                        ago = true;
                    }
                }
            }
            if (ago)
            {
                source = context.caller._arguments;
                if (cachedMemberName != null)
                {
                    if (cachedMemberName.valueType == JSObjectType.Int)
                        res = (source as Arguments)[cachedMemberName.iValue];
                    else
                        res = source.GetMember(cachedMemberName, false, false);
                }
                else
                    res = source.GetMember(memberNameStatement.Evaluate(context), false, false);
            }
            else
            {
                source = objStatement.Evaluate(context);
                res = source.GetMember(cachedMemberName ?? memberNameStatement.Evaluate(context), false, false);
            }
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