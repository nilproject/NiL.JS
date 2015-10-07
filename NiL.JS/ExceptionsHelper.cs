using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.BaseLibrary;

namespace NiL.JS
{
    internal static class ExceptionsHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Throw(BaseLibrary.Error error)
        {
            throw error.Wrap();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNullReference(string message)
        {
            throw new NullReferenceException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowVariableNotDefined(string variableName)
        {
            Throw(new ReferenceError(string.Format(Strings.VariableNotDefined, variableName)));
        }
    }
}
