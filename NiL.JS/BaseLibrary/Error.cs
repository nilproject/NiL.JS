
using System;
using System.Collections.Generic;
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
        public JSValue callstack
        {
            get;
            private set;
        }

        [DoNotEnumerate]
        public Error()
        {
            name = this.GetType().Name;
            message = "";
        }

        [DoNotEnumerate]
        public Error(Arguments args)
        {
            name = this.GetType().Name;
            message = args[0].ToString();
        }

        [DoNotEnumerate]
        public Error(string message)
        {
            name = this.GetType().Name;
            this.message = message;
        }
        internal void MakeCallStack(CodeNode exceptionMaker)
        {
            StringBuilder res = new StringBuilder();

            Context currentContext = Context.CurrentContext;
            CodeCoordinates currentCodeCoordinates = CodeCoordinates.FromTextPosition(currentContext.RootContext._code, exceptionMaker.Position);
            res.Append("    at ").AppendLine($"{currentCodeCoordinates.Code ?? string.Empty}");
            res.Append("    at ").AppendLine($"{currentContext._owner?.name ?? "<anonymous method>"}({currentContext._module?.FilePath ?? "<anonymous>"}:{currentCodeCoordinates.Line}:{currentCodeCoordinates.Column})");


            List<Context> contexts = Context.GetCurrentContextStack();

            for (int i = contexts.Count - 2; i > 0; i--)
            {
                Context context = contexts[i];
                if (context != null)
                {
                    CodeCoordinates codeCoordinates = CodeCoordinates.FromTextPosition(context.RootContext._code, context._currentPosition);
                    res.Append("    at ").AppendLine($"{context._owner?.name ?? "<anonymous method>"}({context._module?.FilePath ?? "<anonymous>"}:{codeCoordinates.Line}:{codeCoordinates.Column})");
                }
            }
            callstack = Context.CurrentGlobalContext.ProxyValue(res.ToString());
        }

        internal void MakeCallStack()
        {
            StringBuilder res = new StringBuilder();

            List<Context> contexts = Context.GetCurrentContextStack();
            if (contexts.Count > 0)
            {
                Context currentContext = contexts[contexts.Count - 1];
                CodeCoordinates currentCodeCoordinates = CodeCoordinates.FromTextPosition(currentContext.RootContext._code, currentContext._currentPosition);
                res.Append("    at ").AppendLine($"{currentCodeCoordinates.Code ?? string.Empty}");
                res.Append("    at ").AppendLine($"{currentContext._owner?.name ?? "<anonymous method>"}({currentContext._module?.FilePath ?? "<anonymous>"}:{currentCodeCoordinates.Line}:{currentCodeCoordinates.Column})");
            }

            for (int i = contexts.Count - 2; i > 0; i--)
            {
                Context context = contexts[i];
                if (context != null)
                {
                    CodeCoordinates codeCoordinates = CodeCoordinates.FromTextPosition(context.RootContext._code, context._currentPosition);
                    res.Append("    at ").AppendLine($"{context._owner?.name ?? "<anonymous method>"}({context._module?.FilePath ?? "<anonymous>"}:{codeCoordinates.Line}:{codeCoordinates.Column})");
                }
            }
            callstack = Context.CurrentGlobalContext.ProxyValue(res.ToString());
        }

        [Hidden]
        public override string ToString()
        {
            string mstring;
            string nstring;
            if (message == null
                || message._valueType <= JSValueType.Undefined
                || string.IsNullOrEmpty((mstring = message.ToString())))
                return name.ToString()

 + Environment.NewLine + callstack
;
            if (name == null
                || name._valueType <= JSValueType.Undefined
                || string.IsNullOrEmpty((nstring = name.ToString())))
                return mstring
 + Environment.NewLine + callstack
;
            return nstring + ": " + mstring
 + Environment.NewLine + callstack
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
