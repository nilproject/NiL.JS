using System;
using NiL.JS.Core;

namespace NiL.JS.Extensions
{
    public static class JSValueExtensions
    {
        public static bool Is(this JSValue self, JSValueType type)
        {
            return self != null && self.valueType == type;
        }

        public static bool Is<T>(this JSValue self)
        {
            if (self == null)
                return false;
#if PORTABLE
            switch (typeof(T).GetTypeCode())
#else
            switch (Type.GetTypeCode(typeof(T)))
#endif
            {
                case TypeCode.Boolean:
                    {
                        return self.Is(JSValueType.Bool);
                    }
                case TypeCode.Byte:
                    {
                        return self.Is(JSValueType.Int) && (self.iValue & ~byte.MaxValue) == 0;
                    }
                case TypeCode.Char:
                    {
                        return (self != null
                            && self.valueType == JSValueType.Object
                            && self.oValue is char);
                    }
                case TypeCode.Decimal:
                    {
                        return false;
                    }
                case TypeCode.Double:
                    {
                        return self.Is(JSValueType.Double);
                    }
                case TypeCode.Int16:
                    {
                        return self.Is(JSValueType.Int) && (self.iValue & ~ushort.MaxValue) == 0;
                    }
                case TypeCode.Int32:
                    {
                        return self.Is(JSValueType.Int);
                    }
                case TypeCode.Int64:
                    {
                        return false;
                    }
                case TypeCode.Object:
                    {
                        return self.Value is T;
                    }
                case TypeCode.SByte:
                    {
                        return self.Is(JSValueType.Int) && (self.iValue & ~byte.MaxValue) == 0;
                    }
                case TypeCode.Single:
                    {
                        return false;
                    }
                case TypeCode.String:
                    {
                        return self.Is(JSValueType.String);
                    }
                case TypeCode.UInt16:
                    {
                        return self.Is(JSValueType.Int) && (self.iValue & ~ushort.MaxValue) == 0;
                    }
                case TypeCode.UInt32:
                    {
                        return self.Is(JSValueType.Int);
                    }
                case TypeCode.UInt64:
                    {
                        return false;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        public static T As<T>(this JSValue self)
        {
#if PORTABLE
            switch (typeof(T).GetTypeCode())
#else
            switch (Type.GetTypeCode(typeof(T)))
#endif
            {
                case TypeCode.Boolean:
                    return (T)(object)(bool)self; // оптимизатор разруливает такой каскад преобразований
                case TypeCode.Byte:
                    {
                        return (T)(object)(byte)Tools.JSObjectToInt32(self);
                    }
                case TypeCode.Char:
                    {
                        if (self.valueType == JSValueType.Object
                            && self.oValue is char)
                            return (T)self.oValue;
                        break;
                    }
                case TypeCode.Decimal:
                    {
                        return (T)(object)(decimal)Tools.JSObjectToDouble(self);
                    }
                case TypeCode.Double:
                    {
                        return (T)(object)Tools.JSObjectToDouble(self);
                    }
                case TypeCode.Int16:
                    {
                        return (T)(object)(Int16)Tools.JSObjectToInt32(self);
                    }
                case TypeCode.Int32:
                    {
                        return (T)(object)Tools.JSObjectToInt32(self);
                    }
                case TypeCode.Int64:
                    {
                        return (T)(object)Tools.JSObjectToInt64(self);
                    }
                case TypeCode.Object:
                    {
                        return (T)self.Value;
                    }
                case TypeCode.SByte:
                    {
                        return (T)(object)(sbyte)Tools.JSObjectToInt32(self);
                    }
                case TypeCode.Single:
                    {
                        return (T)(object)(float)Tools.JSObjectToDouble(self);
                    }
                case TypeCode.String:
                    {
                        return (T)(object)self.ToString();
                    }
                case TypeCode.UInt16:
                    {
                        return (T)(object)(ushort)Tools.JSObjectToInt32(self);
                    }
                case TypeCode.UInt32:
                    {
                        return (T)(object)(uint)Tools.JSObjectToInt32(self);
                    }
                case TypeCode.UInt64:
                    {
                        return (T)(object)(ulong)Tools.JSObjectToInt64(self);
                    }
            }
            throw new InvalidCastException();
        }
    }
}
