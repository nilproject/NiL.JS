using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class With : CodeNode
    {
        private CodeNode _scope;
        private CodeNode _body;

        public CodeNode Body { get { return _body; } }
        public CodeNode Scope { get { return _scope; } }

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "with (", ref i) && !Parser.Validate(state.Code, "with(", ref i))
                return null;
            if (state.strict)
                ExceptionHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("WithStatement is not allowed in strict mode.")));

            if (state.message != null)
                state.message(MessageLevel.CriticalWarning, index, 4, "Do not use \"with\".");

            var obj = Parser.Parse(state, ref i, CodeFragmentType.Expression);
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            if (state.Code[i] != ')')
                ExceptionHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("Invalid syntax WithStatement.")));
            do
                i++;
            while (Tools.IsWhiteSpace(state.Code[i]));

            CodeNode body = null;
            VariableDescriptor[] vars = null;
            var oldVariablesCount = state.Variables.Count;
            state.lexicalScopeLevel++;
            var oldCodeContext = state.CodeContext;
            state.CodeContext |= CodeContext.InWith;
            try
            {
                body = Parser.Parse(state, ref i, 0);
                vars = CodeBlock.extractVariables(state, oldVariablesCount);
                body = new CodeBlock(new[] { body })
                {
                    _variables = vars,
                    Position = body.Position,
                    Length = body.Length
                };
            }
            finally
            {
                state.lexicalScopeLevel--;
                state.CodeContext = oldCodeContext;
            }

            var pos = index;
            index = i;
            return new With()
            {
                _scope = obj,
                _body = body,
                Position = pos,
                Length = index - pos
            };
        }

        public override JSValue Evaluate(Context context)
        {
            JSValue scopeObject = null;
            WithContext intcontext = null;
            Action<Context> action = null;

            if (context._executionMode >= ExecutionMode.Resume)
            {
                action = context.SuspendData[this] as Action<Context>;
                if (action != null)
                {
                    action(context);
                    return null;
                }
            }

            if (context._executionMode != ExecutionMode.Resume && context._debugging)
                context.raiseDebugger(_scope);

            scopeObject = _scope.Evaluate(context);
            if (context._executionMode == ExecutionMode.Suspend)
            {
                context.SuspendData[this] = null;
                return null;
            }

            intcontext = new WithContext(scopeObject, context);
            action = (c) =>
            {
                try
                {
                    intcontext._executionMode = c._executionMode;
                    intcontext._executionInfo = c._executionInfo;
                    intcontext.Activate();
                    c._lastResult = _body.Evaluate(intcontext) ?? intcontext._lastResult;
                    c._executionMode = intcontext._executionMode;
                    c._executionInfo = intcontext._executionInfo;
                    if (c._executionMode == ExecutionMode.Suspend)
                    {
                        c.SuspendData[this] = action;
                    }
                }
                finally
                {
                    intcontext.Deactivate();
                }
            };

            if (context._debugging && !(_body is CodeBlock))
                context.raiseDebugger(_body);

            action(context);
            return null;
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                _body,
                _scope
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (stats != null)
                stats.ContainsWith = true;
            Parser.Build(ref _scope, expressionDepth + 1, variables, codeContext | CodeContext.InExpression, message, stats, opts);
            Parser.Build(ref _body, expressionDepth, new Dictionary<string, VariableDescriptor>(), codeContext | CodeContext.InWith, message, stats, opts);
            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            if (_scope != null)
                _scope.Optimize(ref _scope, owner, message, opts, stats);

            if (_body != null)
                _body.Optimize(ref _body, owner, message, opts, stats);

            if (_body == null)
                _this = _scope;
        }

        public override void Decompose(ref CodeNode self)
        {
            if (_scope != null)
                _scope.Decompose(ref _scope);
            if (_body != null)
                _body.Decompose(ref _body);
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            _scope?.RebuildScope(functionInfo, transferedVariables, scopeBias);

            var tempVariables = new Dictionary<string, VariableDescriptor>();
            _body?.RebuildScope(functionInfo, tempVariables, scopeBias + 1);
            if (tempVariables != null)
            {
                var block = _body as CodeBlock;
                if (block != null)
                {
                    var variables = new List<VariableDescriptor>();
                    foreach (var variable in tempVariables)
                    {
                        if ((variable.Value is ParameterDescriptor) || !(variable.Value.initializer is FunctionDefinition))
                        {
                            transferedVariables.Add(variable.Key, variable.Value);
                        }
                        else
                        {
                            variables.Add(variable.Value);
                        }
                    }

                    block._variables = variables.ToArray();
                    block._suppressScopeIsolation = block._variables.Length == 0 ? SuppressScopeIsolationMode.Suppress : SuppressScopeIsolationMode.DoNotSuppress;
                }
            }
        }

        public override string ToString()
        {
            return "with (" + _scope + ")" + (_body is CodeBlock ? "" : Environment.NewLine + "  ") + _body;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
