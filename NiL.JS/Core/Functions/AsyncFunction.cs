using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;
using NiL.JS.Extensions;

namespace NiL.JS.Core.Functions
{
    [Prototype(typeof(Function), true)]
    internal sealed class AsyncFunction : Function
    {
        private sealed class Сontinuator
        {
            private readonly AsyncFunction _asyncFunction;
            private readonly JSValue _promise;
            private readonly Context _context;

            public JSValue ResultPromise { get; private set; }

            public Сontinuator(JSValue promise, AsyncFunction asyncFunction, Context context)
            {
                _promise = promise;
                _asyncFunction = asyncFunction;
                _context = context;
            }

            public void Build()
            {
                ResultPromise = subscribe(_promise, then, fail);
            }

            private static JSValue subscribe(JSValue promise, Func<JSValue, JSValue> then, Func<JSValue, JSValue> fail)
            {
                var thenFunction = promise["then"];
                if (thenFunction == null || thenFunction.ValueType != JSValueType.Function)
                    throw new JSException(new TypeError("The promise has no function \"then\""));

                return thenFunction.As<Function>().Call(promise, new Arguments { then, fail });
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
                var thread = Thread.CurrentThread;
                do
                {
                    result = _asyncFunction.run(_context);

                    if (_context._executionMode == ExecutionMode.Suspend)
                    {
                        subscribe(
                            result,
                            r =>
                            {
                                _context._executionInfo = r;
                                _context._executionMode = ExecutionMode.Resume;
#pragma warning disable CS0618 // Тип или член устарел
                                thread.Resume();
#pragma warning restore CS0618 // Тип или член устарел
                                return null;
                            },
                            e =>
                            {
                                _context._executionInfo = e;
                                _context._executionMode = ExecutionMode.ResumeThrow;
#pragma warning disable CS0618 // Тип или член устарел
                                thread.Resume();
#pragma warning restore CS0618 // Тип или член устарел
                                return null;
                            });

#pragma warning disable CS0618
                        thread.Suspend();
#pragma warning restore CS0618
                    }
                }
                while (_context._executionMode == ExecutionMode.Resume || _context._executionMode == ExecutionMode.ResumeThrow);

                return result;
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
            internalContext._definedVariables = Body._variables;

            initContext(targetObject, arguments, true, internalContext);
            initParameters(arguments, internalContext);

            var result = run(internalContext);

            result = processSuspend(internalContext, result);

            return result;
        }

        private JSValue processSuspend(Context internalContext, JSValue result)
        {
            if (internalContext._executionMode == ExecutionMode.Suspend)
            {
                var promise = internalContext._executionInfo;
                var continuator = new Сontinuator(promise, this, internalContext);
                continuator.Build();
                result = continuator.ResultPromise;
            }
            else
            {
                result = Marshal(Promise.resolve(result));
            }

            return result;
        }

        private JSValue run(Context internalContext)
        {
            internalContext.Activate();
            JSValue result = null;
            try
            {
                result = evaluateBody(internalContext);
            }
            finally
            {
                internalContext.Deactivate();
            }

            return result;
        }
    }
}
