using System;
using NiL.JS.Expressions;

namespace NiL.JS.Core
{
#if !PORTABLE
    [Serializable]
#endif
    public abstract class VariableReference : Expression
    {
        internal int defineDepth;
        public int DefineDepth { get { return defineDepth; } }

        public abstract string Name { get; }

        internal VariableDescriptor descriptor;
        public VariableDescriptor Descriptor { get { return descriptor; } }

        public override bool IsContextIndependent
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
            defineDepth = -1;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }
    }
}
