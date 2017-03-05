using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    public sealed class AwaitExpression : Expression
    {
        public AwaitExpression(Expression source)
            : base(source, null, false)
        {
        }

        public override JSValue Evaluate(Context context)
        {
            throw new NotImplementedException();
        }

        public static CodeNode Parse(ParseInfo state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "await", ref i) || !Parser.IsIdentifierTerminator(state.Code[i]))
                return null;

            if ((state.CodeContext & CodeContext.InAsync) == 0)
                ExceptionHelper.ThrowSyntaxError("await is not allowed in this context", state.Code, index, "await".Length);

            Tools.SkipSpaces(state.Code, ref i);

            var source = Parser.Parse(state, ref i, CodeFragmentType.Expression) as Expression;
            if (source == null)
                ExceptionHelper.ThrowSyntaxError("Expression missed", state.Code, i);

            return new AwaitExpression(source);
        }
    }
}
