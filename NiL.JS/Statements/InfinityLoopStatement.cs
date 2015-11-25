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
    public sealed class InfinityLoopStatement : CodeNode
    {
        private CodeNode body;
        private string[] labels;

        public CodeNode Body { get { return body; } }
        public ReadOnlyCollection<string> Labels { get { return new ReadOnlyCollection<string>(labels); } }

        internal InfinityLoopStatement(CodeNode body, string[] labels)
        {
            this.body = body ?? new EmptyExpression();
            this.labels = labels;
        }

        public override JSValue Evaluate(Context context)
        {
            for (; ; )
            {
#if DEV
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
#endif
                context.lastResult = body.Evaluate(context) ?? context.lastResult;
                if (context.abortType != AbortType.None)
                {
                    if (context.abortType < AbortType.Return)
                    {
                        var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                        var _break = (context.abortType > AbortType.Continue) || !me;
                        if (me)
                        {
                            context.abortType = AbortType.None;
                            context.abortInfo = JSValue.notExists;
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

        internal protected override bool Build(ref CodeNode _this, int expressionDepth, List<string> scopeVariables, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics stats, Options opts)
        {
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, Expressions.FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics stats)
        {
            body.Optimize(ref body, owner, message, opts, stats);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "for (;;)" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }

        protected internal override void Decompose(ref CodeNode self)
        {
            body.Decompose(ref body);
        }
    }
}