using System;
using System.Reflection;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

#if NET40
using NiL.JS.Backward;
#endif

namespace NiL.JS.Core.Functions
{
    /// <remarks>
    /// Доступ к типу не предоставляется из скрипта. Атрибуты не нужны
    /// </remarks>
    [Prototype(typeof(Function), true)]
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    internal sealed class MethodGroup : Function
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
        private const int PassesCount = 3;

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
            : base(Context.CurrentContext ?? Context._DefaultGlobalContext)
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

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
        {
            int len = arguments == null ? 0 : arguments.length;
            object[] args = null;

            for (int pass = 0; pass < PassesCount; pass++)
            {
                for (var i = 0; i < _methods.Length; i++)
                {
                    if (_methods[i]._parameters.Length == 1 && _methods[i]._raw)
                        return Context.GlobalContext.ProxyValue(_methods[i].Call(targetObject, arguments));

                    if (pass == 2 || _methods[i]._parameters.Length == len)
                    {
                        if (len != 0)
                        {
                            args = _methods[i].ConvertArguments(
                                arguments,
                                (pass >= 1 ? ConvertArgsOptions.Default : ConvertArgsOptions.StrictConversion)
                                | (pass >= 2 ? ConvertArgsOptions.DummyValues : ConvertArgsOptions.Default));

                            if (args == null)
                                continue;

                            for (var j = args.Length; j-- > 0;)
                            {
                                if (args[j] != null ?
                                    !_methods[i]._parameters[j].ParameterType.IsAssignableFrom(args[j].GetType())
                                    :
                                    _methods[i]._parameters[j].ParameterType.GetTypeInfo().IsValueType)
                                {
                                    j = 0;
                                    args = null;
                                }
                            }

                            if (args == null)
                                continue;
                        }

                        object value;
                        var target = _methods[i].GetTargetObject(targetObject, _methods[i]._hardTarget);
                        try
                        {
                            value = _methods[i]._method.Invoke(target, args);

                            if (_methods[i]._returnConverter != null)
                                value = _methods[i]._returnConverter.From(value);
                        }
                        catch (Exception e)
                        {
                            while (e.InnerException != null)
                                e = e.InnerException;

                            if (e is JSException)
                                throw e;

                            ExceptionHelper.Throw(new TypeError(e.Message), e);
                            throw;
                        }

                        return Context.GlobalContext.ProxyValue(value);
                    }
                }
            }

            ExceptionHelper.Throw(new TypeError("Invalid arguments for function " + _methods[0].name));
            return null;
        }
    }
}
