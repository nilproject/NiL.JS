//#define JIT

using System;
using System.Collections.Generic;
using System.Linq;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;
using System.Text;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class CodeBlock : CodeNode
    {
        private sealed class SuspendData
        {
            public int LineIndex;
            public Context Context;
        }

        private static readonly VariableDescriptor[] emptyVariables = new VariableDescriptor[0];

        private string code;
#if (NET40 || INLINE) && JIT
        internal Func<Context, JSObject> compiledVersion;
#endif
#if DEBUG
        internal HashSet<string> directives;
#endif
        internal VariableDescriptor[] _variables;
        internal CodeNode[] lines;
        internal bool strict;
        internal bool builded;
        internal bool suppressScopeIsolation;

        public VariableDescriptor[] Variables { get { return _variables; } }
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

        public CodeBlock(CodeNode[] body)
        {
            if (body == null)
                throw new ArgumentNullException("body");
            code = "";
            this.lines = body;
            _variables = null;
            this.strict = false;
        }

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            int position = index;
            bool sroot = position == 0 && state.AllowDirectives;
            if (!sroot)
            {
                if (state.Code[position] != '{')
                    throw new ArgumentException("code (" + position + ")");
                position++;
            }

            Tools.SkipSpaces(state.Code, ref position);

            var body = new List<CodeNode>();
            bool strictSwitch = false;
            bool allowDirectives = state.AllowDirectives;
            HashSet<string> directives = null;
            state.AllowDirectives = false;

            var oldFunctionScopeLevel = state.functionScopeLevel;
            state.lexicalScopeLevel++;
            if (allowDirectives)
                state.functionScopeLevel = state.lexicalScopeLevel;

            var oldVariablesCount = state.Variables.Count;
            VariableDescriptor[] variables = null;
            state.LabelCount = 0;
            try
            {
                if (allowDirectives)
                {
                    int start = position;
                    do
                    {
                        var s = position;
                        if (position >= state.Code.Length)
                            break;
                        if (Parser.ValidateValue(state.Code, ref position))
                        {
                            while (position < state.Code.Length && Tools.IsWhiteSpace(state.Code[position]))
                                position++;
                            if (position < state.Code.Length && (Parser.IsOperator(state.Code[position])
                                || Parser.Validate(state.Code, "instanceof", position)
                                || Parser.Validate(state.Code, "in", position)))
                            {
                                position = s;
                                break;
                            }
                            var t = s;
                            if (Parser.ValidateString(state.Code, ref t, true))
                            {
                                var str = state.Code.Substring(s + 1, t - s - 2);
                                if (!strictSwitch && str == "use strict" && !state.strict)
                                {
                                    state.strict = true;
                                    strictSwitch = true;
                                }
                                if (directives == null)
                                    directives = new HashSet<string>();
                                directives.Add(str);
                            }
                            else
                            {
                                position = s;
                                break;
                            }
                        }
                        else if (state.Code[position] == ';')
                        {
                            if (directives == null)
                                break;
                            do
                                position++;
                            while (position < state.Code.Length && Tools.IsWhiteSpace(state.Code[position]));
                        }
                        else
                            break;
                    }
                    while (true);
                    position = start;
                }

                for (var j = body.Count; j-- > 0;)
                    (body[j] as ConstantDefinition).value.oValue = Tools.Unescape((body[j] as ConstantDefinition).value.oValue.ToString(), state.strict);

                bool expectSemicolon = false;
                while ((sroot && position < state.Code.Length) || (!sroot && state.Code[position] != '}'))
                {
                    var t = Parser.Parse(state, ref position, 0);
                    if (t == null)
                    {
                        if (position < state.Code.Length)
                        {
                            if (sroot && state.Code[position] == '}')
                                ExceptionsHelper.Throw(new SyntaxError("Unexpected symbol \"}\" at " + CodeCoordinates.FromTextPosition(state.Code, position, 0)));

                            if ((state.Code[position] == ';' || state.Code[position] == ','))
                            {
                                if (state.message != null && !expectSemicolon)
                                    state.message(MessageLevel.Warning, CodeCoordinates.FromTextPosition(state.Code, position, 1), "Unnecessary semicolon.");

                                position++;
                            }

                            expectSemicolon = false;
                        }
                        continue;
                    }

                    if (t is EntityDefinition)
                    {
                        expectSemicolon = false;
                    }
                    else
                    {
                        expectSemicolon = true;
                    }

                    body.Add(t);
                }
            }
            finally
            {
                if (oldVariablesCount != state.Variables.Count)
                {
                    var count = 0;
                    for (var i = oldVariablesCount; i < state.Variables.Count; i++)
                    {
                        if (state.Variables[i].definitionScopeLevel == state.lexicalScopeLevel)
                            count++;
                    }
                    if (count > 0)
                    {
                        variables = new VariableDescriptor[count];
                        HashSet<string> declaredVariables = null;
                        if (state.lexicalScopeLevel != state.functionScopeLevel)
                            declaredVariables = new HashSet<string>();
                        for (int i = oldVariablesCount, targetIndex = 0; i < state.Variables.Count; i++)
                        {
                            if (state.Variables[i].definitionScopeLevel == state.lexicalScopeLevel)
                            {
                                variables[targetIndex] = state.Variables[i];
                                if (declaredVariables != null)
                                {
                                    if (declaredVariables.Contains(variables[targetIndex].name))
                                        ExceptionsHelper.ThrowSyntaxError("Variable \"" + variables[targetIndex].name + "\" has already been defined", state.Code, i);
                                    declaredVariables.Add(variables[targetIndex].name);
                                }
                                targetIndex++;
                            }
                            else if (targetIndex != 0)
                            {
                                state.Variables[i - targetIndex] = state.Variables[i];
                            }
                        }
                        state.Variables.RemoveRange(state.Variables.Count - count, count);
                    }
                }

                state.functionScopeLevel = oldFunctionScopeLevel;
                state.lexicalScopeLevel--;
            }
            if (!sroot)
                position++;
            int startPos = index;
            index = position;
            return new CodeBlock(body.ToArray())
            {
                strict = (state.strict ^= strictSwitch) || strictSwitch,
                _variables = variables ?? emptyVariables,
                Position = startPos,
                code = state.SourceCode,
                Length = position - startPos,
#if DEBUG
                directives = directives
#endif
            };
        }

        public override JSValue Evaluate(Context context)
        {
            var ls = lines;
            int i = 0;
            bool clearSuspendData = false;

            if (ls.Length == 0)
                return null;

            if (context.abortType >= AbortType.Resume)
            {
                var suspendData = context.SuspendData[this] as SuspendData;
                context = suspendData.Context;
                i = suspendData.LineIndex;
                clearSuspendData = true;
            }
            else
            {
                if (!suppressScopeIsolation)
                {
                    context = new Context(context, false, context.owner)
                    {
                        suspendData = context.suspendData,
                        variables = _variables,
                        thisBind = context.thisBind,
                        strict = context.strict
                    };
                }

                if (_variables.Length != 0)
                    initVariables(context);
            }

            var activated = !suppressScopeIsolation && context.Activate();
            try
            {
                for (; i < ls.Length; i++)
                {
#if DEV
                    if (context.debugging)
                        context.raiseDebugger(lines[i]);
#endif
                    var t = ls[i].Evaluate(context);
                    if (t != null)
                        context.lastResult = t;
#if DEBUG && !PORTABLE
                    if (!context.Excecuting)
                        if (System.Diagnostics.Debugger.IsAttached)
                            System.Diagnostics.Debugger.Break();
                        else
                            throw new ApplicationException("Context was stopped");
                    if (NiL.JS.BaseLibrary.Number.NaN.valueType != JSValueType.Double || !double.IsNaN(Number.NaN.dValue))
                        if (System.Diagnostics.Debugger.IsAttached)
                            System.Diagnostics.Debugger.Break();
                        else
                            throw new ApplicationException("NaN has been rewitten");
                    if (JSObject.undefined.valueType != JSValueType.Undefined)
                        if (System.Diagnostics.Debugger.IsAttached)
                            System.Diagnostics.Debugger.Break();
                        else
                            throw new ApplicationException("undefined has been rewitten");
                    if (JSObject.notExists.Exists)
                        if (System.Diagnostics.Debugger.IsAttached)
                            System.Diagnostics.Debugger.Break();
                        else
                            throw new ApplicationException("notExists has been rewitten");
                    if (BaseLibrary.Boolean.False.valueType != JSValueType.Bool
                        || BaseLibrary.Boolean.False.iValue != 0
                        || BaseLibrary.Boolean.False.attributes != JSValueAttributesInternal.SystemObject)
                        if (System.Diagnostics.Debugger.IsAttached)
                            System.Diagnostics.Debugger.Break();
                        else
                            throw new ApplicationException("Boolean.False has been rewitten");
                    if (BaseLibrary.Boolean.True.valueType != JSValueType.Bool
                        || BaseLibrary.Boolean.True.iValue != 1
                        || BaseLibrary.Boolean.True.attributes != JSValueAttributesInternal.SystemObject)
                        if (System.Diagnostics.Debugger.IsAttached)
                            System.Diagnostics.Debugger.Break();
                        else
                            throw new ApplicationException("Boolean.True has been rewitten");
#endif
                    if (context.abortType != AbortType.None)
                    {
                        if (context.abortType == AbortType.Suspend)
                        {
                            context.SuspendData[this] = new SuspendData { Context = context, LineIndex = i };
                        }
                        break;
                    }
                    if (clearSuspendData)
                        context.SuspendData.Clear();
                }

                return null;
            }
            finally
            {
                if (!suppressScopeIsolation)
                {
                    if (activated)
                        context.Deactivate();
                    context.parent.lastResult = context.lastResult;
                    context.parent.abortInfo = context.abortInfo;
                    context.parent.abortType = context.abortType;
                    if (_variables.Length != 0)
                        clearVariablesCache();
                }
            }
        }

        internal void clearVariablesCache()
        {
            for (var i = 0; i < _variables.Length; i++)
            {
                _variables[i].cacheContext = null;
                _variables[i].cacheRes = null;
            }
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>();
            for (int i = 0; i < lines.Length; i++)
            {
                var node = lines[i];
                if (node == null)
                    break;
                res.Add(node);
            }
            if (_variables != null)
                res.AddRange(from v in _variables where v.initializer != null && (!(v.initializer is FunctionDefinition) || (v.initializer as FunctionDefinition).body != this) select v.initializer);
            return res.ToArray();
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (builded)
                return false;
            builded = true;

            List<VariableDescriptor> variablesToRestore = null;
            if (_variables.Length != 0)
            {
                for (var i = 0; i < _variables.Length; i++)
                {
                    VariableDescriptor desc = null;
                    if (variables.TryGetValue(_variables[i].name, out desc))
                    {
                        if (variablesToRestore == null)
                            variablesToRestore = new List<VariableDescriptor>();
                        variablesToRestore.Add(desc);
                    }

                    variables[_variables[i].name] = _variables[i];

                    _variables[i].owner = this;
                }
            }

            for (var i = 0; i < lines.Length; i++)
            {
                var fe = lines[i] as EntityDefinition;
                if (fe != null)
                    lines[i] = null;
            }

            for (var i = 0; i < _variables.Length; i++)
            {
                Parser.Build(ref _variables[i].initializer, (codeContext & CodeContext.InEval) != 0 ? 2 : System.Math.Max(1, expressionDepth), variables, codeContext | (this.strict ? CodeContext.Strict : CodeContext.None), message, stats, opts);
            }

            bool unreachable = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] != null)
                {
                    if (lines[i] is EmptyExpression)
                        lines[i] = null;
                    else
                    {
                        if (unreachable && message != null)
                            message(MessageLevel.CriticalWarning, new CodeCoordinates(0, lines[i].Position, lines[i].Length), "Unreachable code detected.");
                        var cn = lines[i];
                        Parser.Build(ref cn, (codeContext & CodeContext.InEval) != 0 ? 2 : System.Math.Max(1, expressionDepth), variables, codeContext | (this.strict ? CodeContext.Strict : CodeContext.None), message, stats, opts);
                        if (cn is EmptyExpression)
                            lines[i] = null;
                        else
                            lines[i] = cn;
                        unreachable |= cn is ReturnStatement || cn is BreakStatement || cn is ContinueStatement || cn is ThrowStatement;
                    }
                }
            }

            int f = lines.Length, t = lines.Length - 1;
            for (; f-- > 0;)
            {
                if (lines[f] != null && lines[t] == null)
                {
                    lines[t] = lines[f];
                    lines[f] = null;
                }
                if (lines[t] != null)
                    t--;
            }

            if (expressionDepth > 0 && _variables.Length == 0)
            {
                if (lines.Length == 0)
                    _this = EmptyExpression.Instance;
            }
            else
            {
                if (message != null)
                {
                    for (var i = 0; i < _variables.Length; i++)
                    {
                        if (_variables[i].ReferenceCount == 1)
                        {
                            message(
                                MessageLevel.Recomendation,
                                new CodeCoordinates(0, _variables[i].references[0].Position, 0),
                                "Unused variable \"" + _variables[i].name + "\"");
                        }
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
            if (_variables.Length != 0)
            {
                for (var i = 0; i < _variables.Length; i++)
                {
                    variables.Remove(_variables[i].name);
                }
            }
            if (variablesToRestore != null)
            {
                for (var i = 0; i < variablesToRestore.Count; i++)
                {
                    variables[variablesToRestore[i].name] = variablesToRestore[i];
                }
            }
            return false;
        }

        internal void Optimize(ref CodeBlock self, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            CodeNode cn = self;
            Optimize(ref cn, owner, message, opts, stats);
            self = (CodeBlock)cn;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            /*
             * Дублирование оптимизации для локальных переменных нужно для правильной работы ряда оптимизаций
             */

            if (_variables != null)
            {
                for (var i = 0; i < _variables.Length; i++)
                {
                    if (_variables[i].initializer != null)
                    {
                        var cn = _variables[i].initializer as CodeNode;
                        cn.Optimize(ref cn, owner, message, opts, stats);
                    }
                }
            }
            for (int i = 0; i < lines.Length; i++)
            {
                var cn = lines[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, stats);
                lines[i] = cn;
            }
            if (_variables != null)
            {
                for (var i = 0; i < _variables.Length; i++)
                {
                    if (_variables[i].initializer != null)
                    {
                        var cn = _variables[i].initializer as CodeNode;
                        cn.Optimize(ref cn, owner, message, opts, stats);
                    }
                }
            }
        }

        public override void Decompose(ref CodeNode self)
        {
            if (_variables != null)
            {
                for (var i = 0; i < _variables.Length; i++)
                {
                    if (_variables[i].initializer != null)
                    {
                        _variables[i].initializer.Decompose(ref _variables[i].initializer);
                    }
                }
            }

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].Decompose(ref lines[i]);
            }
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            var initialVariables = _variables;

            if (_variables.Length != 0 && !functionInfo.WithLexicalEnvironment)
            {
                for(var i = 0; i < _variables.Length; i++)
                {
                    if (!transferedVariables.ContainsKey(_variables[i].name))
                        transferedVariables.Add(_variables[i].name, _variables[i]);
                }
                _variables = emptyVariables;
            }

            if (_variables.Length == 0)
            {
                suppressScopeIsolation = true;
                scopeBias--;
            }

            for (var i = 0; i < initialVariables.Length; i++)
            {
                initialVariables[i].definitionScopeLevel -= initialVariables[i].scopeBias;
                initialVariables[i].scopeBias = scopeBias;
                initialVariables[i].definitionScopeLevel += scopeBias;

                initialVariables[i].initializer?.RebuildScope(functionInfo, transferedVariables, scopeBias);
            }

            for (var i = 0; i < lines.Length; i++)
            {
                lines[i].RebuildScope(functionInfo, transferedVariables, scopeBias);
            }
        }

        internal void initVariables(Context context)
        {
            var stats = context.owner?.creator._functionInfo;
            var cew = stats == null || stats.ContainsEval || stats.ContainsWith || stats.ContainsYield;
            for (var i = _variables.Length; i-- > 0;)
            {
                var v = _variables[i];

                if (v.cacheContext != null)
                {
                    if (v.cacheContext.fields == null)
                        v.cacheContext.fields = JSObject.getFieldsContainer();
                    v.cacheContext.fields[v.name] = v.cacheRes;
                }

                if (v.lexicalScope)
                    continue;

                bool isArg = stats != null && string.CompareOrdinal(v.name, "arguments") == 0;
                if (isArg && v.initializer == null)
                    continue;

                JSValue f = new JSValue()
                {
                    valueType = JSValueType.Undefined,
                    attributes = JSValueAttributesInternal.DoNotDelete
                };
                v.cacheRes = f;
                v.cacheContext = context;
                if (v.captured || cew)
                    (context.fields ?? (context.fields = JSObject.getFieldsContainer()))[v.name] = f;
                if (v.initializer != null)
                    f.Assign(v.initializer.Evaluate(context));
                if (v.isReadOnly)
                    f.attributes |= JSValueAttributesInternal.ReadOnly;

                if (isArg)
                    context.arguments = f;
            }
        }

#if !PORTABLE
        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            for (int i = 0; i < lines.Length; i++)
                lines[i].TryCompile(true, false, null, dynamicValues);
            for (int i = _variables.Length; i-- > 0;)
                if (_variables[i].initializer != null)
                    _variables[i].initializer.TryCompile(true, false, null, dynamicValues);
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

        public string ToString(bool linewiseStringify)
        {
            if (linewiseStringify)
            {
                if (lines == null || lines.Length == 0)
                    return "{ }";
                StringBuilder res = new StringBuilder().Append(" {").Append(Environment.NewLine);
                var replp = Environment.NewLine;
                var replt = Environment.NewLine + "  ";
                for (int i = lines.Length; i-- > 0;)
                {
                    string lc = lines[i].ToString();
                    if (lc[0] == '(')
                        lc = lc.Substring(1, lc.Length - 2);
                    lc = lc.Replace(replp, replt);
                    res.Append("  ").Append(lc).Append(lc[lc.Length - 1] != '}' ? ";" : "").Append(Environment.NewLine);
                }
                return res.Append("}").ToString();
            }
            else
                return '{' + Code + '}';
        }
    }
}