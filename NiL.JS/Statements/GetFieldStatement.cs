using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal class GetFieldStatement : Statement, IOptimizable
    {
        private Statement objStatement;
        private Statement fieldNameStatement;
        private Func<Context, JSObject> impl;

        public GetFieldStatement(Statement obj, Statement fieldName)
        {
            objStatement = obj;
            fieldNameStatement = fieldName;
            impl = (s) =>
            {
                var n = fieldName.Invoke(s);
                if (n.ValueType == ObjectValueType.NotExist)
                    throw new ArgumentException("Varible not exist");
                var th = obj.Invoke(s);
                s.thisBind = th;
                var res = th.GetField(n.ToPrimitiveValue_String_Value(s).Value.ToString());
                return res;
            };
        }

        public GetFieldStatement(Statement obj, string fieldName)
        {
            objStatement = obj;
            impl = (s) =>
            {
                var th = obj.Invoke(s);
                s.thisBind = th;
                var res = th.GetField(fieldName);
                return res;
            };
        }

        public GetFieldStatement(JSObject obj, string fieldName)
        {
            impl = (s) =>
            {
                s.thisBind = obj;
                var res = obj.GetField(fieldName);
                return res;
            };
        }

        public GetFieldStatement(JSObject obj, Statement fieldName)
        {
            fieldNameStatement = fieldName;
            impl = (s) =>
            {
                s.thisBind = obj;
                var res = obj.GetField(fieldName.Invoke(s).ToPrimitiveValue_String_Value(s).Value.ToString());
                return res;
            };
        }

        public override JSObject InvokeForAssing(Context context)
        {
            return impl(context);
        }

        public override JSObject Invoke(Context context)
        {
            var oldthb = context.thisBind ?? context.GetField("this");
            var otu = context.updateThisBind;
            context.updateThisBind = true;
            try
            {
                var res = impl(context);
                if (res.ValueType == ObjectValueType.Property)
                    res = (res.oValue as Statement[])[1].Invoke(context, null);
                return res;
            }
            finally
            {
                context.updateThisBind = otu;
                if (!otu)
                    context.thisBind = oldthb;
            }
        }

        public override JSObject Invoke(Context context, JSObject args)
        {
            throw new NotImplementedException();
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> varibles)
        {
            Parser.Optimize(ref objStatement, depth + 1, varibles);
            Parser.Optimize(ref fieldNameStatement, depth + 1, varibles);
            return false;
        }
    }
}