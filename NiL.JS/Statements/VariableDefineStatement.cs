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
        private sealed class AllowWriteCN : Expression
        {
            internal VariableReference variable;
            internal readonly CodeNode source;

            internal AllowWriteCN(VariableReference variable, Expression source)
            {
                this.variable = variable;
                this.source = source;
            }

            internal override JSObject Evaluate(Context context)
            {
                var res = source.Evaluate(context);
                var v = variable.Evaluate(context);
                if ((v.attributes & JSObjectAttributesInternal.SystemObject) == 0)
                    v.attributes &= ~JSObjectAttributesInternal.ReadOnly;
                return res;
            }

            internal override JSObject EvaluateForAssing(Context context)
            {
                var res = source.EvaluateForAssing(context);
                var v = variable.Evaluate(context);
                if ((v.attributes & JSObjectAttributesInternal.SystemObject) == 0)
                    v.attributes &= ~JSObjectAttributesInternal.ReadOnly;
                return res;
            }

            internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
            {
                var v = variable as CodeNode;
                var res = variable.Build(ref v, depth, variables, strict);
                variable = v as VariableReference;
                return res;
            }

            protected override CodeNode[] getChildsImpl()
            {
                return variable.Childs;
            }

            public override string ToString()
            {
                return source.ToString();
            }
        }
        internal int functionDepth;
        internal VariableDescriptor[] variables;
        internal CodeNode[] initializators;
        internal readonly string[] names;
        internal readonly bool isConst;

        public bool Const { get { return isConst; } }
        public CodeNode[] Initializators { get { return initializators; } }
        public string[] Names { get { return names; } }

        internal VariableDefineStatement(string name, CodeNode init, bool isConst, int functionDepth)
        {
            names = new[] { name };
            initializators = new[] { init };
            this.isConst = isConst;
            this.functionDepth = functionDepth;
        }

        private VariableDefineStatement(string[] names, CodeNode[] initializators, bool isConst, int functionDepth)
        {
            this.initializators = initializators;
            this.names = names;
            this.isConst = isConst;
            this.functionDepth = functionDepth;
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
                    initializator.Add(new GetVariableExpression(name, state.functionsDepth) { Position = s, Length = name.Length, functionDepth = state.functionsDepth });
                    break;
                }
                if (Tools.isLineTerminator(state.Code[i]))
                {
                    s = i;
                    do i++; while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]));
                    if (i >= state.Code.Length)
                    {
                        initializator.Add(new GetVariableExpression(name, state.functionsDepth) { Position = s, Length = name.Length, functionDepth = state.functionsDepth });
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
                    VariableReference accm = new GetVariableExpression(name, state.functionsDepth) { Position = s, Length = name.Length, functionDepth = state.functionsDepth };
                    Expression source = ExpressionTree.Parse(state, ref i, false).Statement as Expression;
                    if (isConst)
                        source = new AllowWriteCN(accm, source);
                    initializator.Add(
                        new Assign(
                            accm,
                            source)
                        {
                            Position = s,
                            Length = i - s
                        });
                }
                else
                {
                    //if (isConst)
                    //    throw new JSException(new SyntaxError("Constant must contain value at " + Tools.PositionToTextcord(state.Code, i)));
                    initializator.Add(new GetVariableExpression(name, state.functionsDepth) { Position = s, Length = name.Length, functionDepth = state.functionsDepth });
                }
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
                Statement = new VariableDefineStatement(names.ToArray(), inits, isConst, state.functionsDepth)
                {
                    Position = pos,
                    Length = index - pos
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
            {
                initializators[i].Evaluate(context);
                if (isConst)
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
            this.variables = new VariableDescriptor[names.Length];
            for (var i = 0; i < names.Length; i++)
            {
                VariableDescriptor desc = null;
                if (!variables.TryGetValue(names[i], out desc))
                    variables[names[i]] = desc = new VariableDescriptor(names[i], functionDepth);
                this.variables[i] = desc;
                this.variables[i].isDefined = true;
                this.variables[i].readOnly = isConst;
            }
            int actualChilds = 0;
            for (int i = 0; i < initializators.Length; i++)
            {
                Parser.Build(ref initializators[i], 1, variables, strict);
                if (initializators[i] != null)
                    actualChilds++;
            }
            if (this == _this && actualChilds < initializators.Length)
            {
                if (actualChilds == 0)
                {
                    _this = null;
                    return false;
                }
                var newinits = new CodeNode[actualChilds];
                for (int i = 0, j = 0; i < initializators.Length; i++)
                    if (initializators[i] != null)
                        newinits[j++] = initializators[i];
                initializators = newinits;
            }
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner)
        {
            for (int i = 0; i < initializators.Length; i++)
            {
                initializators[i].Optimize(ref initializators[i], owner);
            }
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