using System;

namespace NiL.JS.Core
{
    [Serializable]
    public abstract class VariableReference : Statement
    {
        internal int functionDepth;
        public virtual int FunctionDepth { get { return functionDepth; } }
        public abstract string Name { get; }
        public abstract VariableDescriptor Descriptor { get; internal set; }

        protected VariableReference()
        {
            functionDepth = -1;
        }

        protected override Statement[] getChildsImpl()
        {
            return null;
        }
    }
}
