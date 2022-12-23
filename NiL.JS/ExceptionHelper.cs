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
using System.Collections;
using System.Runtime.ExceptionServices;

namespace NiL.JS
{
    internal static class ExceptionHelper
    {
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

        internal sealed class StackTraceState
        {
            public JsStackFrame JsStack;

            public override string ToString()
                => ToString(null);

            public string ToString(JSException jSException)
            {
                var stack = JsStack;

                var exceptionStackTrace = new StackTrace(jSException, true);

                var originalStackTraceLines = exceptionStackTrace.ToString().Split('\n');
                var wordAt = originalStackTraceLines.FirstOrDefault()?.Trim().Split(' ')[0] ?? "at";
                var wordLine = originalStackTraceLines.FirstOrDefault(x => x.Contains(':'))?.Split(':')?.LastOrDefault().Split(' ')[0] ?? "line";

                wordAt = "   " + wordAt + " ";

                var recordsToRemove = 0;
                JsStackFrame jsFrame = null;
                var frames = exceptionStackTrace.GetFrames();
                var stackTraceTexts = new List<string>();
                for (int i = 0; i < frames.Length; i++)
                {
                    StackFrame frame = frames[i];
                    var method = frame.GetMethod();
                    if (method != null
                             && method.GetCustomAttribute(typeof(StackFrameOverrideAttribute)) != null)
                    {
                        stackTraceTexts.RemoveRange(stackTraceTexts.Count - recordsToRemove, recordsToRemove);
                        recordsToRemove = 0;
                        jsFrame = stack;
                        stack = stack.PrevFrame;

                        var code = GetSourceCode(jsFrame);
                        var codeCoords = code != null ? CodeCoordinates.FromTextPosition(code, jsFrame.CodeNode?.Position ?? 0, jsFrame.CodeNode?.Length ?? 0) : null;

                        stackTraceTexts.Add(
                            wordAt + (jsFrame.Context?._owner?.name ?? "<anonymous method>") +
                            (codeCoords != null ? ":" + wordLine + " " + codeCoords.Line + ":" + codeCoords.Column : ""));
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
        }

        internal sealed class JsStackFrame
        {
            public JsStackFrame PrevFrame;
            public CodeNode CodeNode;
            public Context Context;
            public string SourceCode;
        }

        [AttributeUsage(
            AttributeTargets.Method
            | AttributeTargets.Constructor
            | AttributeTargets.Property
            | AttributeTargets.Delegate
            | AttributeTargets.Event)]
        internal sealed class StackFrameOverrideAttribute : Attribute { }

        [ThreadStatic]
        private static JsStackFrame _executionStack;

        internal static void DropStackFrame(Context context)
        {
            if (!TryDropStackFrame(context))
                throw new InvalidOperationException("_executionStack.Context != context");
        }

        internal static bool TryDropStackFrame(Context context)
        {
            if (_executionStack == null)
                return false;

            if (_executionStack.Context != context)
                return false;

            _executionStack = _executionStack.PrevFrame;
            return true;
        }

        internal static JsStackFrame GetStackFrame(Context context, bool newStackFrame)
        {
            if (newStackFrame
                || _executionStack == null
                || _executionStack.Context != context)
            {
                _executionStack = new JsStackFrame
                {
                    Context = context,
                    PrevFrame = _executionStack
                };
                return _executionStack;
            }

            return _executionStack;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static StackTraceState GetJsStackTrace()
        {
            var stack = _executionStack;
            JsStackFrame stackCopy = null;

            if (stack != null)
            {
                stackCopy = new JsStackFrame
                {
                    CodeNode = stack.CodeNode,
                    Context = stack.Context,
                    SourceCode = stack.SourceCode,
                };

                var stackCopyTail = stackCopy;
                stack = stack.PrevFrame;
                while (stack != null)
                {
                    var frame = new JsStackFrame
                    {
                        CodeNode = stack.CodeNode,
                        Context = stack.Context,
                        SourceCode = stack.SourceCode,
                    };

                    stackCopyTail.PrevFrame = frame;
                    stackCopyTail = frame;
                    stack = stack.PrevFrame;
                }
            }

            return new StackTraceState
            {
                JsStack = stackCopy
            };
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
        internal static void Throw(Error error, CodeNode exceptionMaker, Context context)
        {
            GetStackFrame(context, false).CodeNode = exceptionMaker;
            throw new JSException(error, exceptionMaker);
        }

        /// <exception cref="NiL.JS.Core.JSException">
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        internal static void Throw(JSValue error, CodeNode exceptionMaker, Context context)
        {
            GetStackFrame(context, false).CodeNode = exceptionMaker;
            throw new JSException(error ?? JSValue.undefined, exceptionMaker);
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
            Throw(new ReferenceError(string.Format(Strings.VariableNotDefined, variableName)), exceptionMaker, context);
        }

        internal static string GetSourceCode(JsStackFrame frame)
        {
            while (frame != null && frame.SourceCode == null)
                frame = frame.PrevFrame;

            return frame?.SourceCode ?? Script.CurrentScript?.Code;
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
            Throw(new TypeError(message), exceptionMaker, context);
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
