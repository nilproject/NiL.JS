using System;
using NiL.JS.Expressions;

namespace NiL.JS.Core;

#if !(PORTABLE || NETCORE)
[Serializable]
#endif
public abstract class VariableReference : Expression
{
    internal VariableDescriptor _descriptor;
    public VariableDescriptor Descriptor => _descriptor;

    internal int _scopeLevel;
    public int ScopeLevel
    {
        get => _scopeLevel; 
        internal set => _scopeLevel = value;
    }

    public bool IsCacheEnabled => _scopeLevel >= 0;

    public abstract string Name { get; }

    protected internal override bool ContextIndependent => false;

    internal override bool ResultInTempContainer => false;

    protected internal override PredictedType ResultType => _descriptor.lastPredictedType;

    protected VariableReference()
    {
    }

    protected internal override CodeNode[] GetChildrenImpl() => null;
}
