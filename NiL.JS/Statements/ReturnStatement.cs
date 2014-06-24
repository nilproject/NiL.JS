using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ReturnStatement : Statement
    {
        private Statement body;

        public Statement Body { get { return body; } }

        internal ReturnStatement()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "return", ref i) || !Parser.isIdentificatorTerminator(code[i]))
                return new ParseResult();
            if (state.functionsDepth == 0)
                throw new JSException(TypeProxy.Proxy(new SyntaxError("Invalid use of return statement.")));
            var body = Parser.Parse(state, ref i, 1, true);
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new ReturnStatement()
                {
                    body = body,
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            context.abortInfo = body != null ? body.Invoke(context) : JSObject.undefined;
            context.abort = AbortType.Return;
            return null;
        }

        protected override Statement[] getChildsImpl()
        {
            var res = new List<Statement>()
            {
                body
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Optimize(ref Statement _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            Parser.Optimize(ref body, 2, fdepth, variables, strict);
            return false;
        }

        public override string ToString()
        {
            return "return" + (body != null ? " " + body : "");
        }
    }
}