using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
    [Prototype(typeof(Error))]
    public class TypeError : Error
    {
        public override JSObject name
        {
            get
            {
                return "TypeError";
            }
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
