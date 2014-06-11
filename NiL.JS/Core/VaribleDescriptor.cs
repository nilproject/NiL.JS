using System;
using System.Collections.Generic;
using NiL.JS.Statements;

namespace NiL.JS.Core
{
    [Serializable]
    public sealed class VaribleDescriptor
    {
        internal HashSet<VaribleReference> references;
        private string name;
        private Context cacheContext;
        private JSObject cacheRes;
        private Statement owner;

        public bool Defined { get; internal set; }
        public Statement Owner { get { return owner; } internal set { owner = value; } }
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

        internal JSObject Get(Context context, bool create)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            context.objectSource = null;
            if (context is WithContext)
                return context.GetVarible(name);
            if (cacheContext != context && (
                cacheRes == null
                || context.owner == owner // объявленные переменные
                || null == owner)) // необъявленные переменные
            {
                var res = context.GetVarible(name, create);
                if (create && !Defined && res.valueType == JSObjectType.NotExist)
                    res.attributes = JSObjectAttributes.None;
                if (res.valueType < JSObjectType.Undefined)
                    return res;
                cacheContext = context;
                return cacheRes = res;
            }
#if DEBUG
            if (create)
                cacheRes.attributes &= ~JSObjectAttributes.DBGGettedOverGM;
            else
                cacheRes.attributes |= JSObjectAttributes.DBGGettedOverGM;
#endif
            return cacheRes;
        }

        internal void Add(VaribleReference reference)
        {
            if (reference.Name != name)
                throw new ArgumentException("Try to add reference with different name.");
            if (reference.Descriptor == this)
                return;
            if (reference.Descriptor != null)
                throw new ArgumentException("Try to double registration reference.");
            references.Add(reference);
            reference.Descriptor = this;
        }

        internal void Remove(VaribleReference reference)
        {
            if (reference.Name != name)
                throw new ArgumentException("Try to remove reference with different name.");
            if (reference.Descriptor == null)
                return;
            if (reference.Descriptor != this)
                throw new ArgumentException("Try to remove reference from another descriptor.");
            references.Remove(reference);
            reference.Descriptor = null;
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
