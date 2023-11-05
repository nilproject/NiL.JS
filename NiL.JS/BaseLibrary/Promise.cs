using System;
using System.Collections;
using System.Linq;
using System.Runtime.ExceptionServices;
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

    public sealed class Promise
    {
        private Task _innerTask;
        private Function _callback;
        private TaskCompletionSource<JSValue> _outerTask;

        [Hidden]
        public PromiseState State
        {
            get
            {
                if (!Task.IsCompleted)
                    return PromiseState.Pending;

                return statusToState(_outerTask.Task.Status);
            }
        }

        [Hidden]
        public Task<JSValue> Task => _outerTask.Task;

        [Hidden]
        public bool Complited
        {
            get { return _outerTask.Task.IsCompleted; }
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
        public Function Callback => _callback;

        public Promise(Function callback)
            : this()
        {
            _callback = callback ?? Function.Empty;

            _innerTask = new Task(callbackInvoke);
            _innerTask.Start();
        }

        private Promise()
        {
            _outerTask = new TaskCompletionSource<JSValue>();
        }

        internal Promise(Task<JSValue> task)
            : this()
        {
            var continuation = new Action<Task<JSValue>>((t) =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    handlePromiseCascade(t.Result, false);
                }
                else if (t.Status == TaskStatus.Faulted)
                {
                    _outerTask.SetException(t.Exception.GetBaseException() ?? t.Exception);
                }
                else
                {
                    switch (statusToState(_innerTask.Status))
                    {
                        case PromiseState.Fulfilled:
                            _outerTask.SetResult(t.Result);
                            break;

                        case PromiseState.Rejected:
                            _outerTask.SetException(_innerTask.Exception);
                            break;

                        default:
                            _outerTask.SetResult(JSValue.undefined);
                            break;
                    }
                }
            });

            _innerTask = task.ContinueWith(continuation);
        }

        private void handlePromiseCascade(JSValue value, bool error)
        {
            var task = (value?.Value as Promise)?.Task ?? value?.Value as Task<JSValue>;
            if (task != null)
            {
                task.ContinueWith((t) =>
                {
                    if (t.IsFaulted)
                    {
                        var exception = t.Exception.GetBaseException() as JSException ?? t.Exception.GetBaseException() ?? t.Exception;
                        _outerTask.SetException(exception);
                    }
                    else
                        handlePromiseCascade(t.Result, false);
                });
            }
            else
            {
                if (error)
                    _outerTask.SetException(new JSException(value));
                else
                    _outerTask.SetResult(value);
            }
        }

        private void callbackInvoke()
        {
            var statusSet = false;

            try
            {
                _callback.Call(new Arguments(null)
                {
                    new ExternalFunction((self, args)=>
                    {
                        if (!statusSet)
                        {
                            statusSet = true;

                            handlePromiseCascade(args[0], false);
                        }

                        return null;
                    }),

                    new ExternalFunction((self, args)=>
                    {
                        if (!statusSet)
                        {
                            statusSet = true;

                            handlePromiseCascade(args[0], true);
                        }

                        return null;
                    })
                });
            }
            catch (JSException e)
            {
                if (!statusSet)
                    _outerTask.SetException(e);

                throw;
            }
            catch
            {
                if (!statusSet)
                    _outerTask.SetException(new JSException(new Error("Unknown error")));

                throw;
            }

            //if (!statusSet)
            //    _outerTask.SetResult(JSValue.undefined);
        }

        public static Promise resolve(JSValue data)
        {
            return data.As<Promise>() ?? new Promise(fromResult(data));
        }

        public static Promise reject(JSValue data)
        {
            var result = new Promise();
            result._outerTask.SetException(new JSException(data));
            return result;
        }

        public static Promise race(IIterable promises)
        {
            if (promises == null)
                return new Promise(fromException(new JSException(new TypeError("Invalid argruments for Promise.race(...)"))));

            return new Promise(whenAny(promises.AsEnumerable().Select(convertToTask).ToArray()));
        }

        public static Promise all(IIterable promises)
        {
            if (promises == null)
                return new Promise(fromException(new JSException(new TypeError("Invalid argruments for Promise.all(...)"))));

            return new Promise(whenAll(promises.AsEnumerable().Select(convertToTask).ToArray()).ContinueWith(x => new Array(x.Result as IEnumerable) as JSValue));
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
            return then(
                onFulfilment == null ? null : value => onFulfilment.Call(JSValue.undefined, new Arguments { value }),
                onRejection == null ? null : value => onRejection.Call(JSValue.undefined, new Arguments { value }),
                false);
        }

        public Promise @finally(Function onFinally)
        {
            Func<JSValue, JSValue> func = onFinally == null ? null : value => onFinally.Call(JSValue.undefined, new Arguments { value });
            return then(func, func, true);
        }

        [Hidden]
        public Promise then(Func<JSValue, JSValue> onFulfilment, Func<JSValue, JSValue> onRejection, bool rethrow)
        {
            if (onFulfilment == null && onRejection == null)
                return resolve(JSValue.undefined);

            var catchTask = onRejection == null 
                ? null 
                : _outerTask.Task.ContinueWith(task =>
                {
                    Exception ex = task.Exception.GetBaseException();
                    JSValue result;
                    if (ex is JSException jsException)
                    {
                        result = onRejection(jsException.Error);
                    }
                    else
                    {
                        result = onRejection(Context.CurrentGlobalContext.ProxyValue(ex));
                    }

                    if (rethrow)
                        ExceptionDispatchInfo.Capture(ex).Throw();

                    return result;
                },
                TaskContinuationOptions.NotOnRanToCompletion);

            var thenTask = onFulfilment == null
                ? null
                : catchTask is not null
                    ? _outerTask.Task.ContinueWith(task => onFulfilment(task.Result), TaskContinuationOptions.OnlyOnRanToCompletion)
                    : _outerTask.Task.ContinueWith(task => onFulfilment(task.Result));

            if (thenTask != null)
            {
                if (catchTask != null)
                    return new Promise(whenAny(thenTask, catchTask));

                return new Promise(thenTask);
            }

            return new Promise(catchTask);
        }

        private static Task<JSValue> whenAny(params Task<JSValue>[] tasks)
        {
            Task<JSValue> result = null;
            var task = new TaskCompletionSource<JSValue>();
            Action<Task<JSValue>> contination = t =>
            {
                lock (task)
                {
                    if (result == null)
                    {
                        result = t;
                        if (!t.IsFaulted)
                            task.SetResult(t.Result);
                        else
                            task.SetException(t.Exception);
                    }
                }
            };

            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i].ContinueWith(contination, TaskContinuationOptions.NotOnCanceled);
            }

            return task.Task;
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
            JSValue[] result = new JSValue[tasks.Length];
            var task = new TaskCompletionSource<JSValue[]>();
            var count = tasks.Length;

            Action<Task<JSValue>> contination = t =>
            {
                if (task.Task.IsCompleted)
                    return;

                try
                {
                    var index = System.Array.IndexOf(tasks, t);
                    if (t.IsCanceled)
                        throw new OperationCanceledException();

                    result[index] = t.Result;

                    if (Interlocked.Decrement(ref count) == 0)
                        task.SetResult(result);
                }
                catch (Exception e)
                {
                    task.SetException(e.GetBaseException() ?? e);
                }
            };

            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i].ContinueWith(contination, TaskContinuationOptions.NotOnCanceled);
            }

            return task.Task;
        }

        private static Task<JSValue> fromException(Exception exception)
        {
            var task = new Task<JSValue>(new Func<JSValue>(() => { throw exception; }));
            task.Start();
            return task;
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
}
