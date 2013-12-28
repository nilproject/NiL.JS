using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal class GetFieldStatement : Statement
    {
        private Func<Context, JSObject> impl;
        internal readonly Func<Context, JSObject> obj;

        public GetFieldStatement(Statement obj, Statement fieldName)
        {
            this.obj = (s) => { return obj.Invoke(s); };
            impl = (s) =>
            {
                var n = fieldName.Invoke(s);
                if (n.ValueType == ObjectValueType.NotExist)
                    throw new ArgumentException("Varible not exist");
                return obj.Invoke(s).GetField(n.Value.ToString());
            };
        }

        public GetFieldStatement(Statement obj, string fieldName)
        {
            this.obj = (s) => { return obj.Invoke(s); };
            impl = (s) => { return obj.Invoke(s).GetField(fieldName); };
        }

        public GetFieldStatement(JSObject obj, string fieldName)
        {
            this.obj = (s) => { return obj; };
            Func<Context, JSObject> basic = null;
            basic = (s) =>
            {
                var r = obj.GetField(fieldName);
                Func<Context, JSObject> alt = (c) =>
                {
                    if (s != c)
                        return basic(c);
                    else
                        return r;
                };
                impl = alt;
                return r;
            };
            impl = basic;
        }

        public GetFieldStatement(JSObject obj, Statement fieldName)
        {
            this.obj = (s) => { return obj; };
            impl = (s) => { return obj.GetField(fieldName.Invoke(s).Value.ToString()); };
        }

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        public override JSObject InvokeForAssing(Context context)
        {
            return impl(context);
        }

        public override JSObject Invoke(Context context)
        {
            var res = impl(context);
            if (res.ValueType == ObjectValueType.Property)
                res = (res.oValue as IContextStatement[])[1].Invoke(null, null);
            return res;
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }
    }
}