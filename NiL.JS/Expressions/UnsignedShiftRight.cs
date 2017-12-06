using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class UnsignedShiftRight : Expression
    {
        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Number;
            }
        }

        public UnsignedShiftRight(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            var left = (uint)Tools.JSObjectToInt32(_left.Evaluate(context));
            var t = left >> Tools.JSObjectToInt32(_right.Evaluate(context));
            if (t <= int.MaxValue)
            {
                _tempContainer._iValue = (int)t;
                _tempContainer._valueType = JSValueType.Integer;
            }
            else
            {
                _tempContainer._dValue = (double)t;
                _tempContainer._valueType = JSValueType.Double;
            }
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
                        _this = new ConvertToUnsignedInteger(_left);
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
            return "(" + _left + " >>> " + _right + ")";
        }
    }
}