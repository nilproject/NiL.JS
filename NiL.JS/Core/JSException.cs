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
        public JSValue Error { get; }
        public CodeNode ExceptionMaker { get; }
        public string Code { get; internal set; }
        public CodeCoordinates CodeCoordinates { get; internal set; }

        public JSException(Error data)
        {
            Error = Context.CurrentGlobalContext.ProxyValue(data);
        }

        public JSException(Error data, CodeNode exceptionMaker, string code)
        {
            Error = Context.CurrentGlobalContext.ProxyValue(data);
            ExceptionMaker = exceptionMaker;
            Code = code;
            if (code != null) {
                CodeCoordinates = CodeCoordinates.FromTextPosition(code, exceptionMaker.Position, exceptionMaker.Length);
            }
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
            Error = Context.CurrentGlobalContext.ProxyValue(avatar);
        }

        public override string Message
        {
            get
            {
                var result = " at " + CodeCoordinates;
                if (Error._oValue is Error)
                {
                    var n = Error.GetProperty("name");
                    if (n._valueType == JSValueType.Property)
                        n = (n._oValue as PropertyPair).getter.Call(Error, null).ToString();

                    var m = Error.GetProperty("message");
                    if (m._valueType == JSValueType.Property)
                        result = n + ": " + (m._oValue as PropertyPair).getter.Call(Error, null) + result;
                    else
                        result = n + ": " + m + result;
                }
                else
                {
                    result = Error.ToString() + result;
                }

                return result;
            }
        }
    }
}
