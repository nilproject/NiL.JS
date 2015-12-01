using System;

namespace NiL.JS.Core
{
    public delegate CodeNode ParseDelegate(ParseInfo state, ref int position);

    public delegate bool ValidateDelegate(string code, int position);

    public delegate JSValue ExternalFunctionDelegate(JSValue thisBind, Arguments args);

    public sealed class DebuggerCallbackEventArgs : EventArgs
    {
        public CodeNode Statement { get; internal set; }
    }

    public delegate void DebuggerCallback(Context sender, DebuggerCallbackEventArgs e);
}
