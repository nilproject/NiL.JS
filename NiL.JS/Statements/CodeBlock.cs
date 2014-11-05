//#define JIT

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
    public sealed class CodeBlock : CodeNode
    {
        private static readonly VariableDescriptor[] emptyVariables = new VariableDescriptor[0];

        private string code;
#if (NET40 || INLINE) && JIT
        internal Func<Context, JSObject> compiledVersion;
#endif
        private bool builded;
        internal VariableDescriptor[] variables;
        internal VariableDescriptor[] localVariables;
        internal CodeNode[] body;
        internal readonly bool strict;

        public VariableDescriptor[] Variables { get { return variables; } }
        public VariableDescriptor[] LocalVariables { get { return localVariables; } }
        public CodeNode[] Body { get { return body; } }
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

#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            for (int i = body.Length >> 1; i-- > 0; )
            {
                var t = body[i];
                body[i] = body[body.Length - i - 1];
                body[body.Length - i - 1] = t;
            }
            try
            {
                if (body.Length == 1)
                    return body[0].CompileToIL(state);
                if (body.Length == 0)
                    return JITHelpers.UndefinedConstant;
                return System.Linq.Expressions.Expression.Block(from x in body where x != null select x.CompileToIL(state));
            }
            finally
            {
                for (int i = body.Length >> 1; i-- > 0; )
                {
                    var t = body[i];
                    body[i] = body[body.Length - i - 1];
                    body[body.Length - i - 1] = t;
                }
            }
        }

