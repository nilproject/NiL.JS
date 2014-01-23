using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
    public class RangeError : Error
    {
        public override JSObject name { get { return "RangeError"; } }

        public RangeError()
        {

        }

        public RangeError(string message)
            : base(message)
        {

        }
    }
}
