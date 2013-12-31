using NiL.JS.Core;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements
{
    internal class CodeBlock : Statement, IOptimizable
    {
        internal Statement[] functions;
        private string[] varibles;
        private int length;
        public readonly Statement[] body;

        public CodeBlock(Statement[] body, double fictive)
        {
            this.body = body;
            length = body.Length - 1;
            varibles = new string[0];
        }

        public CodeBlock(Statement[] body)
        {
            this.body = body;
            length = body.Length - 1;
            functions = new Function[0];
            varibles = new string[0];
        }

        public static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != '{')
                throw new ArgumentException("code (" + i + ")");
            do
                i++;
            while (char.IsWhiteSpace(code[i]));
            var body = new List<Statement>();
            var funcs = new List<Statement>();
            state.LabelCount = 0;
            while (code[i] != '}')
            {
                var t = Parser.Parse(state, ref i, 0);
                if (t == null || t is EmptyStatement)
                    continue;
                if (t is Function)
                    funcs.Add(t as Function);
                else if (t is CodeBlock)
                {
                    CodeBlock cb = t as CodeBlock;
                    funcs.AddRange(cb.functions);
                    cb.functions = new Function[0];
                    for (int cbi = cb.body.Length; cbi-- > 0; )
                        body.Add(cb.body[cbi]);
                }
                else if (t is SwitchStatement)
                {
                    SwitchStatement cb = t as SwitchStatement;
                    funcs.AddRange(cb.functions);
                    cb.functions = new Function[0];
                    body.Add(t);
                }
                else
                    body.Add(t);
            };
            i++;
            index = i;
            body.Reverse();
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new CodeBlock(body.ToArray(), 0.0)
                {
                    functions = funcs.ToArray()
                }
            };
        }

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        public override JSObject Invoke(Context context)
        {
            for (int i = functions.Length - 1; i >= 0; i--)
            {
                var o = context.Define((functions[i] as Function).Name);
                o.ValueType = ObjectValueType.Statement;
                o.oValue = functions[i].Implement(context);
            }
            for (int i = varibles.Length - 1; i >= 0; i--)
                context.Define(varibles[i]);
            for (int i = length; i > 0; i--)
            {
                body[i].Invoke(context);
                if (context.abort != AbortType.None)
                    return context.abortInfo;
            }
            if (length >= 0)
                return body[0].Invoke(context);
            return null;
        }

        public override JSObject Invoke(Context context, JSObject _this, JSObject[] args)
        {
            throw new NotImplementedException();
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> varibles)
        {
            var vars = new HashSet<string>();
            for (int i = 0; i < body.Length; i++)
                Parser.Optimize(ref body[i], 1, vars);
            for (int i = 0; i < functions.Length; i++)
                Parser.Optimize(ref functions[i], 1, vars);
            if (depth > 0)
            {
                foreach (var v in vars)
                    varibles.Add(v);
                if (body.Length == 1)
                    _this = body[0];
            }
            else
            {
                this.varibles = new string[vars.Count];
                int i = 0;
                foreach (var v in vars)
                    this.varibles[i++] = v;
            }
            return false;
        }
    }
}