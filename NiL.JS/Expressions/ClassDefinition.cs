﻿using System;
using System.Collections.Generic;
using System.Text;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace NiL.JS.Expressions;

#if !(PORTABLE || NETCORE)
[Serializable]
#endif
public sealed class MemberDescriptor
{
    internal Expression _name;
    internal Expression _value;
    internal bool _static;

    public Expression Name { get { return _name; } }
    public Expression Value { get { return _value; } }
    public bool Static { get { return _static; } }

    public MemberDescriptor(Expression name, Expression value, bool @static)
    {
        _name = name;
        _value = value;
        _static = @static;
    }

    public override string ToString()
    {
        if (_static)
            return "static " + _name + " = " + _value;
        return _name + " = " + _value;
    }
}

#if !(PORTABLE || NETCORE)
[Serializable]
#endif
public sealed class ClassDefinition : EntityDefinition
{
    private sealed class ClassConstructor : Function
    {
        private readonly ClassDefinition _classDefinition;
        public override string name
        {
            get
            {
                return _classDefinition.Name;
            }
        }

        public ClassConstructor(Context context, FunctionDefinition creator, ClassDefinition classDefinition)
            : base(context, creator)
        {
            _classDefinition = classDefinition;
        }

        protected internal override JSValue ConstructObject()
        {
            var result = CreateObject();
            result.__proto__ = prototype._oValue as JSObject;
            _classDefinition.setProperties(Context, result, false);
            return result;
        }

        public override string ToString(bool headerOnly)
        {
            return _classDefinition.ToString();
        }
    }

    protected internal override PredictedType ResultType
    {
        get
        {
            return PredictedType.Function;
        }
    }

    internal override bool ResultInTempContainer
    {
        get { return false; }
    }

    protected internal override bool ContextIndependent
    {
        get
        {
            return false;
        }
    }

    protected internal override bool NeedDecompose
    {
        get
        {
            if (_constructor.NeedDecompose)
                return true;

            for (var i = 0; i < _members.Length; i++)
            {
                if (_members[i]._value.NeedDecompose)
                    return true;
            }

            return false;
        }
    }

    private MemberDescriptor[] _members;
    private Expression _baseClass;
    private FunctionDefinition _constructor;
    private MemberDescriptor[] _computedProperties;

    public IEnumerable<MemberDescriptor> Members { get { return _members; } }
    public Expression BaseClass { get { return _baseClass; } }
    public FunctionDefinition Constructor { get { return _constructor; } }
    public IEnumerable<MemberDescriptor> ComputedProperties { get { return _computedProperties; } }

    public override bool Hoist { get { return false; } }

    private ClassDefinition(string name, Expression baseType, MemberDescriptor[] fields, FunctionDefinition ctor, MemberDescriptor[] computedProperties)
        : base(name)
    {
        this._baseClass = baseType;
        this._constructor = ctor;
        this._members = fields;
        this._computedProperties = computedProperties;
    }

