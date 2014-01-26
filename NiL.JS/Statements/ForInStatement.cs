using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;
using System.Collections.Generic;

namespace NiL.JS.Statements
{
    internal sealed class ForInStatement : Statement, IOptimizable
    {
        private Statement varible;
        private Statement source;
        private Statement body;
        private List<string> labels;

        private ForInStatement()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            while (char.IsWhiteSpace(code[i])) i++;
            if (!Parser.Validate(code, "for(", ref i) && (!Parser.Validate(code, "for (", ref i)))
                return new ParseResult();
            while (char.IsWhiteSpace(code[i])) i++;
            var res = new ForInStatement()
            {
                labels = state.Labels.GetRange(state.Labels.Count - state.LabelCount, state.LabelCount)
            };
            if (Parser.Validate(code, "var", ref i))
            {
                while (char.IsWhiteSpace(code[i])) i++;
                int start = i;
                string varName;
                if (!Parser.ValidateName(code, ref i))
                    throw new ArgumentException();
                varName = Tools.Unescape(code.Substring(start, i - start));
                res.varible = new VaribleDefineStatement(varName, new GetVaribleStatement(varName));
            }
            else
            {
                if (code[i] == ';')
                    return new ParseResult();
                res.varible = OperatorStatement.Parse(state, ref i, true, true).Statement;
            }
            while (char.IsWhiteSpace(code[i])) i++;
            if (!Parser.Validate(code, "in", ref i))
                return new ParseResult();
            while (char.IsWhiteSpace(code[i])) i++;
            res.source = Parser.Parse(state, ref i, 1);
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != ')')
                throw new ArgumentException();
            i++;
            state.AllowBreak++;
            state.AllowContinue++;
            res.body = Parser.Parse(state, ref i, 0);
            state.AllowBreak--;
            state.AllowContinue--;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = res
            };
        }

        public override JSObject Invoke(Context context)
        {
            var s = source.Invoke(context);
            if (s.ValueType >= JSObjectType.Object && s.oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't enumerate properties of undefined.")));
            var v = varible.Invoke(context);
            while (s != null)
            {
                foreach (var o in s)
                {
                    var t = s.GetField(o, true, false);
                    if (t.ValueType > JSObjectType.NotExistInObject && ((t.attributes & ObjectAttributes.DontEnum) == 0))
                    {
                        v.ValueType = JSObjectType.String;
                        v.oValue = o;
                        if (v.assignCallback != null)
                            v.assignCallback();
                        body.Invoke(context);
                        if (context.abort != AbortType.None)
                        {
                            bool _break = (context.abort > AbortType.Continue) || ((context.abortInfo != null) && (labels.IndexOf(context.abortInfo.oValue as string) == -1));
                            if (context.abort < AbortType.Return && ((context.abortInfo == null) || (labels.IndexOf(context.abortInfo.oValue as string) != -1)))
                            {
                                context.abort = AbortType.None;
                                context.abortInfo = null;
                            }
                            if (_break)
                                return null;
                        }
                    }
                }
                s = s.prototype;
            }
            return JSObject.undefined;
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            Parser.Optimize(ref varible, 1, varibles);
            Parser.Optimize(ref source, 1, varibles);
            Parser.Optimize(ref body, 1, varibles);
            if (varible is Operators.None)
            {
                if ((varible as Operators.None).Second != null)
                    throw new InvalidOperationException("Invalid left-hand side in for-in");
                varible = (varible as Operators.None).First;
            }
            return false;
        }
    }
}