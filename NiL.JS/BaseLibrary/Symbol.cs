using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;
using NiL.JS.Core.Modules;

namespace NiL.JS.BaseLibrary
{
    public sealed class Symbol : JSObject
    {
        private static readonly Dictionary<string, Symbol> symbolsCache = new Dictionary<string, Symbol>();

        [Hidden]
        public string Description { get; private set; }

        public Symbol()
            : this("")
        {

        }

        public Symbol(string description)
        {
            Description = description;
            oValue = this;
            valueType = JSObjectType.Symbol;
            symbolsCache[description] = this;
        }

        public static Symbol @for(string description)
        {
            Symbol result = null;
            symbolsCache.TryGetValue(description, out result);
            return result ?? new Symbol(description);
        }

        public override JSObject toString(Arguments args)
        {
            return ToString();
        }

        [Hidden]
        public override string ToString()
        {
            return "Symbol(" + Description + ")";
        }

        protected internal override JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
            if (forWrite)
                return undefined;
            return base.GetMember(name, forWrite, own);
        }

        [Hidden]
        public override void Assign(JSObject value)
        {
            if ((attributes & JSObjectAttributesInternal.ReadOnly) == 0)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                throw new InvalidOperationException("Try to assign to Boolean");
            }
        }
    }
}
