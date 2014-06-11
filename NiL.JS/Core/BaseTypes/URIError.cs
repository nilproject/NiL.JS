using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
    [Serializable]
    public sealed class URIError : Error
    {
        public override JSObject name { get { return "URIError"; } }

        public URIError()
        {

        }

        public URIError(JSObject args)
            : base(args.GetMember("0").ToString())
        {

        }

        public URIError(string message)
            : base(message)
        {

        }
    }
}
