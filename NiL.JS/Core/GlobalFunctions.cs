using System;
using System.Text;
using System.Threading;
using System.Web;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    internal static class GlobalFunctions
    {
        internal static JSObject dummy(JSObject thisBind, Arguments x)
        {
            throw new NotImplementedException();
        }

        internal static JSObject __swap(JSObject thisBind, Arguments args)
        {
            if ((args[0].attributes & (JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.SystemObject)) == 0
                && (args[1].attributes & (JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.SystemObject)) == 0)
            {
                var iv = args[0].iValue;
                var dv = args[0].dValue;
                var ov = args[0].oValue;
                var vt = args[0].valueType;
                args[0].iValue = args[1].iValue;
                args[0].dValue = args[1].dValue;
                args[0].oValue = args[1].oValue;
                args[0].valueType = args[1].valueType;
                args[1].iValue = iv;
                args[1].dValue = dv;
                args[1].oValue = ov;
                args[1].valueType = vt;
            }
            return null;
        }

        internal static JSObject isFinite(JSObject thisBind, Arguments x)
        {
            var d = Tools.JSObjectToDouble(x[0]);
            return !double.IsNaN(d) && !double.IsInfinity(d);
        }

        internal static JSObject isNaN(JSObject thisBind, Arguments x)
        {
            var r = x[0];
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

        internal static JSObject unescape(JSObject thisBind, Arguments x)
        {
            var res = HttpUtility.UrlDecode(x[0].ToString());
            return res;
        }

        [ParametersCount(2)]
        internal static JSObject parseInt(JSObject thisBind, Arguments args)
        {
            double result = double.NaN;
            var radixo = args[1];
            double dradix = radixo.isExist ? Tools.JSObjectToDouble(radixo) : 0;
            int radix;
            if (double.IsNaN(dradix) || double.IsInfinity(dradix))
                radix = 0;
            else
                radix = (int)((long)dradix & 0xFFFFFFFF);
            if (radix != 0 && (radix < 2 || radix > 36))
                return Number.NaN;
            var source = args[0];
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

        internal static JSObject parseFloat(JSObject thisBind, Arguments x)
        {
            double result = double.NaN;
            var source = x[0];
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

        internal static JSObject escape(JSObject thisBind, Arguments x)
        {
            return System.Web.HttpUtility.HtmlEncode(x[0].ToString());
        }

        internal static uint __pinvokeCalled;
        internal static JSObject __pinvoke(JSObject thisBind, Arguments args)
        {
            var argsCount = Tools.JSObjectToInt32(args.GetMember("length"));
            var threadsCount = 1;
            if (argsCount == 0)
                return null;
            if (argsCount > 1)
                threadsCount = Tools.JSObjectToInt32(args[1]);
            var function = args[0].oValue as Function;
            Thread[] threads = null;
            if (function != null && threadsCount > 0)
            {
                threads = new Thread[threadsCount];
                for (var i = 0; i < threadsCount; i++)
                {
                    (threads[i] = new Thread((o) =>
                    {
                        var targs = new Arguments();
                        targs.length = 1;
                        targs[0] = (int)o;
                        function.Invoke(null, targs);
                    }) { Name = "NiL.JS __pinvoke thread (" + __pinvokeCalled + ":" + i + ")" }).Start(i);
                }
                __pinvokeCalled++;
            }
            return TypeProxy.Proxy(new
            {
                isAlive = new Func<Arguments, bool>((arg) =>
                {
                    if (threads == null)
                        return false;
                    argsCount = Tools.JSObjectToInt32(arg.length);
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
                        var threadIndex = Tools.JSObjectToInt32(args[0]);
                        if (threadIndex < threads.Length && threadIndex >= 0)
                            return threads[threadIndex].IsAlive;
                    }
                    return false;
                }),
                wait = new Action(() =>
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

        internal static JSObject decodeURIComponent(JSObject thisBind, Arguments args)
        {
            var str = args[0].ToString();

            if (string.IsNullOrEmpty(str))
                return str;
            StringBuilder res = new StringBuilder(str.Length);
            for (var k = 0; k < str.Length; k++)
            {
                switch (str[k])
                {
                    case '%':
                        {
                            if (k + 2 >= str.Length)
                                throw new JSException(new URIError("Substring after \"%\" not represent valid code."));
                            if (!Tools.isHex(str[k + 1])
                                || !Tools.isHex(str[k + 2]))
                                throw new JSException(new URIError("Substring after \"%\" not represent valid code."));
                            var cc = Tools.anum(str[k + 1]) * 16 + Tools.anum(str[k + 2]);
                            k += 2;
                            if ((cc & 0x80) == 0)
                                res.Append((char)cc);
                            else
                            {
                                var start = k - 2;
                                var n = 1;
                                while (((cc << n) & 0x80) != 0)
                                    n++;
                                if (n == 1 || n > 4)
                                    throw new JSException(new URIError("URI malformed"));
                                if (k + (3 * (n - 1)) >= str.Length)
                                    throw new JSException(new URIError("URI malformed"));
                                var octet = (cc & (((1 << (n * 7 - 1)) - 1) >> (8 * (n - 1)))) << ((n - 1) * 6);
                                for (var j = 1; j < n; j++)
                                {
                                    k++;
                                    if (str[k] != '%')
                                        throw new JSException(new URIError(""));
                                    if (!Tools.isHex(str[k + 1])
                                        || !Tools.isHex(str[k + 2]))
                                        throw new JSException(new URIError("Substring after \"%\" not represent valid code."));
                                    cc = Tools.anum(str[k + 1]) * 16 + Tools.anum(str[k + 2]);
                                    if ((cc & 0xC0) != 0x80)
                                        throw new JSException(new URIError("URI malformed"));
                                    octet |= (cc & 63) << ((n - j - 1) * 6);
                                    k += 2;
                                }
                                if (octet < 0x10000)
                                {
                                    var c = (char)octet;
                                    res.Append(c);
                                }
                                else
                                {
                                    res.Append((char)(((octet - 0x10000) >> 10 & 0x3ff) + 0xd800));
                                    res.Append((char)(((octet - 0x10000) & 0x3ff) + 0xdc00));
                                }
                            }
                            break;
                        }
                    default:
                        {
                            res.Append(str[k]);
                            break;
                        }
                }
            }

            return res.ToString();
        }

        internal static JSObject decodeURI(JSObject thisBind, Arguments args)
        {
            var str = args[0].ToString();

            const string reserver = ";/?:@&=+$,#";

            if (string.IsNullOrEmpty(str))
                return str;
            StringBuilder res = new StringBuilder(str.Length);
            for (var k = 0; k < str.Length; k++)
            {
                switch (str[k])
                {
                    case '%':
                        {
                            if (k + 2 >= str.Length)
                                throw new JSException(new URIError("Substring after \"%\" not represent valid code."));
                            if (!Tools.isHex(str[k + 1])
                                || !Tools.isHex(str[k + 2]))
                                throw new JSException(new URIError("Substring after \"%\" not represent valid code."));
                            var cc = Tools.anum(str[k + 1]) * 16 + Tools.anum(str[k + 2]);
                            k += 2;
                            if ((cc & 0x80) == 0)
                            {
                                if (reserver.IndexOf((char)cc) == -1)
                                    res.Append((char)cc);
                                else
                                    res.Append('%').Append(str[k - 1]).Append(str[k]);
                            }
                            else
                            {
                                var start = k - 2;
                                var n = 1;
                                while (((cc << n) & 0x80) != 0)
                                    n++;
                                if (n == 1 || n > 4)
                                    throw new JSException(new URIError("URI malformed"));
                                if (k + (3 * (n - 1)) >= str.Length)
                                    throw new JSException(new URIError("URI malformed"));
                                var octet = (cc & (((1 << (n * 7 - 1)) - 1) >> (8 * (n - 1)))) << ((n - 1) * 6);
                                for (var j = 1; j < n; j++)
                                {
                                    k++;
                                    if (str[k] != '%')
                                        throw new JSException(new URIError(""));
                                    if (!Tools.isHex(str[k + 1])
                                        || !Tools.isHex(str[k + 2]))
                                        throw new JSException(new URIError("Substring after \"%\" not represent valid code."));
                                    cc = Tools.anum(str[k + 1]) * 16 + Tools.anum(str[k + 2]);
                                    if ((cc & 0xC0) != 0x80)
                                        throw new JSException(new URIError("URI malformed"));
                                    octet |= (cc & 63) << ((n - j - 1) * 6);
                                    k += 2;
                                }
                                if (octet < 0x10000)
                                {
                                    var c = (char)octet;
                                    if (reserver.IndexOf(c) != -1)
                                    {
                                        for (; start < k; start++)
                                            res.Append(str[start]);
                                    }
                                    else
                                    {
                                        res.Append(c);
                                    }
                                }
                                else
                                {
                                    res.Append((char)(((octet - 0x10000) >> 10 & 0x3ff) + 0xd800));
                                    res.Append((char)(((octet - 0x10000) & 0x3ff) + 0xdc00));
                                }
                            }
                            break;
                        }
                    default:
                        {
                            res.Append(str[k]);
                            break;
                        }
                }
            }

            return res.ToString();
        }

        private static bool doNotEscape(char c)
        {
            if ((c >= '0' && c <= '9')
                || (c >= 'A' && c <= 'Z')
                || (c >= 'a' && c <= 'z'))
                return true;
            switch (c)
            {
                case '-':
                case '_':
                case '.':
                case '!':
                case '~':
                case '*':
                case '\'':
                case '(':
                case ')':
                    return true;
            }
            return false;
        }

        internal static JSObject encodeURIComponent(JSObject thisBind, Arguments args)
        {
            var s = args[0].ToString();

            var res = new StringBuilder(s.Length);
            for (var i = 0; i < s.Length; i++)
            {
                if (doNotEscape(s[i]))
                    res.Append(s[i]);
                else
                {
                    int v = 0;
                    if (s[i] >= 0xdc00 && s[i] <= 0xdfff)
                        throw new JSException(new URIError(""));
                    if (s[i] < 0xd800 || s[i] > 0xdbff)
                        v = s[i];
                    else
                    {
                        i++;
                        if (i == s.Length)
                            throw new JSException(new URIError(""));
                        if (s[i] < 0xdc00 || s[i] > 0xdfff)
                            throw new JSException(new URIError(""));
                        v = (s[i - 1] - 0xd800) * 0x400 + (s[i] - 0xdc00) + 0x10000;
                    }
                    var n = 1;
                    if (v > 0x7f)
                    {
                        while ((v >> (n * 6 - (n - 1))) != 0)
                            n++;
                        if (n > 4)
                            throw new JSException(new URIError(""));
                        var b = (v >> ((n - 1) * 6)) | ~((1 << (8 - n)) - 1);
                        res.Append('%')
                            .Append(Tools.NumChars[(b >> 4) & 0xf])
                            .Append(Tools.NumChars[b & 0xf]);
                        while (--n > 0)
                        {
                            b = ((v >> ((n - 1) * 6)) & 0x3f) | ~((1 << 7) - 1);
                            res.Append('%')
                                .Append(Tools.NumChars[(b >> 4) & 0xf])
                                .Append(Tools.NumChars[b & 0xf]);
                        }
                    }
                    else
                        res.Append('%').Append(Tools.NumChars[(s[i] >> 4) & 0x0f]).Append(Tools.NumChars[s[i] & 0x0F]);
                }
            }
            return res.ToString();
        }

        internal static JSObject encodeURI(JSObject thisBind, Arguments args)
        {
            var s = args[0].ToString();

            const string unescaped = ";/?:@&=+$,#";

            var res = new StringBuilder(s.Length);
            for (var i = 0; i < s.Length; i++)
            {
                if (doNotEscape(s[i]) || unescaped.IndexOf(s[i]) != -1)
                    res.Append(s[i]);
                else
                {
                    int v = 0;
                    if (s[i] >= 0xdc00 && s[i] <= 0xdfff)
                        throw new JSException(new URIError(""));
                    if (s[i] < 0xd800 || s[i] > 0xdbff)
                        v = s[i];
                    else
                    {
                        i++;
                        if (i == s.Length)
                            throw new JSException(new URIError(""));
                        if (s[i] < 0xdc00 || s[i] > 0xdfff)
                            throw new JSException(new URIError(""));
                        v = (s[i - 1] - 0xd800) * 0x400 + (s[i] - 0xdc00) + 0x10000;
                    }
                    var n = 1;
                    if (v > 0x7f)
                    {
                        while ((v >> (n * 6 - (n - 1))) != 0)
                            n++;
                        if (n > 4)
                            throw new JSException(new URIError(""));
                        var b = (v >> ((n - 1) * 6)) | ~((1 << (8 - n)) - 1);
                        res.Append('%')
                            .Append(Tools.NumChars[(b >> 4) & 0xf])
                            .Append(Tools.NumChars[b & 0xf]);
                        while (--n > 0)
                        {
                            b = ((v >> ((n - 1) * 6)) & 0x3f) | ~((1 << 7) - 1);
                            res.Append('%')
                                .Append(Tools.NumChars[(b >> 4) & 0xf])
                                .Append(Tools.NumChars[b & 0xf]);
                        }
                    }
                    else
                        res.Append('%').Append(Tools.NumChars[(s[i] >> 4) & 0x0f]).Append(Tools.NumChars[s[i] & 0x0F]);
                }
            }
            return res.ToString();
        }
    }
}
