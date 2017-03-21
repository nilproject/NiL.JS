using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;

namespace NiL.JS.BaseLibrary
{
    [Prototype(typeof(Function), true)]
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
        
        public GeneratorFunction(Context context, FunctionDefinition generator)
            : base(context, generator)
        {
            RequireNewKeywordLevel = RequireNewKeywordLevel.WithoutNewOnly;
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
        {
            if (construct)
                ExceptionHelper.ThrowTypeError("Generators cannot be invoked as a constructor");

            return Context.GlobalContext.ProxyValue(new GeneratorIterator(this, targetObject, arguments));
        }
    }

    internal sealed class GeneratorIterator : IIterator, IIterable
    {
        private static readonly Arguments _emptyArguments = new Arguments();

        private Context _initialContext;
        private Context _generatorContext;
        private Arguments _initialArgs;
        private Function _generator;
        private JSValue _targetObject;

        [Hidden]
        public GeneratorIterator(GeneratorFunction generator, JSValue self, Arguments args)
        {
            _initialContext = Context.CurrentContext;
            _generator = generator;
            _initialArgs = args ?? _emptyArguments;
            _targetObject = self;
        }

        public IIteratorResult next(Arguments args)
        {
            if (_generatorContext == null)
            {
                initContext();
            }
            else
            {
                switch (_generatorContext._executionMode)
                {
                    case ExecutionMode.Suspend:
                        {
                            _generatorContext._executionMode = ExecutionMode.Resume;
                            break;
                        }
                    case ExecutionMode.ResumeThrow:
                        {
                            break;
                        }
                    default:
                        return new GeneratorResult(JSValue.undefined, true);
                };

                _generatorContext._executionInfo = args != null ? args[0] : JSValue.undefined;
            }

            _generatorContext.Activate();
            JSValue result = null;
            try
            {
                result = _generator.evaluateBody(_generatorContext);
            }
            finally
            {
                _generatorContext.Deactivate();
            }

            return new GeneratorResult(result, _generatorContext._executionMode != ExecutionMode.Suspend);
        }

        private void initContext()
        {
            _generatorContext = new Context(_initialContext, true, _generator);
            _generatorContext._definedVariables = _generator._functionDefinition._body._variables;
            _generator.initParameters(_initialArgs, _generatorContext);
            _generator.initContext(_targetObject, _initialArgs, true, _generatorContext);
        }

        public IIteratorResult @return()
        {
            if (_generatorContext == null)
                initContext();
            _generatorContext._executionMode = ExecutionMode.Return;
            return next(null);
        }

        public IIteratorResult @throw(Arguments arguments = null)
        {
            if (_generatorContext == null)
                return new GeneratorResult(JSValue.undefined, true);
            _generatorContext._executionMode = ExecutionMode.ResumeThrow;
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
            _value = value;
            _done = done;
        }
    }
}
