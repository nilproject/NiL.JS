using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class TypeOf : Expression
    {
        private static readonly JSObject numberString = "number";
        private static readonly JSObject undefinedString = "undefined";
        private static readonly JSObject stringString = "string";
        private static readonly JSObject booleanString = "boolean";
        private static readonly JSObject functionString = "function";
        private static readonly JSObject objectString = "object";

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.String;
            }
        }

        public TypeOf(Expression first)
            : base(first, null, false)
        {
            if (second != null)
                throw new InvalidOperationException("Second operand not allowed for typeof operator/");
        }

        internal override JSObject Evaluate(Context context)
        {
            var val = first.Evaluate(context);
            switch (val.valueType)
            {
                case JSObjectType.Int:
                case JSObjectType.Double:
                    {
                        return numberString;
                    }
                case JSObjectType.NotExists:
                case JSObjectType.NotExistsInObject:
                case JSObjectType.Undefined:
                    {
                        return undefinedString;
                    }
                case JSObjectType.String:
                    {
                        return stringString;
                    }
                case JSObjectType.Bool:
                    {
                        return booleanString;
                    }
                case JSObjectType.Function:
                    {
                        return functionString;
                    }
                default:
                    {
                        return objectString;
                    }
            }
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict, CompilerMessageCallback message, FunctionStatistic statistic, Options opts)
        {
            base.Build(ref _this, depth, vars, strict, message, statistic, opts);
            if (first is GetVariableExpression)
                (first as GetVariableExpression).suspendThrow = true;
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