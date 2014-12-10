using System;
using System.Collections.Generic;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;
using NiL.JS.Core.TypeProxing;
using NiL.JS.Expressions;
using NiL.JS.Statements;

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
        private Arguments bindedArguments;

        public override JSObject caller
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

        public override JSObject arguments
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

        public override JSObject prototype
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
                _length = 0;
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
            else bindedArguments = null;
        }

        [Hidden]
        public override JSObject Invoke(JSObject thisBind, Arguments args)
        {
            if (bindedArguments != null)
            {
                args.length += bindedArguments.length;
                for (var i = args.length; i-- > bindedArguments.length; )
                    args[i] = args[i - bindedArguments.length];
                for (var i = bindedArguments.length; i-- > 0; )
                    args[i] = bindedArguments[i];
            }
            if (thisBind != null && thisBind.oValue != null && thisBind.oValue == typeof(New) as object)
                return proto.Invoke(thisBind, args);
            return proto.Invoke(this.thisBind, args);
        }

        [Hidden]
        protected internal override IEnumerator<string> GetEnumeratorImpl(bool pdef)
        {
            return proto.GetEnumeratorImpl(pdef);
        }

        [Hidden]
        protected internal override JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
            return proto.GetMember(name, forWrite, own);
        }

        protected override JSObject getDefaultPrototype()
        {
            return TypeProxy.GetPrototype(typeof(Function));
        }
    }
}
