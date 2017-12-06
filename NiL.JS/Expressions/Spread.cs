using System;
using System.Linq;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Spread : Expression
    {

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Unknown;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public Spread(Expression source)
            : base(source, null, false)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            return new JSObject
            {
                _oValue = _left.Evaluate(context).AsIterable().AsEnumerable().ToArray(),
                _valueType = JSValueType.SpreadOperatorResult
            };
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            return new CodeNode[] { _left };
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            CodeNode f = _left;
            var res = _left.Build(ref f, expressionDepth,  variables, codeContext, message, stats, opts);
            _left = f as Expression ?? _left;
            return res;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "..." + _left;
        }
    }
}