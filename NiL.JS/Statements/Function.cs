using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal sealed class Function : Statement, IOptimizable
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

        private Function(string name)
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
            Function res = new Function(name)
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

        public override JSObject Invoke(Context context, JSObject args)
        {
            Context internalContext = new Context(context);
            var @this = context.GetField("this");
            if (@this.ValueType < ObjectValueType.Object)
            {
                @this = new JSObject(false) { ValueType = ObjectValueType.Object, oValue = @this };
                internalContext.thisBind = @this;
            }
            int i = 0;
            int min = Math.Min(args == null ? 0 : args.GetField("length").iValue, argumentsNames.Length);
            for (; i < min; i++)
                internalContext.Define(argumentsNames[i]).Assign(args.GetField(i.ToString()));
            for (; i < argumentsNames.Length; i++)
                internalContext.Define(argumentsNames[i]).Assign(null);
            body.Invoke(internalContext);
            return internalContext.abortInfo;
        }

        public override JSObject Invoke(Context context)
        {
            var res = new JSObject(true) { ValueType = ObjectValueType.Statement, oValue = this.Implement(context) };
            res.fields["prototype"] = new JSObject() { ValueType = ObjectValueType.Object, oValue = new object(), prototype = BaseObject.Prototype, attributes = ObjectAttributes.DontDelete | ObjectAttributes.DontEnum };
            res.fields["arguments"] = JSObject.Null;
            return res;
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> varibles)
        {
            (body as IOptimizable).Optimize(ref body, 0, varibles);
            return false;
        }
    }
}