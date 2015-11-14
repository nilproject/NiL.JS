using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS.BaseLibrary
{
    internal sealed class GeneratorFunction : Function
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

        private Function generator;

        [Hidden]
        public GeneratorFunction(Function generator)
        {
            this.generator = generator;
            RequireNewKeywordLevel = BaseLibrary.RequireNewKeywordLevel.WithoutNewOnly;
        }

        protected override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
        {
            return TypeProxy.Proxy(new GeneratorIterator(generator, targetObject, arguments));
        }

        internal void Resume()
        {

        }

        internal override JSObject GetDefaultPrototype()
        {
            return TypeProxy.GetPrototype(typeof(Function));
        }
    }

    internal sealed class GeneratorIterator : IIterator, IIterable
    {
        private static readonly Arguments _emptyArguments = new Arguments();

        private Context generatorContext;
        private Arguments initialArgs;
        private Function generator;
        private JSValue targetObject;

        [Hidden]
        public GeneratorIterator(Function generator, JSValue self, Arguments args)
        {
            this.generator = generator;
            this.initialArgs = args ?? _emptyArguments;
            this.targetObject = self;
        }

        public IIteratorResult next(Arguments args)
        {
            if (generatorContext == null)
            {
                initContext();
            }
            else
            {
                switch (generatorContext.abortType)
                {
                    case AbortType.Suspend:
                        {
                            generatorContext.abortType = AbortType.Resume;
                            break;
                        }
                    case AbortType.ResumeThrow:
                        {
                            break;
                        }
                    default:
                        return new GeneratorResult(JSValue.undefined, true);
                };
                generatorContext.abortInfo = args != null ? args[0] : JSValue.undefined;
            }
            generatorContext.Activate();
            JSValue result = null;
            try
            {
                result = generator.evaluate(generatorContext);
            }
            finally
            {
                generatorContext.Deactivate();
            }
            return new GeneratorResult(result, generatorContext.abortType != AbortType.Suspend);
        }

        private void initContext()
        {
            generatorContext = new Context(Context.CurrentContext, true, generator);
            generator.initParameters(initialArgs, generatorContext);
            generator.initContext(targetObject, initialArgs, true, generatorContext);
            generator.initVariables(generatorContext);
        }

        public IIteratorResult @return()
        {
            if (generatorContext == null)
                initContext();
            generatorContext.abortType = AbortType.Return;
            return next(null);
        }

        public IIteratorResult @throw(Arguments arguments = null)
        {
            if (generatorContext == null)
                return new GeneratorResult(JSValue.undefined, true);
            generatorContext.abortType = AbortType.ResumeThrow;
            return next(arguments);                
        }

        public IIterator iterator()
        {
            return this;
        }

        [Hidden]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [Hidden]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [Hidden]
        public override string ToString()
        {
            return base.ToString();
        }
    }

    internal sealed class GeneratorResult : IIteratorResult
    {
        private JSValue _value;
        private bool _done;

        public JSValue value
        {
            get { return _value; }
        }

        public bool done
        {
            get { return _done; }
        }

        [Hidden]
        public GeneratorResult(JSValue value, bool done)
        {
            this._value = value;
            this._done = done;
        }
    }
}
