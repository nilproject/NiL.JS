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
                    res = new JSArray();
                res.prototype = Prototype;
                res.ValueType = ObjectValueType.Object;
                res.oValue = marker;
                if (args != null && args.Length > 0)
                {
                    var arg = args[0].Invoke();
                    if ((args.Length > 1) || (arg.ValueType != ObjectValueType.Int && arg.ValueType != ObjectValueType.Double))
                    {
                        res.GetField("0").Assign(arg);
                        int i = 1;
                        for (; i < args.Length; i++)
                            res.GetField(i.ToString()).Assign(args[i].Invoke());
                    }
                    else
                    {
                        if (arg.ValueType == ObjectValueType.Double)
                        {
                            int l = (int)arg.dValue;
                            if (arg.dValue != l)
                                throw new ArgumentException("Invalid array length");
                        }
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
            var field = proto.GetField("push");
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
                array.GetField(index.ToString()).Assign(args[i] != null ? args[i].Invoke() : JSObject.undefined);
            len.Assign(index);
            return len;
        }

        public static JSObject Pop(JSObject array, IContextStatement[] args)
        {
            return JSObject.undefined;
        }

        public JSArray()
        {
            ValueType = ObjectValueType.Object;
            oValue = marker;
            prototype = Prototype;
            var length = GetField("length");
            length.Assign(0);
            length.attributes |= ObjectAttributes.DontEnum;
            length.assignCallback = null;
            fieldGetter = (name, fast) =>
            {
                if (name == "length")
                    return length;
                var res = DefaultFieldGetter(name, fast);
                var oac = res.assignCallback;
                res.assignCallback = () =>
                {
                    if (oac != null)
                        oac();
                    int n = 0;
                    int i = 0;
                    if (Parser.ParseNumber(name, ref i, true, out n) && (i + 1 == name.Length))
                        length.iValue = Math.Max(n + 1, length.iValue);
                    return true;
                };
                return res;
            };
        }
    }
}