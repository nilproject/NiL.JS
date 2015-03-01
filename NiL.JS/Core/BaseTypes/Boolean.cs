using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
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
            return value ? BaseTypes.Boolean.True : BaseTypes.Boolean.False;
            //var res = value ? BaseTypes.Boolean.True : BaseTypes.Boolean.False;
            //res.iValue = value ? 1 : 0;
            //return res;
        }

        [Hidden]
        public static implicit operator bool(Boolean value)
        {
            return value != null && value.iValue != 0;
        }

        [DoNotEnumerate]
        [AllowUnsafeCall(typeof(JSObject))]
        public override JSObject toLocaleString()
        {
            if (this.GetType() != typeof(Boolean) && valueType != JSObjectType.Bool)
                throw new JSException(new TypeError("Boolean.prototype.toLocaleString called for not boolean."));
            return iValue != 0 ? "true" : "false";
        }

        [DoNotEnumerate]
        [AllowUnsafeCall(typeof(JSObject))]
        public new JSObject valueOf()
        {
            if (this.GetType() == typeof(Boolean))
                return iValue != 0;
            if (!typeof(JSObject).IsAssignableFrom(this.GetType()) || valueType != JSObjectType.Bool)
                throw new JSException(new TypeError("Boolean.prototype.valueOf called for not boolean."));
            return this;
        }

        [CLSCompliant(false)]
        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public new JSObject toString(Arguments args)
        {
            if (this.GetType() != typeof(Boolean) && valueType != JSObjectType.Bool)
                throw new JSException(new TypeError("Boolean.prototype.toString called for not boolean."));
            return iValue != 0 ? "true" : "false";
        }
    }
}
