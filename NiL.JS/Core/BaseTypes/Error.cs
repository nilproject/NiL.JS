using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
    [Serializable]
    public class Error
    {
        public virtual JSObject message { get; private set; }
        public virtual JSObject name { get { return "Error"; } }

        public Error()
        {
            message = "";
        }

        public Error(JSObject args)
        {
            message = args.GetMember("0").ToString();
        }

        public Error(string message)
        {
            this.message = message;
        }

        public override string ToString()
        {
            return name + ": " + message;
        }

        [CLSCompliant(false)]
        public JSObject toString()
        {
            return ToString();
        }
    }
}
