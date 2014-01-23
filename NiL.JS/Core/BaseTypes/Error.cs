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

        }

        public Error(string message)
        {
            this.message = message;
        }
    }
}
