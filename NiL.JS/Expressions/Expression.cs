using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.JIT;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public abstract class Expression : CodeNode
    {
        /// <remarks>
        /// Используется в typeof и delete.
        /// </remarks>
        protected sealed class SafeVariableGetter : VariableReference
        {
            internal SafeVariableGetter(GetVariableStatement gvs)
            {
                functionDepth = gvs.functionDepth;
                descriptor = gvs.Descriptor;
                Descriptor.references.Remove(gvs);
                Descriptor.references.Add(this);
                Position = gvs.Position;
                Length = gvs.Length;
            }

            public override string Name
            {
                get { return descriptor.name; }
            }

            internal override JSObject Evaluate(Context context)
            {
                return Descriptor.Get(context, false, functionDepth);
            }

            internal override JSObject EvaluateForAssing(Context context)
            {
                return Descriptor.Get(context, false, functionDepth);
            }

            public override string ToString()
            {
                return Descriptor.name;
            }
        }

        internal readonly JSObject tempContainer;

        protected CodeNode first;
        protected CodeNode second;

        public CodeNode FirstOperand { get { return first; } }
        public CodeNode SecondOperand { get { return second; } }

        public virtual bool IsContextIndependent
        {
            get
            {
                return (first == null || first is Constant || (first is Expression && (first as Expression).IsContextIndependent))
                    && (second == null || second is Constant || (second is Expression && (second as Expression).IsContextIndependent));
            }
        }

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

        protected Expression(CodeNode first, CodeNode second, bool createTempContainer)
        {
            if (createTempContainer)
                tempContainer = new JSObject() { attributes = JSObjectAttributesInternal.Temporary };
            this.first = first;
            this.second = second;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            //while (first is None && (first as None).second == null)
            //    first = (first as None).first;
            //while (second is None && (second as None).second == null)
            //    second = (second as None).first;
            Parser.Optimize(ref first, depth + 1, vars, strict);
            Parser.Optimize(ref second, depth + 1, vars, strict);
            try
            {
                if (this.IsContextIndependent)
                {
                    var res = this.Evaluate(null);
                    if (res.valueType == JSObjectType.Double
                        && !double.IsNegativeInfinity(1.0 / res.dValue)
                        && res.dValue == (double)(int)res.dValue)
                    {
                        res.iValue = (int)res.dValue;
                        res.valueType = JSObjectType.Int;
                    }
                    _this = new Constant(res);
                    return true;
                }
            }
            catch
            { }
            return false;
        }

        protected override CodeNode[] getChildsImpl()
        {
            if (first != null && second != null)
                return new[]{
                    first,
                    second
                };
            if (first != null)
                return new[]{
                    first
                };
            if (second != null)
                return new[]{
                    second
                };
            return null;
        }
    }
}