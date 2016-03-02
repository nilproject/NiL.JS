using System;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core.Functions
{
    /// <remarks>
    /// Доступ к типу не предоставляется из скрипта. Атрибуты не нужны
    /// </remarks>
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class MethodGroup : BaseLibrary.Function
    {
        private readonly MethodProxy[] methods;
        private readonly int passCount;

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

        public override string _name
        {
            get
            {
                return methods[0]._name;
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
            _length = new BaseLibrary.Number(len) { attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate };


            passCount = 2;
            // На втором проходе будет выбираться первый метод, 
            // для которого получится сгенерировать параметры по-умолчанию.
            // Если нужен более строгий подбор, то количество проходов нужно
            // уменьшить до одного
        }

        internal override JSObject GetDefaultPrototype()
        {
            return TypeProxy.GetPrototype(typeof(Function));
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments, Function newTarget)
        {
            int l = arguments == null ? 0 : arguments.length;
            object[] cargs = null;

            for (int pass = 0; pass < passCount; pass++)
            {
                for (var i = 0; i < methods.Length; i++)
                {
                    if (methods[i].Parameters.Length == 1 && methods[i].Parameters[0].ParameterType == typeof(Arguments))
                        return TypeProxy.Proxy(methods[i].InvokeImpl(targetObject, null, arguments));
                    if (pass == 1 || methods[i].Parameters.Length == l)
                    {
                        if (l != 0)
                        {
                            cargs = methods[i].ConvertArgs(arguments, pass == 1);
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
                        return TypeProxy.Proxy(methods[i].InvokeImpl(targetObject, cargs, arguments));
                    }
                }
            }
            ExceptionsHelper.Throw(new TypeError("Invalid arguments for function " + methods[0]._name));
            return null;
        }
    }
}
