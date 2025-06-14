#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NiL.JS.Core;

namespace NiL.JS.Expressions;

public enum DestructuringType
{
    Object,
    Array
}

#if !NETCORE
[Serializable]
#endif
public sealed class ObjectDesctructor : Expression
{
    private sealed class DestructuringAcceptor : JSValue
    {
        private readonly Context _context;
        private readonly bool _force;
        private readonly ObjectDesctructor _definition;

        public DestructuringAcceptor(ObjectDesctructor definition, Context context, bool force)
        {
            _definition = definition;
            _context = context;
            _force = force;
        }

        public override void Assign(JSValue value)
        {
            Arguments? arguments = null;
            var items = _definition._destructDescription;

            HashSet<string>? usedSrcKeys = _definition.HasRest ? [] : null;

            for (var i = 0; i < items.Length; i++)
            {
                if (_definition.HasRest && i == items.Length - 1)
                {
                    var target = items[i].Target.EvaluateForWrite(_context);
                    if (target is DestructuringAcceptor { _definition: { HasRest: false } }
                        || items.Length == 1)
                    {
                        assign(items[i].Target, target, value, ref arguments);
                    }
                    else if (_definition.Type is DestructuringType.Object)
                    {
                        var obj = JSObject.CreateObject();
                        foreach (var kvp in value)
                        {
                            if (usedSrcKeys!.Contains(kvp.Key))
                                continue;

                            obj[kvp.Key] = Tools.GetPropertyOrValue(kvp.Value, value);
                        }

                        assign(items[i].Target, target, obj, ref arguments);
                    }
                    else if (_definition.Type is DestructuringType.Array)
                    {
                        var nestedArray = new BaseLibrary.Array();
                        var srcLen = Tools.getLengthOfArraylike(value, false);
                        for (var index = items[items.Length - 2].Key.Evaluate(null)._iValue + 1; index < srcLen; index++)
                        {
                            nestedArray.Add(Tools.GetPropertyOrValue(value[Tools.Int32ToString(index)], value));
                        }

                        assign(items[i].Target, target, nestedArray, ref arguments);
                    }
                    else throw new NotImplementedException();
                }
                else
                {
                    var key = items[i].Key.Evaluate(_context).ToString();

                    usedSrcKeys?.Add(key);

                    var src = Tools.GetPropertyOrValue(value[key], value);
                    if (src is { Exists: false } or null)
                        src = items[i].Default?.Evaluate(_context);

                    if (src is { Exists: true })
                    {
                        var target = items[i].Target.EvaluateForWrite(_context);
                        assign(items[i].Target, target, src, ref arguments);
                    }
                }
            }
        }

        private void assign(CodeNode targetExp, JSValue target, JSValue value, ref Arguments? setterArgs)
        {
            if (target._valueType == JSValueType.Property)
            {
                var fieldSource = _context._objectSource;

                setterArgs ??= [];
                setterArgs.Reset();
                setterArgs.Add(value);

                var setter = (target._oValue as Core.PropertyPair)?.setter;
                if (setter != null)
                    setter.Call(fieldSource, setterArgs);
                else if (_context._strict)
                    ExceptionHelper.ThrowTypeError(string.Format(Strings.CannotAssignReadOnly, targetExp));
            }
            else
            {
                if ((target._attributes & JSValueAttributesInternal.ReadOnly) != 0)
                {
                    if (_force)
                    {
                        target._attributes &= ~JSValueAttributesInternal.ReadOnly;
                        target.Assign(value);
                        target._attributes |= JSValueAttributesInternal.ReadOnly;
                    }
                    else if (_context._strict)
                        ExceptionHelper.ThrowTypeError(string.Format(Strings.CannotAssignReadOnly, targetExp));
                }
                else
                    target.Assign(value);
            }
        }
    }

    private readonly (CodeNode Key, CodeNode Target, CodeNode? Default)[] _destructDescription;

    protected internal override bool ContextIndependent => false;

    protected internal override PredictedType ResultType => PredictedType.Object;

    public bool Force { get; internal set; }

    public bool HasRest { get; }

    public DestructuringType Type { get; }

    public ObjectDesctructor((CodeNode Key, CodeNode Target, CodeNode? Default)[] expressions, bool hasRest, DestructuringType type)
    {
        if (!CheckTargets(expressions, false))
            throw new ArgumentException(nameof(expressions));

        _destructDescription = expressions;
        HasRest = hasRest;
        Type = type;
    }

