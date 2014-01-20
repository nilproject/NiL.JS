using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core.BaseTypes
{
    [Modules.Immutable]
    internal class Boolean : EmbeddedType
    {
        public Boolean()
        {
            ValueType = JSObjectType.Bool;
            iValue = 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public Boolean(int value)
        {
            ValueType = JSObjectType.Bool;
            iValue = value != 0 ? 1 : 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public Boolean(double value)
        {
            ValueType = JSObjectType.Double;
            iValue = value != 0 && !double.IsNaN(value) ? 1 : 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public Boolean(string value)
        {
            ValueType = JSObjectType.Bool;
            iValue = !string.IsNullOrEmpty(value) ? 1 : 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public Boolean(JSObject obj)
        {
            ValueType = JSObjectType.Bool;
            iValue = (bool)(obj.GetField("0", true, false)) ? 1 : 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }
    }
}
