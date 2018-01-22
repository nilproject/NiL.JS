using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class SignedShiftRight : Expression
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

        public SignedShiftRight(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            _tempContainer._iValue = Tools.JSObjectToInt32(_left.Evaluate(context)) >> Tools.JSObjectToInt32(_right.Evaluate(context));
            _tempContainer._valueType = JSValueType.Integer;
            return _tempContainer;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            var res = base.Build(ref _this, expressionDepth,  variables, codeContext, message, stats, opts);
            if (!res && _this == this)
            {
                try
                {
                    if (_left.ContextIndependent && Tools.JSObjectToInt32((_left).Evaluate(null)) == 0)
                        _this = new Constant(0);
                    else if (_right.ContextIndependent && Tools.JSObjectToInt32((_right).Evaluate(null)) == 0)
                        _this = new ConvertToInteger(_left);
                }
                catch
                {

                }
            }
            return res;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + _left + " >> " + _right + ")";
        }
    }
}