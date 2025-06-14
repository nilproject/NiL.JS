using System;
using System.Collections;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.Extensions;

internal sealed class EnumeratorResult : HiddenBase, IIteratorResult
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
        _value = value;
        _done = done;
    }
}

internal sealed class EnumeratorToIteratorWrapper : HiddenBase, IIterator, IIterable
{
    private IEnumerator _enumerator;
    private GlobalContext _context;

    [Hidden]
    public EnumeratorToIteratorWrapper(IEnumerator enumerator)
    {
        _enumerator = enumerator;
        _context = Context.CurrentGlobalContext;
    }

    public IIterator iterator() => this;

    public IIteratorResult next(Arguments arguments = null)
    {
        var read = _enumerator.MoveNext();
        return new EnumeratorResult(
            !read,
            _context.ProxyValue(read ? _enumerator.Current : null));
    }

    public IIteratorResult @return() => new EnumeratorResult(true, null);

    public IIteratorResult @throw(Arguments arguments = null) => new EnumeratorResult(true, null);
}

internal sealed class EnumerableToIterableWrapper : HiddenBase, IIterable
{
    private IEnumerable enumerable;

    [Hidden]
    public EnumerableToIterableWrapper(IEnumerable enumerable)
    {
        this.enumerable = enumerable;
    }

    public IIterator iterator() => new EnumeratorToIteratorWrapper(enumerable.GetEnumerator());
}

internal sealed class IteratorItemAdapter : HiddenBase, IIteratorResult
{
    private JSValue result;

    [Hidden]
    public IteratorItemAdapter(JSValue result)
    {
        this.result = result;
    }

    public JSValue value => Tools.GetPropertyOrValue(result["value"], result);

    public bool done => (bool)Tools.GetPropertyOrValue(result["done"], result);
}

internal sealed class IteratorAdapter : HiddenBase, IIterator
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

internal sealed class IterableAdapter : HiddenBase, IIterable
{
    private readonly JSValue _iteratorFunction;
    private JSValue _source;

    [Hidden]
    public IterableAdapter(JSValue source, JSValue iteratorFunction)
    {
        _source = source.IsBox ? source._oValue as JSValue : source;
        _iteratorFunction = iteratorFunction;
    }

    public IIterator iterator()
    {
        var iterator = _iteratorFunction.As<Function>().Call(_source, null);
        if (iterator == null)
            return null;

        return new IteratorAdapter(iterator);
    }
}

public static class IterationProtocolExtensions
{
    public static IEnumerator<JSValue> GetEnumerator(this IIterable iterableObject)
    {
        var iterator = iterableObject.iterator();
        if (iterator is null)
            ExceptionHelper.Throw(new TypeError("source is not iterable"));

        var item = iterator.next();
        while (!item.done)
        {
            yield return item.value;
            item = iterator.next();
        }
    }

    public static IEnumerable<JSValue> ToEnumerable(this IIterable iterableObject)
    {
        var iterator = iterableObject.iterator();
        if (iterator is null)
            ExceptionHelper.Throw(new TypeError("source is not iterable"));

        var item = iterator.next();
        while (!item.done)
        {
            yield return item.value;
            item = iterator.next();
        }
    }

    public static IEnumerable<JSValue> AsEnumerable(this IIterable iterableObject)
    {
        var iterator = iterableObject.iterator();
        if (iterator == null)
            yield break;

        var item = iterator.next();
        while (!item.done)
        {
            yield return item.value;
            item = iterator.next();
        }
    }

    public static IIterable ToIterable(this JSValue source)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        if (source.Value is IIterable iterable)
            return iterable;

        var iteratorFunction = source.GetProperty(Symbol.iterator, false, PropertyScope.Common);
        if (iteratorFunction._valueType != JSValueType.Function)
            ExceptionHelper.Throw(new TypeError("source is not iterable"));

        return new IterableAdapter(source, iteratorFunction);
    }

    public static IIterable AsIterable(this JSValue source)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        return source.Value as IIterable ?? new IterableAdapter(source, source.GetProperty(Symbol.iterator, false, PropertyScope.Common));
    }

    public static bool IsIterable(this JSValue source)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        return source.Value is IIterable || source.GetProperty(Symbol.iterator, false, PropertyScope.Common)._valueType == JSValueType.Function;
    }

    public static IIterable AsIterable(this IEnumerable enumerable) => new EnumerableToIterableWrapper(enumerable);

    public static IIterator AsIterator(this IEnumerator enumerator) => new EnumeratorToIteratorWrapper(enumerator);
}
