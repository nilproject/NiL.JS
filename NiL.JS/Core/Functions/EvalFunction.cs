using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.Functions
{
    public sealed class EvalFunction : Function
    {
        [Hidden]
        public override string name
        {
            [Hidden]
            get
            {
                return "eval";
            }
        }

        [Hidden]
        public override FunctionType Type
        {
            [Hidden]
            get
            {
                return FunctionType.Function;
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
        }

        [Hidden]
        public override NiL.JS.Core.JSValue Invoke(NiL.JS.Core.JSValue thisBind, Arguments args)
        {
            if (args == null)
                return NotExistsInObject;
            var arg = args[0];
            if (arg.valueType != JSValueType.String)
                return arg;
            if ((this.attributes & JSValueAttributesInternal.Eval) != 0)
                return Context.CurrentContext.Eval(arg.oValue.ToString(), false);
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
                        return root.Eval(args[0].ToString(), false);
                    }
                    finally
                    {
                        root.Deactivate();
                    }
                }
                else
                    return ccontext.Eval(args[0].ToString(), false);
            }
            finally
            {
                while (stack.Count != 0) stack.Pop().Activate();
            }
        }

        protected internal override JSValue GetMember(JSValue name, bool forWrite, bool own)
        {
            if (name.ToString() == "prototype")
                return undefined;
            return base.GetMember(name, forWrite, own);
        }
    }
}
