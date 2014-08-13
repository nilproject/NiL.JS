using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Prototype(typeof(Error))]
    [Serializable]
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
