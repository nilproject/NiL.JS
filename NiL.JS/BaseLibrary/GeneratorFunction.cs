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
        }

        [Hidden]
        public override JSValue Invoke(JSValue thisBind, Arguments args)
        {
            return TypeProxy.Proxy(new GeneratorIterator(generator, thisBind, args));
        }

        internal override JSObject GetDefaultPrototype()
        {
            return TypeProxy.GetPrototype(typeof(Function));
        }
    }

    internal sealed class GeneratorIterator : IDisposable, IIterator
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

        ~GeneratorIterator()
        {
            Dispose();
        }

        public JSValue next(Arguments args)
        {
            if (thread == null)
            {
                thread = new Thread(() =>
                {
                    generator.Invoke(self, initialArgs);
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
#if !NET35
                    Thread.Yield();
#else
                    Thread.Sleep(0);
#endif
                var res = JSObject.CreateObject();
                res.fields["value"] = generatorContext.abortInfo;
                res.fields["done"] = generatorContext.abortType == AbortType.Return;
                return res;
            }
            else
            {
                if (thread.ThreadState == ThreadState.Running
                    || thread.ThreadState == ThreadState.WaitSleepJoin)
                {
                    generatorContext.abortInfo = args[0];
                    generatorContext.abortType = AbortType.None;
                    while (generatorContext.abortType == AbortType.None)
#if !NET35
                        Thread.Yield();
#else
                        Thread.Sleep(0);
#endif
                    var res = JSObject.CreateObject();
                    res.fields["value"] = generatorContext.abortInfo;
                    res.fields["done"] = generatorContext.abortType == AbortType.Return;
                    return res;
                }
                else
                {
                    var res = JSObject.CreateObject();
                    res.fields["done"] = true;
                    return res;
                }
            }
        }

        public void @throw()
        {
            if (thread != null)
            {
                if (thread.ThreadState == ThreadState.Suspended)
                {
                    generatorContext.abortType = AbortType.Exception;
                }
            }
        }

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
    }
}
