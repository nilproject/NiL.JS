using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using System.Diagnostics;

namespace NiL.JS
{
    internal static class ExceptionHelper
    {
        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        internal static void Throw(Error error)
        {
            throw new JSException(error);
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        internal static void Throw(Error error, CodeNode exceptionMaker, string code)
        {
            throw new JSException(error, exceptionMaker, code);
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        internal static void Throw(JSValue error)
        {
            throw new JSException(error ?? JSValue.undefined);
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        internal static void Throw(Error error, Exception innerException)
        {
            throw new JSException(error, innerException);
        }

        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        internal static void ThrowArgumentNull(string message)
        {
            throw new ArgumentNullException(message);
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        internal static void ThrowVariableIsNotDefined(string variableName, string code, int position, int length, CodeNode exceptionMaker)
        {
            var cord = CodeCoordinates.FromTextPosition(code, position, 0);
            Throw(new ReferenceError(string.Format(Strings.VariableNotDefined, variableName)), exceptionMaker, code);
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        internal static void ThrowVariableIsNotDefined(string variableName, CodeNode exceptionMaker)
        {
            Throw(new ReferenceError(string.Format(Strings.VariableNotDefined, variableName)), exceptionMaker, null);
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        internal static void ThrowIncrementPropertyWOSetter(object proprtyName)
        {
            Throw(new TypeError(string.Format(Strings.IncrementPropertyWOSetter, proprtyName)));
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        internal static void ThrowIncrementReadonly(object entityName)
        {
            Throw(new TypeError(string.Format(Strings.IncrementReadonly, entityName)));
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
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
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        internal static void ThrowSyntaxError(string message)
        {
            Throw(new SyntaxError(message));
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        internal static void ThrowSyntaxError(string message, string code, int position)
        {
            ThrowSyntaxError(message, code, position, 0);
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [DebuggerStepThrough]
        internal static void ThrowSyntaxError(string message, string code, int position, int length)
        {
            var cord = CodeCoordinates.FromTextPosition(code, position, 0);
            Throw(new SyntaxError(message + " " + cord));
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
#if INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        internal static T ThrowIfNotExists<T>(T obj, object name) where T : JSValue
        {
            if (obj._valueType == JSValueType.NotExists)
                ExceptionHelper.Throw((new NiL.JS.BaseLibrary.ReferenceError("Variable \"" + name + "\" is not defined.")));
            return obj;
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [DebuggerStepThrough]
        internal static void ThrowReferenceError(string message, string code, int position, int length)
        {
            var cord = CodeCoordinates.FromTextPosition(code, position, 0);
            Throw(new ReferenceError(message + " " + cord));
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [DebuggerStepThrough]
        internal static void ThrowReferenceError(string message)
        {
            Throw(new ReferenceError(message));
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [DebuggerStepThrough]
        internal static void ThrowTypeError(string message)
        {
            Throw(new TypeError(message));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        internal static void Throw(Exception exception)
        {
            throw exception;
        }
    }
}
