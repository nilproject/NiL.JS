using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
{
#if !PORTABLE
    [Serializable]
#endif
    [DisallowNewKeyword]
    public sealed class Symbol : JSValue
    {
        private static readonly Dictionary<string, Symbol> symbolsCache = new Dictionary<string, Symbol>();

        public static readonly Symbol iterator = new Symbol("iterator");

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
            valueType = JSValueType.Symbol;
            if (!symbolsCache.ContainsKey(description))
                symbolsCache[description] = this;
        }

        public static Symbol @for(string description)
        {
            Symbol result = null;
            symbolsCache.TryGetValue(description, out result);
            return result ?? new Symbol(description);
        }

        public static string keyFor(Symbol symbol)
        {
            if (symbol == null)
                ExceptionsHelper.Throw(new TypeError("Invalid argument"));
            return symbol.Description;
        }

        public override JSValue toString(Arguments args)
        {
            return ToString();
        }

        [Hidden]
        public override string ToString()
        {
            return "Symbol(" + Description + ")";
        }

        protected internal override JSValue GetMember(JSValue name, bool forWrite, PropertyScope memberScope)
        {
            if (forWrite)
                return undefined;
            return base.GetMember(name, false, memberScope);
        }
    }
}
