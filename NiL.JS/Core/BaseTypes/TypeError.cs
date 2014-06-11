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
    public sealed class TypeError : Error
    {
        public override JSObject name
        {
            get
            {
                return "TypeError";
            }
        }

        public TypeError(JSObject args)
            : base(args.GetMember("0").ToString())
        {

        }

        public TypeError()
        {

        }

        public TypeError(string message)
            : base(message)
        {
        }
    }
}