    public static bool CheckTargets((CodeNode Key, CodeNode Target, CodeNode? Default)[] targets, bool @throw)
    {
        for (var i = 0; i < targets.Length; i++)
        {
            if (targets[i].Target is ObjectDesctructor obj
                ? !CheckTargets(obj._destructDescription, @throw)
                : !ExpressionTree.canBeAssignee((Expression)targets[i].Target))
            {
                if (@throw)
                    ExceptionHelper.ThrowReferenceError(Strings.InvalidLefthandSideInAssignment);
                return false;
            }
        }

        return true;
    }

    internal static CodeNode Parse(ParseInfo state, ref int index)
    {
        int pos = index;

        var destructChar = state.Code[pos] switch
        {
            '[' => ']',
            '{' => '}',
            _ => throw new InvalidOperationException()
        };

        var type = destructChar switch
        {
            ']' => DestructuringType.Array,
            '}' => DestructuringType.Object,
            _ => throw new InvalidOperationException()
        };

        var expressions = new List<(CodeNode? Key, CodeNode Target, CodeNode? Default)>();
        var hasRest = false;

        do
        {
            if (hasRest)
                ExceptionHelper.ThrowSyntaxError("Rest parameters must be the last in parameters list", state.Code, pos);

            pos++;
            Tools.SkipSpaces(state.Code, ref pos);

            while (type == DestructuringType.Array && state.Code[pos] is ',')
            {
                expressions.Add(default);
                pos++;
                Tools.SkipSpaces(state.Code, ref pos);
            }

            hasRest = Parser.Validate(state.Code, "...", ref pos);

            Expression key = null!;

            var nameStart = pos;
            if (type is DestructuringType.Object)
            {
                if (!Parser.ValidateName(state.Code, ref pos, state.Strict))
                {
                    if (state.Code[pos] == '[')
                        key = (Expression)ExpressionTree.Parse(state, ref pos);
                    else
                        ExceptionHelper.ThrowSyntaxError("Name expected", state.Code, pos);
                }
                else
                {
                    var name = state.Code.Substring(nameStart, pos - nameStart);
                    key = new Constant(name)
                    {
                        Position = nameStart,
                        Length = name.Length
                    };
                }

                Tools.SkipSpaces(state.Code, ref pos);
            }
            else
            {
                key = new Constant(
                    state.IntConstants.TryGetValue(expressions.Count, out var val)
                    ? val
                    : state.IntConstants[expressions.Count] = expressions.Count);
            }

            if (state.Code.Length == pos)
                ExceptionHelper.ThrowSyntaxError("Unexpected end of code", state.Code, pos);

            Expression? defaultValue = null;
            Expression? target = null;

            if (type is DestructuringType.Array || (!hasRest && state.Code[pos] is ':'))
            {
                if (type is DestructuringType.Object)
                {
                    pos++;
                    Tools.SkipSpaces(state.Code, ref pos);
                }

                var targetStart = pos;
                if (Parser.ValidateName(state.Code, ref pos, state.Strict))
                {
                    target = new Variable(state.Code.Substring(targetStart, pos - targetStart), state.LexicalScopeLevel)
                    {
                        Position = targetStart,
                        Length = pos - targetStart
                    };
                }
                else
                {
                    target = (Expression)Parse(state, ref pos);
                }
            }
            else
            {
                if (key is not Constant)
                    ExceptionHelper.ThrowSyntaxError("':' expected", state.Code, pos);

                var keyStr = key.Evaluate(null).ToString()!;
                target = new Variable(keyStr, state.LexicalScopeLevel)
                {
                    Position = nameStart,
                    Length = keyStr.Length
                };
            }

            Tools.SkipSpaces(state.Code, ref pos);
            if (state.Code[pos] == '=')
            {
                pos++;
                Tools.SkipSpaces(state.Code, ref pos);

                defaultValue = ExpressionTree.Parse(state, ref pos, processComma: false);
            }

            expressions.Add(new(key, target, defaultValue));
        }
        while (state.Code[pos] == ',');

        if (state.Code[pos] != destructChar)
            ExceptionHelper.ThrowSyntaxError("Unexpected character. '" + destructChar + "' was expected", state.Code, pos);

        var result = new ObjectDesctructor([.. expressions.Where(x => x.Key is not null)!], hasRest, type)
        {
            Position = index,
            Length = pos - index + 1,
        };

        index = pos + 1;

        return result;
    }

    public override JSValue Evaluate(Context context) => throw new InvalidOperationException();

    protected internal override JSValue EvaluateForWrite(Context context) => new DestructuringAcceptor(this, context, Force);

