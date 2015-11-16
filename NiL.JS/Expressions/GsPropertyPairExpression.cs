using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class GsPropertyPairExpression : Expression
    {
        public Expression Getter
        {
            get
            {
                return first;
            }
            internal set
            {
                first = value;
            }
        }

        public Expression Setter
        {
            get
            {
                return second;
            }
            internal set
            {
                second = value;
            }
        }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        public GsPropertyPairExpression(Expression getter, Expression setter)
            : base(getter, setter, true)
        {
            tempContainer.valueType = JSValueType.Property;
        }
        
        public override JSValue Evaluate(Context context)
        {
            tempContainer.oValue = new GsPropertyPair
            (
                Getter == null ? null : (Function)Getter.Evaluate(context),
                Setter == null ? null : (Function)Setter.Evaluate(context)
            );
            return tempContainer;
        }

        protected internal override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            throw new InvalidOperationException();
        }
    }
}
