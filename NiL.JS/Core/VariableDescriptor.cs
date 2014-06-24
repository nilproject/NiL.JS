using System;
using System.Collections.Generic;
using NiL.JS.Statements;

namespace NiL.JS.Core
{
    [Serializable]
    public sealed class VariableDescriptor
    {
        internal readonly HashSet<VariableReference> references;
        internal int defineDepth;
        private string name;
        private JSObject cacheRes;
        private Context prewContext;
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

        internal JSObject Get(Context context, bool create, int depth)
        {
            context.objectSource = null;
            if (depth < 0 || defineDepth < 0)
                return context.GetVariable(name, create);
            if (cacheRes != null)
            {
                while (depth > defineDepth)
                {
                    if (context is WithContext)
                    {
                        cacheRes = null;
                        break;
                    }
                    context = context.prototype;
                    depth--;
                }
                if (context != prewContext)
                    cacheRes = null;
            }
            if (cacheRes == null)
            {
                var res = context.GetVariable(name, create);
                if (create && !Defined && res.valueType == JSObjectType.NotExist)
                    res.attributes = JSObjectAttributes.None;
                if (res.oValue is TypeProxy && (res.oValue as TypeProxy).prototypeInstance != null)
                    res = (res.oValue as TypeProxy).prototypeInstance;
                if ((res.attributes & JSObjectAttributes.SystemObject) != 0)
                    return res; // Могли сначала запросить переменную, а потом её создать и инициализировать. 
                                // В таком случае закешированным остался бы notExist
                prewContext = context;
                return cacheRes = res;
            }
            return cacheRes;
        }

        internal VariableDescriptor(string name, int defineDepth)
        {
            this.defineDepth = defineDepth;
            this.name = name;
            references = new HashSet<VariableReference>();
            Defined = true;
        }

        internal VariableDescriptor(VariableReference proto, bool defined, int defineDepth)
        {
            this.defineDepth = defineDepth;
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
