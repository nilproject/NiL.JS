
#define TYPE_SAFE

using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Substract : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Number;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public Substract(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            //lock (this)
            {
#if TYPE_SAFE
                double da = 0.0;
                JSValue f = _left.Evaluate(context);
                JSValue s = null;
                long l = 0;
                int a;
                if (f._valueType == JSValueType.Integer
                    || f._valueType == JSValueType.Boolean)
                {
                    a = f._iValue;
                    s = _right.Evaluate(context);
                    if (s._valueType == JSValueType.Integer
                    || s._valueType == JSValueType.Boolean)
                    {
                        l = (long)a - s._iValue;
                        //if (l > 2147483647L
                        //    || l < -2147483648L)
                        if (l != (int)l)
                        {
                            _tempContainer._dValue = l;
                            _tempContainer._valueType = JSValueType.Double;
                        }
                        else
                        {
                            _tempContainer._iValue = (int)l;
                            _tempContainer._valueType = JSValueType.Integer;
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
                _tempContainer._dValue = da - Tools.JSObjectToDouble(s);
                _tempContainer._valueType = JSValueType.Double;
                return _tempContainer;
#else
                tempResult.dValue = Tools.JSObjectToDouble(first.Invoke(context)) - Tools.JSObjectToDouble(second.Invoke(context));
                tempResult.valueType = JSObjectType.Double;
                return tempResult;
#endif
            }
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            var res = base.Build(ref _this, expressionDepth,  variables, codeContext, message, stats, opts);
            if (!res)
            {
                if (_left is Constant && Tools.JSObjectToDouble(_left.Evaluate(null)) == 0.0)
                {
                    _this = new Negation(_right);
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
            return "(" + _left + " - " + _right + ")";
        }
    }
}