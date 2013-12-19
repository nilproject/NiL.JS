using System;

namespace NiL.JS.Core.BaseTypes
{
    internal class JSArray : JSObject
    {
        private const string marker = "Array";
        public static JSObject Prototype;

        public static void RegisterTo(Context context)
        {
            var func = context.Assign("Array", new CallableField((_this, args) =>
            {
                JSObject res;
                if (_this.ValueType == ObjectValueType.Object && _this.prototype == Prototype)
                    res = _this;
                else
                    res = new JSObject();
                res.prototype = Prototype;
                res.ValueType = ObjectValueType.Object;
                res.oValue = marker;
                var f = res.GetField("length");
                f.Assign(0);
                f.attributes |= ObjectAttributes.DontEnum;
                if (args != null && args.Length > 0)
                {
                    var arg = args[0].Invoke();
                    if ((args.Length > 1) || (arg.ValueType != ObjectValueType.Int && arg.ValueType != ObjectValueType.Double))
                    {
                        res.GetField("0").Assign(arg);
                        int i = 1;
                        for (; i < args.Length; i++)
                            res.GetField(i.ToString()).Assign(args[i].Invoke());
                        res.GetField("length").Assign(i);
                    }
                    else
                    {
                        if (arg.ValueType == ObjectValueType.Double)
                        {
                            int l = (int)arg.dValue;
                            if (arg.dValue != l)
                                throw new ArgumentException("Invalid array length");
                            res.GetField("length").Assign(l);
                        }
                        else
                            res.GetField("length").Assign(arg);
                    }
                }
                return res;
            }));
            JSObject proto = null;
            proto = func.GetField("prototype");
            proto.Assign(null);
            Prototype = proto;
            proto.ValueType = ObjectValueType.Object;
            proto.oValue = marker;
            var field = proto.GetField("length");
            field.Assign(0);
            field.attributes |= ObjectAttributes.DontEnum;
            field = proto.GetField("push");
            field.Assign(new CallableField(Push));
            field.attributes |= ObjectAttributes.DontEnum;
            proto.prototype = BaseObject.Prototype;
        }

        public static JSObject Push(JSObject array, IContextStatement[] args)
        {
            var len = array.GetField("length");
            int index = 0;
            if (len.ValueType == ObjectValueType.Int)
                index = len.iValue;
            for (int i = 0; i < args.Length; i++, index++)
                array.GetField(index.ToString()).Assign(args[i].Invoke());
            len.Assign(index);
            return len;
        }

        public static JSObject Pop(JSObject array, IContextStatement[] args)
        {
            /*var len = array.GetField("length", false);
            int index = 0;
            if (len.ValueType == ObjectValueType.Int)
                index = len.iValue;
            if (index > 0)
            {
                len.Assign(index);
            }*/
            return JSObject.undefined;
        }

        public JSArray()
        {
            ValueType = ObjectValueType.Object;
            oValue = marker;
            prototype = Prototype;
            var f = GetField("length");
            f.Assign(0);
            f.attributes |= ObjectAttributes.DontEnum;
        }
    }
}