using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    public sealed class EmptyStatement : Statement
    {
        internal override JSObject Invoke(Context context)
        {
            return JSObject.undefined;
        }

        public override string ToString()
        {
            return "";
        }
    }
}