using System;
using System.Collections;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.Extensions
{
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

        private sealed class EnumeratorResult : IIteratorResult
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

        private sealed class EnumeratorToIteratorWrapper : IIterator
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

        private sealed class EnumerableToIterableWrapper : IIterable
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

        public static IIterable AsIterable(this IEnumerable enumerable)
        {
            return new EnumerableToIterableWrapper(enumerable);
        }

        public static IIterator AsIterator(this IEnumerator enumerator)
        {
            return new EnumeratorToIteratorWrapper(enumerator);
        }

        private sealed class IteratorItemDecorator : IIteratorResult
        {
            private JSValue result;

            [Hidden]
            public IteratorItemDecorator(JSValue result)
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

        private sealed class IteratorDecorator : IIterator
        {
            private JSValue iterator;

            [Hidden]
            public IteratorDecorator(JSValue iterator)
            {
                this.iterator = iterator;
            }

            public IIteratorResult next(Arguments arguments = null)
            {
                var result = iterator["next"].As<Function>().Call(iterator, arguments);
                return new IteratorItemDecorator(result);
            }

            public IIteratorResult @return()
            {
                var result = iterator["return"].As<Function>().Call(iterator, null);
                return new IteratorItemDecorator(result);
            }

            public IIteratorResult @throw(Arguments arguments = null)
            {
                var result = iterator["throw"].As<Function>().Call(iterator, null);
                return new IteratorItemDecorator(result);
            }
        }

        private sealed class IterableDecorator : IIterable
        {
            private JSValue source;

            [Hidden]
            public IterableDecorator(JSValue source)
            {
                this.source = source;
            }

            public IIterator iterator()
            {
                var iteratorFunction = source.GetMember(Symbol.iterator, false, MemberScope.Сommon);
                if (iteratorFunction.valueType != JSValueType.Function)
                    return null;
                var iterator = iteratorFunction.As<Function>().Call(source, null);
                if (iterator == null)
                    return null;
                return new IteratorDecorator(iterator);
            }
        }

        public static IIterable AsIterable(this JSValue source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return source.Value as IIterable ?? new IterableDecorator(source);
        }
    }
}
