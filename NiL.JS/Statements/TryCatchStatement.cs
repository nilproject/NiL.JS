using NiL.JS.Core;
using System;

namespace NiL.JS.Statements
{
    internal class TryCatchStatement : Statement, IOptimizable
    {
        private static JSObject tempContainer = new JSObject();

        private Statement body;
        private Statement catchBody;
        private Statement finallyBody;
        private string exptName;

        public TryCatchStatement()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "try", ref i) || !Parser.isIdentificatorTerminator(code[i]))
                return new ParseResult();
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != '{')
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid try statement definition at " + Tools.PositionToTextcord(code, i))));
            var b = CodeBlock.Parse(state, ref i).Statement;
            while (char.IsWhiteSpace(code[i])) i++;
            Statement cb = null;
            string exptn = null;
            if (Parser.Validate(code, "catch (", ref i) || Parser.Validate(code, "catch(", ref i))
            {
                int s = i;
                if (!Parser.ValidateName(code, ref i, true, state.strict.Peek()))
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Catch block must contain varible name " + Tools.PositionToTextcord(code, i))));
                exptn = Tools.Unescape(code.Substring(s, i - s));
                while (char.IsWhiteSpace(code[i])) i++;
                if (!Parser.Validate(code, ")", ref i))
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \")\" at + " + Tools.PositionToTextcord(code, i))));
                while (char.IsWhiteSpace(code[i])) i++;
                if (code[i] != '{')
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid catch block statement definition at " + Tools.PositionToTextcord(code, i))));
                cb = CodeBlock.Parse(state, ref i).Statement;
                while (char.IsWhiteSpace(code[i])) i++;
            }
            Statement f = null;
            if (Parser.Validate(code, "finally", i) && Parser.isIdentificatorTerminator(code[i + 7]))
            {
                i += 7;
                while (char.IsWhiteSpace(code[i])) i++;
                if (code[i] != '{')
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid finally block statement definition at " + Tools.PositionToTextcord(code, i))));
                f = CodeBlock.Parse(state, ref i).Statement;
            }
            if (cb == null && f == null)
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("try block must contain 'catch' or/and 'finally' block")));
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new TryCatchStatement()
                {
                    body = b,
                    catchBody = cb,
                    finallyBody = f,
                    exptName = exptn
                }
            };
        }

        public override JSObject Invoke(Context context)
        {
            Exception except = null;
            try
            {
                body.Invoke(context);
            }
            catch (JSException e)
            {
                if (catchBody != null)
                {
                    var cvar = context.GetField(exptName);
                    tempContainer.Assign(cvar);
                    tempContainer.attributes = cvar.attributes;
                    cvar.Assign(e.Avatar);
                    cvar.attributes = ObjectAttributes.DontDelete;
                    catchBody.Invoke(context);
                    cvar.Assign(tempContainer);
                    cvar.attributes = tempContainer.attributes;
                    tempContainer.attributes = ObjectAttributes.None;
                }
                else except = e;
            }
            catch (Exception e)
            {
                if (catchBody != null)
                {
                    var cvar = context.GetField(exptName);
                    tempContainer.Assign(cvar);
                    tempContainer.attributes = cvar.attributes;
                    cvar.Assign(TypeProxy.Proxy(e));
                    cvar.attributes = ObjectAttributes.DontDelete;
                    catchBody.Invoke(context);
                    cvar.Assign(tempContainer);
                    cvar.attributes = tempContainer.attributes;
                    tempContainer.attributes = ObjectAttributes.None;
                }
                else except = e;
            }
            finally
            {
                if (finallyBody != null)
                {
                    var abort = context.abort;
                    var ainfo = context.abortInfo;
                    context.abort = AbortType.None;
                    context.abortInfo = JSObject.undefined;
                    try
                    {
                        finallyBody.Invoke(context);
                    }
                    finally
                    {
                        if (context.abort == AbortType.None)
                        {
                            context.abort = abort;
                            context.abortInfo = ainfo;
                        }
                        else
                            except = null;
                    }
                }
            }
            if (except != null)
                throw except;
            return null;
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            Parser.Optimize(ref body, 1, varibles);
            Parser.Optimize(ref catchBody, 1, varibles);
            Parser.Optimize(ref finallyBody, 1, varibles);
            return false;
        }
    }
}