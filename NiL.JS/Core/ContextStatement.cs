using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;

namespace NiL.JS.Core
{
    public sealed class ContextStatement : Statement
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
            if (res.ValueType == ObjectValueType.NotExist)
                throw new InvalidOperationException("varible is undefined");
            return res;
        }

        public JSObject Invoke(JSObject args)
        {
            var res = Prototype.Invoke(Context, args);
            return res;
        }

        public override JSObject Invoke(Context context)
        {
            var oldthisBind = Context.thisBind;
            Context.thisBind = context.thisBind;
            var res = Prototype.Invoke(Context);
            Context.thisBind = oldthisBind;
            if (res.ValueType == ObjectValueType.NotExist)
                throw new InvalidOperationException("varible is undefined");
            return res;
        }

        public override JSObject Invoke(Context context, JSObject args)
        {
            var oldthisBind = Context.thisBind;
            Context.thisBind = context.thisBind;
            var res = Prototype.Invoke(Context, args);
            Context.thisBind = oldthisBind;
            if (res.ValueType == ObjectValueType.NotExist)
                throw new InvalidOperationException("varible is undefined");
            return res;
        }

        public override string ToString()
        {
            return "[Function]";
        }
    }
}