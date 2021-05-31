using System;
using System.Collections;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS
{
    public sealed class ExportTable : IEnumerable<KeyValuePair<string, JSValue>>
    {
        private Dictionary<string, JSValue> _items = new Dictionary<string, JSValue>();

        public JSValue this[string key]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(key) || !Parser.ValidateName(key, 0, false, true, false))
                    ExceptionHelper.Throw(new ArgumentException());

                var result = JSValue.undefined;

                if (!_items.TryGetValue(key, out result))
                    return JSValue.undefined;

                return result;
            }
            internal set
            {
                _items[key] = value;
            }
        }

        public int Count => _items.Count;

        public JSValue Default
        {
            get
            {
                var result = JSValue.undefined;

                if (!_items.TryGetValue("", out result))
                    return JSValue.undefined;

                return result;
            }
        }

        public JSObject CreateExportList()
        {
            var result = JSObject.CreateObject(true);

            foreach(var item in _items)
            {
                if (item.Key != "")
                    result._fields[item.Key] = item.Value;
                else
                    result._fields["default"] = item.Value;
            }

            return result;
        }

        public IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