    public override bool Build(
        ref CodeNode _this,
        int expressionDepth, 
        int scopeLevel,
        Dictionary<string, VariableDescriptor> variables,
        CodeContext codeContext,
        InternalCompilerMessageCallback message,
        FunctionInfo stats,
        Options opts)
    {
        var result = false;
        for (var i = 0; i < _destructDescription.Length; i++)
        {
            var key = _destructDescription[i].Key;
            result |= key.Build(ref key, 2, scopeLevel, variables, codeContext, message, stats, opts);

            var target = _destructDescription[i].Target;
            result |= target.Build(ref target, 2, scopeLevel, variables, codeContext, message, stats, opts);

            var @default = _destructDescription[i].Default;
            result |= @default?.Build(ref @default, 2, scopeLevel, variables, codeContext, message, stats, opts) ?? false;

            _destructDescription[i] = (key, target, @default);
        }

        return result;
    }

    public void DefineVariables(Context context)
    {
        for (var i = 0; i < _destructDescription.Length; i++)
        {
            if (_destructDescription[i].Target is Variable v)
            {
                if (v.Descriptor.cacheContext != null)
                {
                    if (v.Descriptor.cacheContext._variables == null)
                        v.Descriptor.cacheContext._variables = JSObject.getFieldsContainer();
                    v.Descriptor.cacheContext._variables[v.Name] = v.Descriptor.cacheValue;
                }

                v.Descriptor.cacheContext = context;

                if (context._variables is not null)
                    v.Descriptor.cacheValue = context.DefineVariable(v.Name);
                else
                    v.Descriptor.cacheValue = new JSValue()
                    {
                        _valueType = JSValueType.Undefined,
                        _attributes = JSValueAttributesInternal.DoNotDelete
                    };
            }
            else
                ((ObjectDesctructor)_destructDescription[i].Target).DefineVariables(context);
        }
    }

    internal void clearVariablesCache()
    {
        for (var i = 0; i < _destructDescription.Length; i++)
        {
            if (_destructDescription[i].Target is Variable v)
            {
                v.Descriptor.cacheContext = null;
                v.Descriptor.cacheValue = null;
            }
            else
                ((ObjectDesctructor)_destructDescription[i].Target).clearVariablesCache();
        }
    }

    public IList<Variable> GetTargetVariables()
    {
        var result = new List<Variable>();
        collectTargetVariables(this, result);
        return result;
    }

    private static void collectTargetVariables(ObjectDesctructor objectDesctructor, List<Variable> result)
    {
        for (var i = 0; i < objectDesctructor._destructDescription.Length; i++)
        {
            if (objectDesctructor._destructDescription[i].Target is Variable)
                result.Add((Variable)objectDesctructor._destructDescription[i].Target);
            else
                collectTargetVariables((ObjectDesctructor)objectDesctructor._destructDescription[i].Target, result);
        }
    }

    public override string ToString()
    {
        if (Type is DestructuringType.Object)
        {
            return "{ "
                + string.Join(
                    ", ",
                    _destructDescription.Select((x, i) =>
                    {
                        var key = x.Key is Constant cnstKey ? cnstKey.Value.ToString() : "[" + x.Key.ToString() + "]";

                        var result = key;
                        if (HasRest && i == _destructDescription.Length - 1)
                            result = "..." + result;

                        if (x.Target is not Variable v0 || x.Key is not Constant || v0.Name != key)
                            result += ": " + x.Target;

                        if (x.Default is not null)
                            result += " = " + x.Default;

                        return result;
                    }))
                + " }";
        }
        else
        {
            return "[ "
                + string.Join(
                    ",",
                    _destructDescription.Select((x, i) =>
                    {
                        var result = string.Empty;
                        var key = Tools.JSObjectToInt32(x.Key.Evaluate(null));
                        if (i != key)
                            result = new string(',', key - i);

                        result += " " + x.Target;
                        if (HasRest && i == _destructDescription.Length - 1)
                            result = "..." + result;

                        if (x.Default is not null)
                            result += " = " + x.Default;

                        return result;
                    }))
                + " ]";
        }
    }

    protected internal override CodeNode[] GetChildrenImpl()
    {
        var src = Type switch
        {
            DestructuringType.Object => _destructDescription.Select(x => new CodeNode[] { x.Key, x.Target, x.Default! }),
            DestructuringType.Array => _destructDescription.Select(x => new CodeNode[] { x.Target, x.Default! }),
            _ => throw new NotSupportedException()
        };
        return src.SelectMany(x => x.Where(y => y != null)).ToArray();
    }
}
