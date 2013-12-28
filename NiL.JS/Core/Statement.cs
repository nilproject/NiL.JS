using System;

namespace NiL.JS.Core
{
    public abstract class Statement
    {
        public virtual IContextStatement Implement(Context context)
        {
            //if (this is IContextStatement)
            //    return this as IContextStatement;
            return new ContextStatement(context, this);
        }

        public virtual JSObject InvokeForAssing(Context context)
        {
            return Invoke(context);
        }

        public abstract JSObject Invoke(Context context);
        public abstract JSObject Invoke(Context context, JSObject _this, IContextStatement[] args);
    }
}
