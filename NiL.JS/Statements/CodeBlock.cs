//#define JIT

using System;
using System.Collections.Generic;
using System.Linq;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class CodeBlock : CodeNode
    {
        private static readonly VariableDescriptor[] emptyVariables = new VariableDescriptor[0];

        private string code;
#if (NET40 || INLINE) && JIT
        internal Func<Context, JSObject> compiledVersion;
#endif
        internal bool builded;
#if DEBUG
        internal HashSet<string> directives;
#endif
        internal VariableDescriptor[] variables;
        internal VariableDescriptor[] localVariables;
        internal CodeNode[] lines;
        internal readonly bool strict;

        public VariableDescriptor[] Variables { get { return variables; } }
        public VariableDescriptor[] LocalVariables { get { return localVariables; } }
        public CodeNode[] Body { get { return lines; } }
        public bool Strict { get { return strict; } }
        public string Code
        {
            get
            {
                if (base.Length >= 0)
                {
                    lock (this)
                    {
                        if (base.Length >= 0)
                        {
                            if (Position != 0)
                                code = code.Substring(Position + 1, Length - 2);
                            Length = -base.Length;
                            return code;
                        }
                    }
                }
                return code;
            }
        }

        public override int Length
        {
            get
            {
                return base.Length < 0 ? -base.Length : base.Length;
            }
            internal set
            {
                base.Length = value;
            }
        }

        public CodeBlock(CodeNode[] body, bool strict)
        {
            if (body == null)
                throw new ArgumentNullException("body");
            code = "";
            this.lines = body;
            variables = null;
            this.strict = strict;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            bool sroot = i == 0 && state.AllowDirectives;
            if (!sroot)
            {
                if (state.Code[i] != '{')
                    throw new ArgumentException("code (" + i + ")");
                i++;
            }
            while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]))
                i++;
            var body = new List<CodeNode>();
            state.LabelCount = 0;
            bool strictSwitch = false;
            bool allowDirectives = state.AllowDirectives;
            HashSet<string> directives = null;
            state.AllowDirectives = false;
            if (allowDirectives)
            {
                int start = i;
                do
                {
                    var s = i;
                    if (i >= state.Code.Length)
                        break;
                    if (Parser.ValidateValue(state.Code, ref i))
                    {
                        while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]))
                            i++;
                        if (i < state.Code.Length && (Parser.isOperator(state.Code[i])
                            || Parser.Validate(state.Code, "instanceof", i)
                            || Parser.Validate(state.Code, "in", i)))
                        {
                            i = s;
                            break;
                        }
                        var t = s;
                        if (Parser.ValidateString(state.Code, ref t, true))
                        {
                            var str = state.Code.Substring(s + 1, t - s - 2);
                            if (!strictSwitch && str == "use strict")
                            {
                                state.strict.Push(true);
                                strictSwitch = true;
                            }
                            if (directives == null)
                                directives = new HashSet<string>();
                            directives.Add(str);
                        }
                        else
                        {
                            i = s;
                            break;
                        }
                    }
                    else if (state.Code[i] == ';')
                    {
                        if (directives == null)
                            break;
                        do
                            i++;
                        while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]));
                    }
                    else
                        break;
                } while (true);
                i = start;
            }
            for (var j = body.Count; j-- > 0; )
                (body[j] as Constant).value.oValue = Tools.Unescape((body[j] as Constant).value.oValue.ToString(), state.strict.Peek());

            bool expectSemicolon = false;
            while ((sroot && i < state.Code.Length) || (!sroot && state.Code[i] != '}'))
            {
                var t = Parser.Parse(state, ref i, 0);
                if (t == null || t is EmptyStatement)
                {
                    if (sroot && i < state.Code.Length && state.Code[i] == '}')
                        throw new JSException(new SyntaxError("Unexpected symbol \"}\" at " + CodeCoordinates.FromTextPosition(state.Code, i, 0)));
                    if (state.message != null
                        && !expectSemicolon
                        && (state.Code[i - 1] == ';' || state.Code[i - 1] == ','))
                        state.message(MessageLevel.Warning, CodeCoordinates.FromTextPosition(state.Code, i, 1), "Unnecessary semicolon.");
                    expectSemicolon = false;
                    continue;
                }
                if (t is FunctionExpression)
                {
                    if (state.strict.Peek() && !allowDirectives)
                        throw new JSException((new NiL.JS.BaseLibrary.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                    if (state.InExpression == 0 && string.IsNullOrEmpty((t as FunctionExpression).Name))
                        throw new JSException((new NiL.JS.BaseLibrary.SyntaxError("Declarated function must have name.")));
                    expectSemicolon = false;
                }
                else
                {
                    expectSemicolon = true;
                }
                body.Add(t);
            }
            if (!sroot)
                i++;
            int startPos = index;
            index = i;
            body.Reverse();
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new CodeBlock(body.ToArray(), strictSwitch ? state.strict.Pop() : state.strict.Peek())
                {
                    variables = emptyVariables,
                    Position = startPos,
                    code = state.SourceCode,
                    Length = i - startPos,
#if DEBUG
                    directives = directives
#endif
                }
            };
        }

        internal sealed override JSObject Evaluate(Context context)
        {
            var ls = lines;
            for (int i = ls.Length; i-- > 0; )
            {
#if DEV
                if (context.debugging)
                    context.raiseDebugger(lines[i]);
#endif
                var t = ls[i].Evaluate(context);
                if (t != null)
                    context.lastResult = t;
#if DEBUG && !PORTABLE
                if (!context.IsExcecuting)
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("Context was stopped");
                if (NiL.JS.BaseLibrary.Number.NaN.valueType != JSObjectType.Double || !double.IsNaN(NiL.JS.BaseLibrary.Number.NaN.dValue))
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("NaN was rewrite");
                if (JSObject.undefined.valueType != JSObjectType.Undefined)
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("undefined was rewrite");
                if (JSObject.notExists.IsExist)
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("notExist was rewrite");
                if (BaseLibrary.Boolean.False.valueType != JSObjectType.Bool
                    || BaseLibrary.Boolean.False.iValue != 0)
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("Boolean.False was rewrite");
                if (BaseLibrary.Boolean.True.valueType != JSObjectType.Bool
                    || BaseLibrary.Boolean.True.iValue != 1)
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("Boolean.True was rewrite");
#endif
                if (context.abort != AbortType.None)
                    return null;
            }
            return null;
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>();
            for (int i = lines.Length; i-- > 0; )
            {
                var node = lines[i];
                if (node == null)
                    break;
                res.Add(node);
            }
            if (variables != null)
                res.AddRange(from v in variables where v.Inititalizator != null && (!(v.Inititalizator is FunctionExpression) || (v.Inititalizator as FunctionExpression).body != this) select v.Inititalizator);
            return res.ToArray();
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            if (builded)
                return false;

            for (int i = lines.Length; i-- > 0; )
            {
                var fe = lines[i] as FunctionExpression;
                if (fe != null)
                {
                    Parser.Build(ref lines[i], depth < 0 ? 2 : System.Math.Max(1, depth), variables, state | (this.strict ? _BuildState.Strict : _BuildState.None), message, statistic, opts);
                    VariableDescriptor desc = null;
                    if (!variables.TryGetValue(fe.name, out desc) || desc == null)
                        variables[fe.name] = fe.Reference.descriptor;
                    else
                    {
                        variables[fe.name] = fe.Reference.descriptor;
                        for (var j = 0; j < desc.references.Count; j++)
                            desc.references[j].descriptor = fe.Reference.descriptor;
                        fe.Reference.descriptor.references.AddRange(desc.references);
                        fe.Reference.descriptor.captured = fe.Reference.descriptor.captured || fe.Reference.descriptor.references.FindIndex(x => x.functionDepth > x.descriptor.defineDepth) != -1;
                    }
                    lines[i] = null;
                }
            }
            bool unreachable = false;
            for (int i = lines.Length; i-- > 0; )
            {
                if (lines[i] != null)
                {
                    if (lines[i] is EmptyStatement)
                        lines[i] = null;
                    else
                    {
                        if (unreachable && message != null)
                            message(MessageLevel.CriticalWarning, new CodeCoordinates(0, lines[i].Position, lines[i].Length), "Unreachable code detected.");
                        var cn = lines[i];
                        Parser.Build(ref cn, depth < 0 ? 2 : System.Math.Max(1, depth), variables, state | (this.strict ? _BuildState.Strict : _BuildState.None), message, statistic, opts);
                        lines[i] = cn;
                        unreachable |= cn is ReturnStatement || cn is BreakStatement || cn is ContinueStatement;
                    }
                }
            }

            int f = lines.Length, t = lines.Length - 1;
            for (; f-- > 0; )
            {
                if (lines[f] != null && lines[t] == null)
                {
                    lines[t] = lines[f];
                    lines[f] = null;
                }
                if (lines[t] != null)
                    t--;
            }

            if (depth > 0)
            {
                variables = null;
                if (lines.Length == 1 || (t >= 0 && (lines.Length - t - 1) == 1))
                    _this = lines[lines.Length - 1] ?? EmptyStatement.Instance; // блок не должен быть null, так как он может быть вложен в выражение
                else if (lines.Length == 0)
                    _this = EmptyStatement.Instance;
            }
            else
            {
                if (variables.Count != 0 &&
                    (this.variables == null || this.variables.Length != variables.Count))
                    this.variables = variables.Values.ToArray();
                if (this.variables != null)
                {
                    int localVariablesCount = 0;
                    for (var i = this.variables.Length; i-- > 0; )
                    {
                        if (this.variables[i].IsDefined && this.variables[i].Owner == null) // все объявленные переменные без хозяина наши
                            this.variables[i].owner = this;
                        if (this.variables[i].owner == this)
                            localVariablesCount++;
                    }
                    this.localVariables = new VariableDescriptor[localVariablesCount];
                    for (var i = this.variables.Length; i-- > 0; )
                    {
                        if (this.variables[i].owner == this)
                            localVariables[--localVariablesCount] = this.variables[i];
                    }
                }
                if (message != null)
                {
                    for (var i = 0; i < localVariables.Length; i++)
                    {
                        if (localVariables[i].ReferenceCount == 1)
                            message(MessageLevel.Recomendation, new CodeCoordinates(0, localVariables[i].references[0].Position, 0), "Unused variable \"" + localVariables[i].name + "\"");
                    }
                }
#if (NET40 || INLINE) && JIT
                compiledVersion = JITHelpers.compile(this, depth >= 0);
#endif
            }
            if (t >= 0 && this == _this)
            {
                var newBody = new CodeNode[lines.Length - t - 1];
                f = 0;
                while (++t < lines.Length)
                    newBody[f++] = lines[t];
                lines = newBody;
            }
            builded = true;
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            if (localVariables != null)
                for (var i = 0; i < localVariables.Length; i++)
                {
                    if (localVariables[i].Inititalizator != null)
                    {
                        var cn = localVariables[i].Inititalizator as CodeNode;
                        cn.Optimize(ref cn, owner, message, opts, statistic);
                    }
                }
            for (var i = lines.Length; i-- > 0; )
            {
                var cn = lines[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, statistic);
                lines[i] = cn;
            }
            if (localVariables != null)
                for (var i = 0; i < localVariables.Length; i++)
                {
                    if (localVariables[i].Inititalizator != null)
                    {
                        var cn = localVariables[i].Inititalizator as CodeNode;
                        cn.Optimize(ref cn, owner, message, opts, statistic);
                    }
                }
        }
#if !PORTABLE
        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            for (int i = lines.Length; i-- > 0; )
                lines[i].TryCompile(true, false, null, dynamicValues);
            for (int i = localVariables.Length; i-- > 0; )
                if (localVariables[i].Inititalizator != null)
                    localVariables[i].Inititalizator.TryCompile(true, false, null, dynamicValues);
            return null;
        }
#endif
        public override string ToString()
        {
            return ToString(false);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public string ToString(bool parsed)
        {
            if (parsed)
            {
                if (lines == null || lines.Length == 0)
                    return "{ }";
                string res = " {" + Environment.NewLine;
                var replp = Environment.NewLine;
                var replt = Environment.NewLine + "  ";
                for (int i = lines.Length; i-- > 0; )
                {
                    string lc = lines[i].ToString();
                    if (lc[0] == '(')
                        lc = lc.Substring(1, lc.Length - 2);
                    lc = lc.Replace(replp, replt);
                    res += "  " + lc + (lc[lc.Length - 1] != '}' ? ";" + Environment.NewLine : Environment.NewLine);
                }
                return res + "}";
            }
            else
                return '{' + Code + '}';
        }
    }
}