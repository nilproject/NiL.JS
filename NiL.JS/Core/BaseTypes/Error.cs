using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public class Error
    {
        [DoNotEnumerate]
        public virtual JSObject message
        {
            [Hidden]
            get;
            private set;
        }
        [DoNotEnumerate]
        public virtual JSObject name
        {
            [Hidden]
            get;
            set;
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

        [Hidden]
        public override string ToString()
        {
            string mstring;
            string nstring;
            if (message == null
                || message.valueType <= JSObjectType.Undefined
                || (mstring = message.ToString()) == "")
                return name.ToString();
            if (name == null
                || name.valueType <= JSObjectType.Undefined
                || (nstring = name.ToString()) == "")
                return mstring;
            return nstring + ": " + mstring;
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public JSObject toString()
        {
            return ToString();
        }
    }
}
