using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
    [Serializable]
    public sealed class SyntaxError : Error
    {
        public override JSObject message
        {
            get
            {
                return base.message;
            }
        }
        public override JSObject name { get { return "SyntaxError"; } }

        public SyntaxError()
        {

        }

        public SyntaxError(JSObject args)
            : base(args.GetMember("0").ToString())
        {

        }

        public SyntaxError(string message)
            : base(message)
        {

        }
    }
}
