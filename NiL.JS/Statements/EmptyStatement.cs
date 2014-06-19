using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class EmptyStatement : Statement
    {
        private static readonly EmptyStatement _instance = new EmptyStatement();
        public static EmptyStatement Instance { get { return _instance; } }

        public EmptyStatement()
        {
        }

        public EmptyStatement(int position)
        {
            Position = position;
            Length = 0;
        }

        internal override JSObject Invoke(Context context)
        {
            return JSObject.undefined;
        }

        protected override Statement[] getChildsImpl()
        {
            return null;
        }

        public override string ToString()
        {
            return "";
        }
    }
}