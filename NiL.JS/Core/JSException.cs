using System;

namespace NiL.JS.Core
{
    /// <summary>
    /// Представляет ошибки, возникшие во время выполнения скрипта.
    /// </summary>
    [Serializable]
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
                var name = "Error";
                var n = Avatar.GetField("name", true, false);
                if (n.ValueType == JSObjectType.Property)
                    name = (n.oValue as BaseTypes.Function[])[1].Invoke(Avatar, null).ToString();
                else
                    name = n.ToString();

                var m = Avatar.GetField("message", true, false);
                if (m.ValueType == JSObjectType.Property)
                    return name + ": " + (m.oValue as BaseTypes.Function[])[1].Invoke(Avatar, null).ToString();
                else
                    return name + ": " + m.ToString();
            }
        }
    }
}
