using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
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
            message = args.GetField("0", true, false).ToString();
        }

        public Error(string message)
        {
            this.message = message;
        }

        public override string ToString()
        {
            return name + ": " + message;
        }

        public JSObject toString()
        {
            return ToString();
        }
    }
}
