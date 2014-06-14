using NiL.JS.Core.BaseTypes;
using System;

namespace NiL.JS.Core
{
    /// <summary>
    /// Представляет ошибки, возникшие во время выполнения скрипта.
    /// </summary>
    [Serializable]
    public sealed class JSException : Exception
    {
        public JSObject Avatar { get; private set; }

        public JSException(Error avatar)
        {
            Avatar = TypeProxy.Proxy(avatar);
        }

        public JSException(JSObject avatar)
        {
            Avatar = avatar;
        }

        public JSException(JSObject avatar, Exception innerException)
            : base("", innerException)
        {
            Avatar = avatar;
        }

        public override string Message
        {
            get
            {
                if (Avatar.oValue is Error)
                {
                    var n = Avatar.GetMember("name");
                    if (n.valueType == JSObjectType.Property)
                        n = (n.oValue as BaseTypes.Function[])[1].Invoke(Avatar, null).ToString();

                    var m = Avatar.GetMember("message");
                    if (m.valueType == JSObjectType.Property)
                        return n + ": " + (m.oValue as BaseTypes.Function[])[1].Invoke(Avatar, null).ToString();
                    else
                        return n + ": " + m.ToString();
                }
                else return Avatar.ToString();
            }
        }
    }
}
