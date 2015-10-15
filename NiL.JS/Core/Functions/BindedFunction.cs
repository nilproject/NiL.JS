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
        private JSValue thisBind;
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
            this.thisBind = args[0];
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
        }

        [Hidden]
        public override JSValue Invoke(JSValue thisBind, Arguments args)
        {
            if (bindedArguments != null)
            {
                if (args == null)
                    args = new Arguments();
                args.length += bindedArguments.length;
                for (var i = args.length; i-- > bindedArguments.length; )
                    args[i] = args[i - bindedArguments.length];
                for (var i = bindedArguments.length; i-- > 0; )
                    args[i] = bindedArguments[i];
            }
            if (thisBind != null && thisBind.oValue == typeof(NewOperator) as object)
                return proto.Invoke(thisBind, args);
            return proto.Invoke(this.thisBind, args);
        }

        [Hidden]
        public override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnumerable, EnumerationMode enumeratorMode)
        {
            return proto.GetEnumerator(hideNonEnumerable, enumeratorMode);
        }

        [Hidden]
        protected internal override JSValue GetMember(JSValue key, bool forWrite, bool own)
        {
            return proto.GetMember(key, forWrite, own);
        }

        internal override JSObject GetDefaultPrototype()
        {
            return TypeProxy.GetPrototype(typeof(Function));
        }
    }
}
