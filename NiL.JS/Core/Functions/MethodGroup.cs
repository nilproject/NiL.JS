using System;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

#if (PORTABLE || NETCORE)
using System.Reflection;
#endif

namespace NiL.JS.Core.Functions
{
    /// <remarks>
    /// Доступ к типу не предоставляется из скрипта. Атрибуты не нужны
    /// </remarks>
#if !(PORTABLE || NETCORE)
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

        private readonly MethodProxy[] _methods;

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
                return _methods[0].name;
            }
        }

        public MethodGroup(MethodProxy[] methods)
            : base(Context.CurrentContext ?? Context.globalContext)
        {
            _methods = methods;

            if (methods == null)
                throw new ArgumentNullException();

            var len = 0;
            for (var i = 0; i < methods.Length; i++)
                len = System.Math.Max(len, methods[i]._parameters.Length);

            _length = new Number(len)
            {
                _attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate
            };
        }

        internal override JSObject GetDefaultPrototype()
        {
            return Context.BaseContext.GetPrototype(typeof(Function));
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
        {
            int l = arguments == null ? 0 : arguments.length;
            object[] args = null;

            for (int pass = 0; pass < passCount; pass++)
            {
                for (var i = 0; i < _methods.Length; i++)
                {
                    if (_methods[i].Parameters.Length == 1 && _methods[i].raw)
                        return Context.BaseContext.ProxyValue(_methods[i].InvokeImpl(targetObject, null, arguments));

                    if (pass == 1 || _methods[i].Parameters.Length == l)
                    {
                        if (l != 0)
                        {
                            args = _methods[i].ConvertArgs(
                                arguments,
                                (pass >= 1 ? ConvertArgsOptions.None : ConvertArgsOptions.StrictConversion) | (pass >= 2 ? ConvertArgsOptions.DummyValues : ConvertArgsOptions.None));

                            if (args == null)
                                continue;

                            for (var j = args.Length; j-- > 0;)
                            {
                                if (args[j] != null ?
                                    !_methods[i]._parameters[j].ParameterType.IsAssignableFrom(args[j].GetType())
                                    :
#if (PORTABLE || NETCORE)
                                    methods[i]._parameters[j].ParameterType.GetTypeInfo().IsValueType)
#else
                                    _methods[i]._parameters[j].ParameterType.IsValueType)
#endif
                                {
                                    j = 0;
                                    args = null;
                                }
                            }

                            if (args == null)
                                continue;
                        }

                        return Context.BaseContext.ProxyValue(_methods[i].InvokeImpl(targetObject, args, arguments));
                    }
                }
            }

            ExceptionHelper.Throw(new TypeError("Invalid arguments for function " + _methods[0].name));
            return null;
        }
    }
}
