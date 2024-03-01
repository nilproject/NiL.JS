namespace NiL.JS.Core;

public sealed class StringSlice
{
    public StringSlice(string source, int start, int length)
    {
        Source = source;
        Start = start;
        Length = length;
    }

    public string Source { get; }
    public int Start { get; }
    public int Length { get; }

    public override string ToString()
    {
        return Source.Substring(Start, Length);
    }
}
