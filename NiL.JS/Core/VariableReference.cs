using System;

namespace NiL.JS.Core
{
    [Serializable]
    public abstract class VariableReference : Statement
    {
        public abstract string Name { get; }
        public abstract VariableDescriptor Descriptor { get; internal set; }

        protected override Statement[] getChildsImpl()
        {
            return null;
        }
    }
}
