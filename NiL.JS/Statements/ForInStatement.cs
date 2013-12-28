using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal sealed class ForInStatement : Statement, IOptimizable
    {
        private Statement varible;
        private Statement source;
        private Statement body;

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
            var res = new ForInStatement();
            if (Parser.Validate(code, "var", ref i))
            {
                while (char.IsWhiteSpace(code[i])) i++;
                int start = i;
                string varName;
                if (!Parser.ValidateName(code, ref i))
                    throw new ArgumentException();
                varName = code.Substring(start, i - start);
                res.varible = new VaribleDefineStatement(varName, new GetVaribleStatement(varName));
            }
            else
            {
                if (code[i] == ';')
                    return new ParseResult();
                res.varible = OperatorStatement.ParseForUnary(state, ref i);
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
            res.body = Parser.Parse(state, ref i, 1);
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

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        public override JSObject Invoke(Context context)
        {
            var s = source.Invoke(context);
            var v = varible.Invoke(context);
            while ((s != null) && (s.ValueType > ObjectValueType.Undefined))
            {
                foreach (var o in s)
                {
                    var t = s.GetField(o, true);
                    if (t.ValueType > ObjectValueType.NotExistInObject && ((t.attributes & ObjectAttributes.DontEnum) == 0))
                    {
                        v.ValueType = ObjectValueType.String;
                        v.oValue = o;
                        if (v.assignCallback != null)
                            v.assignCallback();
                        body.Invoke(context);
                    }
                }
                s = s.prototype;
            }
            return JSObject.undefined;
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> varibles)
        {
            Parser.Optimize(ref varible, depth + 1, varibles);
            Parser.Optimize(ref source, depth + 1, varibles);
            Parser.Optimize(ref body, depth + 1, varibles);
            return false;
        }
    }
}