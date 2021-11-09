using System;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core
{
    public enum PromiseState
    {
        Pending,
        Fulfilled,
        Rejected
    }

    public interface IPromiseLike
    {
        [Hidden]
        PromiseState State { get; }

#pragma warning disable IDE1006

        IPromiseLike @catch(Function onRejection);

        IPromiseLike then(Function onFulfilment, Function onRejection);

        [Hidden]
        IPromiseLike then(Func<JSValue, JSValue> onFulfilment, Func<JSValue, JSValue> onRejection);

#pragma warning restore IDE1006
    }
}
