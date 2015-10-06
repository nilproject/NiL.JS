using System;
using System.Collections.Generic;
using System.Collections;

namespace NiL.JS.Core
{
#if !PORTABLE
    [Serializable]
#endif
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

        internal JSValue a0;
        internal JSValue a1;
        internal JSValue a2;
        internal JSValue a3;
        internal JSValue a4;
        //internal JSObject a5;
        //internal JSObject a6;
        //internal JSObject a7;
        internal JSValue callee;
        internal JSValue caller;
        private _LengthContainer _length;
        internal int length;

        public int Length
        {
            get { return length; }
        }

        public override JSValue this[string name]
        {
            get
            {
                return base[name];
            }
            set
            {
                switch (name)
                {
                    case "callee":
                        callee = value;
                        return;
                    case "caller":
                        caller = value;
                        return;
                }
                base[name] = value;
            }
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
                        if (fields == null)
                            fields = createFields();
                        fields[index.ToString()] = value;
                        break;
                }

            }
        }

        public Arguments()
            : base()
        {
            valueType = JSValueType.Object;
            oValue = this;
            attributes = JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnum | JSValueAttributesInternal.SystemObject;
        }

        public void Add(JSValue arg)
        {
            this[length++] = arg;
        }

        internal override JSObject GetDefaultPrototype()
        {
            return GlobalPrototype ?? Null;
        }

        protected internal override JSValue GetMember(JSValue name, bool createMember, bool own)
        {
            createMember &= (attributes & JSValueAttributesInternal.Immutable) == 0;
            if (name.valueType == JSValueType.Int)
            {
                switch (name.iValue)
                {
                    case 0:
                        return (a0 ?? (!createMember ? notExists : (a0 = new JSValue() { valueType = JSValueType.NotExistsInObject })));
                    case 1:
                        return (a1 ?? (!createMember ? notExists : (a1 = new JSValue() { valueType = JSValueType.NotExistsInObject })));
                    case 2:
                        return (a2 ?? (!createMember ? notExists : (a2 = new JSValue() { valueType = JSValueType.NotExistsInObject })));
                    case 3:
                        return (a3 ?? (!createMember ? notExists : (a3 = new JSValue() { valueType = JSValueType.NotExistsInObject })));
                    case 4:
                        return (a4 ?? (!createMember ? notExists : (a4 = new JSValue() { valueType = JSValueType.NotExistsInObject })));
                    //case 5:
                    //    return (a5 ?? (!createMember ? notExists : (a5 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                    //case 6:
                    //    return (a6 ?? (!createMember ? notExists : (a6 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                    //case 7:
                    //    return (a7 ?? (!createMember ? notExists : (a7 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                }
            }
            switch (name.ToString())
            {
                case "0":
                    return (a0 ?? (!createMember ? notExists : (a0 = new JSValue() { valueType = JSValueType.NotExistsInObject })));
                case "1":
                    return (a1 ?? (!createMember ? notExists : (a1 = new JSValue() { valueType = JSValueType.NotExistsInObject })));
                case "2":
                    return (a2 ?? (!createMember ? notExists : (a2 = new JSValue() { valueType = JSValueType.NotExistsInObject })));
                case "3":
                    return (a3 ?? (!createMember ? notExists : (a3 = new JSValue() { valueType = JSValueType.NotExistsInObject })));
                case "4":
                    return (a4 ?? (!createMember ? notExists : (a4 = new JSValue() { valueType = JSValueType.NotExistsInObject })));
                //case "5":
                //    return (a5 ?? (!createMember ? notExists : (a5 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                //case "6":
                //    return (a6 ?? (!createMember ? notExists : (a6 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                //case "7":
                //    return (a7 ?? (!createMember ? notExists : (a7 = new JSObject() { valueType = JSObjectType.NotExistsInObject })));
                case "length":
                    {
                        if (_length == null)
                            _length = new _LengthContainer(this)
                            {
                                valueType = JSValueType.Int,
                                iValue = length,
                                attributes = JSValueAttributesInternal.DoNotEnum | JSValueAttributesInternal.Reassign
                            };
                        return _length;
                    }
                case "callee":
                    {
                        if (callee == null)
                            callee = NotExistsInObject;
                        if (createMember && (callee.attributes & JSValueAttributesInternal.SystemObject) != 0)
                        {
                            callee = callee.CloneImpl();
                            callee.attributes = JSValueAttributesInternal.DoNotEnum;
                        }
                        return callee;
                    }
                case "caller":
                    {
                        if (caller == null)
                            caller = NotExistsInObject;
                        if (createMember && (caller.attributes & JSValueAttributesInternal.SystemObject) != 0)
                        {
                            caller = caller.CloneImpl();
                            callee.attributes = JSValueAttributesInternal.DoNotEnum;
                        }
                        return caller;
                    }
            }
            return base.GetMember(name, createMember, own);
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            if (a0 != null && a0.IsExists && (!hideNonEnum || (a0.attributes & JSValueAttributesInternal.DoNotEnum) == 0))
                yield return "0";
            if (a1 != null && a1.IsExists && (!hideNonEnum || (a1.attributes & JSValueAttributesInternal.DoNotEnum) == 0))
                yield return "1";
            if (a2 != null && a2.IsExists && (!hideNonEnum || (a2.attributes & JSValueAttributesInternal.DoNotEnum) == 0))
                yield return "2";
            if (a3 != null && a3.IsExists && (!hideNonEnum || (a3.attributes & JSValueAttributesInternal.DoNotEnum) == 0))
                yield return "3";
            if (a4 != null && a4.IsExists && (!hideNonEnum || (a4.attributes & JSValueAttributesInternal.DoNotEnum) == 0))
                yield return "4";
            //if (a5 != null && a5.IsExists && (!hideNonEnum || (a5.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "5";
            //if (a6 != null && a6.IsExists && (!hideNonEnum || (a6.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "6";
            //if (a7 != null && a7.IsExists && (!hideNonEnum || (a7.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "7";
            //if (a8 != null && a8.isExists && (!hideNonEnum || (a8.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "8";
            //if (a9 != null && a9.isExists && (!hideNonEnum || (a9.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "9";
            //if (a10 != null && a10.isExists && (!hideNonEnum || (a10.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "10";
            //if (a11 != null && a11.isExists && (!hideNonEnum || (a11.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "11";
            //if (a12 != null && a12.isExists && (!hideNonEnum || (a12.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "12";
            //if (a13 != null && a13.isExists && (!hideNonEnum || (a13.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "13";
            //if (a14 != null && a14.isExists && (!hideNonEnum || (a14.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "14";
            //if (a15 != null && a15.isExists && (!hideNonEnum || (a15.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
            //    yield return "15";
            if (callee != null && callee.IsExists && (!hideNonEnum || (callee.attributes & JSValueAttributesInternal.DoNotEnum) == 0))
                yield return "callee";
            if (caller != null && callee.IsExists && (!hideNonEnum || (caller.attributes & JSValueAttributesInternal.DoNotEnum) == 0))
                yield return "caller";
            if (_length != null && _length.IsExists && (!hideNonEnum || (_length.attributes & JSValueAttributesInternal.DoNotEnum) == 0))
                yield return "length";
            var be = getBaseEnumerator(hideNonEnum);
            while (be.MoveNext())
                yield return be.Current;
        }

        private IEnumerator<string> getBaseEnumerator(bool hideNonEnum)
        {
            return base.GetEnumeratorImpl(hideNonEnum);
        }

        protected internal override bool DeleteMember(JSValue name)
        {
            if (name.valueType == JSValueType.Int)
            {
                switch (name.iValue)
                {
                    case 0:
                        return a0 == null || ((a0.attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a0 = null) == null;
                    case 1:
                        return a1 == null || ((a1.attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a1 = null) == null;
                    case 2:
                        return a2 == null || ((a2.attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a2 = null) == null;
                    case 3:
                        return a3 == null || ((a3.attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a3 = null) == null;
                    case 4:
                        return a4 == null || ((a4.attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a4 = null) == null;
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
                    return a0 == null || ((a0.attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a0 = null) == null;
                case "1":
                    return a1 == null || ((a1.attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a1 = null) == null;
                case "2":
                    return a2 == null || ((a2.attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a2 = null) == null;
                case "3":
                    return a3 == null || ((a3.attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a3 = null) == null;
                case "4":
                    return a4 == null || ((a4.attributes & JSValueAttributesInternal.DoNotDelete) == 0) && (a4 = null) == null;
                //case "5":
                //    return a5 == null || ((a5.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a5 = null) == null;
                //case "6":
                //    return a6 == null || ((a6.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a6 = null) == null;
                //case "7":
                //    return a7 == null || ((a7.attributes & JSObjectAttributesInternal.DoNotDelete) == 0) && (a7 = null) == null;
            }
            return base.DeleteMember(name);
        }

        internal void Reset()
        {
            fields = null;
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
            __prototype = null;
            _length = null;
            valueType = JSValueType.Object;
            oValue = this;
            attributes = JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnum | JSValueAttributesInternal.SystemObject;
        }
    }
}
