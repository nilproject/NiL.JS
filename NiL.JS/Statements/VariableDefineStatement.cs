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
        private sealed class AllowWriteCN : VariableReference
        {
            internal readonly VariableReference prototype;

            internal AllowWriteCN(VariableReference proto)
            {
                prototype = proto;
            }

            internal override JSObject Evaluate(Context context)
            {
                var res = prototype.Evaluate(context);
                if ((res.attributes & JSObjectAttributesInternal.SystemObject) == 0)
                    res.attributes &= ~JSObjectAttributesInternal.ReadOnly;
                return res;
            }

            internal override JSObject EvaluateForAssing(Context context)
            {
                var res = prototype.EvaluateForAssing(context);
                if ((res.attributes & JSObjectAttributesInternal.SystemObject) == 0)
                    res.attributes &= ~JSObjectAttributesInternal.ReadOnly;
                return res;
            }

            internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
            {
                return prototype.Build(ref _this, depth, variables, strict);
            }

            protected override CodeNode[] getChildsImpl()
            {
                return prototype.Childs;
            }

            public override string Name
            {
                get { return prototype.Name; }
            }
        }

        internal VariableDescriptor[] variables;
        internal readonly CodeNode[] initializators;
        internal readonly string[] names;
        internal readonly bool isConst;

        public bool Const { get { return isConst; } }
        public CodeNode[] Initializators { get { return initializators; } }
        public string[] Names { get { return names; } }

        internal VariableDefineStatement(string name, CodeNode init, bool isConst)
        {
            names = new[] { name };
            initializators = new[] { init };
            this.isConst = isConst;
        }

        private VariableDefineStatement(string[] names, CodeNode[] initializators, bool isConst)
        {
            this.initializators = initializators;
            this.names = names;
            this.isConst = isConst;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            bool isConst = false;
            if (!Parser.Validate(state.Code, "var ", ref i)
                && !(isConst = Parser.Validate(state.Code, "const ", ref i)))
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
                        throw new JSException((new Core.BaseTypes.SyntaxError('\"' + Tools.Unescape(state.Code.Substring(s, i - s), state.strict.Peek()) + "\" is a reserved word at " + Tools.PositionToTextcord(state.Code, s))));
                    throw new JSException((new Core.BaseTypes.SyntaxError("Invalid variable definition at " + Tools.PositionToTextcord(state.Code, s))));
                }
                string name = Tools.Unescape(state.Code.Substring(s, i - s), state.strict.Peek());
                if (state.strict.Peek())
                {
                    if (name == "arguments" || name == "eval")
                        throw new JSException((new Core.BaseTypes.SyntaxError("Varible name may not be \"arguments\" or \"eval\" in strict mode at " + Tools.PositionToTextcord(state.Code, s))));
                }
                names.Add(name);
                isDef = true;
                while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]) && !Tools.isLineTerminator(state.Code[i])) i++;
                if (i < state.Code.Length && (state.Code[i] != ',') && (state.Code[i] != ';') && (state.Code[i] != '=') && (state.Code[i] != '}') && (!Tools.isLineTerminator(state.Code[i])))
                    throw new JSException((new Core.BaseTypes.SyntaxError("Expected \";\", \",\", \"=\" or \"}\" at + " + Tools.PositionToTextcord(state.Code, i))));
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
                        throw new JSException((new SyntaxError("Unexpected end of line in variable defenition.")));
                    VariableReference accm = new GetVariableStatement(name, state.functionsDepth) { Position = s, Length = name.Length, functionDepth = state.functionsDepth };
                    if (isConst)
                        accm = new AllowWriteCN(accm);
                    initializator.Add(
                        new Assign(
                            accm,
                            ExpressionStatement.Parse(state, ref i, false).Statement)
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
                    throw new JSException(new SyntaxError("Unexpected token at " + Tools.PositionToTextcord(state.Code, i)));
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
                Statement = new VariableDefineStatement(names.ToArray(), inits, isConst)
                {
                    Position = pos,
                    Length = index - pos,
                }
            };
        }

#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            if (initializators.Length == 1)
                return initializators[0].CompileToIL(state);
            return System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Block(from x in initializators select x.CompileToIL(state)), JITHelpers.UndefinedConstant).Reduce();
        }

#endif

        internal override JSObject Evaluate(Context context)
        {
            for (int i = 0; i < initializators.Length; i++)
                initializators[i].Evaluate(context);
            if (isConst)
            {
                for (var i = this.variables.Length; i-- > 0; )
                    this.variables[i].cacheRes.attributes |= JSObjectAttributesInternal.ReadOnly;
            }
            return JSObject.undefined;
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>();
            res.AddRange(initializators);
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            for (int i = 0; i < initializators.Length; i++)
                Parser.Build(ref initializators[i], 2, variables, strict);
            if (names.Length == 1 && depth < 2 && depth >= 0)
            {
                if (initializators[0] is GetVariableStatement)
                    _this = null;
                else
                    _this = initializators[0];
                for (var i = 0; i < names.Length; i++)
                {
                    var t = variables[names[i]];
                    t.Defined = true;
                    t.readOnly = isConst;
                }
            }
            else
            {
                this.variables = new VariableDescriptor[names.Length];
                for (var i = 0; i < names.Length; i++)
                {
                    this.variables[i] = variables[names[i]];
                    this.variables[i].Defined = true;
                    this.variables[i].readOnly = isConst;
                }
            }
            return false;
        }

        public override string ToString()
        {
            var res = isConst ? "conts " : "var ";
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