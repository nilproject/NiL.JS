using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
}
