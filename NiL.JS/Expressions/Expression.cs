using System;
using System.Collections.Generic;
using NiL.JS.Core;
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
                Descriptor = gvs.Descriptor;
                Descriptor.references.Remove(gvs);
                Descriptor.references.Add(this);
                Position = gvs.Position;
                Length = gvs.Length;
            }

            public override string Name
            {
                get { return Descriptor.Name; }
            }

            public override VariableDescriptor Descriptor { get; internal set; }

            internal override JSObject Invoke(Context context)
            {
                return Descriptor.Get(context, false, functionDepth);
            }

            internal override JSObject InvokeForAssing(Context context)
            {
                return Descriptor.Get(context, false, functionDepth);
            }

            public override string ToString()
            {
                return Descriptor.Name;
            }
        }

        internal readonly JSObject tempContainer;

        protected internal CodeNode first;
        protected internal CodeNode second;

        public CodeNode FirstOperand { get { return first; } }
        public CodeNode SecondOperand { get { return second; } }

        public virtual bool IsContextIndependent
        {
            get
            {
                return (first == null || first is ImmidateValueStatement || (first is Expression && (first as Expression).IsContextIndependent)) 
                    && (second == null || second is ImmidateValueStatement || (second is Expression && (second as Expression).IsContextIndependent));
            }
        }

        protected Expression(CodeNode first, CodeNode second, bool createTempContainer)
        {
            if (createTempContainer)
                tempContainer = new JSObject() { assignCallback = JSObject.ErrorAssignCallback };
            this.first = first;
            this.second = second;
        }

        internal override bool Optimize(ref CodeNode _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            while (first is None && (first as None).second == null)
                first = (first as None).first;
            while (second is None && (second as None).second == null)
                second = (second as None).first;
            Parser.Optimize(ref first, depth + 1, fdepth, vars, strict);
            Parser.Optimize(ref second, depth + 1, fdepth, vars, strict);
            try
            {
                if (this.IsContextIndependent)
                {
                    var res = this.Invoke(null);
                    if (res.valueType == JSObjectType.Double
                        && !double.IsNegativeInfinity(1.0 / res.dValue)
                        && res.dValue == (double)(int)res.dValue)
                    {
                        res.iValue = (int)res.dValue;
                        res.valueType = JSObjectType.Int;
                    }
                    _this = new ImmidateValueStatement(res);
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