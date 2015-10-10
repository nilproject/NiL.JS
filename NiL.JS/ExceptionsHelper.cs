using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace NiL.JS
{
    internal static class ExceptionsHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Throw(BaseLibrary.Error error)
        {
            throw new JSException(error);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Throw(JSValue error)
        {
            throw new JSException(error);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Throw(Error error, Exception innerException)
        {
            throw new JSException(error, innerException);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentNull(string message)
        {
            throw new ArgumentNullException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowVariableNotDefined(object variableName)
        {
            Throw(new ReferenceError(string.Format(Strings.VariableNotDefined, variableName)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIncrementPropertyWOSetter(object proprtyName)
        {
            Throw(new TypeError(string.Format(Strings.IncrementPropertyWOSetter, proprtyName)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIncrementReadonly(object entityName)
        {
            Throw(new TypeError(string.Format(Strings.IncrementReadonly, entityName)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowUnknowToken(string code, int index)
        {
            var cord = CodeCoordinates.FromTextPosition(code, index, 0);
            Throw(new SyntaxError(string.Format(
                Strings.UnknowIdentifier,
                code.Substring(index, System.Math.Min(50, code.Length - index)).Split(Tools.TrimChars).FirstOrDefault(),
                cord)));
        }
    }
}
