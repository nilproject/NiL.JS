using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class GetMemberStatement : CodeNode
    {
        private CodeNode objStatement;
        private CodeNode memberNameStatement;

        public CodeNode Source { get { return objStatement; } }
        public CodeNode FieldName { get { return memberNameStatement; } }

        internal GetMemberStatement(CodeNode obj, CodeNode fieldName)
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
            var n = memberNameStatement.Invoke(context);
            context.objectSource = source;
            var res = source.GetMember(n, forAssign, false);
            if (!forAssign && res.valueType == JSObjectType.Property)
            {
                var f = (res.oValue as Function[])[1];
                if (f == null)
                    res = JSObject.notExist;
                else
                    res = f.Invoke(source, null);
            }
            if (res.oValue is TypeProxy && (res.oValue as TypeProxy).prototypeInstance != null)
                return (res.oValue as TypeProxy).prototypeInstance;
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

        internal override bool Optimize(ref CodeNode _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            Parser.Optimize(ref objStatement, depth + 1, fdepth, variables, strict);
            Parser.Optimize(ref memberNameStatement, depth + 1, fdepth, variables, strict);
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