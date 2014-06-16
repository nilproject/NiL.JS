using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    [Immutable]
    public class Boolean : EmbeddedType
    {
        [Hidden]
        internal static readonly Boolean True = new Boolean(true) { attributes = JSObjectAttributes.SystemConstant };
        [Hidden]
        internal static readonly Boolean False = new Boolean(false) { attributes = JSObjectAttributes.SystemConstant };

        [DoNotEnumerate]
        public Boolean()
        {
            valueType = JSObjectType.Bool;
            iValue = 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        [DoNotEnumerate]
        public Boolean(JSObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            valueType = JSObjectType.Bool;
            iValue = (bool)obj.GetMember("0") ? 1 : 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        [DoNotEnumerate]
        public Boolean(bool value)
        {
            valueType = JSObjectType.Bool;
            iValue = value ? 1 : 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        [DoNotEnumerate]
        public Boolean(double value)
        {
            valueType = JSObjectType.Bool;
            iValue = value != 0 && !double.IsNaN(value) ? 1 : 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        [DoNotEnumerate]
        public Boolean(int value)
        {
            valueType = JSObjectType.Bool;
            iValue = value != 0 ? 1 : 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        [DoNotEnumerate]
        public Boolean(string value)
        {
            valueType = JSObjectType.Bool;
            iValue = !string.IsNullOrEmpty(value) ? 1 : 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [Hidden]
        public static implicit operator Boolean(bool value)
        {
            var res = value ? BaseTypes.Boolean.True : BaseTypes.Boolean.False;
            res.iValue = value ? 1 : 0;
            return res;
        }

        [Hidden]
        public static implicit operator bool(Boolean value)
        {
            return value != null && value.iValue != 0;
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public override JSObject toLocaleString()
        {
            return toString(null);
        }

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public override JSObject toString(JSObject args)
        {
            return valueType == JSObjectType.Bool ? iValue != 0 ? "true" : "false" : ((bool)(this as JSObject) ? "true" : "false");
        }
    }
}
