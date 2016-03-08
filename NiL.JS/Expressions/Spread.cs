using System;
using System.Linq;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace NiL.JS.Expressions
{
#if !PORTABLE
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
                oValue = first.Evaluate(context).AsIterable().AsEnumerable().ToArray(),
                valueType = JSValueType.SpreadOperatorResult
            };
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            return new CodeNode[] { first };
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            CodeNode f = first;
            var res = first.Build(ref f, expressionDepth,  variables, codeContext, message, stats, opts);
            first = f as Expression ?? first;
            return res;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "..." + first;
        }
    }
}