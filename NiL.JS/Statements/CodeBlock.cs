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
        public readonly bool strict;

        public CodeBlock(Statement[] body, bool strict)
        {
            this.body = body;
            length = body.Length - 1;
            functions = null;
            varibles = new string[0];
            this.strict = strict;
        }

        public static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (code[i] != '{')
                throw new ArgumentException("code (" + i + ")");
            do
                i++;
            while (char.IsWhiteSpace(code[i]));
            var body = new List<Statement>();
            var funcs = new List<FunctionStatement>();
            state.LabelCount = 0;
            bool strictSwitch = false;
            bool allowStrict = state.AllowStrict;
            bool root = state.AllowStrict;
            state.AllowStrict = false;
            while (code[i] != '}')
            {
                var t = Parser.Parse(state, ref i, 0);
                if (allowStrict)
                {
                    allowStrict = false;
                    if (t is OperatorStatement && ((t as OperatorStatement).Type == OperationType.None))
                    {
                        var op = t as OperatorStatement;
                        if (op.Second == null && (op.First is ImmidateValueStatement) && "use strict".Equals((op.First as ImmidateValueStatement).Value.Value))
                        {
                            state.strict.Push(true);
                            strictSwitch = true;
                            continue;
                        }
                    }
                }
                if (t == null || t is EmptyStatement)
                    continue;
                if (t is FunctionStatement)
                {
                    if (state.strict.Peek())
                        if (!root)
                            throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                    funcs.Add(t as FunctionStatement);
                }
                else
                    body.Add(t);
            }
            i++;
            index = i;
            body.Reverse();
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new CodeBlock(body.ToArray(), strictSwitch && state.strict.Pop())
                {
                    functions = funcs.ToArray()
                }
            };
        }

        public override JSObject Invoke(Context context)
        {
            context.strict |= strict;
            for (int i = varibles.Length - 1; i >= 0; i--)
                context.InitField(varibles[i]);
            for (int i = functions.Length - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty((functions[i] as FunctionStatement).Name))
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("Declarated function must have name.")));
                var f = context.InitField((functions[i] as FunctionStatement).Name);
                f.Assign(functions[i].Invoke(context));
                f.assignCallback = null;
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
                Parser.Optimize(ref body[i], depth < 0 ? 2 : Math.Max(1, depth), vars);
            if (functions == null)
                functions = new FunctionStatement[0];
            for (int i = 0; i < functions.Length; i++)
            {
                Statement stat = functions[i];
                Parser.Optimize(ref stat, 1, vars);
                functions[i] = stat as FunctionStatement;
            }

            //for (int i = functions.Length - 1; i >= 0; i--)
            //    vars.Remove((functions[i] as FunctionStatement).Name);

            if (depth > 0)
            {
                foreach (var v in vars)
                    if (v.Value != null || !varibles.ContainsKey(v.Key))
                        varibles[v.Key] = v.Value;
                foreach (var f in functions)
                    varibles[f.Name] = f;
                if (body.Length == 1)
                    _this = body[0];
                if (body.Length == 0)
                    _this = new EmptyStatement();
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
                funcs.Reverse();
                this.functions = funcs.ToArray();
                this.varibles = cvars.ToArray();
            }
            return false;
        }

        public override string ToString()
        {
            if (body == null || body.Length == 0)
                return "{ }";
            string res = " {" + Environment.NewLine;
            var replp = Environment.NewLine;
            var replt = Environment.NewLine + "  ";
            for (var i = 0; i < varibles.Length; i++)
                res += "  var " + varibles[i] + ";" + Environment.NewLine;
            for (var i = 0; i < functions.Length; i++)
            {
                var func = functions[i].ToString().Replace(replp, replt);
                res += "  " + func + Environment.NewLine;
            }
            for (int i = body.Length; i-- > 0; )
            {
                string lc = body[i].ToString().Replace(replp, replt);
                res += "  " + lc + (lc[lc.Length - 1] != '}' ? ";" + Environment.NewLine : Environment.NewLine);
            }
            return res + "}";
        }
    }
}