using NiL.JS.Core;

namespace NiL.JS
{
    public enum MessageLevel
    {
        Regular = 0,
        Recomendation,
        Warning,
        CriticalWarning,
        Error
    }

    public delegate void CompilerMessageCallback(MessageLevel level, CodeCoordinates coords, string message);
    public delegate void InternalCompilerMessageCallback(MessageLevel level, int position, int length, string message);
}
