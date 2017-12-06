
#define TYPE_SAFE

using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Multiplication : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                var pd = _left.ResultType;
                switch (pd)
                {
                    case PredictedType.Double:
                        {
                            return PredictedType.Double;
                        }
                    default:
                        {
                            return PredictedType.Number;
                        }
                }
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public Multiplication(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
#if TYPE_SAFE
            double da = 0.0;
            JSValue f = _left.Evaluate(context);
            JSValue s = null;
            long l = 0;
            if (((int)f._valueType & 0xf) > 3) // bool - b0111, int - b1011
            {
                int a = f._iValue;
                s = _right.Evaluate(context);
                if (((int)s._valueType & 0xf) > 3)
                {
                    if (((a | s._iValue) & 0xFFFF0000) == 0)
                    {
                        _tempContainer._iValue = a * s._iValue;
                        _tempContainer._valueType = JSValueType.Integer;
                    }
                    else
                    {
                        l = (long)a * s._iValue;
                        if (l > 2147483647L || l < -2147483648L)
                        {
                            _tempContainer._dValue = l;
                            _tempContainer._valueType = JSValueType.Double;
                        }
                        else
                        {
                            _tempContainer._iValue = (int)l;
                            _tempContainer._valueType = JSValueType.Integer;
                        }
                    }
                    return _tempContainer;
                }
                else
                    da = a;
            }
            else
            {
                da = Tools.JSObjectToDouble(f);
                s = _right.Evaluate(context);
            }
            _tempContainer._dValue = da * Tools.JSObjectToDouble(s);
            _tempContainer._valueType = JSValueType.Double;
            return _tempContainer;
#else
            tempResult.dValue = Tools.JSObjectToDouble(first.Invoke(context)) * Tools.JSObjectToDouble(second.Invoke(context));
            tempResult.valueType = JSObjectType.Double;
            return tempResult;
#endif
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            var res = base.Build(ref _this, expressionDepth,  variables, codeContext, message, stats, opts);
            if (!res)
            {
                var exp = _left as Constant;
                if (exp != null
                    && Tools.JSObjectToDouble(exp.Evaluate(null)) == 1.0)
                {
                    _this = new ConvertToNumber(_right);
                    return true;
                }
                exp = _right as Constant;
                if (exp != null
                    && Tools.JSObjectToDouble(exp.Evaluate(null)) == 1.0)
                {
                    _this = new ConvertToNumber(_left);
                    return true;
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
            if (_left is Constant
                && ((_left as Constant).value._valueType == JSValueType.Integer)
                && ((_left as Constant).value._iValue == -1))
                return "-" + _right;
            return "(" + _left + " * " + _right + ")";
        }
    }
}