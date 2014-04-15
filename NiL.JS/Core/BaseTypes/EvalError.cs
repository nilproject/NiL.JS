using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
    public sealed class EvalError : Error
    {
        public override JSObject name { get { return "EvalError"; } }

        public EvalError()
        {

        }

        public EvalError(JSObject args)
            : base(args.GetField("0", true, false).ToString())
        {

        }

        public EvalError(string message)
            : base(message)
        {

        }
    }
}
