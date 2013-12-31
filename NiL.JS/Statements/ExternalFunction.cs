using NiL.JS.Core.BaseTypes;
using NiL.JS.Core;
using System;

namespace NiL.JS.Statements
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

        public override JSObject Invoke(Context context, JSObject[] args)
        {
            return del(context, args);
        }
    }
}