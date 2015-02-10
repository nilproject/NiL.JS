using System;
using NiL.JS.Expressions;

namespace NiL.JS.Core
{
    [Serializable]
    public abstract class VariableReference : Expression
    {
        internal int functionDepth;
        public virtual int FunctionDepth { get { return functionDepth; } }
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

        protected internal override PredictedType ResultType
        {
            get
            {
                return descriptor.lastPredictedType;
            }
        }

        protected VariableReference()
        {
            functionDepth = -1;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }
    }
}
