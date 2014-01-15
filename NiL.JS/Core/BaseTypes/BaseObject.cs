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
                if (args != null && args.Length > 0)
                    res.oValue = args[0];
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

        public static JSObject propertyIsEnumerable(Context cont, JSObject[] args)
        {
            if (cont.thisBind == null)
                throw new InvalidOperationException("Can't convert null to object");
            if (args.Length == 0)
                return false;
            string n = "";
            switch ((args[0] ?? JSObject.undefined).ValueType)
            {
                case ObjectValueType.Int:
                    {
                        n = args[0].iValue.ToString();
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        n = args[0].dValue.ToString();
                        break;
                    }
                case ObjectValueType.String:
                    {
                        n = args[0].oValue as string;
                        break;
                    }
                case ObjectValueType.Object:
                    {
                        args[0] = args[0].ToPrimitiveValue_Value_String(new Context(Context.globalContext));
                        if (args[0].ValueType == ObjectValueType.String)
                            goto case ObjectValueType.String;
                        if (args[0].ValueType == ObjectValueType.Int)
                            goto case ObjectValueType.Int;
                        if (args[0].ValueType == ObjectValueType.Double)
                            goto case ObjectValueType.Double;
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
        
        public static JSObject isPrototypeOf(Context cont, JSObject[] obj)
        {
            if (cont.thisBind == null)
                throw new InvalidOperationException("Can't convert null to object");
            if (obj.Length == 0)
                return false;
            var a = obj[0];
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

        public static JSObject hasOwnProperty(Context cont, JSObject[] name)
        {
            if (cont.thisBind == null)
                throw new InvalidOperationException("Can't convert null to object");
            if (name.Length == 0)
                return false;
            string n = "";
            switch ((name[0] ?? JSObject.undefined).ValueType)
            {
                case ObjectValueType.Int:
                    {
                        n = name[0].iValue.ToString();
                        break;
                    }
                case ObjectValueType.Double:
                    {
                        n = name[0].dValue.ToString();
                        break;
                    }
                case ObjectValueType.String:
                    {
                        n = name[0].oValue as string;
                        break;
                    }
                case ObjectValueType.Object:
                    {
                        name[0] = name[0].ToPrimitiveValue_Value_String(new Context(Context.globalContext));
                        if (name[0].ValueType == ObjectValueType.String)
                            goto case ObjectValueType.String;
                        if (name[0].ValueType == ObjectValueType.Int)
                            goto case ObjectValueType.Int;
                        if (name[0].ValueType == ObjectValueType.Double)
                            goto case ObjectValueType.Double;
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