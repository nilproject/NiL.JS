using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class EmptyStatement : Statement
    {
        private static readonly EmptyStatement _instance = new EmptyStatement();
        public static EmptyStatement Instance { get { return _instance; } }

        private EmptyStatement()
        {
        }

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