using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;

namespace NiL.JS.Core.Functions
{
    public sealed class EvalFunction : Function
    {
        [Hidden]
        public override string _name
        {
            [Hidden]
            get
            {
                return "eval";
            }
        }
        
        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public override JSValue prototype
        {
            [Hidden]
            get
            {
                return null;
            }
            [Hidden]
            set
            {
            }
        }

        [Hidden]
        public EvalFunction()
        {
            _length = new Number(1);
            RequireNewKeywordLevel = BaseLibrary.RequireNewKeywordLevel.WithoutNewOnly;
        }

        internal override JSValue InternalInvoke(JSValue targetObject, Expression[] arguments, Context initiator, Function newTarget, bool withSpread, bool construct)
        {
            if (construct)
                ExceptionsHelper.ThrowTypeError("eval can not be called as constructor");

            if ((this.attributes & JSValueAttributesInternal.Eval) == 0)
                return base.InternalInvoke(targetObject, arguments, initiator, newTarget, withSpread, construct);

            this.attributes &= ~JSValueAttributesInternal.Eval;

            if (arguments == null || arguments.Length == 0)
                return NotExists;

            var arg = arguments[0].Evaluate(initiator);
            if (arg.valueType == JSValueType.SpreadOperatorResult)
            {
                var list = arg.oValue as IList<JSValue>;
                if (list.Count == 0)
                    return NotExists;
                arg = list[0];
            }
            for (var i = 1; i < arguments.Length; i++)
                arguments[i].Evaluate(initiator);

            if (arg.valueType != JSValueType.String)
                return arg;

            return initiator.Eval(arg.oValue.ToString(), false);
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments, Function newTarget)
        {
            if (arguments == null)
                return NotExists;
            var arg = arguments[0];
            if (arg.valueType != JSValueType.String)
                return arg;
            Stack<Context> stack = new Stack<Context>();
            try
            {
                var ccontext = Context.CurrentContext;
                var root = ccontext.Root;
                while (ccontext != root && ccontext != null)
                {
                    stack.Push(ccontext);
                    ccontext = ccontext.Deactivate();
                }
                if (ccontext == null)
                {
                    root.Activate();
                    try
                    {
                        return root.Eval(arguments[0].ToString(), false);
                    }
                    finally
                    {
                        root.Deactivate();
                    }
                }
                else
                    return ccontext.Eval(arguments[0].ToString(), false);
            }
            finally
            {
                while (stack.Count != 0)
                    stack.Pop().Activate();
            }
        }

        protected internal override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            if (memberScope < PropertyScope.Super && key.valueType != JSValueType.Symbol)
            {
                if (key.ToString() == "prototype")
                    return undefined;
            }
            return base.GetProperty(key, forWrite, memberScope);
        }
    }
}
