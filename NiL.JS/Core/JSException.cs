using System;

namespace NiL.JS.Core
{
    public class JSException : Exception
    {
        public JSObject Avatar { get; private set; }

        public JSException(JSObject avatar)
        {
            Avatar = avatar;
        }
    }
}
