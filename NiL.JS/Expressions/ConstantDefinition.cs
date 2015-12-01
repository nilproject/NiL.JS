using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ConstantDefinition : Expression
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

        protected internal override bool ContextIndependent
        {
            get
            {
                return true;
            }
        }

        public ConstantDefinition(JSValue value)
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

        protected internal override CodeNode[] getChildsImpl()
        {
            if (value != null && value.oValue is CodeNode[])
                return value.oValue as CodeNode[];
            return null;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            this._codeContext = codeContext;

            if ((opts & Options.SuppressUselessExpressionsElimination) == 0 && expressionDepth <= 1)
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
                for (var i = (value.oValue as CodeNode[]).Length; i-- > 0;)
                    res = (i != 0 ? ", " : "") + (value.oValue as CodeNode[])[i] + res;
                return res;
            }
            return value.ToString();
        }

        public override void Decompose(ref Expression self, IList<CodeNode> result)
        {

        }
    }
}