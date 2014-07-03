using System;
using System.Collections.Generic;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    /// <summary>
    /// Реализует поведение функции Function.prototype.bind.
    /// </summary>
    [Serializable]
    internal sealed class BindedFunction : Function
    {
        private Function proto;
        private JSObject thisBind;

        public BindedFunction(JSObject thisBind, Function proto)
        {
            if (_length == null)
                _length = 0;
            _length.iValue = proto.length.iValue;
            this.proto = proto;
            this.thisBind = thisBind;
        }

        [Hidden]
        public override string name
        {
            [Hidden]
            get
            {
                return proto.name;
            }
        }

        [Hidden]
        public override FunctionType Type
        {
            [Hidden]
            get
            {
                return proto.Type;
            }
        }

        [Hidden]
        public override JSObject Invoke(JSObject thisBind, JSObject args)
        {
            return proto.Invoke(this.thisBind, args);
        }

        [DoNotEnumerate]
        protected internal override IEnumerator<string> GetEnumeratorImpl(bool pdef)
        {
            return proto.GetEnumeratorImpl(pdef);
        }

        [DoNotEnumerate]
        public override JSObject hasOwnProperty(JSObject args)
        {
            return proto.hasOwnProperty(args);
        }

        [Hidden]
        protected internal override JSObject GetMember(JSObject name, bool create, bool own)
        {
            return proto.GetMember(name, create, own);
        }
    }
}
