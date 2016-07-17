using System;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
{
    [Prototype(typeof(Error))]
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class URIError : Error
    {
        [DoNotEnumerate]
        public URIError()
        {

        }

        [DoNotEnumerate]
        public URIError(Arguments args)
            : base(args[0].ToString())
        {

        }

        [DoNotEnumerate]
        public URIError(string message)
            : base(message)
        {

        }
    }
}
