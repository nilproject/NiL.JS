using System;
using System.Collections;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS
{
    public sealed class ExportTable : IEnumerable<KeyValuePair<string, JSValue>>
    {
        private Dictionary<string, JSValue> _items = new Dictionary<string, JSValue>();
        private readonly Context _context;

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
             set
            {
                _items[key] = value;
            }
        }


        public ExportTable(Context moduleContext)
        {
            _context = moduleContext;
        }

        /// <summary>
        /// Add object constructor to export 
        /// </summary>
        /// <remarks>We dont recommend to use, because api can change and its can be broken in future. Use indexers</remarks>
        /// <param name="type">Class</param>
        /// <param name="name"></param>
        public void DefineConstructor(Type type, string name)
        {
            var ctor = _context.GlobalContext.GetConstructor(type);
            _items.Add(name, ctor);
            ctor._attributes |= JSValueAttributesInternal.DoNotEnumerate;
        }

        /// <summary>
        /// Add object to exports 
        /// </summary>
        /// <remarks>We dont recommend to use, because api can change and its can be broken in future. Use indexers</remarks>
        /// <param name="name">Name of variable in export</param>
        /// <param name="deletable"></param>
        public JSValue DefineVariable(string name, bool deletable)
        {
            var defineVariable = new JSValue()
            {
                _valueType = JSValueType.Undefined
            };
            _items.Add(name,defineVariable);
            return defineVariable;
        }

        /// <summary>
        /// Add readonly object to exports 
        /// </summary>
        /// <remarks>We dont recommend to use, because api can change and its can be broken in future. Use indexers</remarks>
        /// <param name="name">Name of variable in export</param>
        /// <param name="value">JSValue (function, constructor, object, etc)</param>
        /// <param name="deletable"></param>
        public JSValue DefineConstant(string name, JSValue value, bool deletable = false)
        {
            var v = DefineVariable(name, deletable);
            v.Assign(value);
            v._attributes |= JSValueAttributesInternal.ReadOnly;
            return v;
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
