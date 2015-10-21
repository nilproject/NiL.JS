using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class GetValueForAssignmentOperator : Expression
    {
        private JSValue result;

        public CodeNode Source { get { return first; } }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return first.ResultType;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        internal GetValueForAssignmentOperator(Expression source)
            : base(source, null, false)
        {
        }

        internal protected override JSValue EvaluateForWrite(Context context)
        {
            var res = first.EvaluateForWrite(context);
            if (res.valueType == JSValueType.Property)
                result = (res.oValue as PropertyPair).get != null ? (res.oValue as PropertyPair).get.Invoke(context.objectSource, null) : JSValue.notExists;
            else
                result = res;
            return res;
        }

        public override JSValue Evaluate(Context context)
        {
            var res = result;
            result = null;
            return res;
        }

        public override string ToString()
        {
            return first.ToString();
        }

        public override int EndPosition
        {
            get
            {
                return first.EndPosition;
            }
        }

        public override int Length
        {
            get
            {
                return first.Length;
            }
            internal set
            {
                first.Length = value;
            }
        }

        public override int Position
        {
            get
            {
                return first.Position;
            }
            internal set
            {
                first.Position = value;
            }
        }

        protected override CodeNode[] getChildsImpl()
        {
            return first.Childs;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        internal protected override bool Build<T>(ref T _this, int depth, Dictionary<string, VariableDescriptor> variables, BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            // second будем использовать как флаг isVisited
            if (second != null)
                return false;
            second = first;

            codeContext = state;

            var res = first.Build(ref _this, depth, variables, state | BuildState.InExpression, message, statistic, opts);
            if (!res && first is GetVariableExpression)
                (first as GetVariableExpression).forceThrow = true;
            return res;
        }
    }
}
