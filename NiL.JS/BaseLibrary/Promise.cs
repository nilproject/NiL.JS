using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private readonly object _sync = new object();
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

        internal Promise(Task<JSValue> task)
        {
            _task = task;
        }

        internal virtual void run()
        {
            if (_task.Status != TaskStatus.Created)
                throw new InvalidOperationException("Task can't be started");

            if (_innerTask == null)
            {
                lock (_sync)
                {
                    if (_innerTask == null)
                    {
                        _innerTask = new Task(callbackInvoke);
                        _innerTask.Start();
                    }
                }
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
                            reject = true;
                            statusSetted = true;
                            _innerResult = args[0];
                            _task.Start();
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
            return new Promise(fromResult(data));
        }

        public static Promise race(IIterable promises)
        {
            if (promises == null)
                return new Promise(fromException(new JSException(new TypeError("Invalid argruments for Promise.race(...)"))));

            return new AnyPromise(whenAny(promises.AsEnumerable().Select(convertToTask).ToArray()));
        }

        public static Promise all(IIterable promises)
        {
            if (promises == null)
                return new Promise(fromException(new JSException(new TypeError("Invalid argruments for Promise.all(...)"))));

            return new AllPromise(whenAll(promises.AsEnumerable().Select(convertToTask).ToArray()));
        }

        private static Task<JSValue> convertToTask(JSValue arg)
        {
            return (arg.Value as Promise)?.Task ?? fromResult(arg);
        }

        public Promise @catch(Function onRejection)
        {
            return then(null, onRejection);
        }

        public Promise then(Function onFulfilment, Function onRejection)
        {
            var thenPromise = onFulfilment != null && onFulfilment._valueType == JSValueType.Function ? new CompletionPromise(onFulfilment) : null;
            var catchPromise = onRejection != null && onRejection._valueType == JSValueType.Function ? new CompletionPromise(onRejection) : null;

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
                    result = new AnyPromise(whenAny(thenPromise.Task, catchPromise.Task));
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
                    _task.ContinueWith(task => catchPromise.run(Result), TaskContinuationOptions.OnlyOnFaulted);
            }

            return result;
        }

        private static Task<Task<JSValue>> whenAny(params Task<JSValue>[] tasks)
        {
#if NET40
            Task<JSValue> result = null;
            var task = new Task<Task<JSValue>>(() => result);
            Action<Task<JSValue>> contination = t =>
            {
                lock (task)
                {
                    if (result == null)
                    {
                        result = t;
                        task.Start();
                    }
                }
            };

            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i].ContinueWith(contination);
                if (tasks[i].Status == TaskStatus.Created)
                    tasks[i].Start();
            }

            return task;
#else
            return Task<JSValue>.WhenAny(tasks);
#endif
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

        private static Task<JSValue[]> whenAll(Task<JSValue>[] tasks)
        {
#if NET40
            JSValue[] result = new JSValue[tasks.Length];
            var task = new Task<JSValue[]>(() => result);
            var count = tasks.Length - 1;
            Action<Task<JSValue>> contination = t =>
            {
                var index = System.Array.IndexOf(tasks, t);
                if (t.IsCanceled)
                    throw new OperationCanceledException();

                result[index] = t.Result;

                if (Interlocked.Decrement(ref count) == 0)
                    task.Start();
            };

            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i].ContinueWith(contination);
                if (tasks[i].Status == TaskStatus.Created)
                    tasks[i].Start();
            }

            return task;
#else
            return Task<JSValue>.WhenAll(tasks);
#endif
        }

        private static Task<JSValue> fromException(Exception exception)
        {
#if NET40
            var task = new Task<JSValue>(new Func<JSValue>(() => { throw exception; }));
            task.Start();
            return task;
#else
            return Task<JSValue>.Run<JSValue>(new Func<JSValue>(() => { throw exception; }));
#endif
        }

        private static Task<JSValue> fromResult(JSValue arg)
        {
#if NET40
            var task = new Task<JSValue>(new Func<JSValue>(() => arg));
            task.Start();
            return task;
#else
            return Task<JSValue>.FromResult(arg);
#endif
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
