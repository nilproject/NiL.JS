using System;
using System.Collections.Generic;
using System.Linq;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class SwitchCase
    {
        internal int index;
        internal CodeNode statement;

        public int Index { get { return index; } }
        public CodeNode Statement { get { return statement; } }
    }

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Switch : CodeNode
    {
        private sealed class SuspendData
        {
            public JSValue imageValue;
            public int caseIndex;
            public int lineIndex;
        }

        private FunctionDefinition[] functions;
        private CodeNode[] lines;
        private SwitchCase[] cases;
        private CodeNode image;

        public FunctionDefinition[] Functions { get { return functions; } }
        public CodeNode[] Body { get { return lines; } }
        public SwitchCase[] Cases { get { return cases; } }
        public CodeNode Image { get { return image; } }

        internal Switch(CodeNode[] body)
        {
            this.lines = body;
        }

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "switch (", ref i) && !Parser.Validate(state.Code, "switch(", ref i))
                return null;

            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;

            var body = new List<CodeNode>();
            var funcs = new List<FunctionDefinition>();
            var cases = new List<SwitchCase>();
            CodeNode result = null;
            cases.Add(new SwitchCase() { index = int.MaxValue });
            state.AllowBreak.Push(true);
            var oldVariablesCount = state.Variables.Count;
            VariableDescriptor[] vars = null;
            state.lexicalScopeLevel++;
            try
            {
                var image = ExpressionTree.Parse(state, ref i);

                if (state.Code[i] != ')')
                    ExceptionHelper.Throw((new SyntaxError("Expected \")\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));

                do
                    i++;
                while (Tools.IsWhiteSpace(state.Code[i]));

                if (state.Code[i] != '{')
                    ExceptionHelper.Throw((new SyntaxError("Expected \"{\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));

                do
                    i++;
                while (Tools.IsWhiteSpace(state.Code[i]));

                while (state.Code[i] != '}')
                {
                    do
                    {
                        if (Parser.Validate(state.Code, "case", i) && Parser.IsIdentifierTerminator(state.Code[i + 4]))
                        {
                            i += 4;
                            while (Tools.IsWhiteSpace(state.Code[i]))
                                i++;
                            var sample = ExpressionTree.Parse(state, ref i);
                            if (state.Code[i] != ':')
                                ExceptionHelper.Throw((new SyntaxError("Expected \":\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                            i++;
                            cases.Add(new SwitchCase() { index = body.Count, statement = sample });
                        }
                        else if (Parser.Validate(state.Code, "default", i) && Parser.IsIdentifierTerminator(state.Code[i + 7]))
                        {
                            i += 7;
                            while (Tools.IsWhiteSpace(state.Code[i]))
                                i++;
                            if (cases[0].index != int.MaxValue)
                                ExceptionHelper.Throw((new SyntaxError("Duplicate default case in switch at " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                            if (state.Code[i] != ':')
                                ExceptionHelper.Throw((new SyntaxError("Expected \":\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                            i++;
                            cases[0].index = body.Count;
                        }
                        else
                            break;
                        while (Tools.IsWhiteSpace(state.Code[i]) || (state.Code[i] == ';'))
                            i++;
                    } while (true);
                    if (cases.Count == 1 && cases[0].index == int.MaxValue)
                        ExceptionHelper.Throw((new SyntaxError("Switch statement must contain cases. " + CodeCoordinates.FromTextPosition(state.Code, index, 0))));

                    var t = Parser.Parse(state, ref i, 0);
                    if (t == null)
                        continue;

                    body.Add(t);
                    while (Tools.IsWhiteSpace(state.Code[i]) || (state.Code[i] == ';'))
                        i++;
                }
                state.AllowBreak.Pop();
                i++;
                var pos = index;
                index = i;
                result = new Switch(body.ToArray())
                {
                    functions = funcs.ToArray(),
                    cases = cases.ToArray(),
                    image = image,
                    Position = pos,
                    Length = index - pos
                };
                vars = CodeBlock.extractVariables(state, oldVariablesCount);
            }
            finally
            {
                state.lexicalScopeLevel--;
            }

            return new CodeBlock(new[] { result })
            {
                _variables = vars,
                Position = result.Position,
                Length = result.Length
            };
        }

        public override JSValue Evaluate(Context context)
        {
#if DEBUG
            if (functions != null)
                throw new InvalidOperationException();
#endif
            JSValue imageValue = null;
            int caseIndex = 1;
            int lineIndex = cases[0].index;

            if (context._executionMode >= ExecutionMode.Resume)
            {
                var sdata = context.SuspendData[this] as SuspendData;
                if (sdata.imageValue == null)
                    imageValue = image.Evaluate(context);
                else
                    imageValue = sdata.imageValue;
                caseIndex = sdata.caseIndex;
                lineIndex = sdata.lineIndex;
            }
            else
            {
                if (context._debugging)
                    context.raiseDebugger(image);

                imageValue = image.Evaluate(context);
            }
            if (context._executionMode == ExecutionMode.Suspend)
            {
                context.SuspendData[this] = new SuspendData() { caseIndex = 1 };
                return null;
            }

            for (; caseIndex < cases.Length; caseIndex++)
            {
                if (context._debugging)
                    context.raiseDebugger(cases[caseIndex].statement);

                var cseResult = cases[caseIndex].statement.Evaluate(context);
                if (context._executionMode == ExecutionMode.Suspend)
                {
                    context.SuspendData[this] = new SuspendData()
                    {
                        caseIndex = caseIndex,
                        imageValue = imageValue
                    };
                    return null;
                }

                if (Expressions.StrictEqual.Check(imageValue, cseResult))
                {
                    lineIndex = cases[caseIndex].index;
                    caseIndex = cases.Length;
                    break;
                }
            }
            for (; lineIndex < lines.Length; lineIndex++)
            {
                if (lines[lineIndex] == null)
                    continue;

                context._lastResult = lines[lineIndex].Evaluate(context) ?? context._lastResult;
                if (context._executionMode != ExecutionMode.None)
                {
                    if (context._executionMode == ExecutionMode.Break)
                    {
                        context._executionMode = ExecutionMode.None;
                    }
                    else if (context._executionMode == ExecutionMode.Suspend)
                    {
                        context.SuspendData[this] = new SuspendData()
                        {
                            caseIndex = caseIndex,
                            imageValue = imageValue,
                            lineIndex = lineIndex
                        };
                    }
                    return null;
                }
            }
            return null;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (expressionDepth < 1)
                throw new InvalidOperationException();
            Parser.Build(ref image, 2, variables, codeContext | CodeContext.InExpression, message, stats, opts);
            for (int i = 0; i < lines.Length; i++)
                Parser.Build(ref lines[i], 1, variables, codeContext | CodeContext.Conditional, message, stats, opts);
            for (int i = 0; functions != null && i < functions.Length; i++)
            {
                CodeNode stat = functions[i];
                Parser.Build(ref stat, 1, variables, codeContext, message, stats, opts);
            }
            functions = null;
            for (int i = 1; i < cases.Length; i++)
                Parser.Build(ref cases[i].statement, 2, variables, codeContext, message, stats, opts);
            return false;
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                image
            };
            res.AddRange(lines);
            if (functions != null && functions.Length > 0)
                res.AddRange(functions);
            if (cases.Length > 0)
                res.AddRange(from c in cases where c != null select c.statement);
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        public override void Optimize(ref CodeNode _this, Expressions.FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            image.Optimize(ref image, owner, message, opts, stats);
            for (var i = 1; i < cases.Length; i++)
                cases[i].statement.Optimize(ref cases[i].statement, owner, message, opts, stats);
            for (var i = lines.Length; i-- > 0;)
            {
                if (lines[i] == null)
                    continue;
                var cn = lines[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, stats);
                lines[i] = cn;
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            string res = "switch (" + image + ") {" + Environment.NewLine;
            var replp = Environment.NewLine;
            var replt = Environment.NewLine + "  ";
            for (int i = lines.Length; i-- > 0;)
            {
                for (int j = 0; j < cases.Length; j++)
                {
                    if (cases[j] != null && cases[j].index == i)
                    {
                        res += "case " + cases[j].statement + ":" + Environment.NewLine;
                    }
                }
                string lc = lines[i].ToString().Replace(replp, replt);
                res += "  " + lc + (lc[lc.Length - 1] != '}' ? ";" + Environment.NewLine : Environment.NewLine);
            }
            if (functions != null)
            {
                for (var i = 0; i < functions.Length; i++)
                {
                    var func = functions[i].ToString().Replace(replp, replt);
                    res += "  " + func + Environment.NewLine;
                }
            }
            return res + "}";
        }

        public override void Decompose(ref CodeNode self)
        {
            for (var i = 0; i < cases.Length; i++)
            {
                if (cases[i].statement != null)
                {
                    cases[i].statement.Decompose(ref cases[i].statement);
                }
            }

            for (var i = 0; i < lines.Length; i++)
            {
                lines[i].Decompose(ref lines[i]);
            }
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            image.RebuildScope(functionInfo, transferedVariables, scopeBias);

            for (var i = 0; i < cases.Length; i++)
            {
                if (cases[i].statement != null)
                {
                    cases[i].statement.RebuildScope(functionInfo, transferedVariables, scopeBias);
                }
            }

            for (var i = 0; i < lines.Length; i++)
            {
                lines[i]?.RebuildScope(functionInfo, transferedVariables, scopeBias);
            }
        }
    }
}