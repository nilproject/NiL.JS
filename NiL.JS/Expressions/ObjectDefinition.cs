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
        private sealed class ObjectSpreadMarker : Expression
        {
            public static readonly ObjectSpreadMarker Instance = new ObjectSpreadMarker();
            public static readonly JSValue FakeResult = new JSValue();

            private ObjectSpreadMarker() { }

            protected internal override bool ContextIndependent => true;
            protected internal override bool NeedDecompose => false;

            public override JSValue Evaluate(Context context)
            {
                return FakeResult;
            }
        }

        private string[] _fieldNames;
        private Expression[] _values;
        private KeyValuePair<Expression, Expression>[] _computedProperties;

        private KeyValuePair<Expression, Expression>[] _properties;

        [Obsolete("Use " + nameof(Properties) + " instead")]
        public string[] FieldNames
        {
            get
            {
                _fieldNames ??= _properties.Where(x => x.Key is Constant).Select(x => (x.Key as Constant).Value.ToString()).ToArray();
                return _fieldNames;
            }
        }
        [Obsolete("Use " + nameof(Properties) + " instead")]
        public Expression[] Values
        {
            get
            {
                _values ??= _properties.Where(x => x.Key is Constant).Select(x => x.Value).ToArray();
                return _values;
            }
        }
        [Obsolete("Use " + nameof(Properties) + " instead")]
        public KeyValuePair<Expression, Expression>[] ComputedProperties
        {
            get
            {
                _computedProperties ??= _properties.Where(x => x.Key is not Constant).ToArray();
                return _computedProperties;
            }
        }

        public KeyValuePair<Expression, Expression>[] Properties { get { return _properties; } }

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
                return _properties.Any(x => x.Value.NeedDecompose);
            }
        }

        [Obsolete]
        private ObjectDefinition(Dictionary<string, Expression> fields, KeyValuePair<Expression, Expression>[] computedProperties)
        {
            _computedProperties = computedProperties;
            _fieldNames = new string[fields.Count];
            _values = new Expression[fields.Count];
            _properties = new KeyValuePair<Expression, Expression>[fields.Count + computedProperties.Length];

            int i = 0;
            foreach (var f in fields)
            {
                _fieldNames[i] = f.Key;
                _values[i] = f.Value;
                _properties[i] = new KeyValuePair<Expression, Expression>(new Constant(f.Key), f.Value);
                i++;
            }

            foreach (var p in computedProperties)
            {
                _properties[i] = p;
                i++;
            }
        }

        private ObjectDefinition(KeyValuePair<Expression, Expression>[] properties)
        {
            _properties = properties;
        }

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            if (state.Code[index] != '{')
                throw new ArgumentException("Invalid JSON definition");

            var fields = new Dictionary<string, Expression>();
            var properties = new List<KeyValuePair<Expression, Expression>>();
            int i = index;
            while (state.Code[i] != '}')
            {
                i++;
                Tools.SkipSpaces(state.Code, ref i);
                int start = i;
                if (state.Code[i] == '}')
                    break;

                bool getOrSet = Parser.Validate(state.Code, "get", ref i) || Parser.Validate(state.Code, "set", ref i);
                Tools.SkipSpaces(state.Code, ref i);
                if (getOrSet && state.Code[i] == '(')  // function with name 'get' or 'set'
                {
                    getOrSet = false;
                    i = start;
                }

                var asterisk = state.Code[i] == '*';
                Tools.SkipSpaces(state.Code, ref i);

                var async = false;
                if (!asterisk)
                {
                    async = Parser.Validate(state.Code, "async", ref i);
                    Tools.SkipSpaces(state.Code, ref i);
                }

                if (Parser.Validate(state.Code, "[", ref i))
                {
                    var nameExpression = ExpressionTree.Parse(state, ref i, false, false, false, true, false);
                    while (Tools.IsWhiteSpace(state.Code[i]))
                        i++;
                    if (state.Code[i] != ']')
                        ExceptionHelper.ThrowSyntaxError("Expected ']'", state.Code, i);
                    do
                        i++;
                    while (Tools.IsWhiteSpace(state.Code[i]));

                    Tools.SkipSpaces(state.Code, ref i);
                    CodeNode initializer;
                    if (state.Code[i] == '(')
                    {
                        initializer = FunctionDefinition.Parse(state, ref i, asterisk ? FunctionKind.AnonymousGenerator : async ? FunctionKind.AsyncAnonymousFunction : FunctionKind.AnonymousFunction);
                    }
                    else
                    {
                        if (!Parser.Validate(state.Code, ":", ref i))
                            ExceptionHelper.ThrowSyntaxError(Strings.UnexpectedToken, state.Code, i);
                        initializer = ExpressionTree.Parse(state, ref i, processComma: false);
                    }

                    switch (state.Code[start])
                    {
                        case 'g':
                        {
                            properties.Add(new KeyValuePair<Expression, Expression>(nameExpression, new PropertyPair((Expression)initializer, null)));
                            break;
                        }
                        case 's':
                        {
                            properties.Add(new KeyValuePair<Expression, Expression>(nameExpression, new PropertyPair(null, (Expression)initializer)));
                            break;
                        }
                        default:
                        {
                            properties.Add(new KeyValuePair<Expression, Expression>(nameExpression, (Expression)initializer));
                            break;
                        }
                    }
                }
                else if (getOrSet && state.Code[i] != ':')
                {
                    i = start;
                    var mode = state.Code[i] == 's' ? FunctionKind.Setter : FunctionKind.Getter;
                    var propertyAccessor = FunctionDefinition.Parse(state, ref i, mode) as FunctionDefinition;
                    var accessorName = propertyAccessor._name;
                    if (!fields.ContainsKey(accessorName))
                    {
                        var propertyPair = new PropertyPair
                        (
                            mode == FunctionKind.Getter ? propertyAccessor : null,
                            mode == FunctionKind.Setter ? propertyAccessor : null
                        );
                        fields.Add(accessorName, propertyPair);
                        properties.Add(
                            new KeyValuePair<Expression, Expression>(
                                new Constant(accessorName)
                                {
                                    Position = start,
                                    Length = accessorName.Length,
                                },
                                propertyPair));
                    }
                    else
                    {
                        var vle = fields[accessorName] as PropertyPair;

                        if (vle == null)
                            ExceptionHelper.ThrowSyntaxError("Try to define " + mode.ToString().ToLowerInvariant() + " for defined field", state.Code, start);

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

                            ExceptionHelper.ThrowSyntaxError("Try to redefine " + mode.ToString().ToLowerInvariant() + " of " + propertyAccessor.Name, state.Code, start);
                        }
                        while (false);
                    }
                }
                else if (Parser.Validate(state.Code, "...", ref i))
                {
                    var value = ExpressionTree.Parse(state, ref i, processComma: false);
                    properties.Add(new KeyValuePair<Expression, Expression>(ObjectSpreadMarker.Instance, value));
                }
                else
                {
                    if (asterisk)
                    {
                        do
                            i++;
                        while (Tools.IsWhiteSpace(state.Code[i]));
                    }

                    i = start;
                    var fieldName = "";
                    if (Parser.ValidateName(state.Code, ref i, false, true, state.Strict))
                    {
                        fieldName = Tools.Unescape(state.Code.Substring(start, i - start), state.Strict);
                    }
                    else if (Parser.ValidateValue(state.Code, ref i))
                    {
                        if (state.Code[start] == '-')
                            ExceptionHelper.Throw(new SyntaxError("Invalid char \"-\" at " + CodeCoordinates.FromTextPosition(state.Code, start, 1)));

                        int n = start;
                        if (Tools.ParseJsNumber(state.Code, ref n, out double d))
                            fieldName = NumberUtils.DoubleToString(d);
                        else if (state.Code[start] == '\'' || state.Code[start] == '"')
                            fieldName = Tools.Unescape(state.Code.Substring(start + 1, i - start - 2), state.Strict);
                        else if (properties.Count != 0)
                            ExceptionHelper.Throw((new SyntaxError("Invalid field name at " + CodeCoordinates.FromTextPosition(state.Code, start, i - start))));
                        else
                            return null;
                    }
                    else
                        return null;

                    Tools.SkipSpaces(state.Code, ref i);

                    Expression initializer = null;

                    if (state.Code[i] == '(')
                    {
                        i = start;
                        initializer = FunctionDefinition.Parse(state, ref i, asterisk ? FunctionKind.MethodGenerator : async ? FunctionKind.AsyncMethod : FunctionKind.Method);
                    }
                    else
                    {
                        if (asterisk || (async && state.Code[i] != ':'))
                            ExceptionHelper.ThrowSyntaxError("Unexpected token", state.Code, i);

                        if (state.Code[i] != ':' && state.Code[i] != ',' && state.Code[i] != '}')
                            ExceptionHelper.ThrowSyntaxError("Expected ',', ';' or '}'", state.Code, i);

                        if (fields.TryGetValue(fieldName, out Expression aei))
                        {
                            if (state.Strict ? (!(aei is Constant constant) || constant.value != JSValue.undefined) : aei is PropertyPair)
                                ExceptionHelper.ThrowSyntaxError("Try to redefine field \"" + fieldName + "\"", state.Code, start, i - start);

                            if (state.Message != null)
                                state.Message(MessageLevel.Warning, i, 0, "Duplicate key \"" + fieldName + "\"");
                        }

                        if (state.Code[i] == ',' || state.Code[i] == '}')
                        {
                            if (!Parser.ValidateName(fieldName, 0))
                                ExceptionHelper.ThrowSyntaxError("Invalid variable name", state.Code, i);

                            var variable = new Variable(fieldName, state.LexicalScopeLevel)
                            {
                                Position = start,
                                Length = fieldName.Length,
                            };

                            initializer = variable;
                        }
                        else
                        {
                            i++;
                            Tools.SkipSpaces(state.Code, ref i);

                            initializer = ExpressionTree.Parse(state, ref i, false, false);
                        }
                    }

                    fields[fieldName] = initializer;
                    properties.Add(
                        new KeyValuePair<Expression, Expression>(
                            new Constant(fieldName)
                            {
                                Position = start,
                                Length = fieldName.Length,
                            },
                            initializer));
                }

                while (Tools.IsWhiteSpace(state.Code[i]))
                    i++;

                if ((state.Code[i] != ',') && (state.Code[i] != '}'))
                    return null;
            }

            i++;
            var pos = index;
            index = i;
            return new ObjectDefinition(properties.ToArray())
            {
                Position = pos,
                Length = index - pos
            };
        }

        public override JSValue Evaluate(Context context)
        {
            var res = JSObject.CreateObject();
            if (_properties.Length == 0)
                return res;

            res._fields = JSObject.getFieldsContainer();

            for (var i = 0; i < _properties.Length; i++)
            {
                var key = _properties[i].Key.Evaluate(context);
                var value = _properties[i].Value.Evaluate(context).CloneImpl(false);
                value._attributes = JSValueAttributesInternal.None;

                JSValue existedValue;
                Symbol symbolKey = null;
                string stringKey = null;

                if (key == ObjectSpreadMarker.FakeResult)
                {
                    for (var enumerator = value.GetEnumerator(true, EnumerationMode.RequireValues, PropertyScope.Own); enumerator.MoveNext();)
                    {
                        var property = enumerator.Current;
                        res._fields[property.Key] = property.Value.CloneImpl(true, (JSValueAttributesInternal)int.MaxValue);
                    }
                }
                else
                {
                    if (key.Is<Symbol>())
                    {
                        symbolKey = key.As<Symbol>();

                        if (res._symbols == null)
                            res._symbols = new Dictionary<Symbol, JSValue>();

                        res._symbols.TryGetValue(symbolKey, out existedValue);
                    }
                    else
                    {
                        stringKey = key.As<string>();

                        res._fields.TryGetValue(stringKey, out existedValue);
                    }

                    if (existedValue != value)
                    {
                        if (existedValue.Is(JSValueType.Property) && value.Is(JSValueType.Property))
                        {
                            var oldGs = existedValue.As<Core.PropertyPair>();
                            var newGs = value.As<Core.PropertyPair>();
                            oldGs.getter = newGs.getter ?? oldGs.getter;
                            oldGs.setter = newGs.setter ?? oldGs.setter;
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
            }

            return res;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            _codeContext = codeContext;

            for (var i = 0; i < _properties.Length; i++)
            {
                var key = _properties[i].Key;
                Parser.Build(ref key, 2, variables, codeContext | CodeContext.InExpression, message, stats, opts);

                var value = _properties[i].Value;
                Parser.Build(ref value, 2, variables, codeContext | CodeContext.InExpression, message, stats, opts);

                _properties[i] = new KeyValuePair<Expression, Expression>(key, value);
            }

            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            for (var i = 0; i < _properties.Length; i++)
            {
                var key = _properties[i].Key;
                key.Optimize(ref key, owner, message, opts, stats);

                var value = _properties[i].Value;
                value.Optimize(ref value, owner, message, opts, stats);

                _properties[i] = new KeyValuePair<Expression, Expression>(key, value);
            }
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            base.RebuildScope(functionInfo, transferedVariables, scopeBias);

            for (var i = 0; i < _properties.Length; i++)
            {
                _properties[i].Key.RebuildScope(functionInfo, transferedVariables, scopeBias);
                _properties[i].Value.RebuildScope(functionInfo, transferedVariables, scopeBias);
            }
        }

        protected internal override CodeNode[] GetChildrenImpl()
        {
            var list = new List<CodeNode>(_properties.Length * 2);
            list.AddRange(_properties.Select(x => x.Value));
            list.AddRange(_properties.Select(x => x.Key));
            return list.ToArray();
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            var lastDecomposeIndex = -1;

            for (var i = 0; i < _properties.Length; i++)
            {
                var key = _properties[i].Key;
                key.Decompose(ref key, result);

                var value = _properties[i].Value;
                value.Decompose(ref value, result);

                if (!value.ContextIndependent)
                {
                    result.Add(new StoreValue(value, false));
                    value = new ExtractStoredValue(value);
                }

                if ((key != _properties[i].Key) || (value != _properties[i].Value))
                {
                    _properties[i] = new KeyValuePair<Expression, Expression>(key, value);
                }
            }

            for (var i = 0; i < lastDecomposeIndex; i++)
            {
                Expression key = null;
                Expression value = null;

                if (_properties[i].Key is not ExtractStoredValue and not Constant)
                {
                    result.Add(new StoreValue(_properties[i].Key, false));
                    key = new ExtractStoredValue(_properties[i].Key);
                }
                if (_properties[i].Value is not ExtractStoredValue and not Constant)
                {
                    result.Add(new StoreValue(_properties[i].Value, false));
                    value = new ExtractStoredValue(_properties[i].Value);
                }
                if ((key != null) || (value != null))
                {
                    _properties[i] = new KeyValuePair<Expression, Expression>(
                        key ?? _properties[i].Key,
                        value ?? _properties[i].Value);
                }
            }
        }

        public override string ToString()
        {
            string res = "{ ";

            for (int i = 0; i < _properties.Length; i++)
            {
                if (_properties[i].Key is ObjectSpreadMarker)
                    res += "..." + _properties[i].Value;
                else
                    res += (_properties[i].Key as Constant as object ?? ("[" + _properties[i].Key + "]")) + " : " + _properties[i].Value;
                if (i + 1 < _properties.Length)
                    res += ", ";
            }

            return res + " }";
        }
    }
}