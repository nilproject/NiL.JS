using System;
using System.Collections.Generic;
using NiL.JS.Statements;

namespace NiL.JS.Core
{
    [Serializable]
    public sealed class VariableDescriptor
    {
        internal readonly HashSet<VariableReference> references;
        internal bool caching;
        private string name;
        private JSObject cacheRes;
        private Statement owner;

        public bool Defined { get; internal set; }
        public Statement Owner { get { return owner; } internal set { owner = value; } }
        public Statement Inititalizator { get; internal set; }
        public string Name { get { return name; } }

        public IEnumerable<VariableReference> References
        {
            get
            {
                foreach (var item in references)
                    yield return item;
            }
        }

        internal JSObject Get(Context context, bool create)
        {
            context.objectSource = null;
            if (!caching)
                return context.GetVariable(name, create);
            if (cacheRes == null)
            {
                var res = context.GetVariable(name, create);
                if (create && !Defined && res.valueType == JSObjectType.NotExist)
                    res.attributes = JSObjectAttributes.None;
                if (res.valueType < JSObjectType.Undefined)
                    return res;
                return cacheRes = res;
            }

#if DEBUG
            else
            {
                if (create)
                    cacheRes.attributes &= ~JSObjectAttributes.DBGGettedOverGM;
                else
                    cacheRes.attributes |= JSObjectAttributes.DBGGettedOverGM;
            }
#endif
            return cacheRes;
        }

        internal void Add(VariableReference reference)
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

        internal void Remove(VariableReference reference)
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

        internal void ClearCache()
        {
            cacheRes = null;
        }

        internal VariableDescriptor(string name, bool defined)
        {
            this.caching = true;
            this.name = name;
            references = new HashSet<VariableReference>();
            Defined = defined;
        }

        internal VariableDescriptor(VariableReference proto, bool defined)
        {
            this.caching = true;
            this.name = proto.Name;
            if (proto is FunctionStatement.FunctionReference)
                Inititalizator = (proto as FunctionStatement.FunctionReference).Owner;
            references = new HashSet<VariableReference>();
            references.Add(proto);
            Defined = defined;
        }

        public override string ToString()
        {
            return "Name: \"" + name + "\". Reference count: " + references.Count;
        }
    }
}
