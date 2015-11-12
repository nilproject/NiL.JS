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

        internal override JSObject GetDefaultPrototype()
        {
            return TypeProxy.GetPrototype(typeof(Function));
        }
    }

    internal sealed class GeneratorIterator : IDisposable, IIterator, IIterable
    {
        private Context generatorContext;
        private Arguments initialArgs;
        private Thread thread;
        private Function generator;
        private JSValue self;
        
        [Hidden]
        public GeneratorIterator(Function generator, JSValue self, Arguments args)
        {
            this.generator = generator;
            this.initialArgs = args;
            this.self = self;
        }

        [Hidden]
        ~GeneratorIterator()
        {
            Dispose();
        }

        public IIteratorResult next(Arguments args)
        {
            if (thread == null)
            {
                thread = new Thread(() =>
                {
                    generator.Call(self, initialArgs);
                    GC.SuppressFinalize(this);
                });
                thread.Start();
                do
                {
                    for (var i = 0; i < Context.MaxConcurentContexts; i++)
                    {
                        if (Context.runnedContexts[i] == null)
                            break;
                        if (Context.runnedContexts[i].threadId == thread.ManagedThreadId)
                        {
                            generatorContext = Context.runnedContexts[i];
                            break;
                        }
                    }
                }
                while (generatorContext == null);

                while (generatorContext.abortType == AbortType.None)
                {
#if !NET35
                    Thread.Yield();
#else
                    Thread.Sleep(0);
#endif
                }

                return new GeneratorResult(generatorContext.abortInfo, generatorContext.abortType == AbortType.Return);
            }
            else
            {
                if (thread.ThreadState == ThreadState.Running || thread.ThreadState == ThreadState.WaitSleepJoin)
                {
                    generatorContext.abortInfo = args != null ? args[0] : JSObject.undefined;
                    generatorContext.abortType = AbortType.None;
                    while (generatorContext.abortType == AbortType.None)
#if !NET35
                        Thread.Yield();
#else
                        Thread.Sleep(0);
#endif
                    return new GeneratorResult(generatorContext.abortInfo, generatorContext.abortType == AbortType.Return);
                }
                else
                {
                    return new GeneratorResult(null, true);
                }
            }
        }

        public IIteratorResult @return()
        {
            if (thread != null)
            {
                if (thread.ThreadState == ThreadState.Suspended)
                {
                    generatorContext.abortType = AbortType.Return;
                }
            }
            Dispose();
            return new GeneratorResult(null, true);
        }

        public IIteratorResult @throw(Arguments arguments = null)
        {
            if (thread != null)
            {
                if (thread.ThreadState == ThreadState.Suspended)
                {
                    generatorContext.abortType = AbortType.Exception;
                }
            }
            return new GeneratorResult(null, true);
        }

        public IIterator iterator()
        {
            return this;
        }

        [Hidden]
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            try
            {
                thread.Abort();
            }
            catch
            {

            }
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
