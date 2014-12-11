using System;
using System.Collections.Generic;
using NiL.JS.Statements;
using System.Collections.ObjectModel;
using NiL.JS.Core.TypeProxing;
using NiL.JS.Expressions;

namespace NiL.JS.Core
{
    public enum PredictedType : int
    {
        Unknown = 0x0,
        Ambiguous = 0x10,
        Undefined = 0x20,
        Bool = 0x30,
        Number = 0x40,
        Int = 0x41,
        Double = 0x42,
        String = 0x50,
        Object = 0x60,
        Function = 0x70,
        Group = 0xF0,
        Full = 0xFF
    }

    [Serializable]
    public sealed class VariableDescriptor
    {
        internal int defineDepth;
        internal JSObject cacheRes;
        internal Context cacheContext;
        internal readonly List<VariableReference> references;
        internal readonly string name;
        internal CodeNode owner;
        internal bool captured;
        internal bool readOnly;
        internal List<CodeNode> assignations;
        internal PredictedType lastPredictedType;

        internal bool isDefined;
        public bool IsDefined { get { return isDefined; } }
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
            context.objectSource = null;
            if (((defineDepth | depth) & int.MinValue) != 0)
                return context.GetVariable(name, create);
            if (context == cacheContext)
                return cacheRes;
            return deepGet(context, create, depth);
        }

        private JSObject deepGet(Context context, bool create, int depth)
        {
            TypeProxy tp = null;
            JSObject res = null;
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
                && !isDefined
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
            isDefined = true;
        }

        internal VariableDescriptor(VariableReference proto, bool defined, int defineDepth)
        {
            this.defineDepth = defineDepth;
            this.name = proto.Name;
            if (proto is FunctionExpression.FunctionReference)
                Inititalizator = (proto as FunctionExpression.FunctionReference).Owner;
            references = new List<VariableReference>();
            references.Add(proto);
            proto.descriptor = this;
            this.isDefined = defined;
        }

        public override string ToString()
        {
            return name;
            //return "Name: \"" + name + "\". Reference count: " + references.Count;
        }
    }
}
