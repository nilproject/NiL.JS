using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class GetSetPropertyPair : Expression
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

        public GetSetPropertyPair(Expression getter, Expression setter)
            : base(getter, setter, true)
        {
            tempContainer._valueType = JSValueType.Property;
        }
        
        public override JSValue Evaluate(Context context)
        {
            tempContainer._oValue = new Core.GsPropertyPair
            (
                Getter == null ? null : (Function)Getter.Evaluate(context),
                Setter == null ? null : (Function)Setter.Evaluate(context)
            );
            return tempContainer;
        }

        public override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            throw new InvalidOperationException();
        }
    }
}