    internal static CodeNode Parse(ParseInfo state, ref int index)
    {
        int i = index;

        if (!Parser.Validate(state.Code, "class", ref i))
            return null;

        Tools.SkipSpaces(state.Code, ref i);

        string name = null;
        Expression baseType = null;
        if (!Parser.Validate(state.Code, "extends ", i))
        {
            var n = i;
            if (Parser.ValidateName(state.Code, ref i, true))
                name = state.Code.Substring(n, i - n);

            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
        }
        if (Parser.Validate(state.Code, "extends ", ref i))
        {
            var n = i;
            if (!Parser.ValidateName(state.Code, ref i, true) && !Parser.Validate(state.Code, "null", ref i))
                ExceptionHelper.ThrowSyntaxError("Invalid base class name", state.Code, i);

            var baseClassName = state.Code.Substring(n, i - n);
            if (baseClassName == "null")
                baseType = new Constant(JSValue.@null);
            else
                baseType = new Variable(baseClassName, 1);

            baseType.Position = n;
            baseType.Length = i - n;

            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
        }

        if (state.Code[i] != '{')
            ExceptionHelper.ThrowSyntaxError(Strings.UnexpectedToken, state.Code, i);

        FunctionDefinition ctor = null;

        var flds = new Dictionary<string, MemberDescriptor>();
        var computedProperties = new List<MemberDescriptor>();

        while (state.Code[i] != '}')
        {
            using (state.WithCodeContext(CodeContext.Strict | CodeContext.InExpression))
            {
                do i++; while (Tools.IsWhiteSpace(state.Code[i]) || state.Code[i] == ';');

                int s = i;
                if (state.Code[i] == '}')
                    break;

                bool @static = Parser.Validate(state.Code, "static", ref i);
                if (@static)
                {
                    Tools.SkipSpaces(state.Code, ref i);
                    s = i;
                }

                bool @async = Parser.Validate(state.Code, "async", ref i);
                if (@async)
                {
                    Tools.SkipSpaces(state.Code, ref i);
                    s = i;
                }

                bool getOrSet = Parser.Validate(state.Code, "get", ref i)
                             || Parser.Validate(state.Code, "set", ref i);

                if (getOrSet)
                {
                    Tools.SkipSpaces(state.Code, ref i);

                    if (state.Code[i] == '(')
                    {
                        i = s;
                        getOrSet = false;
                    }
                }

                var asterisk = state.Code[i] == '*';
                if (asterisk)
                {
                    do
                        i++;
                    while (Tools.IsWhiteSpace(state.Code[i]));
                }

                if (Parser.Validate(state.Code, "[", ref i))
                {
                    Tools.SkipSpaces(state.Code, ref i);

                    var propertyName = ExpressionTree.Parse(state, ref i, false, false, false, true, false);

                    Tools.SkipSpaces(state.Code, ref i);

                    if (state.Code[i] != ']')
                        ExceptionHelper.ThrowSyntaxError("Expected ']'", state.Code, i);
                    
                    do
                        i++;
                    while (Tools.IsWhiteSpace(state.Code[i]));

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
                            computedProperties.Add(new MemberDescriptor(propertyName, new PropertyPair((Expression)initializer, null), @static));
                            break;
                        }
                        case 's':
                        {
                            computedProperties.Add(new MemberDescriptor(propertyName, new PropertyPair(null, (Expression)initializer), @static));
                            break;
                        }
                        default:
                        {
                            computedProperties.Add(new MemberDescriptor(propertyName, (Expression)initializer, @static));
                            break;
                        }
                    }
                }
                else if (getOrSet)
                {
                    i = s;
                    var mode = state.Code[i] == 's' ? FunctionKind.Setter : FunctionKind.Getter;
                    var propertyAccessor = FunctionDefinition.Parse(state, ref i, mode) as FunctionDefinition;
                    var accessorName = (@static ? "static " : "") + propertyAccessor._name;
                    if (!flds.ContainsKey(accessorName))
                    {
                        var propertyPair = new PropertyPair
                        (
                            mode == FunctionKind.Getter ? propertyAccessor : null,
                            mode == FunctionKind.Setter ? propertyAccessor : null
                        );
                        flds.Add(accessorName, new MemberDescriptor(new Constant(propertyAccessor._name), propertyPair, @static));
                    }
                    else
                    {
                        var vle = flds[accessorName].Value as PropertyPair;

                        if (vle == null)
                            ExceptionHelper.Throw((new SyntaxError("Try to define " + mode.ToString().ToLowerInvariant() + " for defined field at " + CodeCoordinates.FromTextPosition(state.Code, s, 0))));

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
                    i = s;
                    string fieldName = null;
                    if (state.Code[i] == '*')
                    {
                        do i++; while (Tools.IsWhiteSpace(state.Code[i]));
                    }

                    if (Parser.ValidateName(state.Code, ref i, false, true, state.Strict))
                        fieldName = Tools.Unescape(state.Code.Substring(s, i - s), state.Strict);

                    else if (Parser.ValidateValue(state.Code, ref i))
                    {
                        double d = 0.0;
                        int n = s;
                        if (Tools.ParseJsNumber(state.Code, ref n, out d))
                            fieldName = NumberUtils.DoubleToString(d);
                        else if (state.Code[s] == '\'' || state.Code[s] == '"')
                            fieldName = Tools.Unescape(state.Code.Substring(s + 1, i - s - 2), state.Strict);
                    }

                    if (fieldName == null)
                        ExceptionHelper.Throw((new SyntaxError("Invalid member name at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s))));

                    if (fieldName == "constructor")
                    {
                        if (@static)
                        {
                            ExceptionHelper.ThrowSyntaxError(Strings.ConstructorCannotBeStatic, state.Code, s);
                        }
                        if (ctor != null)
                        {
                            ExceptionHelper.ThrowSyntaxError("Trying to redefinition constructor", state.Code, s);
                        }

                        state.CodeContext |= CodeContext.InClassConstructor;
                    }
                    else if (@static)
                    {
                        state.CodeContext |= CodeContext.InStaticMember;
                    }

                    if (flds.ContainsKey(fieldName))
                        ExceptionHelper.Throw(new SyntaxError("Trying to redefinition member \"" + fieldName + "\" at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s)));

                    state.CodeContext |= CodeContext.InClassDefinition;
                    state.CodeContext &= ~CodeContext.InGenerator;

                    if (async)
                        state.CodeContext |= CodeContext.InAsync;

                    while (Tools.IsWhiteSpace(state.Code[i])) i++;

                    Expression expression = null;

                    if (async || asterisk || fieldName == "constructor" || state.Code[i] == '(')
                    {
                        i = s;
                        expression = FunctionDefinition.Parse(state, ref i, async ? FunctionKind.AsyncMethod : FunctionKind.Method) as FunctionDefinition;
                        if (expression == null)
                            ExceptionHelper.ThrowSyntaxError("Unable to parse method", state.Code, i);
                    }
                    else if (state.Code[i] == '=')
                    {
                        do i++; while (Tools.IsWhiteSpace(state.Code[i]));
                        expression = ExpressionTree.Parse(state, ref i) as Expression;
                    }
                    else if (state.Code[i] == ';')
                    {
                        i++;
                    }

                    if (fieldName == "constructor")
                    {
                        ctor = expression as FunctionDefinition;
                    }
                    else
                    {
                        flds[fieldName] = new MemberDescriptor(new Constant(fieldName), expression, @static);
                    }
                }
            }
        }

        if (ctor == null)
        {
            string ctorCode;
            int ctorIndex = 0;
            if (baseType != null && !(baseType is Constant))
                ctorCode = "constructor(...args) { super(...args); }";
            else
                ctorCode = "constructor(...args) { }";

            var nestedParseInfo = state.AlternateCode(ctorCode);
            using (nestedParseInfo.WithCodeContext(CodeContext.InClassConstructor | CodeContext.InClassDefinition))
                ctor = (FunctionDefinition)FunctionDefinition.Parse(nestedParseInfo, ref ctorIndex, FunctionKind.Method);
        }

        var classDefinition = new ClassDefinition(name, baseType, new List<MemberDescriptor>(flds.Values).ToArray(), ctor, computedProperties.ToArray());
        classDefinition.reference.ScopeLevel = state.LexicalScopeLevel;
        classDefinition.reference._descriptor.definitionScopeLevel = state.LexicalScopeLevel;

        if ((state.CodeContext & CodeContext.InExpression) == 0)
        {
            if ((state.CodeContext & CodeContext.InExport) == 0 || !string.IsNullOrEmpty(name))
            {
                if (string.IsNullOrEmpty(name))
                {
                    ExceptionHelper.ThrowSyntaxError("Class must have a name", state.Code, index);
                }

                if (state.Strict && state.FunctionScopeLevel != state.LexicalScopeLevel)
                {
                    ExceptionHelper.ThrowSyntaxError("In strict mode code, class can only be declared at top level or immediately within other function.", state.Code, index);
                }

                state.Variables.Add(classDefinition.reference._descriptor);
            }
        }

        index = i + 1;
        return classDefinition;
    }

    public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
    {
        if (Built)
            return false;
        Built = true;

        _codeContext = codeContext;

        if ((codeContext & CodeContext.InExpression) == 0)
            stats.WithLexicalEnvironment = true;

        VariableDescriptor descriptorToRestore = null;
        if (!string.IsNullOrEmpty(_name))
        {
            variables.TryGetValue(_name, out descriptorToRestore);
            variables[_name] = reference._descriptor;
        }

        Parser.Build(ref _constructor, expressionDepth, variables, codeContext | CodeContext.InClassDefinition | CodeContext.InClassConstructor, message, stats, opts);
        Parser.Build(ref _baseClass, expressionDepth, variables, codeContext, message, stats, opts);

        for (var i = 0; i < _members.Length; i++)
        {
            Parser.Build
            (
                ref _members[i]._value,
                expressionDepth,
                variables,
                codeContext | CodeContext.InClassDefinition | (_members[i]._static ? CodeContext.InStaticMember : 0),
                message,
                stats,
                opts
            );
        }

        for (var i = 0; i < _computedProperties.Length; i++)
        {
            Parser.Build(ref _computedProperties[i]._name, 2, variables, codeContext | CodeContext.InExpression, message, stats, opts);
            Parser.Build(ref _computedProperties[i]._value, 2, variables, codeContext | CodeContext.InExpression, message, stats, opts);
        }

        if (descriptorToRestore != null)
        {
            variables[descriptorToRestore.name] = descriptorToRestore;
        }
        else if (!string.IsNullOrEmpty(_name))
        {
            variables.Remove(_name);
        }

        return false;
    }

    public override JSValue Evaluate(Context context)
    {
        JSValue variable = null;
        if ((_codeContext & CodeContext.InExpression) == 0 && !string.IsNullOrEmpty(_name))
        {
            variable = context.DefineVariable(_name, false);
        }

        var ctor = new ClassConstructor(context, _constructor, this);
        ctor.RequireNewKeywordLevel = RequireNewKeywordLevel.WithNewOnly;

        if (_baseClass != null)
        {
            var baseProto = _baseClass.Evaluate(context)._oValue as JSObject;
            if (baseProto == null)
            {
                ctor.prototype.__proto__ = null;
            }
            else
            {
                ctor.prototype.__proto__ = Tools.GetPropertyOrValue(baseProto.GetProperty("prototype"), baseProto)._oValue as JSObject;
            }

            ctor.__proto__ = baseProto;
        }

        setProperties(context, ctor, true);

        variable?.Assign(ctor);

        return ctor;
    }

    private void setProperties(Context context, JSObject target, bool @static)
    {
        for (var i = 0; i < _members.Length; i++)
        {
            var member = _members[i];
            if (member.Static != @static)
                continue;

            var value = member.Value?.Evaluate(context);
            if (value is null)
                continue;

            target.SetProperty(member.Name.Evaluate(null), value, true);
        }

        for (var i = 0; i < _computedProperties.Length; i++)
        {
            var member = _computedProperties[i];
            if (member.Static != @static)
                continue;

            var key = member._name.Evaluate(context).CloneImpl(false);
            var value = member._value.Evaluate(context).CloneImpl(false);

            JSValue existedValue;
            Symbol symbolKey = null;
            string stringKey = null;
            if (key.Is<Symbol>())
            {
                symbolKey = key.As<Symbol>();
                if (target._symbols == null)
                    target._symbols = new Dictionary<Symbol, JSValue>();

                if (!target._symbols.TryGetValue(symbolKey, out existedValue))
                    target._symbols[symbolKey] = existedValue = value;
            }
            else
            {
                stringKey = key.As<string>();
                if (!target._fields.TryGetValue(stringKey, out existedValue))
                    target._fields[stringKey] = existedValue = value;
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
                        target._symbols[symbolKey] = value;
                    }
                    else
                    {
                        target._fields[stringKey] = value;
                    }
                }
            }
        }
    }

    public override T Visit<T>(Visitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    protected internal override CodeNode[] GetChildrenImpl()
    {
        var result = new List<CodeNode>();

        for (var i = 0; i < _members.Length; i++)
        {
            result.Add(_members[i]._value);
        }

        for (var i = 0; i < _computedProperties.Length; i++)
        {
            result.Add(_computedProperties[i].Name);
            result.Add(_computedProperties[i].Value);
        }

        if (_baseClass != null)
            result.Add(_baseClass);

        return result.ToArray();
    }

    public override string ToString()
    {
        var result = new StringBuilder();
        result.Append("class ").Append(_name);
        if (_baseClass != null)
            result.Append(" extends ").Append(_baseClass);
        result.Append(" {").Append(Environment.NewLine);

        var temp = _constructor.ToString().Replace(Environment.NewLine, Environment.NewLine + "  ");
        result.Append("constructor");
        result.Append(temp.Substring("constructor".Length));

        for (var i = 0; i < _members.Length; i++)
        {
            temp = _members[i].ToString().Replace(Environment.NewLine, Environment.NewLine + "  ");
            result.Append(temp);
        }

        result.Append(Environment.NewLine).Append("}");
        return result.ToString();
    }

    public override void Decompose(ref Expression self, IList<CodeNode> result)
    {
        for (var i = 0; i < _members.Length; i++)
        {
            _members[i]._value.Decompose(ref _members[i]._value, result); // results will be empty at each iterations
#if DEBUG
            System.Diagnostics.Debug.Assert(result.Count == 0, "Decompose: results not empty");
#endif
        }

        for (var i = 0; i < _computedProperties.Length; i++)
        {
            _computedProperties[i]._name.Decompose(ref _computedProperties[i]._name, result);

            _computedProperties[i]._value.Decompose(ref _computedProperties[i]._value, result);

#if DEBUG
            System.Diagnostics.Debug.Assert(result.Count == 0, "Decompose: results not empty");
#endif
        }
    }

    public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
    {
        _baseClass?.Optimize(ref _baseClass, owner, message, opts, stats);

        for (var i = _members.Length; i-- > 0;)
        {
            if (_members[i]._value is not null)
                _members[i]._value.Optimize(ref _members[i]._value, owner, message, opts, stats);
        }

        for (var i = 0; i < _computedProperties.Length; i++)
        {
            _computedProperties[i]._name.Optimize(ref _computedProperties[i]._name, owner, message, opts, stats);
            _computedProperties[i]._value.Optimize(ref _computedProperties[i]._value, owner, message, opts, stats);
        }
    }

    public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
    {
        base.RebuildScope(functionInfo, null, scopeBias);

        _baseClass?.RebuildScope(functionInfo, null, scopeBias);
        _constructor.RebuildScope(functionInfo, null, scopeBias);
        for (var i = 0; i < _computedProperties.Length; i++)
        {
            _computedProperties[i].Name.RebuildScope(functionInfo, null, scopeBias);
            _computedProperties[i].Value.RebuildScope(functionInfo, null, scopeBias);
        }
        for (var i = 0; i < _members.Length; i++)
        {
            _members[i].Name.RebuildScope(functionInfo, null, scopeBias);
            _members[i].Value?.RebuildScope(functionInfo, null, scopeBias);
        }
    }
}