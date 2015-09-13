using System;
using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
{
#if !PORTABLE
    [Serializable]
#endif
    public class Boolean : JSObject
    {
        [Hidden]
        internal static readonly Boolean True = new Boolean(true) { attributes = JSValueAttributesInternal.SystemObject };
        [Hidden]
        internal static readonly Boolean False = new Boolean(false) { attributes = JSValueAttributesInternal.SystemObject };

        [DoNotEnumerate]
        public Boolean()
        {
            valueType = JSValueType.Bool;
            iValue = 0;
            attributes |= JSValueAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Boolean(Arguments obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            valueType = JSValueType.Bool;
            iValue = (bool)obj[0] ? 1 : 0;
            attributes |= JSValueAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Boolean(bool value)
        {
            valueType = JSValueType.Bool;
            iValue = value ? 1 : 0;
            attributes |= JSValueAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Boolean(double value)
        {
            valueType = JSValueType.Bool;
            iValue = value != 0 && !double.IsNaN(value) ? 1 : 0;
            attributes |= JSValueAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Boolean(int value)
        {
            valueType = JSValueType.Bool;
            iValue = value != 0 ? 1 : 0;
            attributes |= JSValueAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Boolean(string value)
        {
            valueType = JSValueType.Bool;
            iValue = !string.IsNullOrEmpty(value) ? 1 : 0;
            attributes |= JSValueAttributesInternal.SystemObject;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [Hidden]
        public static implicit operator Boolean(bool value)
        {
#if DEBUG
            if (Boolean.True.iValue != 1)
                System.Diagnostics.Debugger.Break();
            if (Boolean.False.iValue != 0)
                System.Diagnostics.Debugger.Break();
#endif
            return value ? Boolean.True : Boolean.False;
            //var res = value ? Boolean.True : Boolean.False;
            //res.iValue = value ? 1 : 0;
            //return res;
        }

        [Hidden]
        public static implicit operator bool(Boolean value)
        {
            return value != null && value.iValue != 0;
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        public static JSValue toLocaleString(JSValue self)
        {
            if (self.GetType() != typeof(Boolean) && self.valueType != JSValueType.Bool)
                throw new JSException(new TypeError("Boolean.prototype.toLocaleString called for not boolean."));
            return self.iValue != 0 ? "true" : "false";
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        public static JSValue valueOf(JSValue self)
        {
            if (self.GetType() == typeof(Boolean))
                return self.iValue != 0;
            if (!typeof(JSValue).IsAssignableFrom(self.GetType()) || self.valueType != JSValueType.Bool)
                throw new JSException(new TypeError("Boolean.prototype.valueOf called for not boolean."));
            return self;
        }

        [CLSCompliant(false)]
        [InstanceMember]
        [ArgumentsLength(0)]
        [DoNotEnumerate]
        public static JSValue toString(JSValue self, Arguments args)
        {
            if (self.GetType() != typeof(Boolean) && self.valueType != JSValueType.Bool)
                throw new JSException(new TypeError("Boolean.prototype.toString called for not boolean."));
            return self.iValue != 0 ? "true" : "false";
        }
    }
}
