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
        internal string name;
        private JSObject cacheRes;
        private Context prewContext;
        private CodeNode owner;

        public bool Defined { get; internal set; }
        public CodeNode Owner { get { return owner; } internal set { owner = value; } }
        public CodeNode Inititalizator { get; internal set; }
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
            if (((defineDepth | depth) & 0x80000000) != 0)
                return context.GetVariable(name, create);
            if (cacheRes != null)
            {
                while (depth > defineDepth)
                {
                    if (context.GetType() == typeof(WithContext))
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
                if (res.valueType == JSObjectType.NotExists && create && !Defined)
                    res.attributes = JSObjectAttributesInternal.None;
                else
                {
                    TypeProxy tp = res.valueType != JSObjectType.Object ? null : res.oValue as TypeProxy;
                    if (tp != null)
                        res = tp.prototypeInstance ?? res;
                }
                if ((res.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                    return res;
                prewContext = context;
                cacheRes = res;
                return res;
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
            return name;
            //return "Name: \"" + name + "\". Reference count: " + references.Count;
        }
    }
}
