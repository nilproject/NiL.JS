using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    internal sealed class WithStatement : Statement
    {
        private Statement obj;
        private Statement body;

        public Statement Body { get { return body; } }
        public Statement Scope { get { return obj; } }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "with (", ref i) && !Parser.Validate(code, "with(", ref i))
                return new ParseResult();
            if (state.strict.Peek())
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("WithStatement is not allowed in strict mode.")));
            var obj = Parser.Parse(state, ref i, 1);
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != ')')
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("Invalid syntax WithStatement.")));
            do i++; while (char.IsWhiteSpace(code[i]));
            var body = Parser.Parse(state, ref i, 0);
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new WithStatement()
                {
                    obj = obj,
                    body = body,
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            if (context.debugging)
                context.raiseDebugger(obj);
            var intcontext = new WithContext(obj.Invoke(context), context, this);
            if (context.debugging && !(body is CodeBlock))
                context.raiseDebugger(body);
            body.Invoke(intcontext);
            context.abort = intcontext.abort;
            context.abortInfo = intcontext.abortInfo;
            return JSObject.undefined;
        }

        protected override Statement[] getChildsImpl()
        {
            var res = new List<Statement>()
            {
                body,
                obj
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VaribleDescriptor> varibles)
        {
            Parser.Optimize(ref obj, depth, varibles);
            var nvars = new Dictionary<string, VaribleDescriptor>();
            Parser.Optimize(ref body, depth, nvars);
            foreach(var v in nvars)
            {
                if (v.Value.Defined)
                    varibles[v.Key] = v.Value;
            }
            return false;
        }

        public override string ToString()
        {
            return "with (" + obj + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}
