using System.Collections.Generic;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    public sealed class EvalFunction : Function
    {
        [Hidden]
        public override string Name
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

        [DoNotDelete]
        [DoNotEnumerate]
        public override JSObject length
        {
            [Hidden]
            get
            {
                return 1;
            }
        }

        public EvalFunction()
        {

        }

        public override NiL.JS.Core.JSObject Invoke(NiL.JS.Core.JSObject thisBind, NiL.JS.Core.JSObject args)
        {
            var arg = args["0"];
            if (arg.valueType != JSObjectType.String)
                return arg;
            if (this.lastRequestedName == "eval")
                return Context.CurrentContext.Eval(arg.ToString());
            Stack<Context> stack = new Stack<Context>();
            try
            {
                var ccontext = Context.CurrentContext;
                var root = ccontext.Root;
                while (ccontext != root)
                {
                    stack.Push(ccontext);
                    ccontext = ccontext.Deactivate();
                }
                return ccontext.Eval(args["0"].ToString());
            }
            finally
            {
                while (stack.Count != 0) stack.Pop().Activate();
            }
        }

        protected internal override JSObject GetMember(string name, bool create, bool own)
        {
            if (name == "prototype")
                return undefined;
            return base.GetMember(name, create, own);
        }
    }
}
