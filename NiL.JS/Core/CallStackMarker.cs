namespace NiL.JS.Core
{
    internal readonly struct CallStackMarker
    {
        public int Index { get; }

        public CallStackMarker(int index)
        {
            Index = index;
        }

        public override bool Equals(object obj)
        {
            return obj is CallStackMarker marker &&
                   Index == marker.Index;
        }

        public override int GetHashCode()
        {
            return -2134847229 + Index.GetHashCode();
        }
    }
}
