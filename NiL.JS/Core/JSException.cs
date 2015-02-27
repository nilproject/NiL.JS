using System;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.TypeProxing;

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

        public JSException(Error avatar, Exception innerException)
            : base("", innerException)
        {
            Avatar = TypeProxy.Proxy(avatar);
        }

        public override string Message
        {
            get
            {
                if (Avatar.oValue is Error)
                {
                    var n = Avatar.GetMember("name");
                    if (n.valueType == JSObjectType.Property)
                        n = (n.oValue as PropertyPair).get.Invoke(Avatar, null).ToString();

                    var m = Avatar.GetMember("message");
                    if (m.valueType == JSObjectType.Property)
                        return n + ": " + (m.oValue as PropertyPair).get.Invoke(Avatar, null).ToString();
                    else
                        return n + ": " + m.ToString();
                }
                else return Avatar.ToString();
            }
        }
    }
}
