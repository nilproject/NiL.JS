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
        /// <summary>
        /// На первом проходе будут выбираться методы со строгим соответствием типов
        /// 
        /// На втором проходе будут выбираться методы, для которых
        /// получится преобразовать входные аргументы.
        /// 
        /// На третьем проходе будет выбираться первый метод, 
        /// для которого получится сгенерировать параметры по-умолчанию.
        /// 
        /// Если нужен более строгий подбор, то количество проходов нужно
        /// уменьшить до одного
        /// </summary>
        private const int passCount = 3;

        private readonly MethodProxy[] methods;

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
            _length = new BaseLibrary.Number(len) { attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate };
        }

        internal override JSObject GetDefaultPrototype()
        {
            return TypeProxy.GetPrototype(typeof(Function));
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments, Function newTarget)
        {
            int l = arguments == null ? 0 : arguments.length;
            object[] args = null;

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
                            args = methods[i].ConvertArgs(
                                arguments, 
                                (pass >= 1 ? ConvertArgsOptions.None : ConvertArgsOptions.StrictConversion) | (pass >= 2 ? ConvertArgsOptions.DummyValues : ConvertArgsOptions.None));

                            if (args == null)
                                continue;

                            for (var j = args.Length; j-- > 0;)
                            {
                                if (args[j] != null ?
                                    !methods[i].parameters[j].ParameterType.IsAssignableFrom(args[j].GetType())
                                    :
                                    methods[i].parameters[j].ParameterType.IsValueType)
                                {
                                    j = 0;
                                    args = null;
                                }
                            }

                            if (args == null)
                                continue;
                        }
                        return TypeProxy.Proxy(methods[i].InvokeImpl(targetObject, args, arguments));
                    }
                }
            }

            ExceptionsHelper.Throw(new TypeError("Invalid arguments for function " + methods[0].name));
            return null;
        }
    }
}
