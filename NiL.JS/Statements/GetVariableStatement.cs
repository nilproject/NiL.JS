using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class GetVariableStatement : VariableReference
    {
        private string variableName;
        private VariableDescriptor descriptor;
        public override VariableDescriptor Descriptor { get { return descriptor; } internal set { descriptor = value; } }

        public override string Name { get { return variableName; } }

        internal GetVariableStatement(string name)
        {
            int i = 0;
            if ((name != "this") && !Parser.ValidateName(name, i, true, true, false))
                throw new ArgumentException("Invalid variable name");
            this.variableName = name;
        }

        internal override JSObject InvokeForAssing(Context context)
        {
            if (context.strict)
                return Tools.RaiseIfNotExist(descriptor.Get(context, false));
            return descriptor.Get(context, true);
        }

        internal override JSObject Invoke(Context context)
        {
            var res = Tools.RaiseIfNotExist(descriptor.Get(context, false));
            if (res.valueType == JSObjectType.Property)
                return (res.oValue as NiL.JS.Core.BaseTypes.Function[])[1].Invoke(context.objectSource, null);
            return res;
        }

        protected override Statement[] getChildsImpl()
        {
            return null;
        }

        public override string ToString()
        {
            return variableName;
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            VariableDescriptor desc = null;
            if (!variables.TryGetValue(variableName, out desc) || desc == null)
            {
                this.descriptor = new VariableDescriptor(this, desc != null);
                if (variableName == "this")
                    this.descriptor.attributes |= VariableDescriptorAttributes.NoCaching;
                variables[variableName] = this.descriptor;
            }
            else
            {
                if ((desc.attributes & VariableDescriptorAttributes.SuppressRefRegistration) == 0)
                    desc.references.Add(this);
                this.descriptor = desc;
            }
            return false;
        }
    }
}