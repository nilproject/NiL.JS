using NiL.JS.Core.BaseTypes;
using NiL.JS.Core;
using System;

namespace NiL.JS.Core
{
    internal class ExternalFunction : Function
    {
        private readonly CallableField del;

        public ExternalFunction(CallableField del)
            : base(Context.globalContext, null, null, del.Method.Name)
        {
            this.del = del;
        }

        public override JSObject Invoke(Context contextOverride, JSObject args)
        {
            var oldContext = context;
            context = contextOverride;
            try
            {
                return Invoke(null as JSObject, args);
            }
            finally
            {
                context = oldContext;
            }
        }

        public override JSObject Invoke(JSObject thisOverride, JSObject args)
        {
            var otb = context.thisBind;
            if (thisOverride != null)
                context.thisBind = thisOverride;
            try
            {
                var res = del(context, args);
                if (res == null)
                    return JSObject.Null;
                return res;
            }
            finally
            {
                context.thisBind = otb;
            }
        }
    }
}