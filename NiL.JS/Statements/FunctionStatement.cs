using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal sealed class FunctionStatement : Statement, IOptimizable
    {
        public enum FunctionParseMode
        {
            Regular = 0,
            Getter,
            Setter
        }

        private string[] argumentsNames;
        private Statement body;
        public readonly string Name;

        private FunctionStatement(string name)
        {
            this.Name = name;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            return Parse(state, ref index, FunctionParseMode.Regular);
        }

        internal static ParseResult Parse(ParsingState state, ref int index, FunctionParseMode mode)
        {
            string code = state.Code;
            int i = index;
            switch (mode)
            {
                case FunctionParseMode.Regular:
                    {
                        if (!Parser.Validate(code, "function", ref i))
                            return new ParseResult();
                        if ((code[i] != '(') && (!char.IsWhiteSpace(code[i])))
                            return new ParseResult() { IsParsed = false, Message = "Invalid char in function definition" };
                        break;
                    }
                case FunctionParseMode.Getter:
                    {
                        if (!Parser.Validate(code, "get", ref i))
                            return new ParseResult();
                        if ((!char.IsWhiteSpace(code[i])))
                            return new ParseResult() { IsParsed = false, Message = "Invalid char in function definition" };
                        break;
                    }
                case FunctionParseMode.Setter:
                    {
                        if (!Parser.Validate(code, "set", ref i))
                            return new ParseResult();
                        if ((!char.IsWhiteSpace(code[i])))
                            return new ParseResult() { IsParsed = false, Message = "Invalid char in function definition" };
                        break;
                    }
            }
            while (char.IsWhiteSpace(code[i])) i++;
            var arguments = new List<string>();
            string name = null;
            if (code[i] != '(')
            {
                int n = i;
                if (!Parser.ValidateName(code, ref i, true))
                    throw new ArgumentException("code (" + i + ")");
                name = Tools.Unescape(code.Substring(n, i - n));
                while (char.IsWhiteSpace(code[i])) i++;
                if (code[i] != '(')
                    throw new ArgumentException("Invalid char at " + i + ": '" + code[i] + "'");
            }
            else if (mode != FunctionParseMode.Regular)
                throw new ArgumentException("Getters and Setters mast have name");
            do i++; while (char.IsWhiteSpace(code[i]));
            if (code[i] == ',')
                throw new ArgumentException("code (" + i + ")");
            while (code[i] != ')')
            {
                if (code[i] == ',')
                    do i++; while (char.IsWhiteSpace(code[i]));
                int n = i;
                if (!Parser.ValidateName(code, ref i, true))
                    throw new ArgumentException("code (" + i + ")");
                arguments.Add(Tools.Unescape(code.Substring(n, i - n)));
                while (char.IsWhiteSpace(code[i])) i++;
            }
            switch (mode)
            {
                case FunctionParseMode.Getter:
                    {
                        if (arguments.Count != 0)
                            throw new ArgumentException("getter have many arguments");
                        break;
                    }
                case FunctionParseMode.Setter:
                    {
                        if (arguments.Count != 1)
                            throw new ArgumentException("setter have invalid arguments");
                        break;
                    }
            }
            do
                i++;
            while (char.IsWhiteSpace(code[i]));
            if (code[i] != '{')
                throw new ArgumentException("code (" + i + ")");
            Statement body = CodeBlock.Parse(state, ref i).Statement;
            index = i;
            FunctionStatement res = new FunctionStatement(name)
                {
                    argumentsNames = arguments.ToArray(),
                    body = body
                };
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = res
            };
        }

        public override JSObject Invoke(Context context)
        {
            Function res = new Function(context, body, argumentsNames, Name);
            return res;
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> varibles)
        {
            (body as IOptimizable).Optimize(ref body, 0, varibles);
            return false;
        }
    }
}