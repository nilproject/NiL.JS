using System;

namespace NiL.JS.Core
{
    [Serializable]
    public abstract class Statement
    {
        public virtual JSObject InvokeForAssing(Context context)
        {
            return Invoke(context);
        }

        public abstract JSObject Invoke(Context context);
    }
}
