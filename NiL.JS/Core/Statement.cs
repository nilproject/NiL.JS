using System;

namespace NiL.JS.Core
{
    public abstract class Statement
    {
        public virtual JSObject InvokeForAssing(Context context)
        {
            return Invoke(context);
        }

        public abstract JSObject Invoke(Context context);
    }
}
