using NiL.JS.Core;
using System;

namespace NiL.JS.Statements
{
    internal class TryCatchStatement : Statement, IOptimizable
    {
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
            var b = CodeBlock.Parse(state, ref i).Statement;
            while (char.IsWhiteSpace(code[i])) i++;
            Statement cb = null;
            string exptn = null;
            if (Parser.Validate(code, "catch (", ref i) || Parser.Validate(code, "catch(", ref i))
            {
                int s = i;
                if (!Parser.ValidateName(code, ref i, true))
                    throw new ArgumentException("code (" + i + ")");
                exptn = Tools.Unescape(code.Substring(s, i - s));
                while (char.IsWhiteSpace(code[i])) i++;
                if (!Parser.Validate(code, ")", ref i))
                    throw new ArgumentException("code (" + i + ")");
                while (char.IsWhiteSpace(code[i])) i++;
                cb = CodeBlock.Parse(state, ref i).Statement;
                while (char.IsWhiteSpace(code[i])) i++;
            }
            Statement f = null;
            if (Parser.Validate(code, "finally", ref i))
            {
                while (char.IsWhiteSpace(code[i])) i++;
                f = CodeBlock.Parse(state, ref i).Statement;
            }
            if (cb == null && f == null)
                throw new ArgumentException("try block mast contain 'catch' or/and 'finally' block");
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
            try
            {
                body.Invoke(context);
            }
            catch (JSException e)
            {
                if (catchBody != null)
                {
                    var eo = context.Define(exptName);
                    eo.Assign(e.Avatar);
                    catchBody.Invoke(context);
                }
            }
            catch (Exception e)
            {
                if (catchBody != null)
                {
                    var eo = context.Define(exptName);
                    eo.ValueType = JSObjectType.Object;
                    eo.oValue = e;
                    eo.GetField("message", false, false).Assign(e.Message);
                    catchBody.Invoke(context);
                }
            }
            finally
            {
                if (finallyBody != null)
                    finallyBody.Invoke(context);
            }
            return null;
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            Parser.Optimize(ref body, 1, varibles);
            Parser.Optimize(ref catchBody, 0, new System.Collections.Generic.Dictionary<string,Statement>());
            Parser.Optimize(ref finallyBody, 0, new System.Collections.Generic.Dictionary<string,Statement>());
            return false;
        }
    }
}