using System;

namespace NiL.JS.Core
{
    internal sealed class CatchContext : Context
    {
        private JSObject errorContainer;
        private Context prototype;
        private string errorVariableName;

        internal CatchContext(JSObject e, Context proto, string name)
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

        public override JSObject DefineVariable(string name)
        {
            return prototype.DefineVariable(name);
        }

        protected internal override JSObject GetVariable(string name, bool create)
        {
            if (name == errorVariableName && errorContainer.IsExist)
                return errorContainer;
            return base.GetVariable(name, create);
        }
    }
}
