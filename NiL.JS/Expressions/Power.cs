using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Power : Expression
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

        public Power(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            _tempContainer._dValue = Math.Pow(Tools.JSObjectToDouble(_left.Evaluate(context)), Tools.JSObjectToDouble(_right.Evaluate(context)));
            _tempContainer._valueType = JSValueType.Double;
            return _tempContainer;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            var res = base.Build(ref _this, expressionDepth, variables, codeContext, message, stats, opts);
            if (!res && _this == this)
            {
                try
                {
                    if (_left.ContextIndependent)
                        _left = new Constant(Tools.JSObjectToDouble((_left).Evaluate(null)));

                    if (_right.ContextIndependent)
                    {
                        if (_left.ContextIndependent)
                            _this = new Constant(Math.Pow(Tools.JSObjectToDouble(_left.Evaluate(null)), Tools.JSObjectToDouble(_right.Evaluate(null))));
                        else
                        {
                            var value = Tools.JSObjectToInt32((_right).Evaluate(null));
                            if (value == 0)
                                _this = new Constant(1);
                            else if (value == 1)
                                _this = _left;
                            else if (value == 2)
                                _this = new Multiplication(_left, _left);
                            else
                                _right = new Constant(value);
                        }
                    }
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
            return "(" + _left + " ** " + _right + ")";
        }
    }
}