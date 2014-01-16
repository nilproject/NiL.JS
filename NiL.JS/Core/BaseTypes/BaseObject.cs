using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;

namespace NiL.JS.Core.BaseTypes
{
    internal sealed class BaseObject : JSObject
    {
        private static readonly JSObject tempResult = new JSObject() { attributes = ObjectAttributes.DontDelete };

        public static JSObject Prototype;

        public static void RegisterTo(Context context)
        {
            var func = context.Assign("Object", new CallableField((cont, args) =>
            {
                var _this = cont.thisBind ?? cont.GetField("this");
                JSObject res;
                if (_this.ValueType == ObjectValueType.Object && _this.GetField("__proto__", true) == Prototype)
                    res = _this;
                else
                    res = new JSObject();
                res.ValueType = ObjectValueType.Object;
                if (args != null && args.GetField("length").iValue > 0)
                    res.oValue = args.GetField("0", true);
                else
                    res.oValue = new object();
                res.GetField("__proto__").Assign(Prototype);
                return res;
            }));
            JSObject proto = null;
            proto = func.GetField("prototype");
            proto.Assign(null);
            Prototype = proto;
            proto.ValueType = ObjectValueType.Object;
            proto.oValue = "Object";
            var temp = proto.GetField("toString");
            temp.Assign(new CallableField((cont, args) =>
            {
                switch ((cont.thisBind ?? cont.GetField("this")).ValueType)
                {
                    case ObjectValueType.Int:
                    case ObjectValueType.Double:
                        {
                            return  "[object Number]";
                        }
                    case ObjectValueType.Undefined:
                        {
                            return "[object Undefined]";
                        }
                    case ObjectValueType.String:
                        {
                            return "[object String]";
                        }
                    case ObjectValueType.Bool:
                        {
                            return "[object Boolean]";
                        }
                    case ObjectValueType.Statement:
                        {
                            return "[object Function]";
                        }
                    case ObjectValueType.Date:
                    case ObjectValueType.Object:
                        {
                            return "[object Object]";
                        }
                    default: throw new NotImplementedException();
                }
            }));
            temp.attributes |= ObjectAttributes.DontEnum;
            proto.GetField("toLocaleString").Assign(temp);
            proto.GetField("toLocaleString").attributes |= ObjectAttributes.DontEnum;
            temp = proto.GetField("valueOf");
            temp.Assign(new CallableField((cont, args) =>
            {
                return cont.thisBind;
            }));
            temp.attributes |= ObjectAttributes.DontEnum;
            temp = proto.GetField("hasOwnProperty");
            temp.Assign(new CallableField(hasOwnProperty));
            temp.attributes |= ObjectAttributes.DontEnum;
            temp.attributes |= ObjectAttributes.DontEnum;
            temp = proto.GetField("isPrototypeOf");
            temp.Assign(new CallableField(isPrototypeOf));
            temp.attributes |= ObjectAttributes.DontEnum;
            temp = proto.GetField("propertyIsEnumerable");
            temp.Assign(new CallableField(propertyIsEnumerable));
            temp.attributes |= ObjectAttributes.DontEnum;
        }

        public static JSObject propertyIsEnumerable(Context cont, JSObject args)
        {
            if (cont.thisBind == null)
                throw new InvalidOperationException("Can't convert null to object");
            if (args.GetField("0", true).iValue == 0)
                return false;
            string n = "";
            switch (args.GetField("0", true).ValueType)
            {
                case ObjectValueType.Int:
                    {
                        n = args.GetField("0", true).iValue.ToString();
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        n = args.GetField("0", true).dValue.ToString();
                        break;
                    }
                case ObjectValueType.String:
                    {
                        n = args.GetField("0", true).oValue as string;
                        break;
                    }
                case ObjectValueType.Object:
                    {
                        args = args.GetField("0", true).ToPrimitiveValue_Value_String(new Context(Context.globalContext));
                        if (args.ValueType == ObjectValueType.String)
                            n = args.GetField("0", true).oValue as string;
                        if (args.ValueType == ObjectValueType.Int)
                            n = args.GetField("0", true).iValue.ToString();
                        if (args.ValueType == ObjectValueType.Double)
                            n = args.GetField("0", true).dValue.ToString();
                        break;
                    }
                case ObjectValueType.NotExist:
                    throw new InvalidOperationException("Varible not defined.");
                default:
                    throw new NotImplementedException("Object.hasOwnProperty. Invalid Value Type");
            }
            var res = cont.thisBind.GetField(n, true, false);
            res = (res.ValueType >= ObjectValueType.Undefined) && (res != JSObject.undefined) && ((res.attributes & ObjectAttributes.DontEnum) == 0);
            return res;
        }
        
        public static JSObject isPrototypeOf(Context cont, JSObject obj)
        {
            if (cont.thisBind == null)
                throw new InvalidOperationException("Can't convert null to object");
            if (obj.GetField("length", true).iValue == 0)
                return false;
            var a = obj.GetField("0", true);
            var c = cont.thisBind;
            JSObject o = tempResult;
            o.ValueType = ObjectValueType.Bool;
            o.iValue = 0;
            if (c.ValueType >= ObjectValueType.Object && c.oValue != null)
                while (a.ValueType >= ObjectValueType.Object && a.oValue != null)
                {
                    if (a.oValue == c.oValue || (c.oValue is Type && a.oValue.GetType() as object == c.oValue))
                    {
                        o.iValue = 1;
                        return o;
                    }
                    a = a.GetField("__proto__", true);
                }
            return o;
        }

        public static JSObject hasOwnProperty(Context cont, JSObject name)
        {
            if (cont.thisBind == null)
                throw new InvalidOperationException("Can't convert null to object");
            string n = "";
            switch (name.GetField("0", true).ValueType)
            {
                case ObjectValueType.Undefined:
                case ObjectValueType.NotExistInObject:
                    {
                        n = "undefined";
                        break;
                    }
                case ObjectValueType.Int:
                    {
                        n = name.GetField("0", true).iValue.ToString();
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        n = name.GetField("0", true).dValue.ToString();
                        break;
                    }
                case ObjectValueType.String:
                    {
                        n = name.GetField("0", true).oValue as string;
                        break;
                    }
                case ObjectValueType.Object:
                    {
                        name = name.GetField("0", true).ToPrimitiveValue_Value_String(new Context(Context.globalContext));
                        if (name.ValueType == ObjectValueType.String)
                            n = name.GetField("0", true).oValue as string;
                        if (name.ValueType == ObjectValueType.Int)
                            n = name.GetField("0", true).iValue.ToString();
                        if (name.ValueType == ObjectValueType.Double)
                            n = name.GetField("0", true).dValue.ToString();
                        break;
                    }
                case ObjectValueType.NotExist:
                    throw new InvalidOperationException("Varible not defined.");
                default:
                    throw new NotImplementedException("Object.hasOwnProperty. Invalid Value Type");
            }
            var res = cont.thisBind.GetField(n, true, true);
            res = (res.ValueType >= ObjectValueType.Undefined) && (res != JSObject.undefined);
            return res;
        }

        public BaseObject()
        {
            ValueType = ObjectValueType.Object;
            oValue = new object();
            prototype = Prototype;
        }
    }
}