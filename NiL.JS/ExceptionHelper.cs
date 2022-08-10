using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using System.Diagnostics;
using System.Reflection;
using NiL.JS.Expressions;
using NiL.JS.Statements;

namespace NiL.JS
{
    internal static class ExceptionHelper
    {
        internal sealed class JsStackFrame
        {
            public Context Context;
            public CodeNode CodeNode;
        }

        [AttributeUsage(
            AttributeTargets.Method
            | AttributeTargets.Constructor
            | AttributeTargets.Property
            | AttributeTargets.Delegate
            | AttributeTargets.Event)]
        internal sealed class StackFrameOverrideAttribute : Attribute { }

        [ThreadStatic]
        private static Stack<JsStackFrame> _executionStack;

        internal static void DropStackFrame(Context context)
        {
            if (!TryDropStackFrame(context))
                throw new InvalidOperationException("_executionStack.Peek().Context != context");
        }

        internal static bool TryDropStackFrame(Context context)
        {
            if (_executionStack == null || _executionStack.Count == 0)
                return false;

            if (_executionStack.Peek().Context != context)
                return false;

            _executionStack.Pop();
            return true;
        }

        internal static JsStackFrame GetStackFrame(Context context, bool newStackFrame)
        {
            if (_executionStack == null)
                _executionStack = new Stack<JsStackFrame>();

            if (newStackFrame
                || _executionStack.Count == 0
                || _executionStack.Peek().Context != context)
            {
                var frame = new JsStackFrame { Context = context };
                _executionStack.Push(frame);
                return frame;
            }

            return _executionStack.Peek();
        }

        internal static Stack<JsStackFrame> GetExecutionStack()
        {
            var result = new Stack<JsStackFrame>(
                (_executionStack as IEnumerable<JsStackFrame> ?? System.Array.Empty<JsStackFrame>()).Reverse());
            return result;
        }

        private static readonly string[] _namespacesToReplace = new[]
        {
            typeof(CodeBlock).Namespace,
            typeof(Addition).Namespace,
        };

        private static readonly Type[] _baseClassesToHide = new[]
        {
            typeof(Function),
            typeof(ExceptionHelper),
            typeof(ConstructorInfo),
            typeof(RuntimeMethodHandle),
        };

        internal static string GetStackTrace(int skipFrames)
        {
            var stackTraceTexts = new List<string>();

            var stack = GetExecutionStack();

            var stackTrace = new StackTrace(1 + skipFrames, true);
            var originalStackTraceLines = stackTrace.ToString().Split('\n');

            var wordAt = originalStackTraceLines.FirstOrDefault()?.Trim().Split(' ')[0] ?? "at";
            var wordLine = originalStackTraceLines.FirstOrDefault(x => x.Contains(':'))?.Split(':')?.LastOrDefault().Split(' ')[0] ?? "line";

            wordAt = "   " + wordAt + " ";

            var namespaceIndex = wordAt.Length;

            var recordsToRemove = 0;

            JsStackFrame jsFrame = null;
            var frames = stackTrace.GetFrames();
            for (int i = 0; i < frames.Length; i++)
            {
                StackFrame frame = frames[i];
                var method = frame.GetMethod();
                if (method != null && method.GetCustomAttribute(typeof(StackFrameOverrideAttribute)) != null)
                {
                    stackTraceTexts.RemoveRange(stackTraceTexts.Count - recordsToRemove, recordsToRemove);
                    recordsToRemove = 0;

                    jsFrame = stack.Pop();
                    var code = GetCode(jsFrame.Context);
                    var codeCoords = code != null ? CodeCoordinates.FromTextPosition(code, jsFrame.CodeNode?.Position ?? 0, jsFrame.CodeNode?.Length ?? 0) : null;
                    stackTraceTexts.Add(
                        wordAt + (jsFrame.Context?._owner?.name ?? "<anonymous method>") +
                        ":" + wordLine + " " + codeCoords?.Line);
                }
                else if (_baseClassesToHide.Any(x => x.IsAssignableFrom(method.DeclaringType)))
                {
                    // do nothing
                }
                else if (i < originalStackTraceLines.Length)
                {
                    if (method.DeclaringType == null)
                    {
                        recordsToRemove++;
                    }
                    else if (_namespacesToReplace.Any(x => method.DeclaringType.Namespace == x))
                    {
                        recordsToRemove++;
                    }
                    else
                    {
                        recordsToRemove = 0;
                    }

                        stackTraceTexts.Add(originalStackTraceLines[i].Replace("\r", string.Empty));
                }
            }

            return string.Join(Environment.NewLine, stackTraceTexts);
        }

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
        internal static void Throw(Error error, CodeNode exceptionMaker, Context context)
        {
            throw new JSException(error, exceptionMaker, GetCode(context));
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        internal static void Throw(JSValue error, CodeNode exceptionMaker, Context context)
        {
            throw new JSException(error ?? JSValue.undefined, exceptionMaker, GetCode(context));
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
        internal static void ThrowVariableIsNotDefined(string variableName, CodeNode exceptionMaker, Context context)
        {
            var code = GetCode(context);
            Throw(new ReferenceError(string.Format(Strings.VariableNotDefined, variableName)), exceptionMaker, code);
        }

        internal static string GetCode(Context context)
        {
            while (context != null && context._sourceCode == null)
                context = context._parent;

            return context?._sourceCode ?? Script.CurrentScript?.Code;
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        internal static void ThrowVariableIsNotDefined(string variableName, CodeNode exceptionMaker)
        {
            Throw(new ReferenceError(string.Format(Strings.VariableNotDefined, variableName)), exceptionMaker, null as string);
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
            var cord = CodeCoordinates.FromTextPosition(code, position, length);
            Throw(new SyntaxError(message + " " + cord));
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
#if !NET40
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
        internal static void ThrowTypeError(string message, CodeNode exceptionMaker, Context context)
        {
            var code = GetCode(context);
            Throw(new TypeError(message), exceptionMaker, code);
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void SetCallStackData(Exception e, Context context, CodeNode codeNode)
        {
            //var data = e.Data[CallStackMarker.Instance] as List<Tuple<Context, CodeCoordinates>>;
            //if (data == null)
            //    e.Data[CallStackMarker.Instance] = data = new List<Tuple<Context, CodeCoordinates>>();

            //if (data.Count > 0 && data[data.Count - 1].Item1 == context)
            //    return;

            //data.Add(
            //    Tuple.Create(
            //        context,
            //        CodeCoordinates.FromTextPosition(
            //            GetCode(context),
            //            codeNode.Position,
            //            codeNode.Length)));
        }
    }
}
