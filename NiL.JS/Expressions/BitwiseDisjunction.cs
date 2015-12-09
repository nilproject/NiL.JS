using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class BitwiseDisjunction : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Int;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public BitwiseDisjunction(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            tempContainer.iValue = Tools.JSObjectToInt32(first.Evaluate(context)) | Tools.JSObjectToInt32(second.Evaluate(context));
            tempContainer.valueType = JSValueType.Int;
            return tempContainer;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            var res = base.Build(ref _this, expressionDepth,  variables, codeContext, message, stats, opts);
            if (_this != this)
                return res;
            if (second.ContextIndependent
                && Tools.JSObjectToInt32(second.Evaluate(null)) == 0)
            {
                _this = new ConvertToInteger(first);
                return true;
            }
            if (first.ContextIndependent
                 && Tools.JSObjectToInt32(first.Evaluate(null)) == 0)
            {
                _this = new ConvertToInteger(second);
                return true;
            }
            return res;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " | " + second + ")";
        }
    }
}