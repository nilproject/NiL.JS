using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core.BaseTypes
{
    internal class JSArray : JSObject
    {
        private readonly static string marker = new System.String(new char[] { 'A', 'r', 'r', 'a', 'y' });
        public static JSObject Prototype;

        public static void RegisterTo(Context context)
        {
            JSObject proto = null;
            var func = context.Assign("Array", new CallableField((_this, args) =>
            {
                JSObject res;
                if (_this.ValueType == ObjectValueType.Object && _this.prototype == Prototype)
                    res = _this;
                else
                    res = new JSObject();
                res.prototype = proto;
                res.ValueType = ObjectValueType.Object;
                res.oValue = marker;
                if (args != null && args.Length > 0)
                {
                    var f = args[0].Invoke();
                    if ((args.Length > 1) || (f.ValueType != ObjectValueType.Int && f.ValueType != ObjectValueType.Double))
                    {
                        res.GetField("0").Assign(f);
                        int i = 1;
                        for (; i < args.Length; i++)
                            res.GetField(i.ToString()).Assign(args[i].Invoke());
                        res.GetField("length").Assign(i);
                    }
                    else
                    {
                        if (f.ValueType == ObjectValueType.Double)
                        {
                            int l = (int)f.dValue;
                            if (f.dValue != l)
                                throw new ArgumentException("Invalid array length");
                            res.GetField("length").Assign(l);
                        }
                        else
                            res.GetField("length").Assign(f);
                    }
                }
                return res;
            }));
            proto = func.GetField("prototype");
            proto.Assign(null);
            Prototype = proto;
            proto.ValueType = ObjectValueType.Object;
            proto.oValue = marker;
            proto.GetField("length").Assign(0);
            proto.GetField("push").Assign(new CallableField(Push));
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
            var len = array.GetField("length", false);
            int index = 0;
            if (len.ValueType == ObjectValueType.Int)
                index = len.iValue;
            if (index > 0)
            {

                len.Assign(index);
            }
            return JSObject.undefined;
        }

        public JSArray()
        {
            ValueType = ObjectValueType.Object;
            oValue = marker;
            prototype = Prototype;
        }
    }
}
