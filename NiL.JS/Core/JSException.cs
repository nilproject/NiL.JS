using System;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core
{
    /// <summary>
    /// Представляет ошибки, возникшие во время выполнения скрипта.
    /// </summary>
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class JSException : Exception
    {
        public JSValue Error { get; private set; }

        public JSException(Error data)
        {
            Error = Context.CurrentBaseContext.ProxyValue(data);
        }

        public JSException(JSValue data)
        {
            Error = data;
        }

        public JSException(JSValue data, Exception innerException)
            : base("External error", innerException)
        {
            Error = data;
        }

        public JSException(Error avatar, Exception innerException)
            : base("", innerException)
        {
            Error = Context.CurrentBaseContext.ProxyValue(avatar);
        }

        public override string Message
        {
            get
            {
                if (Error._oValue is Error)
                {
                    var n = Error.GetProperty("name");
                    if (n._valueType == JSValueType.Property)
                        n = (n._oValue as PropertyPair).getter.Call(Error, null).ToString();

                    var m = Error.GetProperty("message");
                    if (m._valueType == JSValueType.Property)
                        return n + ": " + (m._oValue as PropertyPair).getter.Call(Error, null);
                    else
                        return n + ": " + m;
                }
                else return Error.ToString();
            }
        }
    }
}
