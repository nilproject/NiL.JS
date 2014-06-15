using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
    [Prototype(typeof(Error))]
    [Serializable]
    public sealed class ReferenceError : Error
    {
        public override JSObject message
        {
            get
            {
                return base.message;
            }
        }
        public override JSObject name
        {
            get
            {
                return "ReferenceError";
            }
        }

        public ReferenceError(JSObject args)
            : base(args.GetMember("0").ToString())
        {

        }

        public ReferenceError()
        {

        }

        public ReferenceError(string message)
            : base(message)
        {
        }
    }
}
