using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Prototype(typeof(Error))]
    [Serializable]
    public sealed class ReferenceError : Error
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
                return "ReferenceError";
            }
        }

        [DoNotEnumerate]
        public ReferenceError(JSObject args)
            : base(args.GetMember("0").ToString())
        {

        }

        [DoNotEnumerate]
        public ReferenceError()
        {

        }

        [DoNotEnumerate]
        public ReferenceError(string message)
            : base(message)
        {
        }
    }
}
