using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core.BaseTypes
{
    internal sealed class BaseObject : JSObject
    {
        public BaseObject()
        {
        }

        private JSObject getField(string name)
        {
            JSObject r = DefaultFieldGetter(name, false);
            return r;
        }
    }
}
