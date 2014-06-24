using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Prototype(typeof(Error))]
    [Serializable]
    public sealed class SyntaxError : Error
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
            get { return "SyntaxError"; }
        }

        [DoNotEnumerate]
        public SyntaxError()
        {

        }

        [DoNotEnumerate]
        public SyntaxError(JSObject args)
            : base(args.GetMember("0").ToString())
        {

        }

        [DoNotEnumerate]
        public SyntaxError(string message)
            : base(message)
        {

        }
    }
}
