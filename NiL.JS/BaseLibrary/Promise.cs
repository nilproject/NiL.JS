using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NiL.JS.Core;
using NiL.JS.Core.Functions;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;

namespace NiL.JS.BaseLibrary
{
    public enum PromiseState
    {
        Pending,
        Fulfilled,
        Rejected
    }

    public class Promise
    {
        private JSValue _innerResult;
        private Task _innerTask;
        private Function _callback;
        private Task<JSValue> _task;

        [Hidden]
        public PromiseState State
        {
            get
            {
                if (!Task.IsCompleted)
                    return PromiseState.Pending;

                return statusToState(_task.Status);
            }
        }

        [Hidden]
        public Task<JSValue> Task
        {
            get { return _task; }
            protected set { _task = value; }
        }
        [Hidden]
        public bool Complited
        {
            get { return _task.IsCompleted; }
        }
        [Hidden]
        public JSValue Result
        {
            get
            {
                return Task.Status == TaskStatus.RanToCompletion ?
                    Task.Result
                    :
                    (Task.Exception.GetBaseException() as JSException).Error;
            }
        }
        [Hidden]
        public Function Callback
        {
            get { return _callback; }
            protected set { _callback = value; }
        }

        public Promise(Function callback)
            : this()
        {
            _callback = callback ?? Function.Empty;
            run();
        }

        protected Promise()
        {
            _task = new Task<JSValue>(() =>
            {
                if (_innerTask == null)
                {
                    return JSValue.undefined;
                }

                _innerTask.Wait();

                switch (statusToState(_innerTask.Status))
                {
                    case PromiseState.Fulfilled:
                        return _innerResult;
                    case PromiseState.Rejected:
                        throw _innerTask.Exception;
                    default:
                        return JSValue.undefined;
                }
            });
        }

        protected Promise(Task<JSValue> task)
        {
            _task = task;
        }

        internal virtual void run()
        {
            if (_innerTask == null)
            {
                _innerTask = new Task(callbackInvoke);
                _innerTask.Start();
            }
        }

        protected void callbackInvoke()
        {
            var statusSetted = false;
            var reject = false;

            try
            {
                _callback.Call(new Arguments
                {
                    new ExternalFunction((self, args)=>
                    {
                        if (!statusSetted)
                        {
                            statusSetted = true;
                            _innerResult = args[0];
                            _task.Start();
                        }

                        return null;
                    }),
                    new ExternalFunction((self, args)=>
                    {
                        if (!statusSetted)
                        {
                            statusSetted = true;
                            _innerResult = args[0];
                            _task.Start();
                            reject = true;
                        }

                        return null;
                    })
                });
            }
            catch (JSException e)
            {
                _innerResult = e.Error;
                throw;
            }
            catch
            {
                _innerResult = JSValue.Wrap(new Error("Unknown error"));
                throw;
            }

            if (reject)
                throw new JSException(_innerResult);
        }

        public static Promise resolve(JSValue data)
        {
            return new Promise(Task<JSValue>.FromResult(data));
        }

        public static Promise race(IIterable promises)
        {
            if (promises == null)
            {
                return new Promise(Task<JSValue>.FromException<JSValue>(new JSException(new TypeError("Invalid Promise.race params"))));
            }

            return new AnyPromise(Task<JSValue>.WhenAny(promises.AsEnumerable().Select(convertToTask)));
        }

        public static Promise all(IIterable promises)
        {
            if (promises == null)
            {
                return new Promise(Task<JSValue>.FromException<JSValue>(new JSException(new TypeError("Invalid Promise.all params"))));
            }

            return new AllPromise(Task<JSValue>.WhenAll(promises.AsEnumerable().Select(convertToTask)));
        }

        private static Task<JSValue> convertToTask(JSValue arg)
        {
            return (arg.Value as Promise)?.Task ?? Task<JSValue>.FromResult(arg);
        }

        public Promise @catch(Function onRejection)
        {
            return then(null, onRejection);
        }

        public Promise then(Function onFulfilment, Function onRejection)
        {
            var thenPromise = onFulfilment != null && onFulfilment.valueType == JSValueType.Function ? new CompletionPromise(onFulfilment) : null;
            var catchPromise = onRejection != null && onRejection.valueType == JSValueType.Function ? new CompletionPromise(onRejection) : null;

            return then(thenPromise, catchPromise);
        }

        internal virtual Promise then(CompletionPromise thenPromise, CompletionPromise catchPromise)
        {
            Promise result;

            if (thenPromise == null && catchPromise == null)
                return resolve(JSValue.undefined);

            if (thenPromise != null)
            {
                if (catchPromise != null)
                    result = new AnyPromise(Task<JSValue>.WhenAny(thenPromise.Task, catchPromise.Task));
                else
                    result = thenPromise;
            }
            else
                result = catchPromise;

            if (thenPromise != null)
            {
                if (_task.Status == TaskStatus.RanToCompletion)
                    thenPromise.run(Result);
                else
                    _task.ContinueWith(task => thenPromise.run(Result), TaskContinuationOptions.OnlyOnRanToCompletion);
            }

            if (catchPromise != null)
            {
                if (_task.IsCanceled || _task.IsFaulted)
                    catchPromise.run(Result);
                else
                    _task.ContinueWith(task => catchPromise.run(Result), TaskContinuationOptions.NotOnRanToCompletion);
            }

            return result;
        }

        private static PromiseState statusToState(TaskStatus status)
        {
            switch (status)
            {
                case TaskStatus.Canceled:
                case TaskStatus.Faulted:
                    return PromiseState.Rejected;
                case TaskStatus.RanToCompletion:
                    return PromiseState.Fulfilled;
                case TaskStatus.Created:
                case TaskStatus.Running:
                case TaskStatus.WaitingForActivation:
                case TaskStatus.WaitingForChildrenToComplete:
                case TaskStatus.WaitingToRun:
                    return PromiseState.Pending;
                default:
                    return PromiseState.Rejected;
            }
        }
    }

    [Prototype(typeof(Promise), true)]
    internal class CompletionPromise : Promise
    {
        private JSValue _arg;

        internal CompletionPromise(Function callback)
            : base(null as Task<JSValue>)
        {
            Callback = callback;
            Task = new Task<JSValue>(() => Callback.Call(new Arguments { _arg }));
        }

        internal CompletionPromise(Task<JSValue> task)
            : base(task)
        {
        }

        internal sealed override void run()
        {
            throw new InvalidOperationException();
        }

        internal void run(JSValue arg)
        {
            if (_arg == null)
            {
                _arg = arg ?? JSValue.undefined;
                Task.Start();
            }
        }
    }

    [Prototype(typeof(Promise), true)]
    internal sealed class AnyPromise : CompletionPromise
    {
        internal AnyPromise(Task<Task<JSValue>> task)
            : base(new Task<JSValue>(() => task.Result.Result))
        {
            task.ContinueWith(x => run(null));
        }
    }

    [Prototype(typeof(Promise), true)]
    internal sealed class AllPromise : CompletionPromise
    {
        internal AllPromise(Task<JSValue[]> task)
            : base(new Task<JSValue>(() => new Array(task.Result)))
        {
            task.ContinueWith(x => run(null));
        }
    }
}
