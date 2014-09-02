using System;
using NiL.JS.Core.JIT;

namespace NiL.JS.Core
{
    [Serializable]
    public abstract class VariableReference : CodeNode
    {
        internal int functionDepth;
        public virtual int FunctionDepth { get { return functionDepth; } }
        public abstract string Name { get; }
        public abstract VariableDescriptor Descriptor { get; internal set; }

#if !NET35

        internal override System.Linq.Expressions.Expression BuildTree(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            return System.Linq.Expressions.Expression.Call(
                       System.Linq.Expressions.Expression.Constant(this),
                       this.GetType().GetMethod("Invoke", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(Context) }, null),
                       JITHelpers.ContextParameter
                       );
        }

#endif

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
