using System;
using System.Collections.Generic;
using System.Linq;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class ObjectDefinition : Expression
    {
        private string[] _fieldNames;
        private Expression[] _values;
        private KeyValuePair<Expression, Expression>[] _computedProperties;

        public string[] FieldNames { get { return _fieldNames; } }
        public Expression[] Values { get { return _values; } }
        public KeyValuePair<Expression, Expression>[] ComputedProperties { get { return _computedProperties; } }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Object;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        protected internal override bool NeedDecompose
        {
            get
            {
                return _values.Any(x => x.NeedDecompose);
            }
        }

        private ObjectDefinition(Dictionary<string, Expression> fields, KeyValuePair<Expression, Expression>[] computedProperties)
        {
            _computedProperties = computedProperties;
            _fieldNames = new string[fields.Count];
            _values = new Expression[fields.Count];

            int i = 0;
            foreach (var f in fields)
            {
                _fieldNames[i] = f.Key;
                _values[i++] = f.Value;
            }
        }

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            if (state.Code[index] != '{')
                throw new ArgumentException("Invalid JSON definition");
            var flds = new Dictionary<string, Expression>();
            var computedProperties = new List<KeyValuePair<Expression, Expression>>();
            int i = index;
            while (state.Code[i] != '}')
            {
                i++;
                Tools.SkipSpaces(state.Code, ref i);
                int s = i;
                if (state.Code[i] == '}')
                    break;

                bool getOrSet = Parser.Validate(state.Code, "get", ref i) || Parser.Validate(state.Code, "set", ref i);
                Tools.SkipSpaces(state.Code, ref i);
                if (getOrSet && state.Code[i] == '(')  // function with name 'get' or 'set'
                {
                    getOrSet = false;
                    i = s;
                }

                var asterisk = state.Code[i] == '*';
                Tools.SkipSpaces(state.Code, ref i);

                if (Parser.Validate(state.Code, "[", ref i))
                {
                    var name = ExpressionTree.Parse(state, ref i, false, false, false, true, false);
                    while (Tools.IsWhiteSpace(state.Code[i]))
                        i++;
                    if (state.Code[i] != ']')
                        ExceptionHelper.ThrowSyntaxError("Expected ']'", state.Code, i);
                    do
                        i++;
                    while (Tools.IsWhiteSpace(state.Code[i]));

                    Tools.SkipSpaces(state.Code, ref i);
                    if (state.Code[s] != 'g' && state.Code[s] != 's')
                    {
                        if (!Parser.Validate(state.Code, ":", ref i))
                            ExceptionHelper.ThrowSyntaxError(Strings.UnexpectedToken, state.Code, i);
                        Tools.SkipSpaces(state.Code, ref i);
                    }

                    CodeNode initializer;
                    if (state.Code[i] == '(')
                    {
                        initializer = FunctionDefinition.Parse(state, ref i, asterisk ? FunctionKind.AnonymousGenerator : FunctionKind.AnonymousFunction);
                    }
                    else
                    {
                        initializer = ExpressionTree.Parse(state, ref i);
                    }

                    switch (state.Code[s])
                    {
                        case 'g':
                            {
                                computedProperties.Add(new KeyValuePair<Expression, Expression>(name, new PropertyPair((Expression)initializer, null)));
                                break;
                            }
                        case 's':
                            {
                                computedProperties.Add(new KeyValuePair<Expression, Expression>(name, new PropertyPair(null, (Expression)initializer)));
                                break;
                            }
                        default:
                            {
                                computedProperties.Add(new KeyValuePair<Expression, Expression>(name, (Expression)initializer));
                                break;
                            }
                    }
                }
                else if (getOrSet && state.Code[i] != ':')
                {
                    i = s;
                    var mode = state.Code[i] == 's' ? FunctionKind.Setter : FunctionKind.Getter;
                    var propertyAccessor = FunctionDefinition.Parse(state, ref i, mode) as FunctionDefinition;
                    var accessorName = propertyAccessor._name;
                    if (!flds.ContainsKey(accessorName))
                    {
                        var propertyPair = new PropertyPair
                        (
                            mode == FunctionKind.Getter ? propertyAccessor : null,
                            mode == FunctionKind.Setter ? propertyAccessor : null
                        );
                        flds.Add(accessorName, propertyPair);
                    }
                    else
                    {
                        var vle = flds[accessorName] as PropertyPair;

                        if (vle == null)
                            ExceptionHelper.ThrowSyntaxError("Try to define " + mode.ToString().ToLowerInvariant() + " for defined field", state.Code, s);

                        do
                        {
                            if (mode == FunctionKind.Getter)
                            {
                                if (vle.Getter == null)
                                {
                                    vle.Getter = propertyAccessor;
                                    break;
                                }
                            }
                            else
                            {
                                if (vle.Setter == null)
                                {
                                    vle.Setter = propertyAccessor;
                                    break;
                                }
                            }

                            ExceptionHelper.ThrowSyntaxError("Try to redefine " + mode.ToString().ToLowerInvariant() + " of " + propertyAccessor.Name, state.Code, s);
                        }
                        while (false);
                    }
                }
                else
                {
                    if (asterisk)
                    {
                        do
                            i++;
                        while (Tools.IsWhiteSpace(state.Code[i]));
                    }

                    i = s;
                    var fieldName = "";
                    if (Parser.ValidateName(state.Code, ref i, false, true, state.strict))
                        fieldName = Tools.Unescape(state.Code.Substring(s, i - s), state.strict);
                    else if (Parser.ValidateValue(state.Code, ref i))
                    {
                        if (state.Code[s] == '-')
                            ExceptionHelper.Throw(new SyntaxError("Invalid char \"-\" at " + CodeCoordinates.FromTextPosition(state.Code, s, 1)));
                        double d = 0.0;
                        int n = s;
                        if (Tools.ParseNumber(state.Code, ref n, out d))
                            fieldName = Tools.DoubleToString(d);
                        else if (state.Code[s] == '\'' || state.Code[s] == '"')
                            fieldName = Tools.Unescape(state.Code.Substring(s + 1, i - s - 2), state.strict);
                        else if (flds.Count != 0)
                            ExceptionHelper.Throw((new SyntaxError("Invalid field name at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s))));
                        else
                            return null;
                    }
                    else
                        return null;

                    while (Tools.IsWhiteSpace(state.Code[i]))
                        i++;

                    Expression initializer = null;

                    if (state.Code[i] == '(')
                    {
                        i = s;
                        initializer = FunctionDefinition.Parse(state, ref i, asterisk ? FunctionKind.MethodGenerator : FunctionKind.Method);
                    }
                    else
                    {
                        if (asterisk)
                            ExceptionHelper.ThrowSyntaxError("Unexpected token", state.Code, i);

                        if (state.Code[i] != ':' && state.Code[i] != ',' && state.Code[i] != '}')
                            ExceptionHelper.ThrowSyntaxError("Expected ',', ';' or '}'", state.Code, i);

                        Expression aei = null;
                        if (flds.TryGetValue(fieldName, out aei))
                        {
                            if (state.strict ? (!(aei is Constant) || (aei as Constant).value != JSValue.undefined) : aei is PropertyPair)
                                ExceptionHelper.ThrowSyntaxError("Try to redefine field \"" + fieldName + "\"", state.Code, s, i - s);

                            if (state.message != null)
                                state.message(MessageLevel.Warning, i, 0, "Duplicate key \"" + fieldName + "\"");
                        }

                        if (state.Code[i] == ',' || state.Code[i] == '}')
                        {
                            if (!Parser.ValidateName(fieldName, 0))
                                ExceptionHelper.ThrowSyntaxError("Invalid variable name", state.Code, i);

                            initializer = new Variable(fieldName, state.lexicalScopeLevel);
                        }
                        else
                        {
                            do
                                i++;
                            while (Tools.IsWhiteSpace(state.Code[i]));
                            initializer = ExpressionTree.Parse(state, ref i, false, false);
                        }
                    }
                    flds[fieldName] = initializer;
                }

                while (Tools.IsWhiteSpace(state.Code[i]))
                    i++;

                if ((state.Code[i] != ',') && (state.Code[i] != '}'))
                    return null;
            }

            i++;
            var pos = index;
            index = i;
            return new ObjectDefinition(flds, computedProperties.ToArray())
            {
                Position = pos,
                Length = index - pos
            };
        }

        public override JSValue Evaluate(Context context)
        {
            var res = JSObject.CreateObject();
            if (_fieldNames.Length == 0 && _computedProperties.Length == 0)
                return res;

            res._fields = JSObject.getFieldsContainer();
            for (int i = 0; i < _fieldNames.Length; i++)
            {
                var val = _values[i].Evaluate(context);
                val = val.CloneImpl(false);
                val._attributes = JSValueAttributesInternal.None;
                if (this._fieldNames[i] == "__proto__")
                    res.__proto__ = val._oValue as JSObject;
                else
                    res._fields[this._fieldNames[i]] = val;
            }

            for (var i = 0; i < _computedProperties.Length; i++)
            {
                var key = _computedProperties[i].Key.Evaluate(context).CloneImpl(false);
                var value = _computedProperties[i].Value.Evaluate(context).CloneImpl(false);

                JSValue existedValue;
                Symbol symbolKey = null;
                string stringKey = null;
                if (key.Is<Symbol>())
                {
                    symbolKey = key.As<Symbol>();
                    if (res._symbols == null)
                        res._symbols = new Dictionary<Symbol, JSValue>();

                    if (!res._symbols.TryGetValue(symbolKey, out existedValue))
                        res._symbols[symbolKey] = existedValue = value;
                }
                else
                {
                    stringKey = key.As<string>();
                    if (!res._fields.TryGetValue(stringKey, out existedValue))
                        res._fields[stringKey] = existedValue = value;
                }

                if (existedValue != value)
                {
                    if (existedValue.Is(JSValueType.Property) && value.Is(JSValueType.Property))
                    {
                        var egs = existedValue.As<Core.PropertyPair>();
                        var ngs = value.As<Core.PropertyPair>();
                        egs.getter = ngs.getter ?? egs.getter;
                        egs.setter = ngs.setter ?? egs.setter;
                    }
                    else
                    {
                        if (key.Is<Symbol>())
                        {
                            res._symbols[symbolKey] = value;
                        }
                        else
                        {
                            res._fields[stringKey] = value;
                        }
                    }
                }
            }
            return res;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            _codeContext = codeContext;

            for (var i = 0; i < _values.Length; i++)
            {
                Parser.Build(ref _values[i], 2, variables, codeContext | CodeContext.InExpression, message, stats, opts);
            }

            for (var i = 0; i < _computedProperties.Length; i++)
            {
                var key = _computedProperties[i].Key;
                Parser.Build(ref key, 2, variables, codeContext | CodeContext.InExpression, message, stats, opts);

                var value = _computedProperties[i].Value;
                Parser.Build(ref value, 2, variables, codeContext | CodeContext.InExpression, message, stats, opts);

                _computedProperties[i] = new KeyValuePair<Expression, Expression>(key, value);
            }

            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            for (var i = Values.Length; i-- > 0;)
            {
                var cn = Values[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, stats);
                Values[i] = cn as Expression;
            }
            for (var i = 0; i < _computedProperties.Length; i++)
            {
                var key = _computedProperties[i].Key;
                key.Optimize(ref key, owner, message, opts, stats);

                var value = _computedProperties[i].Value;
                value.Optimize(ref value, owner, message, opts, stats);

                _computedProperties[i] = new KeyValuePair<Expression, Expression>(key, value);
            }
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            base.RebuildScope(functionInfo, transferedVariables, scopeBias);

            for (var i = 0; i < _values.Length; i++)
            {
                _values[i].RebuildScope(functionInfo, transferedVariables, scopeBias);
            }

            for (var i = 0; i < _computedProperties.Length; i++)
            {
                _computedProperties[i].Key.RebuildScope(functionInfo, transferedVariables, scopeBias);
                _computedProperties[i].Value.RebuildScope(functionInfo, transferedVariables, scopeBias);
            }
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            return _values;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            var lastDecomposeIndex = -1;
            var lastComputeDecomposeIndex = -1;
            for (var i = 0; i < _values.Length; i++)
            {
                _values[i].Decompose(ref _values[i], result);
                if (_values[i].NeedDecompose)
                {
                    lastDecomposeIndex = i;
                }
            }
            for (var i = 0; i < _computedProperties.Length; i++)
            {
                var key = _computedProperties[i].Key;
                key.Decompose(ref key, result);

                var value = _computedProperties[i].Value;
                value.Decompose(ref value, result);

                if ((key != _computedProperties[i].Key)
                    || (value != _computedProperties[i].Value))
                    _computedProperties[i] = new KeyValuePair<Expression, Expression>(key, value);

                if (_computedProperties[i].Value.NeedDecompose
                    || _computedProperties[i].Key.NeedDecompose)
                {
                    lastComputeDecomposeIndex = i;
                }
            }

            if (lastComputeDecomposeIndex >= 0)
                lastDecomposeIndex = _values.Length;

            for (var i = 0; i < lastDecomposeIndex; i++)
            {
                if (!(_values[i] is ExtractStoredValue))
                {
                    result.Add(new StoreValue(_values[i], false));
                    _values[i] = new ExtractStoredValue(_values[i]);
                }
            }

            for (var i = 0; i < lastDecomposeIndex; i++)
            {
                Expression key = null;
                Expression value = null;

                if (!(_computedProperties[i].Key is ExtractStoredValue))
                {
                    result.Add(new StoreValue(_computedProperties[i].Key, false));
                    key = new ExtractStoredValue(_computedProperties[i].Key);
                }
                if (!(_computedProperties[i].Value is ExtractStoredValue))
                {
                    result.Add(new StoreValue(_computedProperties[i].Value, false));
                    value = new ExtractStoredValue(_computedProperties[i].Value);
                }
                if ((key != null)
                    || (value != null))
                    _computedProperties[i] = new KeyValuePair<Expression, Expression>(
                        key ?? _computedProperties[i].Key,
                        value ?? _computedProperties[i].Value);
            }
        }

        public override string ToString()
        {
            string res = "{ ";

            for (int i = 0; i < _fieldNames.Length; i++)
            {
                res += "\"" + _fieldNames[i] + "\"" + " : " + _values[i];
                if (i + 1 < _fieldNames.Length)
                    res += ", ";
            }

            for (int i = 0; i < _computedProperties.Length; i++)
            {
                res += "[" + _computedProperties[i].Key + "]" + " : " + _computedProperties[i].Value;
                if (i + 1 < _fieldNames.Length)
                    res += ", ";
            }

            return res + " }";
        }
    }
}