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
                return Invoke(args);
            }
            finally
            {
                context = oldContext;
            }
        }

        public override JSObject Invoke(Context contextOverride, JSObject thisOverride, JSObject args)
        {
            var oldContext = context;
            if (contextOverride == null || oldContext == contextOverride)
                return Invoke(thisOverride, args);
            context = contextOverride;
            try
            {
                return Invoke(thisOverride, args);
            }
            finally
            {
                context = oldContext;
            }
        }

        public override JSObject Invoke(JSObject thisOverride, JSObject args)
        {
            var oldThis = context.thisBind;
            if (thisOverride == null || oldThis == thisOverride)
                return Invoke(args);
            context.thisBind = thisOverride;
            try
            {
                return Invoke(args);
            }
            finally
            {
                context.thisBind = oldThis;
            }
        }

        public override JSObject length
        {
            get
            {
                _length.iValue = del.Method.GetParameters().Length;
                return _length;
            }
        }

        public override JSObject Invoke(JSObject args)
        {
            var res = del(context, args);
            if (res == null)
                return JSObject.Null;
            return res;
        }
    }
}