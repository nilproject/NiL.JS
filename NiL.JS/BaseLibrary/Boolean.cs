using System;
using NiL.JS.Core;
using NiL.JS.Core.Modules;
using NiL.JS.Core.TypeProxing;

namespace NiL.JS.BaseLibrary
{
#if !PORTABLE
    [Serializable]
#endif
    public class Boolean : JSObject
    {
        [Hidden]
        internal static readonly Boolean True = new Boolean(true) { attributes = JSObjectAttributesInternal.SystemObject };
        [Hidden]
        internal static readonly Boolean False = new Boolean(false) { attributes = JSObjectAttributesInternal.SystemObject };

        [DoNotEnumerate]
        public Boolean()
        {
            valueType = JSObjectType.Bool;
            iValue = 0;
            attributes |= JSObjectAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Boolean(Arguments obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            valueType = JSObjectType.Bool;
            iValue = (bool)obj[0] ? 1 : 0;
            attributes |= JSObjectAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Boolean(bool value)
        {
            valueType = JSObjectType.Bool;
            iValue = value ? 1 : 0;
            attributes |= JSObjectAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Boolean(double value)
        {
            valueType = JSObjectType.Bool;
            iValue = value != 0 && !double.IsNaN(value) ? 1 : 0;
            attributes |= JSObjectAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Boolean(int value)
        {
            valueType = JSObjectType.Bool;
            iValue = value != 0 ? 1 : 0;
            attributes |= JSObjectAttributesInternal.SystemObject;
        }

        [DoNotEnumerate]
        public Boolean(string value)
        {
            valueType = JSObjectType.Bool;
            iValue = !string.IsNullOrEmpty(value) ? 1 : 0;
            attributes |= JSObjectAttributesInternal.SystemObject;
        }

        [Hidden]
        internal protected override JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
            return DefaultFieldGetter(name, forWrite, own); // обращение идёт к Объекту Number, а не к значению number, поэтому члены создавать можно
        }

        [Hidden]
        public override void Assign(JSObject value)
        {
            if ((attributes & JSObjectAttributesInternal.ReadOnly) == 0)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                throw new InvalidOperationException("Try to assign to Boolean");
            }
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
        public static JSObject toLocaleString(JSObject self)
        {
            if (self.GetType() != typeof(Boolean) && self.valueType != JSObjectType.Bool)
                throw new JSException(new TypeError("Boolean.prototype.toLocaleString called for not boolean."));
            return self.iValue != 0 ? "true" : "false";
        }

        [DoNotEnumerate]
        [InstanceMember]
        [ArgumentsLength(0)]
        public static JSObject valueOf(JSObject self)
        {
            if (self.GetType() == typeof(Boolean))
                return self.iValue != 0;
            if (!typeof(JSObject).IsAssignableFrom(self.GetType()) || self.valueType != JSObjectType.Bool)
                throw new JSException(new TypeError("Boolean.prototype.valueOf called for not boolean."));
            return self;
        }

        [CLSCompliant(false)]
        [InstanceMember]
        [ArgumentsLength(0)]
        [DoNotEnumerate]
        public static JSObject toString(JSObject self, Arguments args)
        {
            if (self.GetType() != typeof(Boolean) && self.valueType != JSObjectType.Bool)
                throw new JSException(new TypeError("Boolean.prototype.toString called for not boolean."));
            return self.iValue != 0 ? "true" : "false";
        }
    }
}
