using NiL.JS.Core;
using System;
using System.Collections.Generic;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public abstract class Operator : Statement
    {
        /// <remarks>
        /// Используется в typeof и delete.
        /// </remarks>
        protected sealed class SafeVaribleGetter : VaribleReference
        {
            private VaribleDescriptor desc;

            internal SafeVaribleGetter(GetVaribleStatement gvs)
            {
                desc = gvs.Descriptor;
                desc.Remove(gvs);
                desc.Add(this);
                Position = gvs.Position;
                Length = gvs.Length;
            }

            public override string Name
            {
                get { return desc.Name; }
            }

            public override VaribleDescriptor Descriptor { get; internal set; }

            internal override JSObject Invoke(Context context)
            {
                return desc.Get(context, false);
            }

            internal override JSObject InvokeForAssing(Context context)
            {
                return desc.Get(context, false);
            }

            public override string ToString()
            {
                return desc.Name;
            }
        }

        internal readonly JSObject tempResult;

        protected internal Statement first;
        protected internal Statement second;

        public Statement FirstOperand { get { return first; } }
        public Statement SecondOperand { get { return second; } }

        public virtual bool IsContextIndependent
        {
            get
            {
                return (first == null || first is ImmidateValueStatement || (first is Operator && (first as Operator).IsContextIndependent)) 
                    && (second == null || second is ImmidateValueStatement || (second is Operator && (second as Operator).IsContextIndependent));
            }
        }

        protected Operator(Statement first, Statement second, bool createResultContainer)
        {
            if (createResultContainer)
                tempResult = new JSObject() { attributes = JSObjectAttributes.DoNotDelete, assignCallback = JSObject.ErrorAssignCallback };
            this.first = first;
            this.second = second;
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VaribleDescriptor> vars)
        {
            Parser.Optimize(ref first, depth + 1, vars);
            Parser.Optimize(ref second, depth + 1, vars);
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
                }
            }
            catch
            { }
            return false;
        }

        protected override Statement[] getChildsImpl()
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