using NiL.JS.BaseLibrary;

namespace NiL.JS.Core
{
    public sealed class GsPropertyPair
    {
        internal Function get;
        internal Function set;

        public Function Getter { get { return get; } }
        public Function Setter { get { return set; } }

        internal GsPropertyPair() { }

        public GsPropertyPair(Function getter, Function setter)
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
