using NiL.JS.Core.BaseTypes;
using System;

namespace NiL.JS.Core
{
    public sealed class ContextStatement : IContextStatement
    {
        internal readonly Context Context;
        internal readonly Statement Prototype;

        public ContextStatement(Context context, Statement prototype)
        {
            if (context == null)
                throw new ArgumentNullException();
            Context = context;
            Prototype = prototype;
        }

        public JSObject Invoke()
        {
            var res = Prototype.Invoke(Context);
            if (res.ValueType == ObjectValueType.NoExist)
                    throw new InvalidOperationException("varible is undefined");
            return res;
        }

        public JSObject Invoke(JSObject _this, IContextStatement[] args)
        {
            var res = Prototype.Invoke(Context, _this, args);
            return res;
        }

        public override string ToString()
        {
            return "[Function]";
        }
    }
}