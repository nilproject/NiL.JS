using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
    public class URIError : Error
    {
        public override JSObject name { get { return "URIError"; } }

        public URIError()
        {

        }

        public URIError(string message)
            : base(message)
        {

        }
    }
}
