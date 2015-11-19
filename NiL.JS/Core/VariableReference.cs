using System;
using NiL.JS.Expressions;

namespace NiL.JS.Core
{
#if !PORTABLE
    [Serializable]
#endif
    public abstract class VariableReference : Expression
    {
        internal int defineScopeDepth;
        public int DefineFunctionDepth { get { return defineScopeDepth; } }

        public abstract string Name { get; }

        internal VariableDescriptor descriptor;
        public VariableDescriptor Descriptor { get { return descriptor; } }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return descriptor.lastPredictedType;
            }
        }

        protected VariableReference()
        {
            defineScopeDepth = -1;
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            return null;
        }
    }
}
