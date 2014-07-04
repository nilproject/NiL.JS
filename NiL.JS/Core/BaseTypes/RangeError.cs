using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Prototype(typeof(Error))]
    [Serializable]
    public sealed class RangeError : Error
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
            get { return "RangeError"; }
        }

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
