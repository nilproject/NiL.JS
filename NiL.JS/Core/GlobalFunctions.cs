using System;
using System.Threading;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    internal static class GlobalFunctions
    {
        public static JSObject dummy(JSObject thisBind, JSObject x)
        {
            throw new NotImplementedException();
        }

        public static JSObject isFinite(JSObject thisBind, JSObject x)
        {
            var d = Tools.JSObjectToDouble(x.GetMember("0"));
            return !double.IsNaN(d) && !double.IsInfinity(d);
        }

        public static JSObject isNaN(JSObject thisBind, JSObject x)
        {
            var r = x.GetMember("0");
            if (r.valueType == JSObjectType.Object && r.oValue is JSObject)
                r = r.oValue as JSObject;
            if (r.valueType == JSObjectType.Double)
                return double.IsNaN(r.dValue);
            if (r.valueType == JSObjectType.Bool || r.valueType == JSObjectType.Int || r.valueType == JSObjectType.Date)
                return false;
            if (r.valueType == JSObjectType.String)
            {
                double d = 0;
                int i = 0;
                if (Tools.ParseNumber(r.oValue as string, i, out d, Tools.ParseNumberOptions.Default))
                    return double.IsNaN(d);
                return true;
            }
            return true;
        }

        public static JSObject unescape(JSObject thisBind, JSObject x)
        {
            return System.Web.HttpUtility.HtmlDecode(x.GetMember("0").ToString());
        }

        [ParametersCount(2)]
        public static JSObject parseInt(JSObject thisBind, JSObject x)
        {
            double result = double.NaN;
            var radixo = x["1"];
            double dradix = radixo != JSObject.notExist ? Tools.JSObjectToDouble(radixo) : 0;
            int radix;
            if (double.IsNaN(dradix) || double.IsInfinity(dradix))
                radix = 0;
            else
                radix = (int)((long)dradix & 0xFFFFFFFF);
            if (radix != 0 && (radix < 2 || radix > 36))
                return Number.NaN;
            var source = x.GetMember("0");
            if (source.valueType == JSObjectType.Int)
                return source;
            if (source.valueType == JSObjectType.Double)
                return double.IsInfinity(source.dValue) || double.IsNaN(source.dValue) ? 
                    Number.NaN : source.dValue == 0.0 ? (Number)0 : // +0 и -0 должны стать равными
                    (Number)System.Math.Truncate(source.dValue);
            var arg = source.ToString().Trim();
            if (!string.IsNullOrEmpty(arg))
                Tools.ParseNumber(arg, out result, radix, Tools.ParseNumberOptions.AllowAutoRadix);
            if (double.IsInfinity(result))
                return Number.NaN;
            return System.Math.Truncate(result);
        }

        public static JSObject parseFloat(JSObject thisBind, JSObject x)
        {
            double result = double.NaN;
            var source = x.GetMember("0");
            if (source.valueType == JSObjectType.Int)
                return source;
            if (source.valueType == JSObjectType.Double)
                return source.dValue == 0.0 ? (Number)0 : // +0 и -0 должны стать равными
                    (Number)source.dValue;
            var arg = source.ToString().Trim();
            if (!string.IsNullOrEmpty(arg))
                Tools.ParseNumber(arg, out result, Tools.ParseNumberOptions.AllowFloat);
            return result;
        }

        public static JSObject escape(JSObject thisBind, JSObject x)
        {
            return System.Web.HttpUtility.HtmlEncode(x.GetMember("0").ToString());
        }

        private static uint __pinvokeCalled;
        public static JSObject __pinvoke(JSObject thisBind, JSObject args)
        {
            var argsCount = Tools.JSObjectToInt(args.GetMember("length"));
            var threadsCount = 1;
            if (argsCount == 0)
                return null;
            if (argsCount > 1)
                threadsCount = Tools.JSObjectToInt(args.GetMember("1"));
            var function = args.GetMember("0").oValue as Function;
            Thread[] threads = null;
            if (function != null && threadsCount > 0)
            {
                threads = new Thread[threadsCount];
                for (var i = 0; i < threadsCount; i++)
                {
                    (threads[i] = new Thread((o) =>
                    {
                        var targs = new JSObject(true)
                        {
                            oValue = Arguments.Instance,
                            valueType = JSObjectType.Object,
                            attributes = JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum
                        };
                        targs.fields["length"] = new Number(1) { assignCallback = null, attributes = JSObjectAttributes.DoNotEnum };
                        targs.fields["0"] = new Number((int)o) { assignCallback = null, attributes = JSObjectAttributes.Argument };
                        (targs.fields["callee"] = new JSObject()
                        {
                            valueType = JSObjectType.Function,
                            oValue = function,
                            attributes = JSObjectAttributes.DoNotEnum
                        }).attributes |= JSObjectAttributes.DoNotDelete | JSObjectAttributes.ReadOnly;
                        function.Invoke(null, targs);
                    }) { Name = "NiL.JS __pinvoke thread (" + __pinvokeCalled + ":" + i + ")" }).Start(i);
                }
                __pinvokeCalled++;
            }
            return TypeProxy.Proxy(new
            {
                isAlive = new Func<JSObject, bool>((arg) =>
                {
                    if (threads == null)
                        return false;
                    argsCount = Tools.JSObjectToInt(arg.GetMember("length"));
                    if (argsCount == 0)
                    {
                        for (int i = 0; i < threads.Length; i++)
                        {
                            if (threads[i].IsAlive)
                                return true;
                        }
                    }
                    else
                    {
                        var threadIndex = Tools.JSObjectToInt(args.GetMember("0"));
                        if (threadIndex < threads.Length && threadIndex >= 0)
                            return threads[threadIndex].IsAlive;
                    }
                    return false;
                }),
                waitEnd = new Action(() =>
                {
                    if (threads == null)
                        return;
                    for (int i = 0; i < threads.Length; i++)
                    {
                        if (threads[i].IsAlive)
                        {
                            Thread.Sleep(1);
                            i = -1;
                        }
                    }
                })
            });
        }
    }
}
