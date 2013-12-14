using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core
{
    public class JSException : Exception
    {
        public JSObject Avatar { get; private set; }

        public JSException(JSObject avatar)
        {
            Avatar = avatar;
        }
    }
}
