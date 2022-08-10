using System;
using System.Threading;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;

namespace NiL.JS.Core.Functions
{
    [Prototype(typeof(Function), true)]
    internal sealed class AsyncFunction : Function
    {
        private sealed class Сontinuator
        {
            private readonly AsyncFunction _asyncFunction;
            private readonly Context _context;

            public JSValue ResultPromise { get; private set; }

            public Сontinuator(AsyncFunction asyncFunction, Context context)
            {
                _asyncFunction = asyncFunction;
                _context = context;
            }

            public void Build(JSValue promise)
            {
                ResultPromise = subscribeOrReturnValue(promise);
            }

            private JSValue subscribeOrReturnValue(JSValue promiseOrValue)
            {
                var p = promiseOrValue?.Value as Promise;
                if (p == null)
                    return promiseOrValue;

                var result = p.then(then, fail);

                return Marshal(result);
            }

            private JSValue fail(JSValue arg)
            {
                return @continue(arg, ExecutionMode.ResumeThrow);
            }

            private JSValue then(JSValue arg)
            {
                return @continue(arg, ExecutionMode.Resume);
            }

            private JSValue @continue(JSValue arg, ExecutionMode mode)
            {
                _context._executionInfo = arg;
                _context._executionMode = mode;

                JSValue result = null;
                result = _asyncFunction.run(_context);

                return subscribeOrReturnValue(result);
            }
        }

        public override JSValue prototype
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        public AsyncFunction(Context context, FunctionDefinition implementation)
            : base(context, implementation)
        {
            RequireNewKeywordLevel = RequireNewKeywordLevel.WithoutNewOnly;
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
        {
            if (construct)
                ExceptionHelper.ThrowTypeError("Async function cannot be invoked as a constructor");

            var body = _functionDefinition._body;
            if (body._lines.Length == 0)
            {
                notExists._valueType = JSValueType.NotExists;
                return notExists;
            }

            if (arguments == null)
                arguments = new Arguments(Context.CurrentContext);

            var internalContext = new Context(_initialContext, true, this);
            internalContext._callDepth = (Context.CurrentContext?._callDepth ?? 0) + 1;
            internalContext._definedVariables = Body._variables;

            initContext(
                targetObject,
                arguments,
                _functionDefinition._functionInfo.ContainsArguments,
                internalContext);

            initParameters(
                arguments,
                _functionDefinition._functionInfo.ContainsEval
                || _functionDefinition._functionInfo.ContainsWith
                || _functionDefinition._functionInfo.ContainsDebugger
                || _functionDefinition._functionInfo.NeedDecompose
                || (internalContext?._debugging ?? false),
                internalContext);

            var result = run(internalContext);

            result = processSuspend(internalContext, result);

            return result;
        }

        private JSValue processSuspend(Context internalContext, JSValue result)
        {
            if (internalContext._executionMode == ExecutionMode.Suspend)
            {
                var promise = internalContext._executionInfo;
                var continuator = new Сontinuator(this, internalContext);
                continuator.Build(promise);
                result = continuator.ResultPromise;
            }
            else
            {
                result = Marshal(Promise.resolve(result));
            }

            return result;
        }

        [ExceptionHelper.StackFrameOverride]
        private JSValue run(Context internalContext)
        {
            internalContext.Activate();
            JSValue result = null;
            try
            {
                result = evaluateBody(internalContext);
            }
            catch (JSException)
            {
                throw;
            }
            finally
            {
                internalContext.Deactivate();
            }

            return result;
        }
    }
}
