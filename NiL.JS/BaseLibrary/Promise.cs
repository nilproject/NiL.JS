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
        pending,
        fulfilled,
        rejected
    }

    public class Promise
    {
        private Function _callback;
        private Task _task;
        private PromiseState _state;
        private bool _complited;
        private JSValue _result;
        private List<CompletionPromise> _thenCallbacks;
        private List<CompletionPromise> _catchCallbacks;

        [Hidden]
        public PromiseState State
        {
            get { return _state; }
            protected set { _state = value; }
        }
        [Hidden]
        public Task Task
        {
            get { return _task; }
            protected set { _task = value; }
        }
        [Hidden]
        public bool Complited
        {
            get { return _complited; }
            protected set { _complited = value; }
        }
        [Hidden]
        public JSValue Result
        {
            get { return _result; }
            protected set { _result = value; }
        }
        [Hidden]
        public Function Callback
        {
            get { return _callback; }
            protected set { _callback = value; }
        }

        public Promise(Function callback)
        {
            _callback = callback ?? Function.Empty;
            run();
        }

        internal Promise()
        {
        }

        internal virtual void run()
        {
            if (_task == null)
            {
                _task = new Task(new Action(taskAction));
                _task.Start();
            }
        }

        internal void taskAction()
        {
            try
            {
                InvokeCallback();
            }
            catch (JSException e)
            {
                _state = PromiseState.rejected;

                _result = e.Error;
            }
            catch (Exception)
            {
                _state = PromiseState.rejected;

                _result = TypeProxy.Proxy(new Error("Unknown error"));
            }

            if (_state == PromiseState.pending)
                _state = PromiseState.fulfilled;

            _complited = true;

            InvokeComplition();
        }

        protected virtual void InvokeCallback()
        {
            _callback.Call(new Arguments
            {
                new ExternalFunction((self, args)=>
                {
                    if (_state == PromiseState.pending)
                    {
                        _state = PromiseState.fulfilled;
                        _result = args[0];
                    }

                    return null;
                }),
                new ExternalFunction((self, args)=>
                {
                    if (_state == PromiseState.pending)
                    {
                        _state = PromiseState.rejected;
                        _result = args[0];
                    }

                    return null;
                })
            });
        }

        protected virtual void InvokeComplition()
        {
            if (_state == PromiseState.fulfilled)
            {
                if (_thenCallbacks != null)
                {
                    lock (_thenCallbacks)
                    {
                        for (var i = 0; i < _thenCallbacks.Count; i++)
                        {
                            _thenCallbacks[i].run(this, _result);
                        }
                    }
                }
            }
            else
            {
                if (_catchCallbacks != null)
                {
                    lock (_catchCallbacks)
                    {
                        for (var i = 0; i < _catchCallbacks.Count; i++)
                        {
                            _catchCallbacks[i].run(this, _result);
                        }
                    }
                }
            }
        }

        public static Promise resolve(JSValue data)
        {
            return new ConstantPromise(data, PromiseState.fulfilled);
        }

        public static Promise race(IIterable promises)
        {
            return new RacePromise(promises.AsEnumerable().Select(convertToPromise).ToArray());
        }

        public static Promise all(IIterable promises)
        {
            return new AllPromise(promises.AsEnumerable().Select(convertToPromise).ToArray());
        }

        private static Promise convertToPromise(JSValue arg)
        {
            return arg.Value as Promise ?? resolve(arg);
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
                    result = new RacePromise(new[] { thenPromise, catchPromise }, true);
                else
                    result = thenPromise;
            }
            else
                result = catchPromise;

            if (_complited)
            {
                if (_state == PromiseState.fulfilled)
                {
                    if (thenPromise != null)
                        thenPromise.run(this, _result);
                }
                else
                {
                    if (catchPromise != null)
                        catchPromise.run(this, _result);
                }
            }
            else
            {
                if (thenPromise != null)
                {
                    if (_thenCallbacks == null)
                        _thenCallbacks = new List<CompletionPromise>();
                    lock (_thenCallbacks)
                    {
                        if (_complited)
                            thenPromise.run(this, _result);
                        else
                            _thenCallbacks.Add(thenPromise);
                    }
                }

                if (catchPromise != null)
                {
                    if (_catchCallbacks == null)
                        _catchCallbacks = new List<CompletionPromise>();
                    lock (_catchCallbacks)
                    {
                        if (_complited)
                            catchPromise.run(this, _result);
                        else
                            _catchCallbacks.Add(catchPromise);
                    }
                }
            }

            return result;
        }
    }

    [Prototype(typeof(Promise))]
    internal class CompletionPromise : Promise
    {
        private JSValue _arg;

        internal CompletionPromise(Function callback)
        {
            Callback = callback;
        }

        internal sealed override void run()
        {
            throw new InvalidOperationException();
        }

        internal virtual void run(Promise sender, JSValue arg)
        {
            if (_arg == null)
            {
                _arg = arg;
                base.run();
            }
        }

        protected override void InvokeCallback()
        {
            Callback.Call(new Arguments { _arg });
        }
    }

    [Prototype(typeof(Promise))]
    internal sealed class ConstantPromise : Promise
    {
        internal ConstantPromise(JSValue value, PromiseState state)
        {
            Result = value;
            State = state;
            Complited = true;
        }

        internal override void run()
        {

        }

        internal override Promise then(CompletionPromise thenPromise, CompletionPromise catchPromise)
        {
            var promise = State == PromiseState.fulfilled ? thenPromise : catchPromise;

            if (promise != null)
                promise.run(this, Result);

            return promise ?? resolve(JSValue.undefined);
        }
    }

    [Prototype(typeof(Promise))]
    internal sealed class AllPromise : CompletionPromise
    {
        private int _expectedNumberOfCompletions;
        private Promise[] _promises;
        private bool _subscribed;

        internal AllPromise(Promise[] promises)
            : base(null)
        {
            _promises = promises;
            _expectedNumberOfCompletions = _promises.Length;
            Result = new Array(_expectedNumberOfCompletions);

            for (var i = 0; i < promises.Length; i++)
                promises[i].run();
        }

        internal override void run(Promise sender, JSValue result)
        {
            lock (Result)
            {
                if (State == PromiseState.pending)
                {
                    if (sender.State == PromiseState.rejected)
                    {
                        Result = result;
                        State = PromiseState.rejected;
                        Complited = true;
                        InvokeComplition();
                    }
                    else
                    {
                        (Result as Array)[System.Array.IndexOf(_promises, sender)] = result;
                        if (--_expectedNumberOfCompletions == 0)
                        {
                            State = PromiseState.fulfilled;
                            Complited = true;
                            InvokeComplition();
                        }
                    }
                }
            }
        }

        internal override Promise then(CompletionPromise thenPromise, CompletionPromise catchPromise)
        {
            if (!_subscribed)
            {
                _subscribed = true;
                for (var i = 0; i < _promises.Length; i++)
                    _promises[i].then(this, this);
            }

            return base.then(thenPromise, catchPromise);
        }
    }

    [Prototype(typeof(Promise))]
    internal sealed class RacePromise : CompletionPromise
    {
        private bool _subscribed;
        private Promise[] _promises;

        public RacePromise(Promise[] promises, bool suppressRun)
            : base(null)
        {
            _promises = promises;

            if (!suppressRun)
            {
                for (var i = 0; i < promises.Length; i++)
                    promises[i].run();
            }
        }

        internal RacePromise(Promise[] promises)
            : this(promises, false)
        {
        }

        internal override void run(Promise sender, JSValue result)
        {
            if (!Complited)
            {
                Complited = true;
                Result = result;
                State = sender.State;
                InvokeComplition();
            }
        }

        internal override Promise then(CompletionPromise thenPromise, CompletionPromise catchPromise)
        {
            if (!_subscribed)
            {
                _subscribed = true;
                for (var i = 0; i < _promises.Length; i++)
                    _promises[i].then(this, this);
            }

            return base.then(thenPromise, catchPromise);
        }
    }
}
