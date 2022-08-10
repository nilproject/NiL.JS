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

        private FunctionDefinition[] _functions;
        private CodeNode[] _body;
        private SwitchCase[] _cases;
        private CodeNode _image;

        public FunctionDefinition[] Functions => _functions;
        public CodeNode[] Body => _body;
        public SwitchCase[] Cases => _cases;
        public CodeNode Image => _image;

        internal Switch(CodeNode[] body) => _body = body;

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
            state.LexicalScopeLevel++;
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
                    _functions = funcs.ToArray(),
                    _cases = cases.ToArray(),
                    _image = image,
                    Position = pos,
                    Length = index - pos
                };
                vars = CodeBlock.extractVariables(state, oldVariablesCount);
            }
            finally
            {
                state.LexicalScopeLevel--;
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
            if (_functions != null)
                throw new InvalidOperationException();
#endif
            JSValue imageValue = null;
            int caseIndex = 1;
            int lineIndex = _cases[0].index;

            var frame = ExceptionHelper.GetStackFrame(context, false);

            frame.CodeNode = _image;

            if (context._executionMode >= ExecutionMode.Resume)
            {
                var sdata = context.SuspendData[this] as SuspendData;
                if (sdata.imageValue == null)
                {
                    if (context._debugging)
                        context.raiseDebugger(_image);

                    imageValue = _image.Evaluate(context);
                }
                else
                    imageValue = sdata.imageValue;
                caseIndex = sdata.caseIndex;
                lineIndex = sdata.lineIndex;
            }
            else
            {
                if (context._debugging)
                    context.raiseDebugger(_image);

                imageValue = _image.Evaluate(context);
            }

            if (context._executionMode == ExecutionMode.Suspend)
            {
                context.SuspendData[this] = new SuspendData() { caseIndex = 1 };
                return null;
            }

            for (; caseIndex < _cases.Length; caseIndex++)
            {
                frame.CodeNode = _cases[caseIndex].Statement;

                if (context._debugging)
                    context.raiseDebugger(_cases[caseIndex].statement);

                var cseResult = _cases[caseIndex].statement.Evaluate(context);
                if (context._executionMode == ExecutionMode.Suspend)
                {
                    context.SuspendData[this] = new SuspendData()
                    {
                        caseIndex = caseIndex,
                        imageValue = imageValue
                    };
                    return null;
                }

                if (StrictEqual.Check(imageValue, cseResult))
                {
                    lineIndex = _cases[caseIndex].index;
                    caseIndex = _cases.Length;
                    break;
                }
            }

            for (; lineIndex < _body.Length; lineIndex++)
            {
                if (_body[lineIndex] == null)
                    continue;

                frame.CodeNode = _body[lineIndex];

                context._lastResult = _body[lineIndex].Evaluate(context) ?? context._lastResult;
                if (context._executionMode != ExecutionMode.Regular)
                {
                    if (context._executionMode == ExecutionMode.Break && context._executionInfo == null)
                    {
                        context._executionMode = ExecutionMode.Regular;
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

            Parser.Build(ref _image, 2, variables, codeContext | CodeContext.InExpression, message, stats, opts);

            for (int i = 0; i < _body.Length; i++)
                Parser.Build(ref _body[i], 1, variables, codeContext | CodeContext.Conditional, message, stats, opts);

            for (int i = 0; _functions != null && i < _functions.Length; i++)
            {
                CodeNode stat = _functions[i];
                Parser.Build(ref stat, 1, variables, codeContext, message, stats, opts);
            }

            _functions = null;

            for (int i = 1; i < _cases.Length; i++)
                Parser.Build(ref _cases[i].statement, 2, variables, codeContext, message, stats, opts);

            return false;
        }

        protected internal override CodeNode[] GetChildrenImpl()
        {
            var res = new List<CodeNode>()
            {
                _image
            };

            res.AddRange(_body);

            if (_functions != null && _functions.Length > 0)
                res.AddRange(_functions);

            if (_cases.Length > 0)
                res.AddRange(from c in _cases where c != null select c.statement);

            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        public override void Optimize(ref CodeNode _this, Expressions.FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            _image.Optimize(ref _image, owner, message, opts, stats);
            for (var i = 1; i < _cases.Length; i++)
                _cases[i].statement.Optimize(ref _cases[i].statement, owner, message, opts, stats);
            for (var i = _body.Length; i-- > 0;)
            {
                if (_body[i] == null)
                    continue;
                var cn = _body[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, stats);
                _body[i] = cn;
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            string res = "switch (" + _image + ") {" + Environment.NewLine;
            var replp = Environment.NewLine;
            var replt = Environment.NewLine + "  ";
            for (int i = _body.Length; i-- > 0;)
            {
                for (int j = 0; j < _cases.Length; j++)
                {
                    if (_cases[j] != null && _cases[j].index == i)
                    {
                        res += "case " + _cases[j].statement + ":" + Environment.NewLine;
                    }
                }
                string lc = _body[i].ToString().Replace(replp, replt);
                res += "  " + lc + (lc[lc.Length - 1] != '}' ? ";" + Environment.NewLine : Environment.NewLine);
            }
            if (_functions != null)
            {
                for (var i = 0; i < _functions.Length; i++)
                {
                    var func = _functions[i].ToString().Replace(replp, replt);
                    res += "  " + func + Environment.NewLine;
                }
            }
            return res + "}";
        }

        public override void Decompose(ref CodeNode self)
        {
            for (var i = 0; i < _cases.Length; i++)
            {
                if (_cases[i].statement != null)
                {
                    _cases[i].statement.Decompose(ref _cases[i].statement);
                }
            }

            for (var i = 0; i < _body.Length; i++)
            {
                _body[i].Decompose(ref _body[i]);
            }
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            _image.RebuildScope(functionInfo, transferedVariables, scopeBias);

            for (var i = 0; i < _cases.Length; i++)
            {
                if (_cases[i].statement != null)
                {
                    _cases[i].statement.RebuildScope(functionInfo, transferedVariables, scopeBias);
                }
            }

            for (var i = 0; i < _body.Length; i++)
            {
                _body[i]?.RebuildScope(functionInfo, transferedVariables, scopeBias);
            }
        }
    }
}