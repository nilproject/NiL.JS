using NiL.JS.Core.BaseTypes;
using System;
using System.Threading;

namespace NiL.JS.Core
{
    internal static class GlobalFunctions
    {
        public static JSObject dummy(JSObject thisBind, JSObject x)
        {
            throw new NotImplementedException();
        }

        public static JSObject isNaN(JSObject thisBind, JSObject x)
        {
            var r = x.GetMember("0");
            if (r.valueType == JSObjectType.Double)
                return double.IsNaN(r.dValue);
            if (r.valueType == JSObjectType.Bool || r.valueType == JSObjectType.Int || r.valueType == JSObjectType.Date)
                return false;
            if (r.valueType == JSObjectType.String)
            {
                double d = 0;
                int i = 0;
                if (Tools.ParseNumber(r.oValue as string, i, out d))
                    return double.IsNaN(d);
                return true;
            }
            return true;
        }

        public static JSObject unescape(JSObject thisBind, JSObject x)
        {
            return System.Web.HttpUtility.HtmlDecode(x.GetMember("0").ToString());
        }

        public static JSObject parseInt(JSObject thisBind, JSObject x)
        {
            var r = x.GetMember("0");
            for (; ; )
                switch (r.valueType)
                {
                    case JSObjectType.Bool:
                        {
                            return double.NaN;
                        }
                    case JSObjectType.Int:
                        {
                            return r.iValue;
                        }
                    case JSObjectType.Double:
                        {
                            if (double.IsNaN(r.dValue) || double.IsInfinity(r.dValue))
                                return 0;
                            return (int)((long)r.dValue & 0xFFFFFFFF);
                        }
                    case JSObjectType.String:
                        {
                            double dres = 0;
                            int ix = 0;
                            string s = (r.oValue as string).Trim();
                            if (s == "")
                                return double.NaN;
                            if (!Tools.ParseNumber(s, ref ix, out dres, Tools.JSObjectToInt(x.GetMember("1")), true))
                                return 0;
                            if (double.IsNaN(dres) || double.IsInfinity(dres))
                                return double.NaN;
                            return (int)dres;
                        }
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.Object:
                        {
                            if (r.oValue == null)
                                return 0;
                            r = r.ToPrimitiveValue_Value_String();
                            break;
                        }
                    case JSObjectType.Undefined:
                    case JSObjectType.NotExistInObject:
                        return 0;
                    default:
                        throw new NotImplementedException();
                }
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
