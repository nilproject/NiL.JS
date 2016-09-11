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
        private static readonly JSValue numberString = "number";
        private static readonly JSValue undefinedString = "undefined";
        private static readonly JSValue stringString = "string";
        private static readonly JSValue symbolString = "symbol";
        private static readonly JSValue booleanString = "boolean";
        private static readonly JSValue functionString = "function";
        private static readonly JSValue objectString = "object";

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
            if (second != null)
                throw new InvalidOperationException("Second operand not allowed for typeof operator/");
        }

        public override JSValue Evaluate(Context context)
        {
            var val = first.Evaluate(context);
            switch (val._valueType)
            {
                case JSValueType.Integer:
                case JSValueType.Double:
                    {
                        return numberString;
                    }
                case JSValueType.NotExists:
                case JSValueType.NotExistsInObject:
                case JSValueType.Undefined:
                    {
                        return undefinedString;
                    }
                case JSValueType.String:
                    {
                        return stringString;
                    }
                case JSValueType.Symbol:
                    {
                        return symbolString;
                    }
                case JSValueType.Boolean:
                    {
                        return booleanString;
                    }
                case JSValueType.Function:
                    {
                        return functionString;
                    }
                default:
                    {
                        return objectString;
                    }
            }
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            base.Build(ref _this, expressionDepth,  variables, codeContext, message, stats, opts);
            if (first is GetVariable)
                (first as GetVariable).suspendThrow = true;
            return false;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "typeof " + first;
        }
    }
}