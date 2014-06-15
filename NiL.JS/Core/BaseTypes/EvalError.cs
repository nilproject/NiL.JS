using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
    [Serializable]
    public sealed class EvalError : Error
    {
        public override JSObject message
        {
            get
            {
                return base.message;
            }
        }
        public override JSObject name { get { return "EvalError"; } }

        public EvalError()
        {

        }

        public EvalError(JSObject args)
            : base(args.GetMember("0").ToString())
        {

        }

        public EvalError(string message)
            : base(message)
        {

        }
    }
}
