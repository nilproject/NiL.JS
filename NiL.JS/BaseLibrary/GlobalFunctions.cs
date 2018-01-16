using System;
using System.Text;
using System.Threading;
using NiL.JS.Core.Interop;
using NiL.JS.Core;

namespace NiL.JS.BaseLibrary
{
    public static class GlobalFunctions
    {
        internal static JSValue isFinite(JSValue thisBind, Arguments x)
        {
            var d = Tools.JSObjectToDouble(x[0]);
            return !double.IsNaN(d) && !double.IsInfinity(d);
        }

        internal static JSValue isNaN(JSValue thisBind, Arguments x)
        {
            var r = x[0];
            if (r._valueType >= JSValueType.Object)
                r = r.ToPrimitiveValue_Value_String();
            if (r._valueType == JSValueType.Double)
                return double.IsNaN(r._dValue);
            if (r._valueType == JSValueType.Boolean || r._valueType == JSValueType.Integer)
                return false;
            if (r._valueType == JSValueType.String)
            {
                double d = 0;
                var i = 0;
                if (Tools.ParseNumber(r._oValue.ToString(), i, out d, ParseNumberOptions.Default))
                    return double.IsNaN(d);
                return true;
            }
            return true;
        }

        internal static JSValue unescape(JSValue thisBind, Arguments x)
        {
            var res = Uri.UnescapeDataString(x[0].ToString());
            return res;
        }

        [ArgumentsCount(2)]
        internal static JSValue parseInt(JSValue thisBind, Arguments args)
        {
            var result = double.NaN;
            var radixo = args[1];
            var dradix = radixo.Exists ? Tools.JSObjectToDouble(radixo) : 0;
            int radix;
            if (double.IsNaN(dradix) || double.IsInfinity(dradix))
                radix = 0;
            else
                radix = (int)((long)dradix & 0xFFFFFFFF);
            if (radix != 0 && (radix < 2 || radix > 36))
                return Number.NaN;
            var source = args[0];
            if (source._valueType == JSValueType.Integer)
                return source;
            if (source._valueType == JSValueType.Double)
                return double.IsInfinity(source._dValue) || double.IsNaN(source._dValue) ?
                    Number.NaN : source._dValue == 0.0 ? (Number)0 : // +0 и -0 должны стать равными
                    (Number)System.Math.Truncate(source._dValue);
            var arg = source.ToString().Trim(Tools.TrimChars);
            if (!string.IsNullOrEmpty(arg))
                Tools.ParseNumber(arg, out result, radix, ParseNumberOptions.AllowAutoRadix);
            if (double.IsInfinity(result))
                return Number.NaN;
            return System.Math.Truncate(result);
        }

        internal static JSValue parseFloat(JSValue thisBind, Arguments x)
        {
            var result = double.NaN;
            var source = x[0];
            if (source._valueType == JSValueType.Integer)
                return source;
            if (source._valueType == JSValueType.Double)
                return source._dValue == 0.0 ? (Number)0 : // +0 и -0 должны стать равными
                    source;
            var arg = source.ToString().Trim(Tools.TrimChars);
            if (!string.IsNullOrEmpty(arg))
                Tools.ParseNumber(arg, out result, ParseNumberOptions.AllowFloat);
            return result;
        }

