using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Core
{
    internal delegate ParseResult ParseDelegate(ParsingState state, ref int position);
    internal delegate bool ValidateDelegate(string code, ref int position);
}
