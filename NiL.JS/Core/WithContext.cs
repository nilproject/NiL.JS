using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core
{
    internal class WithContext : Context
    {
        private JSObject obj;

        public WithContext(JSObject obj, Context prototype)
            : base(prototype)
        {
            this.obj = obj;
        }

        public override JSObject GetField(string name)
        {
            var t = obj.GetField(name, true, false);
            if (t == JSObject.undefined || t.ValueType < JSObjectType.Undefined)
                return base.GetField(name);
            return t;
        }
    }
}
