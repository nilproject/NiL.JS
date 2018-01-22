using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class New : Expression
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

        internal New(Call call)
            : base(call, null, false)
        {

        }

        public static CodeNode Parse(ParseInfo state, ref int index)
        {
            var i = index;
            if (!Parser.Validate(state.Code, "new", ref i) || !Parser.IsIdentifierTerminator(state.Code[i]))
                return null;
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            var result = (Expression)ExpressionTree.Parse(state, ref i, true, false, true, true, false);
            if (result == null)
            {
                var cord = CodeCoordinates.FromTextPosition(state.Code, i, 0);
                ExceptionHelper.Throw((new SyntaxError("Invalid prefix operation. " + cord)));
            }
            if (result is Call)
                result = new New(result as Call) { Position = index, Length = i - index };
            else
            {
                if (state.message != null)
                    state.message(MessageLevel.Warning, index, 0, "Missed brackets in a constructor invocation.");
                result = new Expressions.New(new Call(result, new Expression[0]) { Position = result.Position, Length = result.Length }) { Position = index, Length = i - index };
            }
            index = i;
            return result;
        }

        public override JSValue Evaluate(NiL.JS.Core.Context context)
        {
            throw new InvalidOperationException();
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (message != null && expressionDepth <= 1)
                message(MessageLevel.Warning, Position, 0, "Do not use NewOperator for side effect");

            (_left as Call)._callMode = CallMode.Construct;
            _this = _left;

            return true;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "new " + _left.ToString();
        }
    }
}