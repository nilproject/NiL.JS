using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Core
{
    public sealed class PropertyPair
    {
        internal Function get;
        internal Function set;

        public Function Getter { get { return get; } }
        public Function Setter { get { return set; } }

        internal PropertyPair() { }

        public PropertyPair(Function getter, Function setter)
        {
            get = getter;
            set = setter;
        }

        public override string ToString()
        {
            var tempStr = "[";
            if (get != null)
                tempStr += "Getter";
            if (set != null)
                tempStr += (tempStr.Length != 1 ? "/Setter" : "Setter");
            if (tempStr.Length == 1)
                return "[Invalid Property]";
            tempStr += "]";
            return tempStr;
        }
    }
}
