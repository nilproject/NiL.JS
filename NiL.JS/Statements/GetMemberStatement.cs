using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class GetMemberStatement : Statement
    {
        private Statement objStatement;
        private Statement memberNameStatement;

        public Statement Source { get { return objStatement; } }
        public Statement FieldName { get { return memberNameStatement; } }

        internal GetMemberStatement(Statement obj, Statement fieldName)
        {
            objStatement = obj;
            memberNameStatement = fieldName;
        }

        internal override JSObject InvokeForAssing(Context context)
        {
            return impl(context, true);
        }

        internal override JSObject Invoke(Context context)
        {
            return impl(context, false);
        }

        private JSObject impl(Context context, bool forAssign)
        {
            var source = objStatement.Invoke(context);
            if (source.valueType == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));

            var n = memberNameStatement.Invoke(context);
            if (n.valueType == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));

            context.objectSource = source;
            var res = source.GetMember(n.ToString(), forAssign, false);
            if (!forAssign && res.valueType == JSObjectType.Property)
                res = (res.oValue as Function[])[1].Invoke(source, null);
            return res;
        }

        protected override Statement[] getChildsImpl()
        {
            var res = new List<Statement>()
            {
                objStatement,
                memberNameStatement
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            Parser.Optimize(ref objStatement, depth + 1, variables, strict);
            Parser.Optimize(ref memberNameStatement, depth + 1, variables, strict);
            return false;
        }

        public override string ToString()
        {
            var res = objStatement.ToString();
            int i = 0;
            if (memberNameStatement is ImmidateValueStatement
                && (memberNameStatement as ImmidateValueStatement).value.oValue.ToString().Length > 0
                && (Parser.ValidateName((memberNameStatement as ImmidateValueStatement).value.oValue.ToString(), ref i, true)))
                res += "." + (memberNameStatement as ImmidateValueStatement).value.oValue;
            else
                res += "[" + memberNameStatement.ToString() + "]";
            return res;
        }
    }
}