#endif

        public CodeBlock(CodeNode[] body, bool strict)
        {
            if (body == null)
                throw new ArgumentNullException("body");
            code = "";
            this.body = body;
            variables = null;
            this.strict = strict;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            bool sroot = i == 0 && state.AllowStrict;
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
            bool allowStrict = state.AllowStrict;
            HashSet<string> directives = null;
            state.AllowStrict = false;
            if (allowStrict)
            {
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
                            body.Add(new Constant(str));
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
            }
            for (var j = body.Count; j-- > 0; )
                (body[j] as Constant).value.oValue = Tools.Unescape((body[j] as Constant).value.oValue.ToString(), state.strict.Peek());
            Dictionary<string, VariableDescriptor> vars = null;
            while ((sroot && i < state.Code.Length) || (!sroot && state.Code[i] != '}'))
            {
                var t = Parser.Parse(state, ref i, 0);
                if (t == null || t is EmptyStatement)
                {
                    if (sroot && i < state.Code.Length && state.Code[i] == '}')
                        throw new JSException(new SyntaxError("Unexpected symbol \"}\" at " + Tools.PositionToTextcord(state.Code, i)));
                    continue;
                }
                if (t is FunctionStatement)
                {
                    if (state.strict.Peek() && !allowStrict)
                        throw new JSException((new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                    if (state.InExpression == 0 && string.IsNullOrEmpty((t as FunctionStatement).Name))
                        throw new JSException((new NiL.JS.Core.BaseTypes.SyntaxError("Declarated function must have name.")));
                    if (vars == null)
                        vars = new Dictionary<string, VariableDescriptor>();
                    VariableDescriptor vd = null;
                    var f = (t as FunctionStatement);
                    if (!vars.TryGetValue(f.Name, out vd))
                        vars[f.Name] = new VariableDescriptor(f.Reference, true, state.functionsDepth);
                    else
                    {
                        f.Reference.functionDepth = state.functionsDepth;
                        vd.references.Add(f.Reference);
                        f.Reference.descriptor = vd;
                        vd.Inititalizator = f.Reference;
                    }
                }
                else if (t is VariableDefineStatement)
                {
                    var inits = (t as VariableDefineStatement).initializators;
                    if (vars == null)
                        vars = new Dictionary<string, VariableDescriptor>();
                    for (var j = inits.Length; j-- > 0; )
                    {
                        VariableDescriptor desc = null;
                        var gvs = (inits[j] as VariableReference) ?? ((inits[j] as Assign).FirstOperand as VariableReference);
                        if (!vars.TryGetValue(gvs.Name, out desc))
                            vars[gvs.Name] = new VariableDescriptor(gvs, true, state.functionsDepth);
                        else
                        {
                            desc.references.Add(gvs);
                            gvs.descriptor = desc;
                        }
                    }
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
                    variables = vars != null ? vars.Values.ToArray() : emptyVariables,
                    Position = startPos,
                    code = state.SourceCode,
                    Length = i - startPos
                }
            };
        }

        internal override JSObject Evaluate(Context context)
        {
#if (NET40 || INLINE) && JIT
            if (compiledVersion != null)
            {
                context.abortInfo = compiledVersion(context);
                context.abort = AbortType.Return;
                return context.abortInfo;
            }
#endif
            JSObject res = JSObject.notExists;
            for (int i = body.Length; i-- > 0; )
            {
#if DEV
                if (context.debugging)
                    context.raiseDebugger(body[i]);
#endif
                res = body[i].Evaluate(context) ?? res;
#if DEBUG
                if (!context.IsExcecuting)
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("Context was stopped");
                if (NiL.JS.Core.BaseTypes.Number.NaN.valueType != JSObjectType.Double || !double.IsNaN(NiL.JS.Core.BaseTypes.Number.NaN.dValue))
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("NaN was rewrite");
                if (JSObject.undefined.valueType != JSObjectType.Undefined)
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("undefined was rewrite");
                if (JSObject.notExists.isExist)
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("notExist was rewrite");
                if (Core.BaseTypes.Boolean.False.valueType != JSObjectType.Bool
                    || Core.BaseTypes.Boolean.False.iValue != 0)
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("Boolean.False was rewrite");
                if (Core.BaseTypes.Boolean.True.valueType != JSObjectType.Bool
                    || Core.BaseTypes.Boolean.True.iValue != 1)
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("Boolean.True was rewrite");
#endif
                if (context.abort != AbortType.None)
                    return context.abort == AbortType.Return ? context.abortInfo : res;
            }
            return res;
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>();
            for (int i = body.Length; i-- > 0; )
            {
                var node = body[i];
                if (node == null)
                    break;
                res.Add(node);
            }
            if (variables != null)
                res.AddRange(from v in variables where v.Inititalizator != null && (!(v.Inititalizator is FunctionStatement) || (v.Inititalizator as FunctionStatement).body != this) select v.Inititalizator);
            return res.ToArray();
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            if (builded)
                return false;
            if (this.variables != null)
            {
                for (var i = this.variables.Length; i-- > 0; )
                {
                    VariableDescriptor desc = null;
                    if (depth == 0 || !variables.TryGetValue(this.variables[i].name, out desc) || desc == null)
                        variables[this.variables[i].name] = this.variables[i];
                    else
                    {
                        foreach (var r in this.variables[i].References)
                        {
                            desc.references.Add(r);
                            r.descriptor = desc;
                        }
                        this.variables[i] = desc;
                    }
                }
            }

            for (int i = body.Length; i-- > 0; )
                if (body[i] is FunctionStatement)
                {
                    Parser.Build(ref body[i], depth < 0 ? 2 : Math.Max(1, depth), variables, this.strict);
                    body[i] = null;
                }

            for (int i = body.Length; i-- > 0; )
            {
                if (body[i] != null)
                {
                    if (body[i] is EmptyStatement)
                        body[i] = null;
                    else
                        Parser.Build(ref body[i], depth < 0 ? 2 : Math.Max(1, depth), variables, this.strict);
                }
            }

            int f = body.Length, t = body.Length - 1;
            for (; f-- > 0; )
            {
                if (body[f] != null && body[t] == null)
                {
                    body[t] = body[f];
                    body[f] = null;
                }
                if (body[t] != null)
                    t--;
            }

            if (depth > 0)
            {
                variables = null;
                if (body.Length == 1 || (t >= 0 && (body.Length - t - 1) == 1))
                    _this = body[body.Length - 1] ?? EmptyStatement.Instance; // блок не должен быть null, так как он может быть вложен в выражение
                else if (body.Length == 0)
                    _this = EmptyStatement.Instance;
            }
            else
            {

                if (body.Length > 0 && body[0] is Call)
                    (body[0] as Call).allowTCO = true;
                if (variables.Count != 0 &&
                    (this.variables == null || this.variables.Length != variables.Count))
                    this.variables = variables.Values.ToArray();
                if (this.variables != null)
                {
                    int localVariablesCount = 0;
                    for (var i = this.variables.Length; i-- > 0; )
                    {
                        if (this.variables[i].Defined && this.variables[i].Owner == null) // все объявленные переменные без хозяина наши
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
#if (NET40 || INLINE) && JIT
                compiledVersion = JITHelpers.compile(this, depth >= 0);
#endif
            }
            if (t >= 0 && this == _this)
            {
                var newBody = new CodeNode[body.Length - t - 1];
                f = 0;
                while (++t < body.Length)
                    newBody[f++] = body[t];
                body = newBody;
            }
            builded = true;
            return false;
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool parsed)
        {
            if (parsed)
            {
                if (body == null || body.Length == 0)
                    return "{ }";
                string res = " {" + Environment.NewLine;
                var replp = Environment.NewLine;
                var replt = Environment.NewLine + "  ";
                for (int i = body.Length; i-- > 0; )
                {
                    string lc = body[i].ToString();
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