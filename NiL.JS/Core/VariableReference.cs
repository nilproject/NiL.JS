using System;
using System.Collections.Generic;
using NiL.JS.Expressions;

namespace NiL.JS.Core
{
#if !PORTABLE
    [Serializable]
#endif
    public abstract class VariableReference : Expression
    {
        private int _scopeLevel;
        public int ScopeLevel
        {
            get
            {
                return _scopeLevel;
            }
            set
            {
                _scopeLevel = value + _scopeBias;
            }
        }

        private int _scopeBias;
        internal int ScopeBias
        {
            get
            {
                return _scopeBias;
            }
            set
            {
                _scopeLevel -= _scopeBias;
                _scopeLevel += value;
                _scopeBias = value;
            }
        }

        public abstract string Name { get; }

        internal VariableDescriptor _descriptor;
        public VariableDescriptor Descriptor { get { return _descriptor; } }

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
            _scopeLevel = -1;
        }

        protected internal override CodeNode[] getChildsImpl()
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
