using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;

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
            this.body = body ?? new EmptyStatement();
            this.labels = labels;
        }

        internal override JSValue Evaluate(Context context)
        {
            for (; ; )
            {
#if DEV
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
#endif
                context.lastResult = body.Evaluate(context) ?? context.lastResult;
                if (context.abort != AbortType.None)
                {
                    var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                    var _break = (context.abort > AbortType.Continue) || !me;
                    if (context.abort < AbortType.Return && me)
                    {
                        context.abort = AbortType.None;
                        context.abortInfo = null;
                    }
                    if (_break)
                        return null;
                }
            }
        }

        protected override CodeNode[] getChildsImpl()
        {
            return new[] { body };
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            return false;
        }

        internal override void Optimize(ref CodeNode _this, Expressions.FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            body.Optimize(ref body, owner, message, opts, statistic);
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