using NiL.JS.Core.BaseTypes;
using NiL.JS.Core;
using System;

namespace NiL.JS.Core
{
    internal class ExternalFunction : Statement
    {
        private readonly CallableField del;

        public ExternalFunction(CallableField del)
        {
            this.del = del;
        }

        public override JSObject Invoke(Context context)
        {
            throw new InvalidOperationException();
        }

        public override JSObject Invoke(Context context, JSObject args)
        {
            var res = del(context, args);
            if (res == null)
                return JSObject.Null;
            return res;
        }
    }
}