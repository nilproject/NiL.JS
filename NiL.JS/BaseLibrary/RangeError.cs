using System;
using NiL.JS.Core;
using NiL.JS.Core.Modules;

namespace NiL.JS.BaseLibrary
{
    [Prototype(typeof(Error))]
#if !PORTABLE
    [Serializable]
#endif
    public sealed class RangeError : Error
    {
        [DoNotEnumerate]
        public RangeError()
        {

        }

        [DoNotEnumerate]
        public RangeError(Arguments args)
            : base(args[0].ToString())
        {

        }

        [DoNotEnumerate]
        public RangeError(string message)
            : base(message)
        {

        }
    }
}
