using System;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core
{
    /// <summary>
    /// Представляет ошибки, возникшие во время выполнения скрипта.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    public sealed class JSException : Exception
    {
        public JSValue Error { get; private set; }

        public JSException(Error avatar)
        {
            Error = TypeProxy.Proxy(avatar);
        }

        public JSException(JSValue avatar)
        {
            Error = avatar;
        }

        public JSException(JSValue avatar, Exception innerException)
            : base("", innerException)
        {
            Error = avatar;
        }

        public JSException(Error avatar, Exception innerException)
            : base("", innerException)
        {
            Error = TypeProxy.Proxy(avatar);
        }

        public override string Message
        {
            get
            {
                if (Error.oValue is Error)
                {
                    var n = Error.GetProperty("name");
                    if (n.valueType == JSValueType.Property)
                        n = (n.oValue as GsPropertyPair).get.Call(Error, null).ToString();

                    var m = Error.GetProperty("message");
                    if (m.valueType == JSValueType.Property)
                        return n + ": " + (m.oValue as GsPropertyPair).get.Call(Error, null);
                    else
                        return n + ": " + m;
                }
                else return Error.ToString();
            }
        }
    }
}
