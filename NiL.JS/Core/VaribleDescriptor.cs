using System;
using System.Collections.Generic;
using NiL.JS.Statements;

namespace NiL.JS.Core
{
    [Serializable]
    public sealed class VaribleDescriptor
    {
        private HashSet<VaribleReference> references;
        private string name;
        private Context cacheContext;
        private JSObject cacheRes;

        public bool Defined { get; internal set; }
        public Statement Owner { get; internal set; }
        public Statement Inititalizator { get; internal set; }
        public string Name { get { return name; } }

        public IEnumerable<VaribleReference> References
        {
            get
            {
                foreach (var item in references)
                    yield return item;
            }
        }

        internal JSObject Get(Context context)
        {
            context.objectSource = null;
            if (context.GetType() == typeof(WithContext))
                return context.GetField(name);
            if (cacheRes == null
                || cacheRes.ValueType < JSObjectType.Undefined
                || (null == Owner && cacheContext != context) // необъявленные переменные
                || (context.owner == Owner && cacheContext != context)) // объявленные переменные
            {
                cacheContext = context;
                return cacheRes = context.GetField(name);
            }
            return cacheRes;
        }

        internal void Add(VaribleReference reference)
        {
            if (reference.Name != name)
                throw new ArgumentException("Try to add reference with different name.");
            references.Add(reference);
            reference.Descriptor = this;
        }

        internal VaribleDescriptor(VaribleReference proto, bool defined)
        {
            this.name = proto.Name;
            if (proto is FunctionStatement.FunctionReference)
                Inititalizator = (proto as FunctionStatement.FunctionReference).Owner;
            references = new HashSet<VaribleReference>();
            references.Add(proto);
            Defined = defined;
        }
    }
}
