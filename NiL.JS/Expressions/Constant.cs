using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Constant : Expression
    {
        internal JSObject value;

        public JSObject Value { get { return value; } }

        protected internal override PredictedType ResultType
        {
            get
            {
                if (value == null)
                    return PredictedType.Unknown;
                switch (value.valueType)
                {
                    case JSObjectType.Undefined:
                    case JSObjectType.NotExists:
                    case JSObjectType.NotExistsInObject:
                        return PredictedType.Undefined;
                    case JSObjectType.Bool:
                        return PredictedType.Bool;
                    case JSObjectType.Int:
                        return PredictedType.Int;
                    case JSObjectType.Double:
                        return PredictedType.Double;
                    case JSObjectType.String:
                        return PredictedType.String;
                    default:
                        return PredictedType.Object;
                }
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public Constant(JSObject value)
            : base(null, null, false)
        {
            this.value = value;
        }

        internal override JSObject Evaluate(Context context)
        {
            return value;
        }

        internal override NiL.JS.Core.JSObject EvaluateForAssing(NiL.JS.Core.Context context)
        {
            if (value == JSObject.undefined)
                return value;
            return base.EvaluateForAssing(context);
        }

        protected override CodeNode[] getChildsImpl()
        {
            if (value != null && value.Value is CodeNode[])
                return value.Value as CodeNode[];
            return null;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            codeContext = state;

            var vss = value.oValue as CodeNode[];
            if (vss != null)
                throw new InvalidOperationException("It behaviour is deprecated");
            if ((opts & Options.SuppressUselessExpressionsElimination) == 0 && depth <= 1)
            {
                _this = null;
                Eliminated = true;
                if (message != null && (value.valueType != JSObjectType.String || value.oValue.ToString() != "use strict"))
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
            if (value.valueType == JSObjectType.String)
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