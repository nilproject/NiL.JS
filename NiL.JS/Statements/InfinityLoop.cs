using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class InfinityLoop : CodeNode
    {
        private CodeNode _body;
        private readonly string[] _labels;

        public CodeNode Body { get { return _body; } }
        public ReadOnlyCollection<string> Labels { get { return new ReadOnlyCollection<string>(_labels); } }

        internal InfinityLoop(CodeNode body, string[] labels)
        {
            _body = body ?? new Empty();
            _labels = labels;
        }

        public override JSValue Evaluate(Context context)
        {
            var frame = ExceptionHelper.GetStackFrame(context, false);

            for (;;)
            {
                frame.CodeNode = _body;

                if (context._debugging && _body is not CodeBlock)
                    context.raiseDebugger(_body);

                context._lastResult = _body.Evaluate(context) ?? context._lastResult;
                if (context._executionMode != ExecutionMode.Regular)
                {
                    if (context._executionMode < ExecutionMode.Return)
                    {
                        var me = context._executionInfo == null || System.Array.IndexOf(_labels, context._executionInfo._oValue as string) != -1;
                        var _break = (context._executionMode > ExecutionMode.Continue) || !me;
                        if (me)
                        {
                            context._executionMode = ExecutionMode.Regular;
                            context._executionInfo = JSValue.notExists;
                        }
                        if (_break)
                            return null;
                    }
                    else
                        return null;
                }
            }
        }

        protected internal override CodeNode[] GetChildrenImpl()
        {
            return new[] { _body };
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            return false;
        }

        public override void Optimize(ref CodeNode _this, Expressions.FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            _body.Optimize(ref _body, owner, message, opts, stats);
        }

        public override void Decompose(ref CodeNode self)
        {
            _body.Decompose(ref _body);
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            _body.RebuildScope(functionInfo, transferedVariables, scopeBias);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "for (;;)" + (_body is CodeBlock ? "" : Environment.NewLine + "  ") + _body;
        }
    }
}