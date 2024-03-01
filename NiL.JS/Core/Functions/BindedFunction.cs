using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;

namespace NiL.JS.Core.Functions;

/// <summary>
/// Реализует поведение функции Function.prototype.bind.
/// </summary>
[Prototype(typeof(Function), true)]
#if !(PORTABLE || NETCORE)
[Serializable]
#endif
internal sealed class BindedFunction : Function
{
    private Function _target;
    private JSValue _thisBind;
    private JSValue[] _bindedArguments;

    public override JSValue caller
    {
        get
        {
            ThrowTypeError();
            return null;
        }
        set
        {
            ThrowTypeError();
        }
    }

    public override JSValue arguments
    {
        get
        {
            ThrowTypeError();
            return null;
        }
        set
        {
            ThrowTypeError();
        }
    }

    public override JSValue prototype
    {
        get
        {
            return null;
        }
        set
        {

        }
    }

    public BindedFunction(Function proto, Arguments args)
        : base(proto.Context, proto._functionDefinition)
    {
        if (_length == null)
            _length = new Number(0);

        _length._iValue = proto.length._iValue;
        _target = proto;
        _thisBind = args[0];
        _bindedArguments = new JSValue[System.Math.Max(args.Length - 1, 0)];
        if (args.Length > 0)
        {
            for (var i = 0; i < _bindedArguments.Length; i++)
                _bindedArguments[i] = args[i + 1];

            _length._iValue -= args.Length - 1;
            if (_length._iValue < 0)
                _length._iValue = 0;
        }

        RequireNewKeywordLevel = proto.RequireNewKeywordLevel;
    }

    protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
    {
        var internalArgs = arguments;

        if (_bindedArguments.Length != 0)
        {
            internalArgs = new Arguments();

            internalArgs.Length = _bindedArguments.Length + arguments.Length;

            for (var i = 0; i < _bindedArguments.Length; i++)
                internalArgs[i] = _bindedArguments[i].CloneImpl();

            for (var i = 0; i < arguments.Length; i++)
                internalArgs[i + _bindedArguments.Length] = arguments[i];
        }

        if ((construct || _thisBind == null || _thisBind.IsNull || !_thisBind.Defined) && (targetObject != null && targetObject.Defined))
            return _target.Invoke(construct, targetObject, internalArgs);

        return construct ? _target.Construct(internalArgs) : _target.Call(_thisBind, internalArgs);
    }

    internal override JSValue InternalInvoke(JSValue targetObject, Expression[] arguments, Context initiator, bool withSpread, bool construct)
    {
        var internalArgs = new Arguments();

        internalArgs.Length = _bindedArguments.Length + arguments.Length;

        for (var i = 0; i < _bindedArguments.Length; i++)
            internalArgs[i] = _bindedArguments[i].CloneImpl();

        for (var i = 0; i < arguments.Length; i++)
            internalArgs[i + _bindedArguments.Length] = arguments[i].Evaluate(initiator);

        if ((construct || _thisBind == null || _thisBind.IsNull || !_thisBind.Defined) && (targetObject != null && targetObject.Defined))
            return _target.Invoke(construct, targetObject, internalArgs);

        return construct ? _target.Construct(internalArgs) : _target.Call(_thisBind, internalArgs);
    }

    protected internal override JSValue ConstructObject()
    {
        return _target.ConstructObject();
    }

    protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnumerable, EnumerationMode enumeratorMode, PropertyScope propertyScope = PropertyScope.Common)
    {
        return _target.GetEnumerator(hideNonEnumerable, enumeratorMode, propertyScope);
    }

    protected internal override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
    {
        return _target.GetProperty(key, forWrite, memberScope);
    }

    public override string ToString(bool headerOnly)
    {
        return _target.ToString(headerOnly);
    }
}
