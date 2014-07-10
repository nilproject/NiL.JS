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
            functionDepth = -1;
            int i = 0;
            if ((name != "this") && !Parser.ValidateName(name, i, true, true, false))
                throw new ArgumentException("Invalid variable name");
            this.variableName = name;
        }

        internal override JSObject InvokeForAssing(Context context)
        {
            if (context.strict)
            {
                var res = descriptor.Get(context, false, functionDepth);
                if (res.valueType < JSObjectType.Undefined)
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable \"" + variableName + "\" is not defined.")));
                return res;
            }
            return descriptor.Get(context, true, functionDepth);
        }

        internal override JSObject Invoke(Context context)
        {
            var res = descriptor.Get(context, false, functionDepth);
            if (res.valueType < JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable \"" + variableName + "\" is not defined.")));
            if (res.valueType == JSObjectType.Property)
            {
                var getter = (res.oValue as NiL.JS.Core.BaseTypes.Function[])[1];
                if (getter == null)
                    return JSObject.notExists;
                return getter.Invoke(context.objectSource, null);
            }
            return res;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        public override string ToString()
        {
            return variableName;
        }

        internal override bool Optimize(ref CodeNode _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            VariableDescriptor desc = null;
            if (!variables.TryGetValue(variableName, out desc) || desc == null)
            {
                this.descriptor = new VariableDescriptor(this, false, fdepth);
                variables[variableName] = this.descriptor;
            }
            else
            {
                desc.references.Add(this);
                this.descriptor = desc;
            }
            return false;
        }
    }
}