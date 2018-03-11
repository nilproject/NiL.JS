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
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public enum SuppressScopeIsolationMode
    {
        Auto,
        Suppress,
        DoNotSuppress
    }

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class CodeBlock : CodeNode
    {
        private sealed class SuspendData
        {
            public int LineIndex;
            public Context Context;
        }

        internal static readonly VariableDescriptor[] emptyVariables = new VariableDescriptor[0];

        private string code;
#if (NET40 || INLINE) && JIT
        internal Func<Context, JSObject> compiledVersion;
#endif
#if DEBUG
        internal HashSet<string> directives;
#endif
        internal VariableDescriptor[] _variables;
        internal CodeNode[] _lines;
        internal bool _strict;
        internal bool built;
        internal SuppressScopeIsolationMode _suppressScopeIsolation;

        public VariableDescriptor[] Variables { get { return _variables; } }
        public CodeNode[] Body { get { return _lines; } }
        public bool Strict { get { return _strict; } }
        public string Code
        {
            get
            {
                var res = ToString();
                return res.Substring(1, res.Length - 2);
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
            _lines = body;
            _variables = null;
            _strict = false;
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
            state.LabelsCount = 0;
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
                    (body[j] as Constant).value._oValue = Tools.Unescape((body[j] as Constant).value._oValue.ToString(), state.strict);

                bool expectSemicolon = false;
                while ((sroot && position < state.Code.Length) || (!sroot && state.Code[position] != '}'))
                {
                    var t = Parser.Parse(state, ref position, 0);
                    if (t == null)
                    {
                        if (position < state.Code.Length)
                        {
                            if (sroot && state.Code[position] == '}')
                                ExceptionHelper.Throw(new SyntaxError("Unexpected symbol \"}\" at " + CodeCoordinates.FromTextPosition(state.Code, position, 0)));

                            if ((state.Code[position] == ';' || state.Code[position] == ','))
                            {
                                if (state.message != null && !expectSemicolon)
                                    state.message(MessageLevel.Warning, position, 1, "Unnecessary semicolon.");

                                position++;
                            }

                            expectSemicolon = false;
                        }

                        continue;
                    }

                    expectSemicolon = !(t is EntityDefinition);

                    body.Add(t);
                }
            }
            finally
            {
                if (oldVariablesCount != state.Variables.Count)
                {
                    variables = extractVariables(state, oldVariablesCount);
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
                _strict = (state.strict ^= strictSwitch) || strictSwitch,
                _variables = variables ?? emptyVariables,
                Position = startPos,
                code = state.SourceCode,
                Length = position - startPos,
#if DEBUG
                directives = directives
#endif
            };
        }

        internal static VariableDescriptor[] extractVariables(ParseInfo state, int oldVariablesCount)
        {
            VariableDescriptor[] variables = emptyVariables;
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
                            if (declaredVariables.Contains(variables[targetIndex].name) && variables[targetIndex].lexicalScope)
                                ExceptionHelper.ThrowSyntaxError("Variable \"" + variables[targetIndex].name + "\" already has been defined", state.Code, i);

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

            return variables;
        }

        public override JSValue Evaluate(Context context)
        {
            int i = 0;
            bool clearSuspendData = false;

            if (context._executionMode >= ExecutionMode.Resume)
            {
                var suspendData = context.SuspendData[this] as SuspendData;
                context = suspendData.Context;
                i = suspendData.LineIndex;
                clearSuspendData = true;
            }
            else
            {
                if (_suppressScopeIsolation != SuppressScopeIsolationMode.Suppress)
                {
                    context = new Context(context, false, context._owner)
                    {
                        SuspendData = context.SuspendData,
                        _definedVariables = _variables,
                        _thisBind = context._thisBind,
                        _strict = context._strict,
                        _executionInfo = context._executionInfo,
                        _executionMode = context._executionMode
                    };
                }

                if (_variables != null && _variables.Length != 0)
                    initVariables(context);
            }

            if (_suppressScopeIsolation != SuppressScopeIsolationMode.Suppress)
                evaluateWithScope(context, i, clearSuspendData);
            else
                evaluateLines(context, i, clearSuspendData);

            return null;
        }

        private void evaluateWithScope(Context context, int i, bool clearSuspendData)
        {
            var activated = _suppressScopeIsolation != SuppressScopeIsolationMode.Suppress && context.Activate();
            try
            {
                evaluateLines(context, i, clearSuspendData);
            }
            finally
            {
                if (_suppressScopeIsolation != SuppressScopeIsolationMode.Suppress)
                {
                    if (activated)
                        context.Deactivate();
                    context._parent._lastResult = context._lastResult;
                    context._parent._executionInfo = context._executionInfo;
                    context._parent._executionMode = context._executionMode;
                    if (_variables.Length != 0)
                        clearVariablesCache();
                }
            }
        }

        private void evaluateLines(Context context, int i, bool clearSuspendData)
        {
            for (var ls = _lines; i < ls.Length; i++)
            {
                if (context._debugging)
                    context.raiseDebugger(_lines[i]);
                var t = ls[i].Evaluate(context);
                if (t != null)
                    context._lastResult = t;
#if DEBUG && !(PORTABLE || NETCORE)
                if (!context.Running)
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("Context was stopped");
                if (Number.NaN._valueType != JSValueType.Double || !double.IsNaN(Number.NaN._dValue))
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("NaN has been rewitten");
                if (JSValue.undefined._valueType != JSValueType.Undefined)
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("undefined has been rewitten");
                if (JSValue.notExists.Exists)
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("notExists has been rewitten");
                if (BaseLibrary.Boolean.False._valueType != JSValueType.Boolean
                    || BaseLibrary.Boolean.False._iValue != 0
                    || BaseLibrary.Boolean.False._attributes != JSValueAttributesInternal.SystemObject)
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("Boolean.False has been rewitten");
                if (BaseLibrary.Boolean.True._valueType != JSValueType.Boolean
                    || BaseLibrary.Boolean.True._iValue != 1
                    || BaseLibrary.Boolean.True._attributes != JSValueAttributesInternal.SystemObject)
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        throw new ApplicationException("Boolean.True has been rewitten");
#endif
                if (context._executionMode != ExecutionMode.None)
                {
                    if (context._executionMode == ExecutionMode.Suspend)
                    {
                        context.SuspendData[this] = new SuspendData { Context = context, LineIndex = i };
                    }

                    break;
                }

                if (clearSuspendData)
                    context.SuspendData.Clear();
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

        protected internal override CodeNode[] GetChildsImpl()
        {
            var res = new List<CodeNode>();
            for (int i = 0; i < _lines.Length; i++)
            {
                var node = _lines[i];
                if (node == null)
                    break;
                res.Add(node);
            }

            if (_variables != null)
                res.AddRange(from v in _variables where v.initializer != null && (!(v.initializer is FunctionDefinition) || (v.initializer as FunctionDefinition)._body != this) select v.initializer);

            return res.ToArray();
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (built)
                return false;
            built = true;

            List<VariableDescriptor> variablesToRestore = null;
            if (_variables != null && _variables.Length != 0)
            {
                for (var i = 0; i < _variables.Length; i++)
                {
                    VariableDescriptor desc = null;
                    if (variables.TryGetValue(_variables[i].name, out desc) && desc.definitionScopeLevel < _variables[i].definitionScopeLevel)
                    {
                        if (variablesToRestore == null)
                            variablesToRestore = new List<VariableDescriptor>();
                        variablesToRestore.Add(desc);
                    }

                    variables[_variables[i].name] = _variables[i];

                    _variables[i].owner = this;
                }

                for (var i = 0; i < _variables.Length; i++)
                {
                    Parser.Build(
                        ref _variables[i].initializer,
                        System.Math.Max(2, expressionDepth),
                        variables,
                        codeContext | (this._strict ? CodeContext.Strict : CodeContext.None),
                        message,
                        stats,
                        opts);
                }
            }

            for (var i = 0; i < _lines.Length; i++)
            {
                var ed = _lines[i] as EntityDefinition;
                if (ed != null && ed.Hoist)
                    _lines[i] = null;
            }

            bool unreachable = false;
            for (int i = 0; i < _lines.Length; i++)
            {
                if (_lines[i] != null)
                {
                    if (_lines[i] is Empty)
                        _lines[i] = null;
                    else
                    {
                        if (unreachable && message != null)
                            message(MessageLevel.CriticalWarning, _lines[i].Position, _lines[i].Length, "Unreachable code detected.");
                        var cn = _lines[i];
                        Parser.Build(ref cn, (codeContext & CodeContext.InEval) != 0 ? 2 : System.Math.Max(1, expressionDepth), variables, codeContext | (this._strict ? CodeContext.Strict : CodeContext.None), message, stats, opts);
                        if (cn is Empty)
                            _lines[i] = null;
                        else
                            _lines[i] = cn;
                        unreachable |= cn is Return || cn is Break || cn is Continue || cn is Throw;
                    }
                }
            }

            int f = _lines.Length, t = _lines.Length - 1;
            for (; f-- > 0;)
            {
                if (_lines[f] != null && _lines[t] == null)
                {
                    _lines[t] = _lines[f];
                    _lines[f] = null;
                }

                if (_lines[t] != null)
                    t--;
            }

            if (expressionDepth > 0 && (_variables == null || _variables.Length == 0))
            {
                if (_lines.Length == 0)
                    _this = Empty.Instance;
            }
            else
            {
                if (message != null)
                {
                    for (var i = 0; i < _variables.Length; i++)
                    {
                        if (_variables[i].ReferenceCount == 1 && !(_variables[i].references[0] is ParameterReference))
                        {
                            message(
                                MessageLevel.Recomendation,
                                _variables[i].references[0].Position,
                                0,
                                "Unused variable \"" + _variables[i].name + "\"");
                        }
                        else
                            break;
                    }
                }
#if (NET40 || INLINE) && JIT
                compiledVersion = JITHelpers.compile(this, depth >= 0);
#endif
            }

            if (t >= 0 && this == _this)
            {
                var newBody = new CodeNode[_lines.Length - t - 1];
                f = 0;
                while (++t < _lines.Length)
                    newBody[f++] = _lines[t];
                _lines = newBody;
            }

            if (_variables != null && _variables.Length != 0)
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

        internal void Optimize(ref CodeBlock self, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            CodeNode cn = self;
            Optimize(ref cn, owner, message, opts, stats);
            self = (CodeBlock)cn;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
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

            for (int i = 0; i < _lines.Length; i++)
            {
                var cn = _lines[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, stats);
                _lines[i] = cn;
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

            if (_lines.Length == 1 && _suppressScopeIsolation == SuppressScopeIsolationMode.Suppress && _variables.Length == 0)
            {
                _this = _lines[0];
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

            for (int i = 0; i < _lines.Length; i++)
            {
                _lines[i].Decompose(ref _lines[i]);
            }
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            if (_variables != null)
            {
                var initialVariables = _variables;

                if (_variables.Length != 0 && !functionInfo.WithLexicalEnvironment)
                {
                    for (var i = 0; i < _variables.Length; i++)
                    {
                        VariableDescriptor desc;
                        if (!transferedVariables.TryGetValue(_variables[i].name, out desc) || _variables[i].initializer != null)
                            transferedVariables[_variables[i].name] = _variables[i];
                    }

                    _variables = emptyVariables;
                }

                if (_variables.Length == 0)
                {
                    if (_suppressScopeIsolation == SuppressScopeIsolationMode.Auto)
                        _suppressScopeIsolation = SuppressScopeIsolationMode.Suppress;

                    scopeBias--;
                }

                for (var i = 0; i < initialVariables.Length; i++)
                {
                    if (initialVariables[i].definitionScopeLevel != -1)
                    {
                        initialVariables[i].definitionScopeLevel -= initialVariables[i].scopeBias;
                        initialVariables[i].definitionScopeLevel += scopeBias;
                    }

                    initialVariables[i].scopeBias = scopeBias;
                    initialVariables[i].initializer?.RebuildScope(functionInfo, transferedVariables, scopeBias);
                }
            }
            else
                _suppressScopeIsolation = SuppressScopeIsolationMode.Suppress;

            if (transferedVariables == null)
            {
                for (var i = 0; i < _lines.Length; i++)
                {
                    _lines[i].RebuildScope(functionInfo, null, scopeBias);
                }
            }
            else
            {
                bool needRerun;
                do
                {
                    needRerun = false;
                    for (var i = 0; i < _lines.Length; i++)
                    {
                        var oldVariablesCount = transferedVariables.Count;
                        _lines[i].RebuildScope(functionInfo, transferedVariables, scopeBias);
                        if (transferedVariables.Count > oldVariablesCount && i > 0)
                            needRerun = true;
                    }
                }
                while (needRerun);
            }
        }

        internal void initVariables(Context context)
        {
            var stats = context._owner?._functionDefinition?._functionInfo;
            var cew = stats == null || stats.ContainsEval || stats.ContainsWith || stats.NeedDecompose;
            for (var i = 0; i < _variables.Length; i++)
            {
                var v = _variables[i];

                if (v.cacheContext != null)
                {
                    if (v.cacheContext._variables == null)
                        v.cacheContext._variables = JSObject.getFieldsContainer();
                    v.cacheContext._variables[v.name] = v.cacheRes;
                }

                if (v.lexicalScope)
                    continue;

                var isArg = stats != null && string.CompareOrdinal(v.name, "arguments") == 0;
                if (isArg && v.initializer == null)
                    continue;

                var f = new JSValue()
                {
                    _valueType = JSValueType.Undefined,
                    _attributes = JSValueAttributesInternal.DoNotDelete
                };
                v.cacheRes = f;
                v.cacheContext = context;
                if (v.definitionScopeLevel < 0 || v.captured || cew)
                    (context._variables ?? (context._variables = JSObject.getFieldsContainer()))[v.name] = f;
                if (v.initializer != null)
                    f.Assign(v.initializer.Evaluate(context));
                if (v.isReadOnly)
                    f._attributes |= JSValueAttributesInternal.ReadOnly;

                if (isArg)
                    context._arguments = f;
            }
        }

#if !PORTABLE
        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            for (int i = _variables.Length; i-- > 0;)
                if (_variables[i].initializer != null)
                    _variables[i].initializer.TryCompile(true, false, null, dynamicValues);

            for (int i = 0; i < _lines.Length; i++)
                _lines[i].TryCompile(true, false, null, dynamicValues);

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
            if (linewiseStringify || string.IsNullOrEmpty(code))
            {
                if (_lines == null || _lines.Length == 0)
                    return "{ }";

                StringBuilder res = new StringBuilder()
                    .Append("{")
                    .Append(Environment.NewLine);

                var replp = Environment.NewLine;
                var replt = Environment.NewLine + "  ";
                for (int i = 0; i < _lines.Length; i++)
                {
                    string lc = _lines[i].ToString();
                    if (lc[0] == '(')
                        lc = lc.Substring(1, lc.Length - 2);
                    lc = lc.Replace(replp, replt);
                    res.Append("  ").Append(lc).Append(lc[lc.Length - 1] != '}' ? ";" : "").Append(Environment.NewLine);
                }
                return res.Append("}").ToString();
            }
            else
            {
                if (base.Length > 0)
                {
                    Length = -base.Length;
                    if (Position > 0)
                        code = code.Substring(Position + 1, Length - 2);
                }

                return '{' + code + '}';
            }
        }
    }
}