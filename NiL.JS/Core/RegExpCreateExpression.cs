using System;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Core
{
    [Serializable]
    internal sealed class RegExpStatement : CodeNode
    {
        private string pattern;
        private string flags;

        public RegExpStatement(string pattern, string flags)
        {
            this.pattern = pattern;
            this.flags = flags;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        internal override JSObject Invoke(Context context)
        {
            return new RegExp(pattern, flags);
        }
    }
}
