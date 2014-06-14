using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core
{
    public sealed class EvalFunction : Function
    {
        public override string Name
        {
            get
            {
                return "eval";
            }
        }

        public override FunctionType Type
        {
            get
            {
                return FunctionType.Function;
            }
        }

        public override JSObject length
        {
            get
            {
                return 1;
            }
        }

        public EvalFunction()
            : base(null, null)
        {

        }

        public override NiL.JS.Core.JSObject Invoke(NiL.JS.Core.JSObject thisOverride, NiL.JS.Core.JSObject args)
        {
            if (this.lastRequestedName == "eval")
                return Context.CurrentContext.Eval(args["0"].ToString());
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
    }
}
