using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;

namespace NiL.JS.Core
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    [Prototype(typeof(JSObject), true)]
    public sealed class Arguments : JSObject, IEnumerable, IIterable
    {
        private sealed class _LengthContainer : JSValue
        {
            private readonly Arguments _owner;

            public _LengthContainer(Arguments owner)
            {
                this._owner = owner;
            }

            public override void Assign(JSValue value)
            {
                base.Assign(value);
                _owner._iValue = Tools.JSObjectToInt32(value);
            }
        }

        private JSValue _a0;
        private JSValue _a1;
        private JSValue _a2;
        private JSValue _a3;
        internal JSValue _callee;
        internal JSValue _caller;

        private _LengthContainer _lengthContainer;
        internal bool _suppressClone;

        public int Length
        {
            get => _iValue;
            internal set => _iValue = value;
        }

        public JSValue this[int index]
        {
            get
            {
                JSValue res;
                switch (index)
                {
                    case 0:
                        res = _a0;
                        break;
                    case 1:
                        res = _a1;
                        break;
                    case 2:
                        res = _a2;
                        break;
                    case 3:
                        res = _a3;
                        break;
                    default:
                        return base[index.ToString()];
                }

                if (res == null)
                    return notExists;

                return res;
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _a0 = value;
                        break;
                    case 1:
                        _a1 = value;
                        break;
                    case 2:
                        _a2 = value;
                        break;
                    case 3:
                        _a3 = value;
                        break;
                    default:
                        if (_fields == null)
                            _fields = getFieldsContainer();
                        _fields[index.ToString()] = value;
                        break;
                }

            }
        }

        internal Arguments(Context callerContext)
            : this()
        {
            if (callerContext != null)
            {
                _caller = callerContext._strict
                    && callerContext._owner != null
                    && callerContext._owner._functionDefinition._body._strict ? Function.propertiesDummySM : callerContext._owner;

                _objectPrototype = callerContext.GlobalContext._globalPrototype;
            }

            _suppressClone = true;
        }

        public Arguments()
        {
            _valueType = JSValueType.Object;
            _oValue = this;
            _attributes = JSValueAttributesInternal.DoNotDelete
                | JSValueAttributesInternal.DoNotEnumerate
                | JSValueAttributesInternal.SystemObject;
        }

        public void Add(JSValue arg)
        {
            this[_iValue++] = arg;
        }

        public void Add(object value)
        {
            this[_iValue++] = Marshal(value);
        }

        protected internal override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            if (forWrite && !_suppressClone)
                cloneValues();

            if (memberScope < PropertyScope.Super && key._valueType != JSValueType.Symbol)
            {
                forWrite &= (_attributes & JSValueAttributesInternal.Immutable) == 0;
                if (key._valueType == JSValueType.Integer)
                {
                    switch (key._iValue)
                    {
                        case 0:
                            return (_a0 ?? (forWrite ? (_a0 = new JSValue() { _valueType = JSValueType.NotExistsInObject }) : notExists));
                        case 1:
                            return (_a1 ?? (forWrite ? (_a1 = new JSValue() { _valueType = JSValueType.NotExistsInObject }) : notExists));
                        case 2:
                            return (_a2 ?? (forWrite ? (_a2 = new JSValue() { _valueType = JSValueType.NotExistsInObject }) : notExists));
                        case 3:
                            return (_a3 ?? (forWrite ? (_a3 = new JSValue() { _valueType = JSValueType.NotExistsInObject }) : notExists));
                    }
                }
                else
                {
                    switch (key.ToString())
                    {
                        case "0":
                            return (_a0 ?? (forWrite ? (_a0 = new JSValue() { _valueType = JSValueType.NotExistsInObject }) : notExists));
                        case "1":
                            return (_a1 ?? (forWrite ? (_a1 = new JSValue() { _valueType = JSValueType.NotExistsInObject }) : notExists));
                        case "2":
                            return (_a2 ?? (forWrite ? (_a2 = new JSValue() { _valueType = JSValueType.NotExistsInObject }) : notExists));
                        case "3":
                            return (_a3 ?? (forWrite ? (_a3 = new JSValue() { _valueType = JSValueType.NotExistsInObject }) : notExists));
                        case "length":
                        {
                            if (_lengthContainer == null)
                                _lengthContainer = new _LengthContainer(this)
                                {
                                    _valueType = JSValueType.Integer,
                                    _iValue = _iValue,
                                    _attributes = JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.Reassign
                                };
                            return _lengthContainer;
                        }
                        case "callee":
                        {
                            if (_callee == null)
                                _callee = NotExistsInObject;

                            if (forWrite && (_callee._attributes & JSValueAttributesInternal.SystemObject) != 0)
                            {
                                _callee = _callee.CloneImpl(false);
                                _callee._attributes = JSValueAttributesInternal.DoNotEnumerate;
                            }
                            return _callee;
                        }
                        case "caller":
                        {
                            if (_caller == null)
                                _caller = NotExistsInObject;

                            if (forWrite && (_caller._attributes & JSValueAttributesInternal.SystemObject) != 0)
                            {
                                _caller = _caller.CloneImpl(false);
                                _callee._attributes = JSValueAttributesInternal.DoNotEnumerate;
                            }
                            return _caller;
                        }
                    }
                }
            }

            return base.GetProperty(key, forWrite, memberScope);
        }

        public IIterator iterator()
            => this.Select(x => x.Value).GetEnumerator().AsIterator();

        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnum, EnumerationMode enumeratorMode, PropertyScope propertyScope = PropertyScope.Common)
        {
            cloneValues();

            if (propertyScope is PropertyScope.Common or PropertyScope.Own)
            {
                if (_a0 != null && _a0.Exists && (!hideNonEnum || (_a0._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                    yield return new KeyValuePair<string, JSValue>("0", _a0);

                if (_a1 != null && _a1.Exists && (!hideNonEnum || (_a1._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                    yield return new KeyValuePair<string, JSValue>("1", _a1);

                if (_a2 != null && _a2.Exists && (!hideNonEnum || (_a2._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                    yield return new KeyValuePair<string, JSValue>("2", _a2);

                if (_a3 != null && _a3.Exists && (!hideNonEnum || (_a3._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                    yield return new KeyValuePair<string, JSValue>("3", _a3);

                if (_callee != null && _callee.Exists && (!hideNonEnum || (_callee._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                    yield return new KeyValuePair<string, JSValue>("callee", _callee);

                if (_caller != null && _caller.Exists && (!hideNonEnum || (_caller._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                    yield return new KeyValuePair<string, JSValue>("caller", _caller);

                if (_lengthContainer != null && _lengthContainer.Exists && (!hideNonEnum || (_lengthContainer._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                    yield return new KeyValuePair<string, JSValue>("length", _lengthContainer);
            }

            var be = base.GetEnumerator(hideNonEnum, enumeratorMode, propertyScope);
            while (be.MoveNext())
                yield return be.Current;
        }

        private void cloneValues()
        {
            if (_suppressClone)
                return;
            _suppressClone = true;

            var mask = JSValueAttributesInternal.ReadOnly
                    | JSValueAttributesInternal.SystemObject
                    | JSValueAttributesInternal.Temporary
                    | JSValueAttributesInternal.Reassign
                    | JSValueAttributesInternal.ProxyPrototype
                    | JSValueAttributesInternal.DoNotEnumerate
                    | JSValueAttributesInternal.NonConfigurable
                    | JSValueAttributesInternal.DoNotDelete;

            for (var i = 0; i < _iValue; i++)
            {
                if (this[i].Exists)
                    this[i] = this[i].CloneImpl(false, mask);
            }
        }

        protected internal override bool DeleteProperty(JSValue name)
        {
            if (name._valueType == JSValueType.Integer)
            {
                switch (name._iValue)
                {
                    case 0:
                        return _a0 == null || ((_a0._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (_a0 = null) == null;
                    case 1:
                        return _a1 == null || ((_a1._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (_a1 = null) == null;
                    case 2:
                        return _a2 == null || ((_a2._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (_a2 = null) == null;
                    case 3:
                        return _a3 == null || ((_a3._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (_a3 = null) == null;}
            }
            
            switch (name.ToString())
            {
                case "0":
                    return _a0 == null || ((_a0._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (_a0 = null) == null;
                case "1":
                    return _a1 == null || ((_a1._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (_a1 = null) == null;
                case "2":
                    return _a2 == null || ((_a2._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (_a2 = null) == null;
                case "3":
                    return _a3 == null || ((_a3._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (_a3 = null) == null;
            }

            return base.DeleteProperty(name);
        }

        internal void Reset()
        {
            _fields = null;
            _iValue = 0;
            _a0 = null;
            _a1 = null;
            _a2 = null;
            _a3 = null;
            _callee = null;
            _caller = null;
            _objectPrototype = null;
            _lengthContainer = null;
            _valueType = JSValueType.Object;
            _oValue = this;
            _attributes = JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.SystemObject;
        }
    }
}
