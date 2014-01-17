using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
    internal class Number : EmbeddedType
    {
        [Modules.Protected]
        public static JSObject NaN = double.NaN;
        [Modules.Protected]
        public static JSObject POSITIVE_INFINITY = double.PositiveInfinity;
        [Modules.Protected]
        public static JSObject NEGATIVE_INFINITY = double.NegativeInfinity;

        static Number()
        {
            POSITIVE_INFINITY.assignCallback = null;
            POSITIVE_INFINITY.Protect();
            NEGATIVE_INFINITY.assignCallback = null;
            NEGATIVE_INFINITY.Protect();
            NaN.assignCallback = null;
            NaN.Protect();
        }
        
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
                case ObjectValueType.Bool:
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

        public JSObject toPrecision(JSObject digits)
        {
            double res = 0;
            switch (ValueType)
            {
                case ObjectValueType.Int:
                    {
                        res = iValue;
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        res = dValue;
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }
            int dgts = 0;
            switch ((digits ?? JSObject.undefined).ValueType)
            {
                case ObjectValueType.Int:
                    {
                        dgts = digits.iValue;
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        dgts = (int)digits.dValue;
                        break;
                    }
                case ObjectValueType.String:
                    {
                        double d = 0;
                        int i = 0;
                        if (Parser.ParseNumber(digits.oValue.ToString(), ref i, false, out d))
                        {
                            dgts = (int)d;
                        }
                        break;
                    }
                case ObjectValueType.Object:
                    {
                        digits = digits.GetField("0", true).ToPrimitiveValue_Value_String(new Context(Context.globalContext));
                        if (digits.ValueType == ObjectValueType.String)
                            goto case ObjectValueType.String;
                        if (digits.ValueType == ObjectValueType.Int)
                            goto case ObjectValueType.Int;
                        if (digits.ValueType == ObjectValueType.Double)
                            goto case ObjectValueType.Double;
                        break;
                    }
                case ObjectValueType.NotExist:
                    throw new InvalidOperationException("Varible not defined.");
                default:
                    return res.ToString();
            }
            string integerPart = ((int)res).ToString();
            if (integerPart.Length <= dgts)
                return System.Math.Round(res, dgts - integerPart.Length).ToString();
            var sres = ((int)res).ToString("e" + (dgts - 1));
            return sres;
        }

        public JSObject toExponential(JSObject digits)
        {
            double res = 0;
            switch (ValueType)
            {
                case ObjectValueType.Int:
                    {
                        res = iValue;
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        res = dValue;
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }
            int dgts = 0;
            switch ((digits ?? JSObject.undefined).ValueType)
            {
                case ObjectValueType.Int:
                    {
                        dgts = digits.iValue;
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        dgts = (int)digits.dValue;
                        break;
                    }
                case ObjectValueType.String:
                    {
                        double d = 0;
                        int i = 0;
                        if (Parser.ParseNumber(digits.oValue.ToString(), ref i, false, out d))
                        {
                            dgts = (int)d;
                        }
                        break;
                    }
                case ObjectValueType.Object:
                    {
                        digits = digits.GetField("0", true).ToPrimitiveValue_Value_String(new Context(Context.globalContext));
                        if (digits.ValueType == ObjectValueType.String)
                            goto case ObjectValueType.String;
                        if (digits.ValueType == ObjectValueType.Int)
                            goto case ObjectValueType.Int;
                        if (digits.ValueType == ObjectValueType.Double)
                            goto case ObjectValueType.Double;
                        break;
                    }
                case ObjectValueType.NotExist:
                    throw new InvalidOperationException("Varible not defined.");
                default:
                    return res.ToString("e");
            }
            return res.ToString("e" + dgts);
        }

        public JSObject toFixed(JSObject digits)
        {
            double res = 0;
            switch (ValueType)
            {
                case ObjectValueType.Int:
                    {
                        res = iValue;
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        res = dValue;
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }
            int dgts = 0;
            switch ((digits ?? JSObject.undefined).ValueType)
            {
                case ObjectValueType.Int:
                    {
                        dgts = digits.iValue;
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        dgts = (int)digits.dValue;
                        break;
                    }
                case ObjectValueType.String:
                    {
                        double d = 0;
                        int i = 0;
                        if (Parser.ParseNumber(digits.oValue.ToString(), ref i, false, out d))
                        {
                            dgts = (int)d;
                        }
                        break;
                    }
                case ObjectValueType.Object:
                    {
                        digits = digits.GetField("0", true).ToPrimitiveValue_Value_String(new Context(Context.globalContext));
                        if (digits.ValueType == ObjectValueType.String)
                            goto case ObjectValueType.String;
                        if (digits.ValueType == ObjectValueType.Int)
                            goto case ObjectValueType.Int;
                        if (digits.ValueType == ObjectValueType.Double)
                            goto case ObjectValueType.Double;
                        break;
                    }
                case ObjectValueType.NotExist:
                    throw new InvalidOperationException("Varible not defined.");
                default:
                    return ((int)res).ToString();
            }
            if (dgts < 0 || dgts > 20)
                throw new ArgumentException("toFixed() digits argument must be between 0 and 20");
            return System.Math.Round(res, dgts).ToString();
        }

        public JSObject toLocaleString()
        {
            return ValueType == ObjectValueType.Int ? iValue.ToString(System.Threading.Thread.CurrentThread.CurrentUICulture) : dValue.ToString(System.Threading.Thread.CurrentThread.CurrentUICulture);
        }

        public override string ToString()
        {
            return ValueType == ObjectValueType.Int ? iValue.ToString() : dValue.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is Number)
            {
                var n = obj as Number;
                switch(ValueType)
                {
                    case ObjectValueType.Int:
                        {
                            switch (n.ValueType)
                            {
                                case ObjectValueType.Int:
                                    return iValue == n.iValue;
                                case ObjectValueType.Double:
                                    return iValue == n.dValue;
                            }
                            break;
                        }
                    case ObjectValueType.Double:
                        {
                            switch (n.ValueType)
                            {
                                case ObjectValueType.Int:
                                    return dValue == n.iValue;
                                case ObjectValueType.Double:
                                    return dValue == n.dValue;
                            }
                            break;
                        }
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ValueType == ObjectValueType.Int ? iValue.GetHashCode() : dValue.GetHashCode();
        }
    }
}