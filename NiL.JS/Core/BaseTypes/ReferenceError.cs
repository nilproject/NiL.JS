using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
    [Prototype(typeof(Error))]
    public class ReferenceError : Error
    {
        public override JSObject name
        {
            get
            {
                return "TypeError";
            }
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
