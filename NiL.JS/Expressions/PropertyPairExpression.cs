using System;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class PropertyPairExpression : Expression
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

        public override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        public PropertyPairExpression(Expression getter, Expression setter)
            : base(getter, setter, true)
        {
            tempContainer.valueType = JSValueType.Property;
        }
        
        public override JSValue Evaluate(Context context)
        {
            tempContainer.oValue = new PropertyPair
            (
                Getter == null ? null : (Function)Getter.Evaluate(context),
                Setter == null ? null : (Function)Setter.Evaluate(context)
            );
            return tempContainer;
        }
    }
}
