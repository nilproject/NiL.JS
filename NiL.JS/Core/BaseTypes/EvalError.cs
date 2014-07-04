using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public sealed class EvalError : Error
    {
        public override JSObject message
        {
            [Hidden]
            get
            {
                return base.message;
            }
        }
        public override JSObject name
        {
            [Hidden]
            get { return "EvalError"; }
        }

        [DoNotEnumerate]
        public EvalError()
        {

        }

        [DoNotEnumerate]
        public EvalError(Arguments args)
            : base(args[0].ToString())
        {

        }

        [DoNotEnumerate]
        public EvalError(string message)
            : base(message)
        {

        }
    }
}
