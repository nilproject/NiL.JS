using System;

namespace NiL.JS.Core
{
    [Serializable]
    public abstract class VariableReference : Statement
    {
        public virtual int FunctionDepth { get; internal protected set; }
        public abstract string Name { get; }
        public abstract VariableDescriptor Descriptor { get; internal set; }

        protected VariableReference()
        {
            FunctionDepth = -1;
        }

        protected override Statement[] getChildsImpl()
        {
            return null;
        }
    }
}
