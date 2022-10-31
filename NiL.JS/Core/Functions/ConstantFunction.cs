using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;

namespace NiL.JS.Core.Functions
{
    [Prototype(typeof(Function), true)]
    internal sealed class ConstantFunction : Function
    {
        private readonly JSValue _value;

        public ConstantFunction(JSValue value, FunctionDefinition functionDefinition)
            : base(Context.CurrentGlobalContext, functionDefinition)
        {
            _value = value;
        }

        internal override JSValue InternalInvoke(JSValue targetObject, Expression[] arguments, Context initiator, bool withSpread, bool construct)
        {
            for (var i = 0; i < arguments.Length; i++)
                arguments[i].Evaluate(initiator);

            if (construct)
            {
                if (RequireNewKeywordLevel == RequireNewKeywordLevel.WithoutNewOnly)
                    ExceptionHelper.ThrowTypeError(string.Format(Strings.InvalidTryToCreateWithNew, name));

                return ConstructObject();
            }

            if (RequireNewKeywordLevel == RequireNewKeywordLevel.WithNewOnly)
                ExceptionHelper.ThrowTypeError(string.Format(Strings.InvalidTryToCreateWithoutNew, name));

            return _value;
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
        {
            return _value;
        }
    }
}
