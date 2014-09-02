using System;
using System.Collections.Generic;
using System.Linq;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.JIT;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class VariableDefineStatement : CodeNode
    {
        internal readonly CodeNode[] initializators;
        internal readonly string[] names;

        public CodeNode[] Initializators { get { return initializators; } }
        public string[] Names { get { return names; } }

        public VariableDefineStatement(string name, CodeNode init)
        {
            names = new[] { name };
            initializators = new[] { init };
        }

        private VariableDefineStatement(string[] names, CodeNode[] initializators)
        {
            this.initializators = initializators;
            this.names = names;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (!Parser.Validate(state.Code, "var ", ref i))
                return new ParseResult();
            bool isDef = false;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            var initializator = new List<CodeNode>();
            var names = new List<string>();
            while ((state.Code[i] != ';') && (state.Code[i] != '}') && !Tools.isLineTerminator(state.Code[i]))
            {
                int s = i;
                if (!Parser.ValidateName(state.Code, ref i, state.strict.Peek()))
                {
                    if (Parser.ValidateName(state.Code, ref i, false, true, state.strict.Peek()))
                        throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError('\"' + Tools.Unescape(state.Code.Substring(s, i - s), state.strict.Peek()) + "\" is a reserved word at " + Tools.PositionToTextcord(state.Code, s))));
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid variable definition at " + Tools.PositionToTextcord(state.Code, s))));
                }
                string name = Tools.Unescape(state.Code.Substring(s, i - s), state.strict.Peek());
                if (state.strict.Peek())
                {
                    if (name == "arguments" || name == "eval")
                        throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Varible name may not be \"arguments\" or \"eval\" in strict mode at " + Tools.PositionToTextcord(state.Code, s))));
                }
                names.Add(name);
                isDef = true;
                while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]) && !Tools.isLineTerminator(state.Code[i])) i++;
                if (i < state.Code.Length && (state.Code[i] != ',') && (state.Code[i] != ';') && (state.Code[i] != '=') && (state.Code[i] != '}') && (!Tools.isLineTerminator(state.Code[i])))
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \";\", \",\", \"=\" or \"}\" at + " + Tools.PositionToTextcord(state.Code, i))));
                if (i >= state.Code.Length)
                {
                    initializator.Add(new GetVariableStatement(name, state.functionsDepth) { Position = s, Length = name.Length, functionDepth = state.functionsDepth });
                    break;
                }
                if (Tools.isLineTerminator(state.Code[i]))
                {
                    s = i;
                    do i++; while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]));
                    if (i >= state.Code.Length)
                    {
                        initializator.Add(new GetVariableStatement(name, state.functionsDepth) { Position = s, Length = name.Length, functionDepth = state.functionsDepth });
                        break;
                    }
                    if (state.Code[i] != '=')
                        i = s;
                }
                if (state.Code[i] == '=')
                {
                    do i++; while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]));
                    if (i == state.Code.Length)
                        throw new JSException(TypeProxy.Proxy(new SyntaxError("Unexpected end of line in variable defenition.")));
                    initializator.Add(
                        new Assign(new GetVariableStatement(name, state.functionsDepth) { Position = s, Length = name.Length, functionDepth = state.functionsDepth }, ExpressionStatement.Parse(state, ref i, false).Statement)
                        {
                            Position = s,
                            Length = i - s
                        });
                }
                else
                    initializator.Add(new GetVariableStatement(name, state.functionsDepth) { Position = s, Length = name.Length, functionDepth = state.functionsDepth });
                if (i >= state.Code.Length)
                    break;
                s = i;
                if ((state.Code[i] != ',') && (state.Code[i] != ';') && (state.Code[i] != '=') && (state.Code[i] != '}') && (!Tools.isLineTerminator(state.Code[i])))
                    throw new ArgumentException("code (" + i + ")");
                while (s < state.Code.Length && char.IsWhiteSpace(state.Code[s])) s++;
                if (s >= state.Code.Length)
                    break;
                if (state.Code[s] == ',')
                {
                    i = s;
                    do i++; while (char.IsWhiteSpace(state.Code[i]));
                }
                else
                    while (char.IsWhiteSpace(state.Code[i]) && !Tools.isLineTerminator(state.Code[i])) i++;
            }
            if (!isDef)
                throw new ArgumentException("code (" + i + ")");
            var inits = initializator.ToArray();
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new VariableDefineStatement(names.ToArray(), inits)
                {
                    Position = pos,
                    Length = index - pos
                }
            };
        }

#if !NET35

        internal override System.Linq.Expressions.Expression BuildTree(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            if (initializators.Length == 1)
                return initializators[0].BuildTree(state);
            return System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Block(from x in initializators select x.BuildTree(state)), JITHelpers.UndefinedConstant).Reduce();
        }

#endif

        internal override JSObject Evaluate(Context context)
        {
            for (int i = 0; i < initializators.Length; i++)
                initializators[i].Evaluate(context);
            return JSObject.undefined;
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>();
            res.AddRange(initializators);
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Optimize(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            for (int i = 0; i < initializators.Length; i++)
                Parser.Optimize(ref initializators[i], 2, variables, strict);
            for (var i = 0; i < names.Length; i++)
                variables[names[i]].Defined = true;
            return false;
        }

        public override string ToString()
        {
            var res = "var ";
            for (var i = 0; i < initializators.Length; i++)
            {
                var t = initializators[i].ToString();
                if (string.IsNullOrEmpty(t))
                    continue;
                if (t[0] == '(')
                    t = t.Substring(1, t.Length - 2);
                if (i > 0)
                    res += ", ";
                res += t;
            }
            return res;
        }
    }
}