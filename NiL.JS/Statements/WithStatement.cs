using System;
using System.Collections.Generic;
using System.Linq;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    internal sealed class WithStatement : Statement
    {
        private Statement obj;
        private Statement body;
        private VariableDescriptor[] variables;

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
#if DEV
            if (context.debugging)
                context.raiseDebugger(obj);
#endif
            var intcontext = new WithContext(obj.Invoke(context), context);
#if DEV
            if (context.debugging && !(body is CodeBlock))
                context.raiseDebugger(body);
#endif
            try
            {
                intcontext.variables = variables;
                intcontext.Activate();
                body.Invoke(intcontext);
                context.abort = intcontext.abort;
                context.abortInfo = intcontext.abortInfo;
                return JSObject.undefined;
            }
            finally
            {
                intcontext.Deactivate();
            }
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

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            Parser.Optimize(ref obj, depth, variables, strict);
            var nvars = new Dictionary<string, VariableDescriptor>();
            Parser.Optimize(ref body, depth, nvars, strict);
            foreach(var v in nvars.Values)
            {
                VariableDescriptor desc = null;
                if (v.Defined && !variables.TryGetValue(v.Name, out desc))
                    variables[v.Name] = new VariableDescriptor(v.Name, true);
                v.attributes |= VariableDescriptorAttributes.NoCaching;
            }
            this.variables = nvars.Values.ToArray();
            return false;
        }

        public override string ToString()
        {
            return "with (" + obj + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}
