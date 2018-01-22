using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    internal sealed class EntityReference : VariableReference
    {
        public EntityDefinition Entity { get { return (EntityDefinition)Descriptor.initializer; } }

        public override string Name
        {
            get
            {
                return Entity._name;
            }
        }

        public override JSValue Evaluate(Context context)
        {
            throw new InvalidOperationException();
        }

        public EntityReference(EntityDefinition entityDefinition)
        {
            ScopeLevel = 1;
            this._descriptor = new VariableDescriptor(entityDefinition._name, 1)
            {
                lexicalScope = !entityDefinition.Hoist,
                initializer = entityDefinition
            };
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return Descriptor.ToString();
        }
    }

    /// <summary>
    /// Базовый тип для ClassNotation и FunctionNotation.
    /// 
    /// Base type fot ClassNotation and FunctionNotation.
    /// </summary>
    public abstract class EntityDefinition : Expression
    {
        [CLSCompliant(false)]
        protected bool Built { get; set; }

        internal readonly VariableReference reference;

        internal readonly string _name;
        public string Name { get { return _name; } }
        public VariableReference Reference { get { return reference; } }

        public abstract bool Hoist { get; }

        protected EntityDefinition(string name)
        {
            _name = name;
            reference = new EntityReference(this);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
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
