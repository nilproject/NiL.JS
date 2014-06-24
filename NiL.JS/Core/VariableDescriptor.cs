using System;
using System.Collections.Generic;
using NiL.JS.Statements;

namespace NiL.JS.Core
{
    [Serializable]
    public sealed class VariableDescriptor
    {
        internal readonly HashSet<VariableReference> references;
        internal int definDepth;
        private string name;
        private JSObject cacheRes;
        private Statement owner;

        public bool Defined { get; internal set; }
        public Statement Owner { get { return owner; } internal set { owner = value; } }
        public Statement Inititalizator { get; internal set; }
        public string Name { get { return name; } }
        public int ReferenceCount { get { return references.Count; } }

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
            //if ((attributes & VariableDescriptorAttributes.NoCaching) != 0)
            //    return context.GetVariable(name, create);
            if (cacheRes == null)
            {
                var res = context.GetVariable(name, create);
                if (create && !Defined && res.valueType == JSObjectType.NotExist)
                    res.attributes = JSObjectAttributes.None;
                if (!create && res.valueType < JSObjectType.Undefined)
                    return res;
                if (res.oValue is TypeProxy && (res.oValue as TypeProxy).prototypeInstance != null)
                    res = (res.oValue as TypeProxy).prototypeInstance;
                return cacheRes = res;
            }
            return cacheRes;
        }

        internal void ClearCache()
        {
            cacheRes = null;
        }

        internal VariableDescriptor(string name, int definDepth)
        {
            this.name = name;
            references = new HashSet<VariableReference>();
            Defined = true;
        }

        internal VariableDescriptor(VariableReference proto, bool defined, int definDepth)
        {
            this.name = proto.Name;
            if (proto is FunctionStatement.FunctionReference)
                Inititalizator = (proto as FunctionStatement.FunctionReference).Owner;
            references = new HashSet<VariableReference>();
            references.Add(proto);
            proto.Descriptor = this;
            Defined = defined;
        }

        public override string ToString()
        {
            return "Name: \"" + name + "\". Reference count: " + references.Count;
        }
    }
}
