using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class InfinityLoop : CodeNode
    {
        private CodeNode body;
        private string[] labels;

        public CodeNode Body { get { return body; } }
        public ReadOnlyCollection<string> Labels { get { return new ReadOnlyCollection<string>(labels); } }

        internal InfinityLoop(CodeNode body, string[] labels)
        {
            this.body = body ?? new Empty();
            this.labels = labels;
        }

        public override JSValue Evaluate(Context context)
        {
            for (;;)
            {
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);

                context.lastResult = body.Evaluate(context) ?? context.lastResult;
                if (context.executionMode != AbortReason.None)
                {
                    if (context.executionMode < AbortReason.Return)
                    {
                        var me = context.executionInfo == null || System.Array.IndexOf(labels, context.executionInfo.oValue as string) != -1;
                        var _break = (context.executionMode > AbortReason.Continue) || !me;
                        if (me)
                        {
                            context.executionMode = AbortReason.None;
                            context.executionInfo = JSValue.notExists;
                        }
                        if (_break)
                            return null;
                    }
                    else
                        return null;
                }
            }
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            return new[] { body };
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            return false;
        }

        public override void Optimize(ref CodeNode _this, Expressions.FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            body.Optimize(ref body, owner, message, opts, stats);
        }

        public override void Decompose(ref CodeNode self)
        {
            body.Decompose(ref body);
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            body.RebuildScope(functionInfo, transferedVariables, scopeBias);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "for (;;)" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}