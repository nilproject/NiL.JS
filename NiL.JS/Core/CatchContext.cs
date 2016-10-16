using System;

namespace NiL.JS.Core
{
    internal sealed class CatchContext : Context
    {
        private JSValue errorContainer;
        private Context prototype;
        private string errorVariableName;

        internal CatchContext(JSValue e, Context proto, string name)
            : base(proto, false, proto._owner)
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
            _strict = proto._strict;
            _variables = proto._variables;
        }

        public override JSValue DefineVariable(string name, bool deletable)
        {
            return prototype.DefineVariable(name);
        }

        protected internal override JSValue GetVariable(string name, bool forWrite)
        {
            if (name == errorVariableName && errorContainer.Exists)
                return errorContainer;

            return base.GetVariable(name, forWrite);
        }
    }
}
