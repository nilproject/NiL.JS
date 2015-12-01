using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class TryCatchStatement : CodeNode
    {
        private bool @catch;

        private CodeNode body;
        private CodeNode catchBody;
        private CodeNode finallyBody;
        private VariableDescriptor catchVariableDesc;

        public CodeNode Body { get { return body; } }
        public CodeNode CatchBody { get { return catchBody; } }
        public CodeNode FinalBody { get { return finallyBody; } }
        public string ExceptionVariableName { get { return catchVariableDesc.name; } }

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            if (!Parser.Validate(state.Code, "try", ref i) || !Parser.IsIdentificatorTerminator(state.Code[i]))
                return null;
            while (i < state.Code.Length && Tools.IsWhiteSpace(state.Code[i]))
                i++;
            if (i >= state.Code.Length)
                ExceptionsHelper.Throw(new SyntaxError("Unexpected end of line."));
            if (state.Code[i] != '{')
                ExceptionsHelper.Throw((new SyntaxError("Invalid try statement definition at " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            var b = CodeBlock.Parse(state, ref i);
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            CodeNode cb = null;
            string exptn = null;
            if (Parser.Validate(state.Code, "catch (", ref i) || Parser.Validate(state.Code, "catch(", ref i))
            {
                int s = i;
                if (!Parser.ValidateName(state.Code, ref i, state.strict))
                    ExceptionsHelper.Throw((new SyntaxError("Catch block must contain variable name " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                exptn = Tools.Unescape(state.Code.Substring(s, i - s), state.strict);
                if (state.strict)
                {
                    if (exptn == "arguments" || exptn == "eval")
                        ExceptionsHelper.Throw((new SyntaxError("Varible name may not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s))));
                }
                while (Tools.IsWhiteSpace(state.Code[i]))
                    i++;
                if (!Parser.Validate(state.Code, ")", ref i))
                    ExceptionsHelper.Throw((new SyntaxError("Expected \")\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                while (Tools.IsWhiteSpace(state.Code[i]))
                    i++;
                if (state.Code[i] != '{')
                    ExceptionsHelper.Throw((new SyntaxError("Invalid catch block statement definition at " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                state.lexicalScopeLevel++;
                try
                {
                    cb = CodeBlock.Parse(state, ref i);
                }
                finally
                {
                    state.lexicalScopeLevel--;
                }
                while (i < state.Code.Length && Tools.IsWhiteSpace(state.Code[i]))
                    i++;
            }
            CodeNode f = null;
            if (Parser.Validate(state.Code, "finally", i) && Parser.IsIdentificatorTerminator(state.Code[i + 7]))
            {
                i += 7;
                while (Tools.IsWhiteSpace(state.Code[i]))
                    i++;
                if (state.Code[i] != '{')
                    ExceptionsHelper.Throw((new SyntaxError("Invalid finally block statement definition at " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                f = CodeBlock.Parse(state, ref i);
            }
            if (cb == null && f == null)
                ExceptionsHelper.Throw((new SyntaxError("try block must contain 'catch' or/and 'finally' block")));
            var pos = index;
            index = i;
            return new TryCatchStatement()
            {
                body = (CodeBlock)b,
                catchBody = (CodeBlock)cb,
                finallyBody = (CodeBlock)f,
                catchVariableDesc = new VariableDescriptor(exptn, state.lexicalScopeLevel + 1),
                Position = pos,
                Length = index - pos
            };
        }

        public override JSValue Evaluate(Context context)
        {
            Exception exception = null;
            if (context.abortType >= AbortType.Resume)
            {
                var action = context.SuspendData[this] as Action<Context>;
                if (action != null)
                {
                    action(context);
                    return null;
                }
            }
            else
            {
#if DEV
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
#endif
            }
            try
            {
                body.Evaluate(context);
                if (context.abortType == AbortType.Suspend)
                {
                    context.SuspendData[this] = null;
                }
            }
            catch (Exception e)
            {
                if (this.@catch)
                {
                    if (catchBody != null)
                        catchHandler(context, e);
                }
                else
                {
                    if (finallyBody == null)
                        throw;
                    exception = e;
                }
            }
            finally
            {
                if (context.abortType != AbortType.Suspend && finallyBody != null)
                {
                    finallyHandler(context, exception);
                    exception = null;
                }
            }
            if (context.abortType != AbortType.Suspend && exception != null)
                throw exception;
            return null;
        }

        private void finallyHandler(Context context, Exception exception)
        {
#if DEV
            if (context.debugging)
                context.raiseDebugger(finallyBody);
#endif
            var abort = context.abortType;
            var ainfo = context.abortInfo;
            if (abort == AbortType.Return && ainfo != null)
            {
                if (ainfo.Defined)
                    ainfo = ainfo.CloneImpl(false);
                else
                    ainfo = JSValue.Undefined;
            }
            context.abortType = AbortType.None;
            context.abortInfo = JSValue.undefined;

            Action<Context> finallyAction = null;
            finallyAction = (c) =>
            {
                c.lastResult = finallyBody.Evaluate(c) ?? context.lastResult;
                if (c.abortType == AbortType.None)
                {
                    c.abortType = abort;
                    c.abortInfo = ainfo;
                    if (exception != null)
                        throw exception;
                }
                else if (c.abortType == AbortType.Suspend)
                {
                    c.SuspendData[this] = finallyAction;
                }
            };
            finallyAction(context);
        }

        private void catchHandler(Context context, Exception e)
        {
#if DEV
            if (context.debugging)
                context.raiseDebugger(catchBody);
#endif
            if (catchBody is EmptyExpression)
                return;
            JSValue cvar = null;
#if !PORTABLE
            if (e is RuntimeWrappedException)
            {
                cvar = new JSValue();
                cvar.Assign((e as RuntimeWrappedException).WrappedException as JSValue);
            }
            else
#endif
                cvar = e is JSException ? (e as JSException).Avatar.CloneImpl(false) : TypeProxy.Marshal(e);
            cvar.attributes |= JSValueAttributesInternal.DoNotDelete;
            var catchContext = new CatchContext(cvar, context, catchVariableDesc.name);
#if DEBUG
            if (!(e is JSException))
                System.Diagnostics.Debugger.Break();
#endif

            Action<Context> catchAction = null;
            catchAction = (c) =>
            {
                try
                {
                    catchContext.abortType = c.abortType;
                    catchContext.abortInfo = c.abortInfo;
                    catchContext.Activate();
                    catchContext.lastResult = catchBody.Evaluate(catchContext) ?? catchContext.lastResult;
                }
                finally
                {
                    c.lastResult = catchContext.lastResult ?? c.lastResult;
                    catchContext.Deactivate();
                }
                c.abortType = catchContext.abortType;
                c.abortInfo = catchContext.abortInfo;

                if (c.abortType == AbortType.Suspend)
                {
                    if (finallyBody != null)
                    {
                        c.SuspendData[this] = new Action<Context>((c2) =>
                        {
                            try
                            {
                                catchAction(c2);
                            }
                            finally
                            {
                                if (c2.abortType != AbortType.Suspend)
                                    finallyHandler(c2, e);
                            }
                        });
                    }
                    else
                        c.SuspendData[this] = catchAction;
                }
            };
            catchAction(context);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (stats != null)
                stats.ContainsTry = true;
            Parser.Build(ref body, expressionDepth, variables, codeContext | CodeContext.Conditional, message, stats, opts);
            if (catchBody != null)
            {
                this.@catch = true;
                catchVariableDesc.owner = this;
                VariableDescriptor oldVarDesc = null;
                variables.TryGetValue(catchVariableDesc.name, out oldVarDesc);
                variables[catchVariableDesc.name] = catchVariableDesc;

                Parser.Build(ref catchBody, expressionDepth, variables, codeContext | CodeContext.Conditional, message, stats, opts);

                if (oldVarDesc != null)
                    variables[catchVariableDesc.name] = oldVarDesc;
                else
                    variables.Remove(catchVariableDesc.name);
            }
            if (finallyBody != null)
                Parser.Build(ref finallyBody, expressionDepth, variables, codeContext, message, stats, opts);
            if (body == null || (body is EmptyExpression))
            {
                if (message != null)
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Empty (or reduced to empty) try" + (catchBody != null ? "..catch" : "") + (finallyBody != null ? "..finally" : "") + " block. Maybe, something missing.");
                _this = finallyBody;
            }
            if (@catch && (catchBody == null || (catchBody is EmptyExpression)))
            {
                if (message != null)
                    message(MessageLevel.Warning, new CodeCoordinates(0, (catchBody ?? this as CodeNode).Position, (catchBody ?? this as CodeNode).Length), "Empty (or reduced to empty) catch block. Do not ignore exceptions.");
            }
            return false;
        }

        public override void Optimize(ref CodeNode _this, Expressions.FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            body.Optimize(ref body, owner, message, opts, stats);
            if (catchBody != null)
            {
                catchBody.Optimize(ref catchBody, owner, message, opts, stats);
            }
            if (finallyBody != null)
                finallyBody.Optimize(ref finallyBody, owner, message, opts, stats);
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                body,
                catchBody,
                finallyBody
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override void Decompose(ref CodeNode self)
        {
            body.Decompose(ref body);
            if (catchBody != null)
                catchBody.Decompose(ref catchBody);
            if (finallyBody != null)
                finallyBody.Decompose(ref finallyBody);
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            body.RebuildScope(functionInfo, transferedVariables, scopeBias);
            catchBody?.RebuildScope(functionInfo, transferedVariables, scopeBias);
            finallyBody?.RebuildScope(functionInfo, transferedVariables, scopeBias);
        }

        public override string ToString()
        {
            var sbody = (body as CodeBlock as object ?? "{" + Environment.NewLine + " " + body + Environment.NewLine + "}").ToString();
            var fbody = finallyBody == null ? "" : (finallyBody as CodeBlock as object ?? "{" + Environment.NewLine + " " + finallyBody + Environment.NewLine + "}").ToString();
            var cbody = catchBody == null ? "" : (catchBody as CodeBlock as object ?? "{" + Environment.NewLine + " " + catchBody + Environment.NewLine + "}").ToString();
            return "try" +
                sbody +
                (catchBody != null ? Environment.NewLine +
                "catch (" + catchVariableDesc + ")" + Environment.NewLine +
                cbody : "") +
                (finallyBody != null ? Environment.NewLine + "finally" + fbody : "");
        }
    }
}