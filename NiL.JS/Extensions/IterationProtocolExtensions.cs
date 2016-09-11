using System;
using System.Collections;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.Extensions
{
    internal sealed class EnumeratorResult : IIteratorResult
    {
        private JSValue _value;
        private bool _done;

        public JSValue value
        {
            get { return _value; }
        }

        public bool done
        {
            get { return _done; }
        }

        [Hidden]
        public EnumeratorResult(bool done, JSValue value)
        {
            this._value = value;
            this._done = done;
        }
    }

    internal sealed class EnumeratorToIteratorWrapper : IIterator
    {
        private IEnumerator enumerator;

        public EnumeratorToIteratorWrapper(IEnumerator enumerator)
        {
            this.enumerator = enumerator;
        }

        public IIteratorResult next(Arguments arguments = null)
        {
            return new EnumeratorResult(!enumerator.MoveNext(), TypeProxy.Proxy(enumerator.Current));
        }

        public IIteratorResult @return()
        {
            return new EnumeratorResult(true, null);
        }

        public IIteratorResult @throw(Arguments arguments = null)
        {
            return new EnumeratorResult(true, null);
        }
    }

    internal sealed class EnumerableToIterableWrapper : IIterable
    {
        private IEnumerable enumerable;

        public EnumerableToIterableWrapper(IEnumerable enumerable)
        {
            this.enumerable = enumerable;
        }

        public IIterator iterator()
        {
            return new EnumeratorToIteratorWrapper(enumerable.GetEnumerator());
        }
    }

    internal sealed class IteratorItemAdapter : IIteratorResult
    {
        private JSValue result;

        [Hidden]
        public IteratorItemAdapter(JSValue result)
        {
            this.result = result;
        }

        public JSValue value
        {
            get
            {
                return Tools.InvokeGetter(result["value"], result);
            }
        }

        public bool done
        {
            get
            {
                return (bool)Tools.InvokeGetter(result["done"], result);
            }
        }
    }

    internal sealed class IteratorAdapter : IIterator
    {
        private JSValue iterator;

        [Hidden]
        public IteratorAdapter(JSValue iterator)
        {
            this.iterator = iterator;
        }

        public IIteratorResult next(Arguments arguments = null)
        {
            var result = iterator["next"].As<Function>().Call(iterator, arguments);
            return new IteratorItemAdapter(result);
        }

        public IIteratorResult @return()
        {
            var result = iterator["return"].As<Function>().Call(iterator, null);
            return new IteratorItemAdapter(result);
        }

        public IIteratorResult @throw(Arguments arguments = null)
        {
            var result = iterator["throw"].As<Function>().Call(iterator, null);
            return new IteratorItemAdapter(result);
        }
    }

    internal sealed class IterableAdapter : IIterable
    {
        private JSValue source;

        [Hidden]
        public IterableAdapter(JSValue source)
        {
            this.source = source.IsBox ? source._oValue as JSValue : source;
        }

        public IIterator iterator()
        {
            var iteratorFunction = source.GetProperty(Symbol.iterator, false, PropertyScope.Сommon);
            if (iteratorFunction._valueType != JSValueType.Function)
                return null;
            var iterator = iteratorFunction.As<Function>().Call(source, null);
            if (iterator == null)
                return null;
            return new IteratorAdapter(iterator);
        }
    }

    public static class IterationProtocolExtensions
    {
        public static IEnumerable<JSValue> AsEnumerable(this IIterable iterableObject)
        {
            var iterator = iterableObject.iterator();
            var item = iterator.next();
            while (!item.done)
            {
                yield return item.value;
                item = iterator.next();
            }
        }
        
        public static IIterable AsIterable(this JSValue source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return source.Value as IIterable ?? new IterableAdapter(source);
        }

        public static IIterable AsIterable(this IEnumerable enumerable)
        {
            return new EnumerableToIterableWrapper(enumerable);
        }

        public static IIterator AsIterator(this IEnumerator enumerator)
        {
            return new EnumeratorToIteratorWrapper(enumerator);
        }
    }
}
