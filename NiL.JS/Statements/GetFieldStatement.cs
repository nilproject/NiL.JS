using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;
using System.Collections.Generic;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class GetFieldStatement : Statement
    {
        private Statement objStatement;
        private Statement fieldNameStatement;

        public Statement Source { get { return objStatement; } }
        public Statement FieldName { get { return fieldNameStatement; } }

        internal GetFieldStatement(Statement obj, Statement fieldName)
        {
            objStatement = obj;
            fieldNameStatement = fieldName;
        }

        internal GetFieldStatement(Statement obj, string fieldName)
        {
            objStatement = obj;
            fieldNameStatement = new ImmidateValueStatement(fieldName);
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

            var n = fieldNameStatement.Invoke(context);
            if (n.ValueType == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));

            context.objectSource = th;
            var res = th.GetField(n.ToString(), callProp, false);
            if (callProp && res.ValueType == JSObjectType.Property)
                res = (res.oValue as Function[])[1].Invoke(th, null);
            return res;
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> varibles)
        {
            Parser.Optimize(ref objStatement, depth + 1, varibles);
            Parser.Optimize(ref fieldNameStatement, depth + 1, varibles);
            return false;
        }

        public override string ToString()
        {
            var res = objStatement.ToString();
            var field = fieldNameStatement.ToString();
            int i = 0;
            if (fieldNameStatement is ImmidateValueStatement
                && field.Length > 0
                && ((field[0] == field[field.Length - 1]) && (field[0] == '"') && Parser.ValidateName(field.Substring(1, field.Length - 2), ref i, true, true)))
                res += "." + field.Substring(1, field.Length - 2);
            else
                res += "[" + field + "]";
            return res;
        }
    }
}