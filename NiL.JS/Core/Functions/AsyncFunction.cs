using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;

namespace NiL.JS.Core.Functions
{
    [Prototype(typeof(Function), true)]
    internal sealed class AsyncFunction : Function
    {
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

        public AsyncFunction(Context context, FunctionDefinition generator)
            : base(context, generator)
        {
            RequireNewKeywordLevel = RequireNewKeywordLevel.WithoutNewOnly;
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
        {
            if (construct)
                ExceptionHelper.ThrowTypeError("Async function cannot be invoked as a constructor");

            throw new NotImplementedException();
        }
    }
}
