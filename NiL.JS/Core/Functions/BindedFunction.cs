using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core.Functions
{
    /// <summary>
    /// Реализует поведение функции Function.prototype.bind.
    /// </summary>
    [Prototype(typeof(Function), true)]
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    internal sealed class BindedFunction : Function
    {
        private Function original;
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
            : base(proto.Context, proto._functionDefinition)
        {
            if (_length == null)
                _length = new Number(0);
            _length._iValue = proto.length._iValue;
            this.original = proto;
            this._thisBind = args[0];
            this.bindedArguments = args;
            if (args.length > 0)
            {
                args.length--;
                for (var i = 0; i < args.length; i++)
                    args[i] = args[i + 1];
                _length._iValue -= args.length;
                if (_length._iValue < 0)
                    _length._iValue = 0;
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
                return original.Invoke(construct, targetObject, arguments);
            return original.Call(_thisBind, arguments);
        }

        protected internal override JSValue ConstructObject()
        {
            return original.ConstructObject();
        }

        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnumerable, EnumerationMode enumeratorMode)
        {
            return original.GetEnumerator(hideNonEnumerable, enumeratorMode);
        }

        protected internal override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            return original.GetProperty(key, forWrite, memberScope);
        }

        public override string ToString(bool headerOnly)
        {
            return original.ToString(headerOnly);
        }
    }
}
