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

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments, Function newTarget)
        {
            if (construct)
                ExceptionsHelper.ThrowTypeError("Generators cannot be invoked as a constructor");

            return TypeProxy.Marshal(new GeneratorIterator(generator, targetObject, arguments));
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

        private Context initialContext;
        private Context generatorContext;
        private Arguments initialArgs;
        private Function generator;
        private JSValue targetObject;

        [Hidden]
        public GeneratorIterator(Function generator, JSValue self, Arguments args)
        {
            this.initialContext = Context.CurrentContext;
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
            generatorContext = new Context(initialContext, true, generator);
            generatorContext.variables = generator.creator.body._variables;
            generator.initParameters(initialArgs, generatorContext);
            generator.initContext(targetObject, initialArgs, true, generatorContext);
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

        [Field]
        public JSValue value
        {
            get { return _value; }
        }

        [Field]
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
