using NiL.JS.BaseLibrary;

namespace NiL.JS.Core
{
    public sealed class PropertyPair
    {
        internal Function getter;
        internal Function setter;

        public Function Getter { get { return getter; } }
        public Function Setter { get { return setter; } }

        internal PropertyPair() { }

        public PropertyPair(Function getter, Function setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public override string ToString()
        {
            var tempStr = "[";
            if (getter != null)
                tempStr += "Getter";
            if (setter != null)
                tempStr += (tempStr.Length != 1 ? "/Setter" : "Setter");
            if (tempStr.Length == 1)
                return "[Invalid Property]";
            tempStr += "]";
            return tempStr;
        }
    }
}
