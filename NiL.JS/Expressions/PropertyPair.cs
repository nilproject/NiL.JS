using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class PropertyPair : Expression
    {
        public Expression Getter
        {
            get
            {
                return _left;
            }
            internal set
            {
                _left = value;
            }
        }

        public Expression Setter
        {
            get
            {
                return _right;
            }
            internal set
            {
                _right = value;
            }
        }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        public PropertyPair(Expression getter, Expression setter)
            : base(getter, setter, true)
        {
            _tempContainer._valueType = JSValueType.Property;
        }
        
        public override JSValue Evaluate(Context context)
        {
            _tempContainer._oValue = new Core.PropertyPair
            (
                Getter == null ? null : (Function)Getter.Evaluate(context),
                Setter == null ? null : (Function)Setter.Evaluate(context)
            );
            return _tempContainer;
        }

        public override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            throw new InvalidOperationException();
        }
    }
}
