using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class EntityReference : VariableReference
    {
        private EntityDefinition owner;

        public EntityDefinition Entity { get { return owner; } }

        public override string Name
        {
            get { return owner.name; }
        }

        public override JSValue Evaluate(Context context)
        {
            return owner.Evaluate(context);
        }

        public EntityReference(EntityDefinition owner)
        {
            this.owner = owner;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return owner.ToString();
        }
    }

    /// <summary>
    /// Базовый тип для ClassNotation и FunctionNotation.
    /// 
    /// Base type fot ClassNotation and FunctionNotation.
    /// </summary>
    public abstract class EntityDefinition : Expression
    {
        internal VariableReference reference;
        internal string name;

        public string Name { get { return name; } }
        public VariableReference Reference { get { return reference; } }

        protected EntityDefinition()
        {
            reference = new EntityReference(this);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            _codeContext = codeContext;
            return false;
        }

        public override abstract void Decompose(ref Expression self, IList<CodeNode> result);

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            reference.ScopeBias = scopeBias;
            if (reference._descriptor != null)
            {
                if (reference._descriptor.definitionScopeLevel >= 0)
                {
                    reference._descriptor.definitionScopeLevel = reference.ScopeLevel;
                    reference._descriptor.scopeBias = scopeBias;
                }
            }
        }
    }
}
