using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal class GetFieldStatement : Statement, IOptimizable
    {
        private Statement objStatement;
        private Statement fieldNameStatement;

        public GetFieldStatement(Statement obj, Statement fieldName)
        {
            objStatement = obj;
            fieldNameStatement = fieldName;
        }

        public GetFieldStatement(Statement obj, string fieldName)
        {
            objStatement = obj;
            fieldNameStatement = new ImmidateValueStatement(fieldName);
        }

        public override JSObject InvokeForAssing(Context context)
        {
            return impl(context, false);
        }

        public override JSObject Invoke(Context context)
        {
            var res = impl(context, true);
            return res;
        }

        private JSObject impl(Context context, bool callProp)
        {
            var th = objStatement.Invoke(context);
            if (th.ValueType == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));

            var n = fieldNameStatement.Invoke(context);
            if (n.ValueType == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));

            if (context.updateThisBind)
                context.thisBind = th;
            var res = th.GetField(n.ToString(), callProp, false);
            if (callProp && res.ValueType == JSObjectType.Property)
                res = (res.oValue as Function[])[1].Invoke(th, null);
            return res;
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
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