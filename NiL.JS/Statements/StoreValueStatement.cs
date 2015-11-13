using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
    public sealed class StoreValueStatement : CodeNode
    {
        private readonly Expression _source;

        public override int Position
        {
            get
            {
                return _source.Position;
            }
            internal set
            {
                _source.Position = value;
            }
        }

        public override int Length
        {
            get
            {
                return _source.Length;
            }
            internal set
            {
                _source.Length = value;
            }
        }

        public StoreValueStatement(Expression source)
        {
            _source = source;
        }

        public override JSValue Evaluate(Context context)
        {
            var scontext = context as SuspendableContext;
            if (scontext == null)
                throw new ArgumentException("context must be " + typeof(SuspendableContext).Name);

            scontext.SuspendData[_source] = _source.Evaluate(context).CloneImpl(false);
            return null;
        }

        public override string ToString()
        {
            return _source.ToString();
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            return _source.getChildsImpl();
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return _source.Visit<T>(visitor);
        }
    }
}
