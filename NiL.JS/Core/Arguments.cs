using System;
using System.Collections;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    [Prototype(typeof(JSObject), true)]
    public sealed class Arguments : JSObject, IEnumerable
    {
        private sealed class _LengthContainer : JSValue
        {
            private readonly Arguments owner;

            public _LengthContainer(Arguments owner)
            {
                this.owner = owner;
            }

            public override void Assign(JSValue value)
            {
                base.Assign(value);
                owner.length = Tools.JSObjectToInt32(value);
            }
        }

        private JSValue a0;
        private JSValue a1;
        private JSValue a2;
        private JSValue a3;
        private JSValue a4;
        internal JSValue callee;
        internal JSValue caller;

        private _LengthContainer _lengthContainer;
        internal int length;
        internal bool suppressClone;

        public int Length
        {
            get { return length; }
        }

        public JSValue this[int index]
        {
            get
            {
                JSValue res = null;
                switch (index)
                {
                    case 0:
                        res = a0;
                        break;
                    case 1:
                        res = a1;
                        break;
                    case 2:
                        res = a2;
                        break;
                    case 3:
                        res = a3;
                        break;
                    case 4:
                        res = a4;
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
                        a0 = value;
                        break;
                    case 1:
                        a1 = value;
                        break;
                    case 2:
                        a2 = value;
                        break;
                    case 3:
                        a3 = value;
                        break;
                    case 4:
                        a4 = value;
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
                caller = callerContext._strict
                    && callerContext._owner != null
                    && callerContext._owner._functionDefinition._body._strict ? Function.propertiesDummySM : callerContext._owner;

                _objectPrototype = callerContext.GlobalContext._GlobalPrototype;
            }

            suppressClone = true;
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
            this[length++] = arg;
        }

        public void Add(object value)
        {
            this[length++] = Marshal(value);
        }

        protected internal override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            if (forWrite)
                cloneValues();

            if (memberScope < PropertyScope.Super && key._valueType != JSValueType.Symbol)
            {
                forWrite &= (_attributes & JSValueAttributesInternal.Immutable) == 0;
                if (key._valueType == JSValueType.Integer)
                {
                    switch (key._iValue)
                    {
                        case 0:
                            return (a0 ?? (!forWrite ? notExists : (a0 = new JSValue() { _valueType = JSValueType.NotExistsInObject })));
                        case 1:
                            return (a1 ?? (!forWrite ? notExists : (a1 = new JSValue() { _valueType = JSValueType.NotExistsInObject })));
                        case 2:
                            return (a2 ?? (!forWrite ? notExists : (a2 = new JSValue() { _valueType = JSValueType.NotExistsInObject })));
                        case 3:
                            return (a3 ?? (!forWrite ? notExists : (a3 = new JSValue() { _valueType = JSValueType.NotExistsInObject })));
                        case 4:
                            return (a4 ?? (!forWrite ? notExists : (a4 = new JSValue() { _valueType = JSValueType.NotExistsInObject })));
                    }
                }
                switch (key.ToString())
                {
                    case "0":
                        return (a0 ?? (!forWrite ? notExists : (a0 = new JSValue() { _valueType = JSValueType.NotExistsInObject })));
                    case "1":
                        return (a1 ?? (!forWrite ? notExists : (a1 = new JSValue() { _valueType = JSValueType.NotExistsInObject })));
                    case "2":
                        return (a2 ?? (!forWrite ? notExists : (a2 = new JSValue() { _valueType = JSValueType.NotExistsInObject })));
                    case "3":
                        return (a3 ?? (!forWrite ? notExists : (a3 = new JSValue() { _valueType = JSValueType.NotExistsInObject })));
                    case "4":
                        return (a4 ?? (!forWrite ? notExists : (a4 = new JSValue() { _valueType = JSValueType.NotExistsInObject })));
                    case "length":
                        {
                            if (_lengthContainer == null)
                                _lengthContainer = new _LengthContainer(this)
                                {
                                    _valueType = JSValueType.Integer,
                                    _iValue = length,
                                    _attributes = JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.Reassign
                                };
                            return _lengthContainer;
                        }
                    case "callee":
                        {
                            if (callee == null)
                                callee = NotExistsInObject;

                            if (forWrite && (callee._attributes & JSValueAttributesInternal.SystemObject) != 0)
                            {
                                callee = callee.CloneImpl(false);
                                callee._attributes = JSValueAttributesInternal.DoNotEnumerate;
                            }
                            return callee;
                        }
                    case "caller":
                        {
                            if (caller == null)
                                caller = NotExistsInObject;

                            if (forWrite && (caller._attributes & JSValueAttributesInternal.SystemObject) != 0)
                            {
                                caller = caller.CloneImpl(false);
                                callee._attributes = JSValueAttributesInternal.DoNotEnumerate;
                            }
                            return caller;
                        }
                }
            }

            return base.GetProperty(key, forWrite, memberScope);
        }

        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnum, EnumerationMode enumeratorMode)
        {
            cloneValues();

            if (a0 != null && a0.Exists && (!hideNonEnum || (a0._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                yield return new KeyValuePair<string, JSValue>("0", a0);

            if (a1 != null && a1.Exists && (!hideNonEnum || (a1._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                yield return new KeyValuePair<string, JSValue>("1", a1);

            if (a2 != null && a2.Exists && (!hideNonEnum || (a2._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                yield return new KeyValuePair<string, JSValue>("2", a2);

            if (a3 != null && a3.Exists && (!hideNonEnum || (a3._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                yield return new KeyValuePair<string, JSValue>("3", a3);

            if (a4 != null && a4.Exists && (!hideNonEnum || (a4._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                yield return new KeyValuePair<string, JSValue>("4", a4);

            if (callee != null && callee.Exists && (!hideNonEnum || (callee._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                yield return new KeyValuePair<string, JSValue>("callee", callee);

            if (caller != null && caller.Exists && (!hideNonEnum || (caller._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                yield return new KeyValuePair<string, JSValue>("caller", caller);

            if (_lengthContainer != null && _lengthContainer.Exists && (!hideNonEnum || (_lengthContainer._attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                yield return new KeyValuePair<string, JSValue>("length", _lengthContainer);

            var be = base.GetEnumerator(hideNonEnum, enumeratorMode);
            while (be.MoveNext())
                yield return be.Current;
        }

        private void cloneValues()
        {
            if (suppressClone)
                return;
            suppressClone = true;

            var mask = JSValueAttributesInternal.ReadOnly
                    | JSValueAttributesInternal.SystemObject
                    | JSValueAttributesInternal.Temporary
                    | JSValueAttributesInternal.Reassign
                    | JSValueAttributesInternal.ProxyPrototype
                    | JSValueAttributesInternal.DoNotEnumerate
                    | JSValueAttributesInternal.NonConfigurable
                    | JSValueAttributesInternal.DoNotDelete;

            for (var i = 0; i < length; i++)
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
                        return a0 == null || ((a0._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a0 = null) == null;
                    case 1:
                        return a1 == null || ((a1._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a1 = null) == null;
                    case 2:
                        return a2 == null || ((a2._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a2 = null) == null;
                    case 3:
                        return a3 == null || ((a3._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a3 = null) == null;
                    case 4:
                        return a4 == null || ((a4._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a4 = null) == null;
                        //case 5:
                        //    return a5 == null || ((a5.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a5 = null) == null;
                        //case 6:
                        //    return a6 == null || ((a6.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a6 = null) == null;
                        //case 7:
                        //    return a7 == null || ((a7.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a7 = null) == null;
                }
            }
            switch (name.ToString())
            {
                case "0":
                    return a0 == null || ((a0._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a0 = null) == null;
                case "1":
                    return a1 == null || ((a1._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a1 = null) == null;
                case "2":
                    return a2 == null || ((a2._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a2 = null) == null;
                case "3":
                    return a3 == null || ((a3._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a3 = null) == null;
                case "4":
                    return a4 == null || ((a4._attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a4 = null) == null;
                    //case "5":
                    //    return a5 == null || ((a5.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a5 = null) == null;
                    //case "6":
                    //    return a6 == null || ((a6.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a6 = null) == null;
                    //case "7":
                    //    return a7 == null || ((a7.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a7 = null) == null;
            }
            return base.DeleteProperty(name);
        }

        internal void Reset()
        {
            _fields = null;
            length = 0;
            a0 = null;
            a1 = null;
            a2 = null;
            a3 = null;
            a4 = null;
            //a5 = null;
            //a6 = null;
            //a7 = null;
            callee = null;
            caller = null;
            _objectPrototype = null;
            _lengthContainer = null;
            _valueType = JSValueType.Object;
            _oValue = this;
            _attributes = JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.SystemObject;
        }
    }
}
