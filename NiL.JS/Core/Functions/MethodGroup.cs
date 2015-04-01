using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.TypeProxing;

namespace NiL.JS.Core.Functions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class MethodGroup : BaseLibrary.Function
    {
        private readonly MethodProxy[] methods;
        private readonly int passCount;

        public override JSObject prototype
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        public override string name
        {
            get
            {
                return methods[0].name;
            }
        }

        public MethodGroup(MethodProxy[] methods)
        {
            this.methods = methods;

            if (methods == null)
                throw new ArgumentNullException();

            var len = 0;
            for (var i = 0; i < methods.Length; i++)
                len = System.Math.Max(len, methods[i].parameters.Length);
            _length = new BaseLibrary.Number(len) { attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum };
            
            
            passCount = 2; 
            // На втором проходе будет выбираться первый метод, 
            // для которого получится сгенерировать параметры по-умолчанию.
            // Если нужен более строгий подбор, то количество проходов нужно
            // уменьшить до одного
        }

        protected override JSObject getDefaultPrototype()
        {
            return TypeProxy.GetPrototype(typeof(Function));
        }

        public override NiL.JS.Core.JSObject Invoke(NiL.JS.Core.JSObject thisBind, NiL.JS.Core.Arguments args)
        {
            int l = args == null ? 0 : args.length;
            object[] cargs = null;

            for (int pass = 0; pass < passCount; pass++)
            {
                for (var i = 0; i < methods.Length; i++)
                {
                    if (methods[i].Parameters.Length == 1 && methods[i].Parameters[0].ParameterType == typeof(Arguments))
                        return TypeProxy.Proxy(methods[i].InvokeImpl(thisBind, null, args));
                    if (pass == 1 || methods[i].Parameters.Length == l)
                    {
                        if (l != 0)
                        {
                            cargs = methods[i].ConvertArgs(args, pass == 1);
                            for (var j = cargs.Length; j-- > 0; )
                            {
                                var prmType = methods[i].Parameters[j].ParameterType;
                                if (!prmType.IsAssignableFrom(cargs[j].GetType()))
                                {
                                    cargs = null;
                                    break;
                                }
                            }
                            if (cargs == null)
                                continue;
                        }
                        return TypeProxy.Proxy(methods[i].InvokeImpl(thisBind, cargs, args));
                    }
                }
            }
            throw new JSException(new TypeError("Invalid arguments for function " + methods[0].name));
        }
    }
}
