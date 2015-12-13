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
        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Throw(Error error)
        {
            throw new JSException(error);
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Throw(JSValue error)
        {
            throw new JSException(error ?? JSValue.undefined);
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Throw(Error error, Exception innerException)
        {
            throw new JSException(error, innerException);
        }

        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentNull(string message)
        {
            throw new ArgumentNullException(message);
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowVariableNotDefined(object variableName)
        {
            Throw(new ReferenceError(string.Format(Strings.VariableNotDefined, variableName)));
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIncrementPropertyWOSetter(object proprtyName)
        {
            Throw(new TypeError(string.Format(Strings.IncrementPropertyWOSetter, proprtyName)));
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIncrementReadonly(object entityName)
        {
            Throw(new TypeError(string.Format(Strings.IncrementReadonly, entityName)));
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowUnknownToken(string code, int index)
        {
            var cord = CodeCoordinates.FromTextPosition(code, index, 0);
            Throw(new SyntaxError(string.Format(
                Strings.UnknowIdentifier,
                code.Substring(index, System.Math.Min(50, code.Length - index)).Split(Tools.TrimChars).FirstOrDefault(),
                cord)));
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        internal static void ThrowSyntaxError(string message, string code, int position)
        {
            ThrowSyntaxError(message, code, position, 0);
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        internal static void ThrowSyntaxError(string message, string code, int position, int length)
        {
            var cord = CodeCoordinates.FromTextPosition(code, position, 0);
            Throw(new SyntaxError(message + " " + cord));
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal static T ThrowIfNotExists<T>(T obj, object name) where T : JSValue
        {
            if (obj.valueType == JSValueType.NotExists)
                ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.ReferenceError("Variable \"" + name + "\" has not been defined.")));
            return obj;
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        internal static void ThrowReferenceError(string message)
        {
            Throw(new ReferenceError(message));
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        internal static void ThrowTypeError(string message)
        {
            Throw(new TypeError(message));
        }

        internal static void Throw(Exception exception)
        {
            throw exception;
        }
    }
}
