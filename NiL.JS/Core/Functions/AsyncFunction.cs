using System;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;
using NiL.JS.Extensions;

namespace NiL.JS.Core.Functions;

[Prototype(typeof(Function), true)]
internal sealed class AsyncFunction : Function
{
    internal sealed class Сontinuator
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
            if (promiseOrValue is null)
                return promiseOrValue;

            if (promiseOrValue.Value is Promise promise)
            {
                var result = promise.then(then, fail, false);
                return _context.GlobalContext.ProxyValue(result);
            }
            else
            {
                if (!promiseOrValue.Defined)
                    return promiseOrValue;

                var thenFunc = promiseOrValue["then"];
                if (thenFunc._valueType != JSValueType.Function)
                    return promiseOrValue;

                var result = thenFunc.As<ICallable>().Call(
                    promiseOrValue, 
                    new() { new Func<JSValue, JSValue>(then), new Func<JSValue, JSValue>(fail) });

                return _context.GlobalContext.ProxyValue(result);
            }
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
            return Context.GlobalContext.WrapValue(Promise.resolve(notExists));
        }

        arguments ??= new Arguments(Context.CurrentContext);

        var internalContext = new Context(_initialContext, true, this);
        internalContext._callDepth = (Context.CurrentContext?._callDepth ?? 0) + 1;

        // Async invocations can run concurrently on different thread-pool threads (each
        // C# await after Task.Yield() can resume on a different thread). The shared
        // VariableDescriptor.cacheContext/cacheValue fields on the function definition
        // are not per-invocation, so concurrent calls overwrite each other's cache.
        // Passing true to both initContext and initParameters forces every binding
        // (arguments object, function name, and all parameters) into the per-invocation
        // internalContext._variables dictionary. deepGet() then finds them via
        // context._variables.TryGetValue() even after the cache has been overwritten.
        // Body-variable storage is handled in CodeBlock.initVariables via the async-kind
        // check that forces cew=true for all async function kinds.
        // This mirrors GeneratorIterator.initContext() which also passes true for both.
        initContext(
            targetObject,
            arguments,
            true,
            internalContext);

        initParameters(
            arguments,
            true,
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
            result = _initialContext.GlobalContext.ProxyValue(Promise.resolve(result));
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
