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

        public override string Message
        {
            get
            {
                return Avatar.GetField("message", true, false).ToString();
            }
        }
    }
}
