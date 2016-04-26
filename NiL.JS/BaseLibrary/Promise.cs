using NiL.JS.Core;
using NiL.JS.Core.Functions;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        protected Function _callback;
        protected Task _task;
        protected PromiseState _state;
        protected bool _complited;
        protected JSValue _result;
        private List<CompletionPromise> _thenCallbacks;
        private List<CompletionPromise> _catchCallbacks;

        [Hidden]
        public PromiseState State => _state;

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
                _task = Task.Run(new Action(taskAction));
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
            _callback = callback;
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
            _callback.Call(new Arguments { _arg });
        }
    }

    [Prototype(typeof(Promise))]
    internal sealed class ConstantPromise : Promise
    {
        internal ConstantPromise(JSValue value, PromiseState state)
        {
            _result = value;
            _state = state;
            _complited = true;
        }

        internal override void run()
        {

        }

        internal override Promise then(CompletionPromise thenPromise, CompletionPromise catchPromise)
        {
            var promise = _state == PromiseState.fulfilled ? thenPromise : catchPromise;

            if (promise != null)
                promise.run(this, _result);

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
            _result = new Array(_expectedNumberOfCompletions);

            for (var i = 0; i < promises.Length; i++)
                promises[i].run();
        }

        internal override void run(Promise sender, JSValue result)
        {
            lock (_result)
            {
                if (_state == PromiseState.pending)
                {
                    if (sender.State == PromiseState.rejected)
                    {
                        _result = result;
                        _state = PromiseState.rejected;
                        _complited = true;
                        InvokeComplition();
                    }
                    else
                    {
                        (_result as Array)[System.Array.IndexOf(_promises, sender)] = result;
                        if (--_expectedNumberOfCompletions == 0)
                        {
                            _state = PromiseState.fulfilled;
                            _complited = true;
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
            if (!_complited)
            {
                _complited = true;
                _result = result;
                _state = sender.State;
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
