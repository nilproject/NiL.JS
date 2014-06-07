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
        [Modules.Hidden]
        internal static readonly Boolean True = new Boolean(true);
        [Modules.Hidden]
        internal static readonly Boolean False = new Boolean(false);

        public Boolean()
        {
            ValueType = JSObjectType.Bool;
            iValue = 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public Boolean(JSObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            ValueType = JSObjectType.Bool;
            iValue = (bool)obj.GetField("0", true, false) ? 1 : 0;
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

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Boolean(bool value)
        {
            var res = value ? BaseTypes.Boolean.True : BaseTypes.Boolean.False;
            res.iValue = value ? 1 : 0;
            return res;
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
