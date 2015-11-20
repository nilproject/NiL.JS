using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;

namespace NiL.JS.Core.Functions
{
    /// <summary>
    /// Реализует поведение функции Function.prototype.bind.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class BindedFunction : Function
    {
        private Function proto;
        private JSValue _thisBind;
        private Arguments bindedArguments;

        public override JSValue caller
        {
            get
            {
                ThrowTypeError();
                return null;
            }
            set
            {
                ThrowTypeError();
            }
        }

        public override JSValue arguments
        {
            get
            {
                ThrowTypeError();
                return null;
            }
            set
            {
                ThrowTypeError();
            }
        }

        public override JSValue prototype
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        public BindedFunction(Function proto, Arguments args)
            : base(null, proto.creator)
        {
            if (_length == null)
                _length = new Number(0);
            _length.iValue = proto.length.iValue;
            this.proto = proto;
            this._thisBind = args[0];
            this.bindedArguments = args;
            if (args.length > 0)
            {
                args.length--;
                for (var i = 0; i < args.length; i++)
                    args[i] = args[i + 1];
                _length.iValue -= args.length;
                if (_length.iValue < 0)
                    _length.iValue = 0;
                args[args.length] = null;
                if (args.length == 0)
                    bindedArguments = null;
            }
            else
                bindedArguments = null;

            RequireNewKeywordLevel = proto.RequireNewKeywordLevel;
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
        {
            if (bindedArguments != null)
            {
                if (arguments == null)
                    arguments = new Arguments();
                arguments.length += bindedArguments.length;
                for (var i = arguments.length; i-- > bindedArguments.length; )
                    arguments[i] = arguments[i - bindedArguments.length];
                for (var i = bindedArguments.length; i-- > 0; )
                    arguments[i] = bindedArguments[i];
            }
            if ((construct || _thisBind == null || _thisBind.IsNull || !_thisBind.Defined) && (targetObject != null && targetObject.Defined))
                return proto.Invoke(construct, targetObject, arguments);
            return proto.Invoke(construct, _thisBind, arguments);
        }

        protected internal override JSValue ConstructObject()
        {
            return proto.ConstructObject();
        }

        [Hidden]
        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnumerable, EnumerationMode enumeratorMode)
        {
            return proto.GetEnumerator(hideNonEnumerable, enumeratorMode);
        }

        [Hidden]
        protected internal override JSValue GetMember(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            return proto.GetMember(key, forWrite, memberScope);
        }

        internal override JSObject GetDefaultPrototype()
        {
            return TypeProxy.GetPrototype(typeof(Function));
        }
    }
}
