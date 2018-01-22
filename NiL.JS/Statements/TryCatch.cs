using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class TryCatch : CodeNode
    {
        private bool _catch;
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
            int i = index;
            if (!Parser.Validate(state.Code, "try", ref i) || !Parser.IsIdentifierTerminator(state.Code[i]))
                return null;
            while (i < state.Code.Length && Tools.IsWhiteSpace(state.Code[i]))
                i++;
            if (i >= state.Code.Length)
                ExceptionHelper.Throw(new SyntaxError("Unexpected end of line."));
            if (state.Code[i] != '{')
                ExceptionHelper.Throw((new SyntaxError("Invalid try statement definition at " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            var b = CodeBlock.Parse(state, ref i);
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            CodeNode cb = null;
            string exptn = null;
            if (Parser.Validate(state.Code, "catch (", ref i) || Parser.Validate(state.Code, "catch(", ref i))
            {
                Tools.SkipSpaces(state.Code, ref i);

                int s = i;
                if (!Parser.ValidateName(state.Code, ref i, state.strict))
                    ExceptionHelper.Throw((new SyntaxError("Catch block must contain variable name " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));

                exptn = Tools.Unescape(state.Code.Substring(s, i - s), state.strict);
                if (state.strict)
                {
                    if (exptn == "arguments" || exptn == "eval")
                        ExceptionHelper.Throw((new SyntaxError("Varible name can not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s))));
                }

                Tools.SkipSpaces(state.Code, ref i);

                if (!Parser.Validate(state.Code, ")", ref i))
                    ExceptionHelper.Throw((new SyntaxError("Expected \")\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                while (Tools.IsWhiteSpace(state.Code[i]))
                    i++;
                if (state.Code[i] != '{')
                    ExceptionHelper.Throw((new SyntaxError("Invalid catch block statement definition at " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
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
            if (Parser.Validate(state.Code, "finally", i) && Parser.IsIdentifierTerminator(state.Code[i + 7]))
            {
                i += 7;
                while (Tools.IsWhiteSpace(state.Code[i]))
                    i++;
                if (state.Code[i] != '{')
                    ExceptionHelper.Throw((new SyntaxError("Invalid finally block statement definition at " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                f = CodeBlock.Parse(state, ref i);
            }
            if (cb == null && f == null)
                ExceptionHelper.ThrowSyntaxError("try block must contain 'catch' or/and 'finally' block", state.Code, index);

            var pos = index;
            index = i;
            return new TryCatch()
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
            if (context._executionMode >= ExecutionMode.Resume)
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
                if (context._debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
            }

            try
            {
                body.Evaluate(context);

                if (context._executionMode == ExecutionMode.Suspend)
                    context.SuspendData[this] = null;
            }
            catch (Exception e)
            {
                if (this._catch)
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
                if (context._executionMode != ExecutionMode.Suspend && finallyBody != null)
                {
                    finallyHandler(context, exception);
                    exception = null;
                }
            }

            if (context._executionMode != ExecutionMode.Suspend && exception != null)
                throw exception;

            return null;
        }

        private void finallyHandler(Context context, Exception exception)
        {
            if (context._debugging)
                context.raiseDebugger(finallyBody);

            var abort = context._executionMode;
            var ainfo = context._executionInfo;
            if (abort == ExecutionMode.Return && ainfo != null)
            {
                if (ainfo.Defined)
                    ainfo = ainfo.CloneImpl(false);
                else
                    ainfo = JSValue.Undefined;
            }

            context._executionMode = ExecutionMode.None;
            context._executionInfo = JSValue.undefined;

            Action<Context> finallyAction = null;
            finallyAction = (c) =>
            {
                c._lastResult = finallyBody.Evaluate(c) ?? context._lastResult;
                if (c._executionMode == ExecutionMode.None)
                {
                    c._executionMode = abort;
                    c._executionInfo = ainfo;
                    if (exception != null)
                        throw exception as JSException ?? new JSException(null as JSValue, exception);
                }
                else if (c._executionMode == ExecutionMode.Suspend)
                {
                    c.SuspendData[this] = finallyAction;
                }
            };
            finallyAction(context);
        }

        private void catchHandler(Context context, Exception e)
        {
            if (context._debugging)
                context.raiseDebugger(catchBody);

            if (catchBody is Empty)
                return;

            JSValue cvar = null;
#if !(PORTABLE || NETCORE)
            if (e is RuntimeWrappedException)
            {
                cvar = new JSValue();
                cvar.Assign((e as RuntimeWrappedException).WrappedException as JSValue);
            }
            else
#endif
            {
                cvar = e is JSException ? (e as JSException).Error.CloneImpl(false) : context.GlobalContext.ProxyValue(e);
            }

            cvar._attributes |= JSValueAttributesInternal.DoNotDelete;
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
                    catchContext._executionMode = c._executionMode;
                    catchContext._executionInfo = c._executionInfo;
                    catchContext.Activate();
                    catchContext._lastResult = catchBody.Evaluate(catchContext) ?? catchContext._lastResult;
                }
                finally
                {
                    c._lastResult = catchContext._lastResult ?? c._lastResult;
                    catchContext.Deactivate();
                }
                c._executionMode = catchContext._executionMode;
                c._executionInfo = catchContext._executionInfo;

                if (c._executionMode == ExecutionMode.Suspend)
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
                                if (c2._executionMode != ExecutionMode.Suspend)
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

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (stats != null)
                stats.ContainsTry = true;

            Parser.Build(ref body, expressionDepth, variables, codeContext | CodeContext.Conditional, message, stats, opts);
            var catchPosition = Position;
            if (catchBody != null)
            {
                _catch = true;
                catchVariableDesc.owner = this;
                VariableDescriptor oldVarDesc = null;
                variables.TryGetValue(catchVariableDesc.name, out oldVarDesc);
                variables[catchVariableDesc.name] = catchVariableDesc;
                catchPosition = catchBody.Position;
                Parser.Build(ref catchBody, expressionDepth, variables, codeContext | CodeContext.Conditional, message, stats, opts);
                if (oldVarDesc != null)
                    variables[catchVariableDesc.name] = oldVarDesc;
                else
                    variables.Remove(catchVariableDesc.name);
            }

            var finallyPosition = 0;
            if (finallyBody != null)
            {
                finallyPosition = finallyBody.Position;
                Parser.Build(ref finallyBody, expressionDepth, variables, codeContext, message, stats, opts);
            }

            if (body == null || (body is Empty))
            {
                if (message != null)
                    message(MessageLevel.Warning, Position, Length, "Empty (or reduced to empty) try" + (catchBody != null ? "..catch" : "") + (finallyBody != null ? "..finally" : "") + " block. Maybe, something missing.");

                _this = finallyBody;
            }

            if (_catch && (catchBody == null || (catchBody is Empty)))
            {
                if (message != null)
                    message(MessageLevel.Warning, catchPosition, (catchBody ?? this as CodeNode).Length, "Empty (or reduced to empty) catch block. Do not ignore exceptions.");
            }

            if (finallyPosition != 0 && (finallyBody == null || (finallyBody is Empty)))
            {
                if (message != null)
                    message(MessageLevel.Warning, catchPosition, (catchBody ?? this as CodeNode).Length, "Empty (or reduced to empty) finally block.");
            }

            return false;
        }

        public override void Optimize(ref CodeNode _this, Expressions.FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            body.Optimize(ref body, owner, message, opts, stats);

            if (catchBody != null)
                catchBody.Optimize(ref catchBody, owner, message, opts, stats);

            if (finallyBody != null)
                finallyBody.Optimize(ref finallyBody, owner, message, opts, stats);
        }

        protected internal override CodeNode[] GetChildsImpl()
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

            if (catchBody != null)
            {
                VariableDescriptor variableToRestore = null;
                if (transferedVariables != null)
                {
                    transferedVariables.TryGetValue(catchVariableDesc.name, out variableToRestore);
                    transferedVariables[catchVariableDesc.name] = catchVariableDesc;
                }

                catchBody.RebuildScope(functionInfo, transferedVariables, scopeBias);

                if (transferedVariables != null)
                {
                    if (variableToRestore != null)
                        transferedVariables[variableToRestore.name] = variableToRestore;
                    else
                        transferedVariables.Remove(catchVariableDesc.name);
                }
            }

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