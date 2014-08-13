using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Prototype(typeof(Error))]
    [Serializable]
    public sealed class SyntaxError : Error
    {
        [DoNotEnumerate]
        public SyntaxError()
        {

        }

        [DoNotEnumerate]
        public SyntaxError(Arguments args)
            : base(args[0].ToString())
        {

        }

        [DoNotEnumerate]
        public SyntaxError(string message)
            : base(message)
        {

        }
    }
}
