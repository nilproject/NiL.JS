using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal sealed class EmptyStatement : Statement
    {
        public override JSObject Invoke(Context context)
        {
            return JSObject.undefined;
        }

        public override string ToString()
        {
            return "";
        }
    }
}