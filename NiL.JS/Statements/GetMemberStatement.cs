using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;
using System.Collections.Generic;

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
            return impl(context, false);
        }

        internal override JSObject Invoke(Context context)
        {
            return impl(context, true);
        }

        private JSObject impl(Context context, bool callProp)
        {
            var th = objStatement.Invoke(context);
            if (th.ValueType == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));

            var n = memberNameStatement.Invoke(context);
            if (n.ValueType == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));

            context.objectSource = th;
            var res = th.GetField(n.ToString(), callProp, false);
            if (callProp && res.ValueType == JSObjectType.Property)
                res = (res.oValue as Function[])[1].Invoke(th, null);
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

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VaribleDescriptor> varibles)
        {
            Parser.Optimize(ref objStatement, depth + 1, varibles);
            Parser.Optimize(ref memberNameStatement, depth + 1, varibles);
            return false;
        }

        public override string ToString()
        {
            var res = objStatement.ToString();
            int i = 0;
            if (memberNameStatement is ImmidateValueStatement
                && (memberNameStatement as ImmidateValueStatement).value.oValue.ToString().Length > 0
                && (Parser.ValidateName((memberNameStatement as ImmidateValueStatement).value.oValue.ToString(), ref i, true, true)))
                res += "." + (memberNameStatement as ImmidateValueStatement).value.oValue;
            else
                res += "[" + memberNameStatement.ToString() + "]";
            return res;
        }
    }
}