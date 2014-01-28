using NiL.JS.Core;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements
{
    internal class CodeBlock : Statement, IOptimizable
    {
        private FunctionStatement[] functions;
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
            functions = new FunctionStatement[0];
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
            var funcs = new List<FunctionStatement>();
            state.LabelCount = 0;
            while (code[i] != '}')
            {
                var t = Parser.Parse(state, ref i, 0);
                if (t == null || t is EmptyStatement)
                    continue;
                if (t is FunctionStatement)
                    funcs.Add(t as FunctionStatement);
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

        public override JSObject Invoke(Context context)
        {
            for (int i = varibles.Length - 1; i >= 0; i--)
                context.Define(varibles[i]);
            for (int i = functions.Length - 1; i >= 0; i--)
            {
                var o = context.Define((functions[i] as FunctionStatement).Name);
                o.assignCallback = null;
                o.Assign(functions[i].Invoke(context));
                o.assignCallback = JSObject.ErrorAssignCallback;
            }
            JSObject res = JSObject.undefined;
            for (int i = length; i >= 0; i--)
            {
                res = Tools.RaiseIfNotExist(body[i].Invoke(context)) ?? res;
                if (context.abort != AbortType.None)
                    return context.abort == AbortType.Return ? context.abortInfo : res;
            }
            return res;
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            var vars = new Dictionary<string, Statement>();
            for (int i = 0; i < body.Length; i++)
                Parser.Optimize(ref body[i], depth < 0 ? 2 : 1, vars);
            for (int i = 0; i < functions.Length; i++)
            {
                Statement stat = functions[i];
                Parser.Optimize(ref stat, 1, vars);
                functions[i] = stat as FunctionStatement;
            }

            for (int i = functions.Length - 1; i >= 0; i--)
                vars.Remove((functions[i] as FunctionStatement).Name);

            if (depth > 0)
            {
                foreach (var v in vars)
                    if (v.Value != null || !varibles.ContainsKey(v.Key))
                        varibles[v.Key] = v.Value;
                foreach (var f in functions)
                    varibles[f.Name] = f;
                if (body.Length == 1)
                    _this = body[0];
            }
            else
            {
                List<string> cvars = new List<string>(this.varibles);
                List<FunctionStatement> funcs = new List<FunctionStatement>(this.functions);
                foreach (var v in vars)
                {
                    if (v.Value != null)
                        funcs.Add(v.Value as FunctionStatement);
                    else
                        cvars.Add(v.Key);
                }
                this.functions = funcs.ToArray();
                this.varibles = cvars.ToArray();
            }
            return false;
        }

        public override string ToString()
        {
            if (body == null || body.Length == 0)
                return "{ }";
            string res = "{" + Environment.NewLine;
            var replp = Environment.NewLine + "\t";
            var replt = Environment.NewLine + "\t\t";
            for (int i = body.Length; i-- > 0; )
                res += "\t" + body[i].ToString().Replace(replp, replt) + ";" + Environment.NewLine;
            return res + "}";
        }
    }
}