        internal static JSValue escape(JSValue thisBind, Arguments x)
        {
            return Uri.EscapeDataString(x[0].ToString());
        }
#if !(PORTABLE || NETCORE)
        internal static uint __pinvokeCallCount;
        internal static JSValue __pinvoke(JSValue thisBind, Arguments args)
        {
            if (args == null)
                return null;
            var argsCount = args.length;
            var threadsCount = 1;
            if (argsCount == 0)
                return null;
            if (argsCount > 1)
                threadsCount = Tools.JSObjectToInt32(args[1]);
            var function = args[0]._oValue as Function;
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
                        function.Call(null, targs);
                    }) { Name = "NiL.JS __pinvoke thread (" + __pinvokeCallCount + ":" + i + ")" }).Start(i);
                }
                __pinvokeCallCount++;
            }

            return Context.CurrentGlobalContext.ProxyValue(new
            {
                isAlive = new Func<Arguments, bool>((arg) =>
                {
                    if (threads == null)
                        return false;
                    argsCount = Tools.JSObjectToInt32(arg.length);
                    if (argsCount == 0)
                    {
                        for (var i = 0; i < threads.Length; i++)
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
                    for (var i = 0; i < threads.Length; i++)
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
#endif
        internal static JSValue decodeURIComponent(JSValue thisBind, Arguments args)
        {
            var str = args[0].ToString();

            if (string.IsNullOrEmpty(str))
                return str;
            var res = new StringBuilder(str.Length);
            for (var k = 0; k < str.Length; k++)
            {
                switch (str[k])
                {
                    case '%':
                        {
                            if (k + 2 >= str.Length)
                                ExceptionHelper.Throw(new URIError("Substring after \"%\" not represent valid code."));
                            if (!Tools.isHex(str[k + 1])
                                || !Tools.isHex(str[k + 2]))
                                ExceptionHelper.Throw(new URIError("Substring after \"%\" not represent valid code."));
                            var cc = Tools.hexCharToInt(str[k + 1]) * 16 + Tools.hexCharToInt(str[k + 2]);
                            k += 2;
                            if ((cc & 0x80) == 0)
                                res.Append((char)cc);
                            else
                            {
                                var n = 1;
                                while (((cc << n) & 0x80) != 0)
                                    n++;
                                if (n == 1 || n > 4)
                                    ExceptionHelper.Throw(new URIError("URI malformed"));
                                if (k + (3 * (n - 1)) >= str.Length)
                                    ExceptionHelper.Throw(new URIError("URI malformed"));
                                var octet = (cc & (((1 << (n * 7 - 1)) - 1) >> (8 * (n - 1)))) << ((n - 1) * 6);
                                for (var j = 1; j < n; j++)
                                {
                                    k++;
                                    if (str[k] != '%')
                                        ExceptionHelper.Throw(new URIError(""));
                                    if (!Tools.isHex(str[k + 1])
                                        || !Tools.isHex(str[k + 2]))
                                        ExceptionHelper.Throw(new URIError("Substring after \"%\" not represent valid code."));
                                    cc = Tools.hexCharToInt(str[k + 1]) * 16 + Tools.hexCharToInt(str[k + 2]);
                                    if ((cc & 0xC0) != 0x80)
                                        ExceptionHelper.Throw(new URIError("URI malformed"));
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

        internal static JSValue decodeURI(JSValue thisBind, Arguments args)
        {
            var str = args[0].ToString();

            const string reserver = ";/?:@&=+$,#";

            if (string.IsNullOrEmpty(str))
                return str;
            var res = new StringBuilder(str.Length);
            for (var k = 0; k < str.Length; k++)
            {
                switch (str[k])
                {
                    case '%':
                        {
                            if (k + 2 >= str.Length)
                                ExceptionHelper.Throw(new URIError("Substring after \"%\" not represent valid code."));
                            if (!Tools.isHex(str[k + 1])
                                || !Tools.isHex(str[k + 2]))
                                ExceptionHelper.Throw(new URIError("Substring after \"%\" not represent valid code."));
                            var cc = Tools.hexCharToInt(str[k + 1]) * 16 + Tools.hexCharToInt(str[k + 2]);
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
                                    ExceptionHelper.Throw(new URIError("URI malformed"));
                                if (k + (3 * (n - 1)) >= str.Length)
                                    ExceptionHelper.Throw(new URIError("URI malformed"));
                                var octet = (cc & (((1 << (n * 7 - 1)) - 1) >> (8 * (n - 1)))) << ((n - 1) * 6);
                                for (var j = 1; j < n; j++)
                                {
                                    k++;
                                    if (str[k] != '%')
                                        ExceptionHelper.Throw(new URIError(""));
                                    if (!Tools.isHex(str[k + 1])
                                        || !Tools.isHex(str[k + 2]))
                                        ExceptionHelper.Throw(new URIError("Substring after \"%\" not represent valid code."));
                                    cc = Tools.hexCharToInt(str[k + 1]) * 16 + Tools.hexCharToInt(str[k + 2]);
                                    if ((cc & 0xC0) != 0x80)
                                        ExceptionHelper.Throw(new URIError("URI malformed"));
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

        internal static JSValue encodeURIComponent(JSValue thisBind, Arguments args)
        {
            var s = args[0].ToString();

            var res = new StringBuilder(s.Length);
            for (var i = 0; i < s.Length; i++)
            {
                if (doNotEscape(s[i]))
                    res.Append(s[i]);
                else
                {
                    var v = 0;
                    if (s[i] >= 0xdc00 && s[i] <= 0xdfff)
                        ExceptionHelper.Throw(new URIError(""));
                    if (s[i] < 0xd800 || s[i] > 0xdbff)
                        v = s[i];
                    else
                    {
                        i++;
                        if (i == s.Length)
                            ExceptionHelper.Throw(new URIError(""));
                        if (s[i] < 0xdc00 || s[i] > 0xdfff)
                            ExceptionHelper.Throw(new URIError(""));
                        v = (s[i - 1] - 0xd800) * 0x400 + (s[i] - 0xdc00) + 0x10000;
                    }
                    var n = 1;
                    if (v > 0x7f)
                    {
                        while ((v >> (n * 6 - (n - 1))) != 0)
                            n++;
                        if (n > 4)
                            ExceptionHelper.Throw(new URIError(""));
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

        internal static JSValue encodeURI(JSValue thisBind, Arguments args)
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
                    var v = 0;
                    if (s[i] >= 0xdc00 && s[i] <= 0xdfff)
                        ExceptionHelper.Throw(new URIError(""));
                    if (s[i] < 0xd800 || s[i] > 0xdbff)
                        v = s[i];
                    else
                    {
                        i++;
                        if (i == s.Length)
                            ExceptionHelper.Throw(new URIError(""));
                        if (s[i] < 0xdc00 || s[i] > 0xdfff)
                            ExceptionHelper.Throw(new URIError(""));
                        v = (s[i - 1] - 0xd800) * 0x400 + (s[i] - 0xdc00) + 0x10000;
                    }
                    var n = 1;
                    if (v > 0x7f)
                    {
                        while ((v >> (n * 6 - (n - 1))) != 0)
                            n++;
                        if (n > 4)
                            ExceptionHelper.Throw(new URIError(""));
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
