using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public class Error
    {
        public virtual JSObject message
        {
            [Hidden]
            get;
            private set;
        }
        public virtual JSObject name
        {
            [Hidden]
            get { return "Error"; }
        }

        [DoNotEnumerate]
        public Error()
        {
            message = "";
        }

        [DoNotEnumerate]
        public Error(Arguments args)
        {
            message = args[0].ToString();
        }

        [DoNotEnumerate]
        public Error(string message)
        {
            this.message = message;
        }

        [Hidden]
        public override string ToString()
        {
            return name + ": " + message;
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public JSObject toString()
        {
            return ToString();
        }
    }
}
