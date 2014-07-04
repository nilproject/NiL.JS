using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
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
            oValue = this;
        }

        [DoNotEnumerate]
        public Boolean(JSObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            valueType = JSObjectType.Bool;
            iValue = (bool)obj.GetMember("0") ? 1 : 0;
            oValue = this;
        }

        [DoNotEnumerate]
        public Boolean(bool value)
        {
            valueType = JSObjectType.Bool;
            iValue = value ? 1 : 0;
            oValue = this;
        }

        [DoNotEnumerate]
        public Boolean(double value)
        {
            valueType = JSObjectType.Bool;
            iValue = value != 0 && !double.IsNaN(value) ? 1 : 0;
            oValue = this;
        }

        [DoNotEnumerate]
        public Boolean(int value)
        {
            valueType = JSObjectType.Bool;
            iValue = value != 0 ? 1 : 0;
            oValue = this;
        }

        [DoNotEnumerate]
        public Boolean(string value)
        {
            valueType = JSObjectType.Bool;
            iValue = !string.IsNullOrEmpty(value) ? 1 : 0;
            oValue = this;
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

        [Hidden]
        internal protected override JSObject GetMember(JSObject name, bool create, bool own)
        {
            if (__proto__ == null)
                __proto__ = TypeProxy.GetPrototype(typeof(Boolean));
            return DefaultFieldGetter(name, create, own); // обращение идёт к Объекту Boolean, а не к значению boolean, поэтому члены создавать можно
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

        [DoNotEnumerate]
        [AllowUnsafeCall(typeof(JSObject))]
        public override JSObject toLocaleString()
        {
            var self = this.oValue as JSObject ?? this;
            return self.valueType == JSObjectType.Bool ? self.iValue != 0 ? "true" : "false" : ((bool)(this as JSObject) ? "true" : "false");
        }

        [DoNotEnumerate]
        [AllowUnsafeCall(typeof(JSObject))]
        public override JSObject valueOf()
        {
            var self = this.oValue as JSObject ?? this;
            return self;
        }

        [CLSCompliant(false)]
        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public new JSObject toString(JSObject args)
        {
            return valueType == JSObjectType.Bool ? iValue != 0 ? "true" : "false" : ((bool)(this as JSObject) ? "true" : "false");
        }
    }
}
