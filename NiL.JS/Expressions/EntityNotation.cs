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
        private EntityNotation owner;

        public EntityNotation Entity { get { return owner; } }

        public override string Name
        {
            get { return owner.name; }
        }

        internal override JSValue Evaluate(Context context)
        {
            return owner.Evaluate(context);
        }

        public EntityReference(EntityNotation owner)
        {
            defineDepth = -1;
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
    public abstract class EntityNotation : Expression
    {
        internal VariableReference reference;
        internal string name;

        public string Name { get { return name; } }
        public VariableReference Reference { get { return reference; } }

        protected EntityNotation()
        {
            reference = new EntityReference(this);
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            if ((state & _BuildState.InExpression) == 0 && name != null)
            {
                VariableDescriptor desc = null;
                if (!variables.TryGetValue(name, out desc) || desc == null)
                    variables[name] = Reference.descriptor ?? new VariableDescriptor(Reference, true, Reference.defineDepth);
                else
                {
                    variables[name] = Reference.descriptor;
                    for (var j = 0; j < desc.references.Count; j++)
                        desc.references[j].descriptor = Reference.descriptor;
                    Reference.descriptor.references.AddRange(desc.references);
                    Reference.descriptor.captured = Reference.descriptor.captured || Reference.descriptor.references.FindIndex(x => x.defineDepth > x.descriptor.defineDepth) != -1;
                }
            }
            return false;
        }
    }
}
