using NiL.JS.Core;
using NiL.JS.Statements.Operators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class CodeBlock : Statement
    {
        private int linesCount;
        private string code;
        internal VaribleDescriptor[] varibles;
        internal readonly Statement[] body;
        internal readonly bool strict;

        public VaribleDescriptor[] Varibles { get { return varibles; } }
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
            code = "";
            this.body = body;
            linesCount = body.Length - 1;
            varibles = null;
            this.strict = strict;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            bool sroot = i == 0;
            if (!sroot)
            {
                if (code[i] != '{')
                    throw new ArgumentException("code (" + i + ")");
                do
                    i++;
                while (char.IsWhiteSpace(code[i]));
            }
            var body = new List<Statement>();
            state.LabelCount = 0;
            bool strictSwitch = false;
            bool allowStrict = state.AllowStrict;
            bool root = state.AllowStrict;
            state.AllowStrict = false;
            Dictionary<string, VaribleDescriptor> vars = null;
            while ((sroot && i < code.Length) || (!sroot && code[i] != '}'))
            {
                var t = Parser.Parse(state, ref i, 0);
                if (allowStrict)
                {
                    allowStrict = false;
                    if (t is ImmidateValueStatement)
                    {
                        var op = (t as ImmidateValueStatement).value.Value;
                        if ("use strict".Equals(op))
                        {
                            state.strict.Push(true);
                            strictSwitch = true;
                            continue;
                        }
                    }
                }
                if (t == null || t is EmptyStatement)
                    continue;
                if (t is FunctionStatement)
                {
                    if (state.strict.Peek())
                        if (!root)
                            throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                    if (string.IsNullOrEmpty((t as FunctionStatement).name))
                        throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("Declarated function must have name.")));
                    if (vars == null)
                        vars = new Dictionary<string, VaribleDescriptor>();
                    VaribleDescriptor vd = null;
                    var f = (t as FunctionStatement);
                    if (!vars.TryGetValue(f.Name, out vd))
                        vars[f.Name] = new VaribleDescriptor(f.Reference, true);
                    else
                    {
                        vd.Add(f.Reference);
                        vd.Inititalizator = f.Reference;
                    }
                }
                else if (t is VaribleDefineStatement)
                {
                    var inits = (t as VaribleDefineStatement).initializators;
                    if (vars == null)
                        vars = new Dictionary<string, VaribleDescriptor>();
                    for (var j = inits.Length; j-- > 0; )
                    {
                        VaribleDescriptor vd = null;
                        var gvs = (inits[j] as GetVaribleStatement) ?? ((inits[j] as Assign).first as GetVaribleStatement);
                        if (!vars.TryGetValue(gvs.Name, out vd))
                            vars[gvs.Name] = new VaribleDescriptor(gvs, true);
                        else
                            vd.Add(gvs);
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
                Statement = new CodeBlock(body.ToArray(), strictSwitch && state.strict.Pop())
                {
                    varibles = vars != null ? vars.Values.ToArray() : null,
                    Position = startPos,
                    code = state.SourceCode,
                    Length = i - startPos
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            context.strict |= strict;
            for (int i = varibles == null ? 0 : varibles.Length; i-- > 0; )
            {
                if (varibles[i].Defined)
                {
                    var f = context.InitField(varibles[i].Name);
                    if (varibles[i].Inititalizator != null)
                        f.Assign(varibles[i].Inititalizator.Invoke(context));
                }
            }
            JSObject res = JSObject.undefined;
            for (int i = linesCount; i >= 0; i--)
            {
                if (body[i] is FunctionStatement) continue;
                if (context.debugging)
                    context.raiseDebugger(body[i]);
                res = body[i].Invoke(context) ?? res;
#if DEBUG
                if (JSObject.undefined.ValueType != JSObjectType.Undefined)
                    throw new ApplicationException("undefined was rewrite");
                if (Core.BaseTypes.String.EmptyString.oValue as string != "")
                    throw new ApplicationException("EmptyString was rewrite");
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
            if (varibles != null)
                res.AddRange(from v in varibles where v.Inititalizator != null select v.Inititalizator);
            return res.ToArray();
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VaribleDescriptor> varibles)
        {
            if (this.varibles != null)
            {
                for (var i = this.varibles.Length; i-- > 0; )
                {
                    varibles[this.varibles[i].Name] = this.varibles[i];
                    if (depth == 0)
                        this.varibles[i].Owner = this;
                }
            }
            for (int i = body.Length; i-- > 0; )
            {
                Parser.Optimize(ref body[i], depth < 0 ? 2 : Math.Max(1, depth), varibles);
                //if (depth >= 0 && (body[i] is FunctionStatement)) body[i] = null;
            }

            if (depth != 0)
            {
                varibles = null;
                if (body.Length == 1)
                    _this = body[0];
                if (body.Length == 0)
                    _this = EmptyStatement.Instance;
            }
            else
            {
                List<VaribleDescriptor> vars = null;
                foreach (var v in varibles)
                {
                    if (v.Value.Defined && v.Value.Owner == null)
                    {
                        if (vars == null)
                            vars = this.varibles != null ? new List<VaribleDescriptor>(this.varibles) : new List<VaribleDescriptor>();
                        vars.Add(v.Value);
                    }
                }
                if (vars != null)
                    this.varibles = vars.ToArray();
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