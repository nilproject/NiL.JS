using System;
using NiL.JS.Core.JIT;
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

#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            return System.Linq.Expressions.Expression.Call(
                       System.Linq.Expressions.Expression.Constant(this),
                       JITHelpers.methodof(Evaluate),
                       JITHelpers.ContextParameter
                       );
        }

#endif

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
