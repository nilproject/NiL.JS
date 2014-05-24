using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    [Modules.Immutable]
    public class Boolean : EmbeddedType
    {
        [Modules.Protected]
        public static Boolean True = true;
        [Modules.Protected]
        public static Boolean False = false;
        [Modules.Hidden]
        [Modules.Protected]
        public static Boolean TrueEr = new Boolean(true) { assignCallback = JSObject.ErrorAssignCallback };
        [Modules.Hidden]
        [Modules.Protected]
        public static Boolean FalseEr = new Boolean(false) { assignCallback = JSObject.ErrorAssignCallback };

        public Boolean()
        {
            ValueType = JSObjectType.Bool;
            iValue = 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public Boolean(JSObject obj)
        {
            ValueType = JSObjectType.Bool;
            iValue = (bool)(obj.GetField("0", true, false)) ? 1 : 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public Boolean(bool value)
        {
            ValueType = JSObjectType.Bool;
            iValue = value ? 1 : 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public Boolean(double value)
        {
            ValueType = JSObjectType.Bool;
            iValue = value != 0 && !double.IsNaN(value) ? 1 : 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public Boolean(int value)
        {
            ValueType = JSObjectType.Bool;
            iValue = value != 0 ? 1 : 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public Boolean(string value)
        {
            ValueType = JSObjectType.Bool;
            iValue = !string.IsNullOrEmpty(value) ? 1 : 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public static implicit operator Boolean(bool value)
        {
            return new Boolean(value);
        }

        public static implicit operator bool(Boolean value)
        {
            return value != null && value.iValue != 0;
        }

        public override JSObject toLocaleString()
        {
            return toString(null);
        }

        public override JSObject toString(JSObject args)
        {
            return iValue != 0 ? "true" : "false";
        }
    }
}
