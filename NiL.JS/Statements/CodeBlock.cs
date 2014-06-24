using System;
using System.Collections.Generic;
using System.Linq;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Statements.Operators;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class CodeBlock : Statement
    {
        private static readonly VariableDescriptor[] emptyVariables = new VariableDescriptor[0];

        private string code;
        internal VariableDescriptor[] variables;
        internal readonly Statement[] body;
        internal readonly bool strict;

        public VariableDescriptor[] Variables { get { return variables; } }
        public Statement[] Body { get { return body; } }
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

        public CodeBlock(Statement[] body, bool strict)
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
            string code = state.Code;
            int i = index;
            bool sroot = i == 0 && state.AllowStrict;
            if (!sroot)
            {
                if (code[i] != '{')
                    throw new ArgumentException("code (" + i + ")");
                i++;
            }
            while (i < code.Length && char.IsWhiteSpace(code[i])) i++;
            var body = new List<Statement>();
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
                    if (i >= code.Length)
                        break;
                    if (Parser.ValidateValue(code, ref i))
                    {
                        while (i < code.Length && char.IsWhiteSpace(code[i])) i++;
                        if (i < code.Length && (Parser.isOperator(code[i])
                            || Parser.Validate(code, "instanceof", i)
                            || Parser.Validate(code, "in", i)))
                        {
                            i = s;
                            break;
                        }
                        var t = s;
                        if (Parser.ValidateString(code, ref t))
                        {
                            var str = code.Substring(s + 1, t - s - 2);
                            if (!strictSwitch && str == "use strict")
                            {
                                state.strict.Push(true);
                                strictSwitch = true;
                            }
                            if (directives == null)
                                directives = new HashSet<string>();
                            directives.Add(str);
                            body.Add(new ImmidateValueStatement(str));
                        }
                        else
                        {
                            i = s;
                            break;
                        }
                    }
                    else if (code[i] == ';')
                    {
                        if (directives == null)
                            break;
                        do i++; while (i < code.Length && char.IsWhiteSpace(code[i]));
                    }
                    else break;
                } while (true);
            }
            for (var j = body.Count; j-- > 0; )
                (body[j] as ImmidateValueStatement).value.oValue = Tools.Unescape((body[j] as ImmidateValueStatement).value.oValue.ToString(), state.strict.Peek());
            Dictionary<string, VariableDescriptor> vars = null;
            while ((sroot && i < code.Length) || (!sroot && code[i] != '}'))
            {
                var t = Parser.Parse(state, ref i, 0);
                if (t == null || t is EmptyStatement)
                {
                    if (sroot && i < code.Length && code[i] == '}')
                        throw new JSException(new SyntaxError("Unexpected symbol \"}\" at " + Tools.PositionToTextcord(code, i)));
                    continue;
                }
                if (t is FunctionStatement)
                {
                    if (state.strict.Peek() && !allowStrict)
                        throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                    if (state.InExpression == 0 && string.IsNullOrEmpty((t as FunctionStatement).Name))
                        throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("Declarated function must have name.")));
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
                        f.Reference.Descriptor = vd;
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
                        var gvs = (inits[j] as GetVariableStatement) ?? ((inits[j] as Assign).first as GetVariableStatement);
                        if (!vars.TryGetValue(gvs.Name, out desc))
                            vars[gvs.Name] = new VariableDescriptor(gvs, true, state.functionsDepth);
                        else
                        {
                            desc.references.Add(gvs);
                            gvs.Descriptor = desc;
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

        internal override JSObject Invoke(Context context)
        {
            JSObject res = JSObject.notExist;
            for (int i = body.Length; i-- > 0; )
            {
                if (body[i] is FunctionStatement) continue;
#if DEV
                if (context.debugging)
                    context.raiseDebugger(body[i]);
#endif
                res = body[i].Invoke(context) ?? res;
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
                if (JSObject.notExist.valueType >= JSObjectType.Undefined)
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

        protected override Statement[] getChildsImpl()
        {
            var res = new List<Statement>();
            res.AddRange(body);
            res.RemoveAll(x => x == null || x is FunctionStatement);
            if (variables != null)
                res.AddRange(from v in variables where v.Inititalizator != null && (!(v.Inititalizator is FunctionStatement) || (v.Inititalizator as FunctionStatement).body != this) select v.Inititalizator);
            return res.ToArray();
        }

        internal override bool Optimize(ref Statement _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            if (this.variables != null)
            {
                for (var i = this.variables.Length; i-- > 0; )
                {
                    VariableDescriptor desc = null;
                    if (depth == 0 || !variables.TryGetValue(this.variables[i].Name, out desc) || desc == null)
                        variables[this.variables[i].Name] = this.variables[i];
                    else
                    {
                        foreach (var r in this.variables[i].References)
                        {
                            desc.references.Add(r);
                            r.Descriptor = desc;
                        }
                        this.variables[i] = desc;
                    }
                }
            }

            for (int i = body.Length; i-- > 0; )
                Parser.Optimize(ref body[i], depth < 0 ? 2 : Math.Max(1, depth), fdepth, variables, this.strict);

            if (depth > 0)
            {
                variables = null;
                if (body.Length == 1)
                    _this = body[0];
                if (body.Length == 0)
                    _this = EmptyStatement.Instance;
            }
            else
            {
                if (variables.Count != 0 &&
                    (this.variables == null || this.variables.Length != variables.Count))
                    this.variables = variables.Values.ToArray();
                if (this.variables != null)
                    for (var i = this.variables.Length; i-- > 0; )
                        if (this.variables[i].Defined && this.variables[i].Owner == null) // все объявленные переменные без хозяина наши
                        {
                            this.variables[i].Owner = this;
                            this.variables[i].defineDepth = fdepth;
                        }
            }
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