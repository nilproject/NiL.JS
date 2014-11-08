using System;
using System.Collections.Generic;
using NiL.JS.Statements;
using System.Collections.ObjectModel;
using NiL.JS.Core.TypeProxing;

namespace NiL.JS.Core
{
    [Serializable]
    public sealed class VariableDescriptor
    {
        internal readonly List<VariableReference> references;
        internal readonly string name;
        internal CodeNode owner;
        internal int defineDepth;
        internal bool captured;
        internal bool readOnly;
        internal JSObject cacheRes;
        internal Context cacheContext;
        internal List<CodeNode> assignations;

        internal bool defined;
        public bool Defined { get { return defined; } }
        public CodeNode Owner
        {
            get { return owner; }
        }
        public CodeNode Inititalizator { get; internal set; }
        public string Name { get { return name; } }
        public int ReferenceCount { get { return references.Count; } }
        public ReadOnlyCollection<CodeNode> Assignations { get { return assignations.AsReadOnly(); } }

        public IEnumerable<VariableReference> References
        {
            get
            {
                for (var i = 0; i < references.Count; i++)
                    yield return references[i];
            }
        }

        internal JSObject Get(Context context, bool create, int depth)
        {
            TypeProxy tp = null;
            JSObject res = null;
            context.objectSource = null;
            if (((defineDepth | depth) & int.MinValue) != 0)
                return context.GetVariable(name, create);
            if (cacheRes != null && depth > defineDepth)
            {
                do
                {
                    if (context is WithContext)
                    {
                        cacheContext = null;
                        break;
                    }
                    context = context.parent;
                    depth--;
                }
                while (depth > defineDepth);
            }
            if (context != cacheContext)
                cacheRes = null;
            else if (cacheRes != null)
                return cacheRes;
            res = context.GetVariable(name, create);
            if (create
                && !defined
                && res.valueType == JSObjectType.NotExists)
                res.attributes = JSObjectAttributesInternal.None;
            else
            {
                tp = res.oValue as TypeProxy;
                if (tp != null)
                    res = tp.prototypeInstance ?? res;
            }
            if ((res.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                return res;
            cacheContext = context;
            cacheRes = res;
            return res;
        }

        internal VariableDescriptor(string name, int defineDepth)
        {
            this.defineDepth = defineDepth;
            this.name = name;
            references = new List<VariableReference>();
            defined = true;
        }

        internal VariableDescriptor(VariableReference proto, bool defined, int defineDepth)
        {
            this.defineDepth = defineDepth;
            this.name = proto.Name;
            if (proto is FunctionStatement.FunctionReference)
                Inititalizator = (proto as FunctionStatement.FunctionReference).Owner;
            references = new List<VariableReference>();
            references.Add(proto);
            proto.descriptor = this;
            this.defined = defined;
        }

        public override string ToString()
        {
            return name;
            //return "Name: \"" + name + "\". Reference count: " + references.Count;
        }
    }
}
