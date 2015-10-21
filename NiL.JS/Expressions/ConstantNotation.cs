using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ConstantNotation : Expression
    {
        internal JSValue value;

        public JSValue Value { get { return value; } }

        protected internal override PredictedType ResultType
        {
            get
            {
                if (value == null)
                    return PredictedType.Unknown;
                switch (value.valueType)
                {
                    case JSValueType.Undefined:
                    case JSValueType.NotExists:
                    case JSValueType.NotExistsInObject:
                        return PredictedType.Undefined;
                    case JSValueType.Bool:
                        return PredictedType.Bool;
                    case JSValueType.Int:
                        return PredictedType.Int;
                    case JSValueType.Double:
                        return PredictedType.Double;
                    case JSValueType.String:
                        return PredictedType.String;
                    default:
                        return PredictedType.Object;
                }
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public ConstantNotation(JSValue value)
            : base(null, null, false)
        {
            this.value = value;
        }

        public override JSValue Evaluate(Context context)
        {
            return value;
        }

        internal protected override JSValue EvaluateForWrite(NiL.JS.Core.Context context)
        {
            if (value == JSValue.undefined)
                return value;
            return base.EvaluateForWrite(context);
        }

        protected override CodeNode[] getChildsImpl()
        {
            if (value != null && value.oValue is CodeNode[])
                return value.oValue as CodeNode[];
            return null;
        }

        internal protected override bool Build<T>(ref T _this, int depth, Dictionary<string, VariableDescriptor> variables, BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            codeContext = state;

            var vss = value.oValue as CodeNode[];
            if (vss != null)
                throw new InvalidOperationException("It behaviour is deprecated");
            if ((opts & Options.SuppressUselessExpressionsElimination) == 0 && depth <= 1)
            {
                _this = null;
                Eliminated = true;
                if (message != null && (value.valueType != JSValueType.String || value.oValue.ToString() != "use strict"))
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Unused constant was removed. Maybe, something missing.");
            }
            return false;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            if (value == null)
                return "";
            if (value.valueType == JSValueType.String)
                return "\"" + value.oValue + "\"";
            if (value.oValue is CodeNode[])
            {
                string res = "";
                for (var i = (value.oValue as CodeNode[]).Length; i-- > 0; )
                    res = (i != 0 ? ", " : "") + (value.oValue as CodeNode[])[i] + res;
                return res;
            }
            return value.ToString();
        }
    }
}