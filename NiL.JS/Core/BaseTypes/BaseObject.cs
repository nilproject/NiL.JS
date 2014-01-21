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
                object oVal = null;
                if (args != null && args.GetField("length", true, false).iValue > 0)
                    oVal = args.GetField("0", true, false);
                JSObject res;
                if (_this.ValueType == JSObjectType.Object && _this.prototype != null && _this.prototype.oValue == Prototype.oValue)
                    res = _this;
                else
                {
                    if ((oVal is JSObject) && (oVal as JSObject).ValueType >= JSObjectType.Object)
                        return oVal as JSObject;
                    res = new JSObject();
                }
                res.oValue = oVal ?? new object();
                res.ValueType = JSObjectType.Object;
                if (args != null && args.GetField("length", true, false).iValue > 0)
                    res.oValue = args.GetField("0", true, false);
                else
                    res.oValue = new object();
                res.GetField("__proto__", false, true).Assign(Prototype);
                if (res.fields == null)
                    res.fields = new Dictionary<string, JSObject>();
                return res;
            }));
            JSObject proto = null;
            proto = func.GetField("prototype", false, false);
            proto.Assign(null);
            Prototype = proto;
            proto.ValueType = JSObjectType.Object;
            proto.oValue = "Object";
            var temp = proto.GetField("toString", false, false);
            temp.Assign(new CallableField((cont, args) =>
            {
                switch ((cont.thisBind ?? cont.GetField("this")).ValueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Double:
                        {
                            return  "[object Number]";
                        }
                    case JSObjectType.Undefined:
                        {
                            return "[object Undefined]";
                        }
                    case JSObjectType.String:
                        {
                            return "[object String]";
                        }
                    case JSObjectType.Bool:
                        {
                            return "[object Boolean]";
                        }
                    case JSObjectType.Function:
                        {
                            return "[object Function]";
                        }
                    case JSObjectType.Date:
                    case JSObjectType.Object:
                        {
                            return "[object Object]";
                        }
                    default: throw new NotImplementedException();
                }
            }));
            temp.attributes |= ObjectAttributes.DontEnum;
            proto.GetField("toLocaleString", false, false).Assign(temp);
            proto.GetField("toLocaleString", false, false).attributes |= ObjectAttributes.DontEnum;
            temp = proto.GetField("valueOf", false, true);
            temp.Assign(new CallableField((cont, args) =>
            {
                return cont.thisBind;
            }));
            temp.attributes |= ObjectAttributes.DontEnum;
            temp = proto.GetField("hasOwnProperty", false, true);
            temp.Assign(new CallableField(hasOwnProperty));
            temp.attributes |= ObjectAttributes.DontEnum;
            temp.attributes |= ObjectAttributes.DontEnum;
            temp = proto.GetField("isPrototypeOf", false, true);
            temp.Assign(new CallableField(isPrototypeOf));
            temp.attributes |= ObjectAttributes.DontEnum;
            temp = proto.GetField("propertyIsEnumerable", false, true);
            temp.Assign(new CallableField(propertyIsEnumerable));
            temp.attributes |= ObjectAttributes.DontEnum;
            temp = proto.GetField("constructor", false, true);
            temp.Assign(func);
            temp.attributes |= ObjectAttributes.DontEnum;
        }

        public static JSObject propertyIsEnumerable(Context cont, JSObject args)
        {
            if (cont.thisBind == null)
                throw new InvalidOperationException("Can't convert null to object");
            if (args.GetField("0", true, false).iValue == 0)
                return false;
            string n = "";
            switch (args.GetField("0", true, false).ValueType)
            {
                case JSObjectType.Int:
                    {
                        n = args.GetField("0", true, false).iValue.ToString();
                        break;
                    }
                case JSObjectType.Double:
                    {
                        n = args.GetField("0", true, false).dValue.ToString();
                        break;
                    }
                case JSObjectType.String:
                    {
                        n = args.GetField("0", true, false).oValue as string;
                        break;
                    }
                case JSObjectType.Object:
                    {
                        args = args.GetField("0", true, false).ToPrimitiveValue_Value_String(new Context(Context.globalContext));
                        if (args.ValueType == JSObjectType.String)
                            n = args.GetField("0", true, false).oValue as string;
                        if (args.ValueType == JSObjectType.Int)
                            n = args.GetField("0", true, false).iValue.ToString();
                        if (args.ValueType == JSObjectType.Double)
                            n = args.GetField("0", true, false).dValue.ToString();
                        break;
                    }
                case JSObjectType.NotExist:
                    throw new InvalidOperationException("Varible not defined.");
                default:
                    throw new NotImplementedException("Object.hasOwnProperty. Invalid Value Type");
            }
            var res = cont.thisBind.GetField(n, true, false);
            res = (res.ValueType >= JSObjectType.Undefined) && (res != JSObject.undefined) && ((res.attributes & ObjectAttributes.DontEnum) == 0);
            return res;
        }
        
        public static JSObject isPrototypeOf(Context cont, JSObject obj)
        {
            if (cont.thisBind == null)
                throw new InvalidOperationException("Can't convert null to object");
            if (obj.GetField("length", true, false).iValue == 0)
                return false;
            var a = obj.GetField("0", true, false);
            var c = cont.thisBind;
            JSObject o = tempResult;
            o.ValueType = JSObjectType.Bool;
            o.iValue = 0;
            if (c.ValueType >= JSObjectType.Object && c.oValue != null)
                while (a.ValueType >= JSObjectType.Object && a.oValue != null)
                {
                    if (a.oValue == c.oValue || (c.oValue is Type && a.oValue.GetType() as object == c.oValue))
                    {
                        o.iValue = 1;
                        return o;
                    }
                    a = a.GetField("__proto__", true, false);
                }
            return o;
        }

        public static JSObject hasOwnProperty(Context cont, JSObject name)
        {
            if (cont.thisBind == null)
                throw new InvalidOperationException("Can't convert null to object");
            string n = "";
            switch (name.GetField("0", true, false).ValueType)
            {
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    {
                        n = "undefined";
                        break;
                    }
                case JSObjectType.Int:
                    {
                        n = name.GetField("0", true, false).iValue.ToString();
                        break;
                    }
                case JSObjectType.Double:
                    {
                        n = name.GetField("0", true, false).dValue.ToString();
                        break;
                    }
                case JSObjectType.String:
                    {
                        n = name.GetField("0", true, false).oValue as string;
                        break;
                    }
                case JSObjectType.Object:
                    {
                        name = name.GetField("0", true, false).ToPrimitiveValue_Value_String(new Context(Context.globalContext));
                        if (name.ValueType == JSObjectType.String)
                            n = name.GetField("0", true, false).oValue as string;
                        if (name.ValueType == JSObjectType.Int)
                            n = name.GetField("0", true, false).iValue.ToString();
                        if (name.ValueType == JSObjectType.Double)
                            n = name.GetField("0", true, false).dValue.ToString();
                        break;
                    }
                case JSObjectType.NotExist:
                    throw new InvalidOperationException("Varible not defined.");
                default:
                    throw new NotImplementedException("Object.hasOwnProperty. Invalid Value Type");
            }
            var res = cont.thisBind.GetField(n, true, true);
            res = (res.ValueType >= JSObjectType.Undefined) && (res != JSObject.undefined);
            return res;
        }

        public BaseObject()
        {
            ValueType = JSObjectType.Object;
            oValue = new object();
            prototype = Prototype;
        }
    }
}