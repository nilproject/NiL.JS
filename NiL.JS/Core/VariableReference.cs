using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NiL.JS.Expressions;

namespace NiL.JS.Core
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public abstract class VariableReference : Expression
    {
        internal VariableDescriptor _descriptor;
        public VariableDescriptor Descriptor { get { return _descriptor; } }

        internal int _scopeLevel;
        public int ScopeLevel
        {
            get
            {
                return _scopeLevel;
            }
            internal set
            {
                _scopeLevel = value + _scopeBias;
            }
        }

        public bool IsCacheEnabled
        {
            get
            {
                return _scopeLevel >= 0;
            }
        }

        private int _scopeBias;
        public int ScopeBias
        {
            get
            {
                return _scopeBias;
            }
            internal set
            {
                var sign = Math.Sign(_scopeLevel);
                _scopeLevel -= _scopeBias * sign;
                _scopeLevel += value * sign;
                _scopeBias = value;
            }
        }

        public abstract string Name { get; }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return _descriptor.lastPredictedType;
            }
        }

        protected VariableReference()
        {

        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            return null;
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            ScopeBias = scopeBias;

            VariableDescriptor desc = null;
            if (transferedVariables != null && transferedVariables.TryGetValue(Name, out desc))
            {
                _descriptor?.references.Remove(this);
                desc.references.Add(this);
                _descriptor = desc;
            }
        }
    }
}
