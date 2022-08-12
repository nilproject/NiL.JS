using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
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
        private ExceptionHelper.StackTraceState _stackTraceData;

        public JSValue Error { get; }
        public CodeNode ExceptionMaker { get; }
        public string Code { get; internal set; }
        public CodeCoordinates CodeCoordinates { get; internal set; }

        private JSException()
        {
            _stackTraceData = ExceptionHelper.GetStackTrace(1);
        }

        public JSException(Error data)
            : this()
        {
            Error = Context.CurrentGlobalContext.ProxyValue(data);
        }

        public JSException(Error data, CodeNode exceptionMaker, string code)
            : this(Context.CurrentGlobalContext.ProxyValue(data), exceptionMaker, code)
        {
        }

        public JSException(JSValue data, CodeNode exceptionMaker, string code)
            : this()
        {
            Error = data;
            ExceptionMaker = exceptionMaker;
            Code = code;
            if (code != null)
            {
                CodeCoordinates = CodeCoordinates.FromTextPosition(code, exceptionMaker.Position, exceptionMaker.Length);
            }
        }

        public JSException(JSValue data, Exception innerException)
            : base("External error", innerException)
        {
            Error = data;

            _stackTraceData = ExceptionHelper.GetStackTrace(1);
        }

        public JSException(Error avatar, Exception innerException)
            : this(Context.CurrentGlobalContext.ProxyValue(avatar), innerException)
        {
        }

        public override string StackTrace => _stackTraceData?.ToString(this) ?? base.StackTrace;

        public override string Message
        {
            get
            {
                var result = CodeCoordinates != null ? " at " + CodeCoordinates : null as string;
                if (Error?._oValue is Error)
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
                    result = Error?.ToString() + result;
                }

                return result ?? "JavaScript Error";
            }
        }
    }
}
