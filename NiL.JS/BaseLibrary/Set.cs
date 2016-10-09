using System.Collections.Generic;
using System.Linq;
using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;

namespace NiL.JS.BaseLibrary
{
    [RequireNewKeyword]
    public sealed class Set : IIterable
    {
        private HashSet<object> _storage;

        public int size
        {
            get
            {
                return _storage.Count;
            }
        }

        public Set()
        {
            _storage = new HashSet<object>();
        }

        public Set(IIterable iterable)
            : this()
        {
            if (iterable == null)
                return;

            foreach (var value in iterable.AsEnumerable())
            {                
                _storage.Add(value.Value as JSValue ?? value);
            }
        }

        public Set add(object item)
        {
            _storage.Add(item);

            return this;
        }

        public void clear()
        {
            _storage.Clear();
        }

        public bool delete(object key)
        {
            return _storage.Remove(key);
        }

        public bool has(object key)
        {
            return _storage.Contains(key);
        }

        public void forEach(Function callback, JSValue thisArg)
        {
            foreach (var item in _storage)
            {
                var args = new Arguments { item, null, this };
                args[1] = args[0];
                callback.Call(thisArg, args);
            }
        }

        public IIterator keys()
        {
            return _storage.AsIterable().iterator();
        }

        public IIterator values()
        {
            return _storage.AsIterable().iterator();
        }

        public IIterator iterator()
        {
            return _storage
                .GetEnumerator()
                .AsIterator();
        }
    }
}
