#define CALLSTACKTOSTRING

using System;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public class Error
    {
        [DoNotEnumerate]
        public JSValue message
        {
            [Hidden]
            get;
            private set;
        }
        [DoNotEnumerate]
        public JSValue name
        {
            [Hidden]
            get;
            set;
        }
#if CALLSTACKTOSTRING
        public JSValue callstack
        {
            get;
            private set;
        }
#endif
        [DoNotEnumerate]
        public Error()
        {
            name = this.GetType().Name;
            message = "";
#if CALLSTACKTOSTRING
            makeCallStack();
#endif
        }

        [DoNotEnumerate]
        public Error(Arguments args)
        {
            name = this.GetType().Name;
            message = args[0].ToString();
#if CALLSTACKTOSTRING
            makeCallStack();
#endif
        }

        [DoNotEnumerate]
        public Error(string message)
        {
            name = this.GetType().Name;
            this.message = message;
#if CALLSTACKTOSTRING
            makeCallStack();
#endif
        }
#if CALLSTACKTOSTRING
        private void makeCallStack()
        {
            StringBuilder res = new StringBuilder();
            var currentContext = Context.CurrentContext;
            var contexts = Context.GetCurrectContextStack();
            res.Append("    at ").AppendLine(currentContext.CodeNode?.ToString() ?? string.Empty);
            for (int i = contexts.Count - 1; i > 0; i--)
            {
                var context = contexts[i];
                if (context != null)
                {
                    var line = FromTextPosition(context.RootContext.Code, context.CodeNode.Position);
                    res.Append("    at ").AppendLine($"{context._owner?.name ?? "<anonymous method>"}({context._module?.FilePath ?? "<anonymous>"}:{line})");
                }
            }
            callstack = Context.CurrentGlobalContext.ProxyValue(res.ToString());
        }

        public static int FromTextPosition(string text, int position)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            if (position < 0)
                throw new ArgumentOutOfRangeException("position");

            int line = 1;
            int column = 1;
            for (int i = 0; i < position; i++)
            {
                if (i >= text.Length)
                {
                    return -1;
                }
                if (text[i] == '\n')
                {
                    column = 0;
                    line++;
                    if (text.Length > i + 1 && text[i + 1] == '\r')
                        i++;
                }
                else if (text[i] == '\r')
                {
                    column = 0;
                    line++;
                    if (text.Length > i + 1 && text[i + 1] == '\n')
                        i++;
                }
                column++;
            }
            return line;
        }
#endif
        [Hidden]
        public override string ToString()
        {
            string mstring;
            string nstring;
            if (message == null
                || message._valueType <= JSValueType.Undefined
                || string.IsNullOrEmpty((mstring = message.ToString())))
                return name.ToString()
#if CALLSTACKTOSTRING
 + Environment.NewLine + callstack
#endif
;
            if (name == null
                || name._valueType <= JSValueType.Undefined
                || string.IsNullOrEmpty((nstring = name.ToString())))
                return mstring
#if CALLSTACKTOSTRING
 + Environment.NewLine + callstack
#endif
;
            return nstring + ": " + mstring
#if CALLSTACKTOSTRING
 + Environment.NewLine + callstack
#endif
;
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public JSValue toString()
        {
            return ToString();
        }
    }
}
