using System;

namespace NiL.JS.Core
{
    internal sealed class CatchContext : Context
    {
        private JSValue errorContainer;
        private Context prototype;
        private string errorVariableName;

        internal CatchContext(JSValue e, Context proto, string name)
            : base(proto, false, proto.caller)
        {
            if (e == null)
                throw new ArgumentNullException();
            if (proto == null)
                throw new ArgumentNullException();
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException();
            errorContainer = e;
            prototype = proto;
            errorVariableName = name;
            strict = proto.strict;
            tempContainer = proto.tempContainer;
            fields = proto.fields;
            variables = proto.variables;
        }

        public override JSValue DefineVariable(string name)
        {
            return prototype.DefineVariable(name);
        }

        protected internal override JSValue GetVariable(string name, bool create)
        {
            if (name == errorVariableName && errorContainer.IsExists)
                return errorContainer;
            return base.GetVariable(name, create);
        }
    }
}
