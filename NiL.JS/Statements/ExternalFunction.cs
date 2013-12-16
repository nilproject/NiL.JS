using NiL.JS.Core.BaseTypes;
using NiL.JS.Core;
using System;

namespace NiL.JS.Statements
{
    internal class ExternalFunction : IContextStatement
    {
        private readonly CallableField del;

        public ExternalFunction(CallableField del)
        {
            this.del = del;
        }

        public JSObject Invoke()
        {
            throw new System.InvalidOperationException();
        }

        public JSObject Invoke(JSObject _this, IContextStatement[] args)
        {
            return del(_this, args);
        }
    }
}