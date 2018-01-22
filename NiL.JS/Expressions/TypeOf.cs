using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class TypeOf : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.String;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public TypeOf(Expression first)
            : base(first, null, false)
        {
            if (_right != null)
                throw new InvalidOperationException("Second operand not allowed for typeof operator/");
        }

        public override JSValue Evaluate(Context context)
        {
            var val = _left.Evaluate(context);
            switch (val._valueType)
            {
                case JSValueType.Integer:
                case JSValueType.Double:
                    {
                        return JSValue.numberString;
                    }
                case JSValueType.NotExists:
                case JSValueType.NotExistsInObject:
                case JSValueType.Undefined:
                    {
                        return JSValue.undefinedString;
                    }
                case JSValueType.String:
                    {
                        return JSValue.stringString;
                    }
                case JSValueType.Symbol:
                    {
                        return JSValue.symbolString;
                    }
                case JSValueType.Boolean:
                    {
                        return JSValue.booleanString;
                    }
                case JSValueType.Function:
                    {
                        return JSValue.functionString;
                    }
                default:
                    {
                        return JSValue.objectString;
                    }
            }
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            base.Build(ref _this, expressionDepth,  variables, codeContext, message, stats, opts);
            if (_left is Variable)
                (_left as Variable)._SuspendThrow = true;
            return false;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "typeof " + _left;
        }
    }
}