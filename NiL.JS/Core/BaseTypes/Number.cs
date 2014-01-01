using System;
using NiL.JS.Modules;

namespace NiL.JS.Core.BaseTypes
{
    internal class Number : JSObject
    {
        private ClassProxy proxy;
        
        public Number()
        {
            ValueType = ObjectValueType.Int;
            iValue = 0;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public Number(int value)
        {
            ValueType = ObjectValueType.Int;
            iValue = value;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public Number(double value)
        {
            ValueType = ObjectValueType.Double;
            dValue = value;
            assignCallback = JSObject.ErrorAssignCallback;
        }

        public Number(string value)
        {
            ValueType = ObjectValueType.Int;
            assignCallback = JSObject.ErrorAssignCallback;
            double d = 0;
            int i = 0;
            if (Parser.ParseNumber(value, ref i, false, out d))
            {
                dValue = d;
                ValueType = ObjectValueType.Double;
            }
        }

        public Number(JSObject obj)
        {
            ValueType = ObjectValueType.Int;
            switch(obj.ValueType)
            {
                case ObjectValueType.Int:
                    {
                        iValue = obj.iValue;
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        dValue = obj.dValue;
                        ValueType = ObjectValueType.Double;
                        break;
                    }
                case ObjectValueType.String:
                    {
                        double d = 0;
                        int i = 0;
                        if (Parser.ParseNumber(obj.oValue.ToString(), ref i, false, out d))
                        {
                            dValue = d;
                            ValueType = ObjectValueType.Double;
                        }
                        break;
                    }
                case ObjectValueType.Object:
                    {
                        obj = obj.ToPrimitiveValue_Value_String(new Context(Context.globalContext));
                        if (obj.ValueType == ObjectValueType.String)
                            goto case ObjectValueType.String;
                        if (obj.ValueType == ObjectValueType.Int)
                            goto case ObjectValueType.Int;
                        if (obj.ValueType == ObjectValueType.Double)
                            goto case ObjectValueType.Double;
                        break;
                    }
                case ObjectValueType.NotExist:
                    throw new InvalidOperationException("Varible not defined.");
            }
            assignCallback = JSObject.ErrorAssignCallback;
        }



        public override JSObject GetField(string name, bool fast)
        {
            return (proxy ?? (proxy = new ClassProxy(this))).GetField(name, fast);
        }
    }
}