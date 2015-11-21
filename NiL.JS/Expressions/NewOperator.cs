using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class NewOperator : Expression
    {
        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Object;
            }
        }
        
        internal NewOperator(CallOperator call)
            : base(call, null, false)
        {

        }

        public static CodeNode Parse(ParsingState state, ref int index)
        {
            var i = index;
            if (!Parser.Validate(state.Code, "new", ref i) || !Parser.IsIdentificatorTerminator(state.Code[i]))
                return null;
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            var result = (Expression)ExpressionTree.Parse(state, ref i, true, false, true, true, false, false);
            if (result == null)
            {
                var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                ExceptionsHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
            }
            if (result is CallOperator)
                result = new NewOperator(result as CallOperator) { Position = index, Length = i - index };
            else
            {
                if (state.message != null)
                    state.message(MessageLevel.Warning, CodeCoordinates.FromTextPosition(state.Code, index, 0), "Missed brackets in a constructor invocation.");
                result = new Expressions.NewOperator(new CallOperator(result, new Expression[0]) { Position = result.Position, Length = result.Length }) { Position = index, Length = i - index };
            }
            index = i;
            return result;
        }

        public override JSValue Evaluate(NiL.JS.Core.Context context)
        {
            throw new InvalidOperationException();
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            if (message != null && depth <= 1)
                message(MessageLevel.Warning, new CodeCoordinates(0, Position, 0), "Do not use NewOperator for side effect");

            (first as CallOperator).callMode = CallMode.Construct;
            _this = first;

            return true;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "new " + first.ToString();
        }
    }
}