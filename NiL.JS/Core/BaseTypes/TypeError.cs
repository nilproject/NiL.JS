using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Prototype(typeof(Error))]
    [Serializable]
    public sealed class TypeError : Error
    {
        [DoNotEnumerate]
        public override JSObject message
        {
            [Hidden]
            get
            {
                return base.message;
            }
        }
        [DoNotEnumerate]
        public override JSObject name
        {
            [Hidden]
            get
            {
                return "TypeError";
            }
        }

        [DoNotEnumerate]
        public TypeError(Arguments args)
            : base(args[0].ToString())
        {

        }

        [DoNotEnumerate]
        public TypeError()
        {

        }

        [DoNotEnumerate]
        public TypeError(string message)
            : base(message)
        {
        }
    }
}
