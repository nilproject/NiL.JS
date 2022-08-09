using System;
using System.Collections.Generic;
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
        private string _stackTraceOverride;

        public JSValue Error { get; }
        public CodeNode ExceptionMaker { get; }
        public string Code { get; internal set; }
        public CodeCoordinates CodeCoordinates { get; internal set; }

        public JSException(Error data)
        {
            Error = Context.CurrentGlobalContext.ProxyValue(data);
        }

        public JSException(Error data, CodeNode exceptionMaker, string code)
            : this(Context.CurrentGlobalContext.ProxyValue(data), exceptionMaker, code)
        {
        }

        public JSException(JSValue data, CodeNode exceptionMaker, string code)
        {
            Error = Context.CurrentGlobalContext.ProxyValue(data);
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
        }

        public JSException(Error avatar, Exception innerException)
            : base("", innerException)
        {
            Error = Context.CurrentGlobalContext.ProxyValue(avatar);
        }

        public override string StackTrace => (_stackTraceOverride ??= getStackTrace()) ?? base.StackTrace;

        public override string Message
        {
            get
            {
                var result = CodeCoordinates != null ? " at " + CodeCoordinates : "";
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

        private string getStackTrace()
        {
            var stackTrace = new List<string>();

            var innerEx = this as Exception;
            while (innerEx != null)
            {
                var isOurException = false;
                for (var i = innerEx.Data.Count; i-- > 0;)
                {
                    var item = innerEx.Data[new CallStackMarker(i)] as Tuple<Context, CodeCoordinates>;
                    if (item == null)
                        continue;

                    isOurException = true;
                    stackTrace.Add("   at " + (item.Item1?._owner?.name ?? "<unknown function>") + (item.Item2 != null ? ": line " + item.Item2.Line : string.Empty));
                }

                if (!isOurException)
                    stackTrace.Add(innerEx.StackTrace);

                innerEx = innerEx.InnerException;
            }

            if (stackTrace.Count == 0)
                return null;

            stackTrace.Reverse();
            return string.Join(Environment.NewLine, stackTrace);
        }
    }
}
