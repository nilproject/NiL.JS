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

        public JSException(JSObject avatar)
        {
            Avatar = avatar;
        }

        public JSException(JSObject avatar, Exception innerException)
            :base("", innerException)
        {
            Avatar = avatar;
        }

        public override string Message
        {
            get
            {
                var name = "Error";
                var n = Avatar.GetMember("name");
                if (n.valueType == JSObjectType.Property)
                    name = (n.oValue as BaseTypes.Function[])[1].Invoke(Avatar, null).ToString();
                else
                    name = n.ToString();

                var m = Avatar.GetMember("message");
                if (m.valueType == JSObjectType.Property)
                    return name + ": " + (m.oValue as BaseTypes.Function[])[1].Invoke(Avatar, null).ToString();
                else
                    return name + ": " + m.ToString();
            }
        }
    }
}
