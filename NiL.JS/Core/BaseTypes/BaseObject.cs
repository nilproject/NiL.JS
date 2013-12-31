using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;

namespace NiL.JS.Core.BaseTypes
{
    internal sealed class BaseObject : JSObject
    {
        public static JSObject Prototype;

        public static void RegisterTo(Context context)
        {
            var func = context.Assign("Object", new CallableField((_this, args) =>
            {
                JSObject res;
                if (_this.ValueType == ObjectValueType.Object && _this.prototype == Prototype)
                    res = _this;
                else
                    res = new JSObject();
                res.prototype = Prototype;
                res.ValueType = ObjectValueType.Object;
                res.oValue = new object();
                if (args != null && args.Length > 0)
                    res.oValue = args[0];
                return res;
            }));
            JSObject proto = null;
            proto = func.GetField("prototype");
            proto.Assign(null);
            Prototype = proto;
            proto.ValueType = ObjectValueType.Object;
            proto.oValue = "Object";
            var tos = proto.GetField("toString");
            tos.Assign(new CallableField((_this, args) =>
            {
                switch (_this.ValueType)
                {
                    case ObjectValueType.Int:
                    case ObjectValueType.Double:
                        {
                            return "[object Number]";
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
            tos.attributes |= ObjectAttributes.DontEnum;
        }

        public BaseObject()
        {
            ValueType = ObjectValueType.Object;
            oValue = new object();
            prototype = Prototype;
        }
    }
}