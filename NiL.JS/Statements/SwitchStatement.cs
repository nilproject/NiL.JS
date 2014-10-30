using System;
using System.Collections.Generic;
using System.Linq;
using NiL.JS.Core;
using NiL.JS.Core.TypeProxing;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class SwitchCase
    {
        internal int index;
        internal CodeNode statement;

        public int Index { get { return index; } }
        public CodeNode Statement { get { return statement; } }
    }

    [Serializable]
    public sealed class SwitchStatement : CodeNode
    {
        private FunctionStatement[] functions;
        private readonly CodeNode[] body;
        private SwitchCase[] cases;
        private CodeNode image;

        public FunctionStatement[] Functions { get { return functions; } }
        public CodeNode[] Body { get { return body; } }
        public SwitchCase[] Cases { get { return cases; } }
        public CodeNode Image { get { return image; } }

        internal SwitchStatement(CodeNode[] body)
        {
            this.body = body;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            if (!Parser.Validate(state.Code, "switch (", ref i) && !Parser.Validate(state.Code, "switch(", ref i))
                return new ParseResult();
            while (char.IsWhiteSpace(state.Code[i])) i++;
            var image = ExpressionStatement.Parse(state, ref i).Statement;
            if (state.Code[i] != ')')
                throw new JSException((new Core.BaseTypes.SyntaxError("Expected \")\" at + " + Tools.PositionToTextcord(state.Code, i))));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            if (state.Code[i] != '{')
                throw new JSException((new Core.BaseTypes.SyntaxError("Expected \"{\" at + " + Tools.PositionToTextcord(state.Code, i))));
            do i++; while (char.IsWhiteSpace(state.Code[i]));
            var body = new List<CodeNode>();
            var funcs = new List<FunctionStatement>();
            var cases = new List<SwitchCase>();
            cases.Add(null);
            state.AllowBreak.Push(true);
            while (state.Code[i] != '}')
            {
                do
                {
                    if (Parser.Validate(state.Code, "case", i) && Parser.isIdentificatorTerminator(state.Code[i + 4]))
                    {
                        i += 4;
                        while (char.IsWhiteSpace(state.Code[i])) i++;
                        var sample = ExpressionStatement.Parse(state, ref i).Statement;
                        if (state.Code[i] != ':')
                            throw new JSException((new Core.BaseTypes.SyntaxError("Expected \":\" at + " + Tools.PositionToTextcord(state.Code, i))));
                        i++;
                        cases.Add(new SwitchCase() { index = body.Count, statement = sample });
                    }
                    else if (Parser.Validate(state.Code, "default", i) && Parser.isIdentificatorTerminator(state.Code[i + 7]))
                    {
                        if (cases[0] != null)
                            throw new JSException((new Core.BaseTypes.SyntaxError("Duplicate default case in switch at " + Tools.PositionToTextcord(state.Code, i))));
                        i += 7;
                        if (state.Code[i] != ':')
                            throw new JSException((new Core.BaseTypes.SyntaxError("Expected \":\" at + " + Tools.PositionToTextcord(state.Code, i))));
                        i++;
                        cases[0] = new SwitchCase() { index = body.Count, statement = null };
                    }
                    else break;
                    while (char.IsWhiteSpace(state.Code[i]) || (state.Code[i] == ';')) i++;
                } while (true);
                if (cases.Count == 1 && cases[0] == null)
                    throw new JSException((new Core.BaseTypes.SyntaxError("Switch statement must be contain cases. " + Tools.PositionToTextcord(state.Code, index))));
                var t = Parser.Parse(state, ref i, 0);
                if (t == null)
                    continue;
                if (t is FunctionStatement)
                {
                    if (state.strict.Peek())
                        throw new JSException((new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                    funcs.Add(t as FunctionStatement);
                }
                else
                    body.Add(t);
                while (char.IsWhiteSpace(state.Code[i]) || (state.Code[i] == ';')) i++;
            }
            state.AllowBreak.Pop();
            i++;
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new SwitchStatement(body.ToArray())
                {
                    functions = funcs.ToArray(),
                    cases = cases.ToArray(),
                    image = image,
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Evaluate(Context context)
        {
            if (functions != null)
                throw new InvalidOperationException();
            int i = cases[0] != null ? cases[0].index : 0;
            var imageVal = image.Evaluate(context);
            for (int j = 1; j < cases.Length; j++)
            {
#if DEV
                if (context.debugging)
                    context.raiseDebugger(cases[j].statement);
#endif
                if (Expressions.StrictEqual.Check(imageVal, cases[j].statement.Evaluate(context), context))
                {
                    i = cases[j].index;
                    break;
                }
            }
            while (i-- > 0)
            {
                body[i].Evaluate(context);
                if (context.abort != AbortType.None)
                {
                    if (context.abort == AbortType.Break)
                        context.abort = AbortType.None;
                    return context.abortInfo;
                }
            }
            return JSObject.undefined;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            if (depth < 1)
                throw new InvalidOperationException();
            Parser.Build(ref image, 2, variables, strict);
            for (int i = 0; i < body.Length; i++)
                Parser.Build(ref body[i], 1, variables, strict);
            for (int i = 0; i < functions.Length; i++)
            {
                CodeNode stat = functions[i];
                Parser.Build(ref stat, 1, variables, strict);
                variables[functions[i].Name] = new VariableDescriptor(functions[i].Reference, true, functions[i].Reference.functionDepth);
            }
            functions = null;
            for (int i = 1; i < cases.Length; i++)
                Parser.Build(ref cases[i].statement, 2, variables, strict);
            for (int i = 0; i < body.Length / 2; i++)
            {
                var t = body[i];
                body[i] = body[body.Length - 1 - i];
                body[body.Length - 1 - i] = t;
            }
            for (int i = cases[0] != null ? 0 : 1; i < cases.Length; i++)
                cases[i].index = body.Length - cases[i].index;
            return false;
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                image
            };
            res.AddRange(body);
            res.AddRange(functions);
            res.AddRange(from c in cases select c.statement);
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        public override string ToString()
        {
            string res = "switch (" + image + ") {" + Environment.NewLine;
            var replp = Environment.NewLine;
            var replt = Environment.NewLine + "  ";
            for (int i = body.Length; i-- > 0; )
            {
                for (int j = 0; j < cases.Length; j++)
                {
                    if (cases[j] != null && cases[j].index == i)
                    {
                        res += "case " + cases[j].statement + ":" + Environment.NewLine;
                    }
                }
                string lc = body[i].ToString().Replace(replp, replt);
                res += "  " + lc + (lc[lc.Length - 1] != '}' ? ";" + Environment.NewLine : Environment.NewLine);
            }
            if (functions != null)
                for (var i = 0; i < functions.Length; i++)
                {
                    var func = functions[i].ToString().Replace(replp, replt);
                    res += "  " + func + Environment.NewLine;
                }
            return res + "}";
        }
    }
}