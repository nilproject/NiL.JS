using System;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class EvalError : Error
    {
        [DoNotEnumerate]
        public EvalError()
        {

        }

        [DoNotEnumerate]
        public EvalError(Arguments args)
            : base(args[0].ToString())
        {

        }

        [DoNotEnumerate]
        public EvalError(string message)
            : base(message)
        {

        }
    }
}
