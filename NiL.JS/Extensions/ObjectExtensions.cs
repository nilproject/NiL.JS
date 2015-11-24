using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.Extensions
{
    public static class ObjectExtensions
    {
        public static JSValue AsJSValue(this object value)
        {
            return TypeProxy.Marshal(value);
        }

        public static JSValue WrapToJSValue(this object value)
        {
            if (value == null)
                return JSValue.Null;
            return new ObjectWrapper(value);
        }
    }
}
