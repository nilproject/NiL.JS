using System;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using System.Collections.Generic;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class InstanceOf : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public InstanceOf(Expression first, Expression second)
            : base(first, second, true)
        {
        }

        public override JSValue Evaluate(Context context)
        {
            var a = _tempContainer ?? new JSValue { _attributes = JSValueAttributesInternal.Temporary };
            _tempContainer = null;
            a.Assign(_left.Evaluate(context));
            var c = _right.Evaluate(context);
            _tempContainer = a;
            if (c._valueType != JSValueType.Function)
                ExceptionHelper.Throw(new TypeError("Right-hand value of instanceof is not a function."));

            if (a._valueType < JSValueType.Object)
                return false;

            var p = (c._oValue as Function).prototype;
            if (p._valueType < JSValueType.Object || p.IsNull)
                ExceptionHelper.Throw(new TypeError("Property \"prototype\" of function not represent object."));

            if (p._oValue != null)
            {
                while (a != null && a._valueType >= JSValueType.Object && a._oValue != null)
                {
                    if (a._oValue == p._oValue)
                        return true;
                    a = a.__proto__;
                }
            }

            return false;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            var res = base.Build(ref _this, expressionDepth,  variables, codeContext, message, stats, opts);
            if (!res)
            {
                if (_left is Constant)
                {
                    // TODO
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
            return "(" + _left + " instanceof " + _right + ")";
        }
    }
}