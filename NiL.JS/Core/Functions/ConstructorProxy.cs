using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NiL.JS.Backward;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;

namespace NiL.JS.Core.Functions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    [Prototype(typeof(Function), true)]
    internal class ConstructorProxy : Function
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
        private const int passesCount = 3;

        private static readonly object[] _emptyObjectArray = new object[0];
        private static readonly Arguments _emptyArguments = new Arguments();

        internal readonly StaticProxy _staticProxy;
        private readonly MethodProxy[] _constructors;

        public override string name
        {
            get
            {
                return _staticProxy._hostedType.Name;
            }
        }

        public override JSValue prototype
        {
            get
            {
                return _prototype;
            }
            set
            {
                _prototype = value;
            }
        }

        public ConstructorProxy(Context context, StaticProxy staticProxy, JSObject prototype)
            : base(context)
        {
            if (staticProxy == null)
                throw new ArgumentNullException(nameof(staticProxy));
            if (prototype == null)
                throw new ArgumentNullException(nameof(prototype));

            _fields = staticProxy._fields;
            _staticProxy = staticProxy;
            _prototype = prototype;

#if (PORTABLE || NETCORE)
            if (_staticProxy._hostedType.GetTypeInfo().ContainsGenericParameters)
                ExceptionHelper.Throw(new TypeError(_staticProxy._hostedType.Name + " can't be created because it's generic type."));
#else
            if (_staticProxy._hostedType.ContainsGenericParameters)
                ExceptionHelper.ThrowTypeError(_staticProxy._hostedType.Name + " can't be created because it's generic type.");
#endif
            var withNewOnly = staticProxy._hostedType.GetTypeInfo().IsDefined(typeof(RequireNewKeywordAttribute), true);
            var withoutNewOnly = staticProxy._hostedType.GetTypeInfo().IsDefined(typeof(DisallowNewKeywordAttribute), true);

            if (withNewOnly && withoutNewOnly)
                ExceptionHelper.Throw(new InvalidOperationException("Unacceptably use of " + typeof(RequireNewKeywordAttribute).Name + " and " + typeof(DisallowNewKeywordAttribute).Name + " for same type."));

            if (withNewOnly)
                RequireNewKeywordLevel = RequireNewKeywordLevel.WithNewOnly;
            if (withoutNewOnly)
                RequireNewKeywordLevel = RequireNewKeywordLevel.WithoutNewOnly;

            if (_length == null)
                _length = new Number(0) { _attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate };

#if (PORTABLE || NETCORE)
            var ctors = staticProxy._hostedType.GetTypeInfo().DeclaredConstructors.Where(x => x.IsPublic).ToArray();
            var ctorsL = new List<MethodProxy>(ctors.Length + (staticProxy._hostedType.GetTypeInfo().IsValueType ? 1 : 0));
#else
            var ctors = staticProxy._hostedType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var ctorsL = new List<MethodProxy>(ctors.Length + (staticProxy._hostedType.IsValueType ? 1 : 0));
#endif
            for (int i = 0; i < ctors.Length; i++)
            {
                if (ctors[i].IsStatic)
                    continue;

                if (ctors[i].GetParameters().Any(x => x.ParameterType.IsPointer || x.IsOut || x.IsIn || x.IsRetval || x.ParameterType.IsByRef))
                    continue;

                if (!ctors[i].IsDefined(typeof(HiddenAttribute), false) || ctors[i].IsDefined(typeof(ForceUseAttribute), true))
                {
                    ctorsL.Add(new MethodProxy(context, ctors[i]));
                    length._iValue = System.Math.Max(ctorsL[ctorsL.Count - 1]._length._iValue, _length._iValue);
                }
            }

            ctorsL.Sort((x, y) =>
                x.Parameters.Length == 1 && x.Parameters[0].ParameterType == typeof(Arguments) ? 1 :
                y.Parameters.Length == 1 && y.Parameters[0].ParameterType == typeof(Arguments) ? -1 :
                x.Parameters.Length - y.Parameters.Length);

            _constructors = ctorsL.ToArray();
        }

        internal protected override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            if (memberScope < PropertyScope.Super && key._valueType != JSValueType.Symbol)
            {
                var keyString = key.ToString();

                if (keyString == "prototype") // Все прокси-прототипы read-only и non-configurable. Это и оптимизация, и устранение необходимости навешивания атрибутов
                    return prototype;

                if (key._valueType != JSValueType.String)
                    key = keyString;

                JSValue res;
                if (forWrite || (keyString != "toString" && keyString != "constructor"))
                {
                    res = _staticProxy.GetProperty(key, forWrite && memberScope == PropertyScope.Own, memberScope);
                    if (res.Exists || (memberScope == PropertyScope.Own && forWrite))
                    {
                        if (forWrite && res.NeedClone)
                            res = _staticProxy.GetProperty(key, true, memberScope);

                        return res;
                    }

                    res = __proto__.GetProperty(key, forWrite, memberScope);
                    if (memberScope == PropertyScope.Own && (res._valueType != JSValueType.Property || (res._attributes & JSValueAttributesInternal.Field) == 0))
                        return notExists; // если для записи, то первая ветка всё разрулит и сюда выполнение не придёт

                    return res;
                }
            }

            return base.GetProperty(key, forWrite, memberScope);
        }

        protected internal override bool DeleteProperty(JSValue name)
        {
            return _staticProxy.DeleteProperty(name) && __proto__.DeleteProperty(name);
        }

        internal override JSValue InternalInvoke(JSValue targetObject, Expression[] arguments, Context initiator, bool withSpread, bool construct)
        {
            if (_functionDefinition._body == null)
                return NotExists;

            var argumentsObject = arguments.Length == 0 ? _emptyArguments : Tools.CreateArguments(arguments, initiator);

            initiator._objectSource = null;

            if (construct)
            {
                if (targetObject == null || targetObject._valueType < JSValueType.Object)
                    return Construct(argumentsObject);

                return Construct(targetObject, argumentsObject);
            }
            else
                return Call(targetObject, argumentsObject);
        }

        public override JSValue Construct(Arguments arguments)
        {
            return Invoke(true, null, arguments);
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
        {
            if (!construct && _staticProxy._hostedType == typeof(Date))
            {
                return new Date().ToString();
            }

            object obj;
            if (_staticProxy._hostedType == typeof(BaseLibrary.Array))
            {
                if (arguments == null)
                {
                    obj = new BaseLibrary.Array();
                }
                else
                {
                    switch (arguments._iValue)
                    {
                        case 0:
                            obj = new BaseLibrary.Array();
                            break;
                        case 1:
                        {
                            var a0 = arguments[0];
                            switch (a0._valueType)
                            {
                                case JSValueType.Integer:
                                    obj = new BaseLibrary.Array(a0._iValue);
                                    break;
                                case JSValueType.Double:
                                    obj = new BaseLibrary.Array(a0._dValue);
                                    break;
                                default:
                                    obj = new BaseLibrary.Array(arguments);
                                    break;
                            }
                            break;
                        }
                        default:
                            obj = new BaseLibrary.Array(arguments);
                            break;
                    }
                }
            }
            else
            {
                if ((arguments == null || arguments._iValue == 0)
#if (PORTABLE || NETCORE)
&& _staticProxy._hostedType.GetTypeInfo().IsValueType)
#else
 && _staticProxy._hostedType.IsValueType)
#endif
                {
                    obj = Activator.CreateInstance(_staticProxy._hostedType);
                }
                else
                {
                    var constructor = findConstructor(arguments, out var args);

                    if (constructor == null)
                        ExceptionHelper.ThrowTypeError(_staticProxy._hostedType.Name + " can't be created.");

                    var target = constructor.GetTargetObject(targetObject, null);

                    try
                    {
                        if (target != null)
                            obj = constructor._method.Invoke(target, args);
                        else
                            obj = (constructor._method as ConstructorInfo).Invoke(args);

                    }
                    catch (TargetInvocationException e)
                    {
                        ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                        return null;
                    }
                }
            }

            JSValue res = obj as JSValue;
            if (construct)
            {
                if (res != null)
                {
                    // Для Number, Boolean и String
                    if (res._valueType < JSValueType.Object)
                    {
                        var objc = (targetObject as ObjectWrapper ?? ConstructObject()) as ObjectWrapper;
                        objc.instance = obj;
                        if (objc._objectPrototype == null)
                            objc._objectPrototype = res.__proto__;
                        res = objc;
                    }
                    else if (res._oValue is JSValue)
                    {
                        res._oValue = res;
                        // На той стороне понять, по new или нет вызван конструктор не получится,
                        // поэтому по соглашению такие типы себя настраивают так, как будто они по new,
                        // а в oValue пишут экземпляр аргумента на тот случай, если вызван конструктор типа как функция
                        // с передачей в качестве аргумента существующего экземпляра
                    }
                }
                else
                {
                    var objc = wrapObject(targetObject, obj);

                    if (obj.GetType() == typeof(Date))
                        objc._valueType = JSValueType.Date;
                    else if (res != null)
                        objc._valueType = (JSValueType)System.Math.Max((int)objc._valueType, (int)res._valueType);

                    res = objc;
                }
            }
            else
            {
                if (_staticProxy._hostedType == typeof(JSValue))
                {
                    if ((res._oValue is JSValue) && (res._oValue as JSValue)._valueType >= JSValueType.Object)
                        return res._oValue as JSValue;
                }

                if (res == null)
                {
                    res = wrapObject(targetObject, obj);
                }
            }

            return res;
        }

        private ObjectWrapper wrapObject(JSValue targetObject, object obj)
        {
            var objc = (targetObject as ObjectWrapper ?? ConstructObject()) as ObjectWrapper;
            objc.instance = obj;
            objc._attributes |= _staticProxy._hostedType.GetTypeInfo().IsDefined(typeof(ImmutableAttribute), false) ? JSValueAttributesInternal.Immutable : JSValueAttributesInternal.None;
            return objc;
        }

        protected internal override JSValue ConstructObject()
        {
            return new ObjectWrapper(null)
            {
                _objectPrototype = Context.GlobalContext.GetPrototype(_staticProxy._hostedType)
            };
        }

        private MethodProxy findConstructor(Arguments arguments, out object[] args)
        {
            args = null;
            var len = arguments == null ? 0 : arguments._iValue;
            for (var pass = 0; pass < passesCount; pass++)
            {
                for (int i = 0; i < _constructors.Length; i++)
                {
                    if (_constructors[i]._parameters.Length == 1 && _constructors[i]._parameters[0].ParameterType == typeof(Arguments))
                    {
                        args = new object[] { arguments };
                        return _constructors[i];
                    }

                    if (pass == 1 
                        || _constructors[i]._parameters.Length == len 
                        || (_constructors[i]._parameters.Length >= len && _constructors[i]._parameters.Count(x => !x.IsOptional) <= len))
                    {
                        if (len == 0)
                        {
                            args = _emptyObjectArray;
                        }
                        else
                        {
                            args = _constructors[i].ConvertArguments(
                                arguments,
                                (pass >= 1 ? 0 : ConvertArgsOptions.StrictConversion)
                                | (pass >= 2 ? ConvertArgsOptions.AllowDefaultValues : 0));

                            if (args == null)
                                continue;

                            for (var j = args.Length; j-- > 0;)
                            {
                                if (args[j] != null ?
                                    !_constructors[i]._parameters[j].ParameterType.IsAssignableFrom(args[j].GetType())
                                    :
#if (PORTABLE || NETCORE)
                                    constructors[i]._parameters[j].ParameterType.GetTypeInfo().IsValueType)
#else
                                    _constructors[i]._parameters[j].ParameterType.IsValueType)
#endif
                                {
                                    j = 0;
                                    args = null;
                                }
                            }

                            if (args == null)
                                continue;
                        }

                        return _constructors[i];
                    }
                }
            }

            return null;
        }

        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnumerable, EnumerationMode enumerationMode, PropertyScope propertyScope = PropertyScope.Common)
        {
            var e = _staticProxy.GetEnumerator(hideNonEnumerable, enumerationMode, propertyScope);
            while (e.MoveNext())
                yield return e.Current;

            if (propertyScope is not PropertyScope.Own)
            {
                e = __proto__.GetEnumerator(hideNonEnumerable, enumerationMode, PropertyScopeForProto(propertyScope));
                while (e.MoveNext())
                    yield return e.Current;
            }
        }

        public override string ToString(bool headerOnly)
        {
            var result = "function " + name + "()";

            if (!headerOnly)
            {
                result += " { [native code] }";
            }

            return result;
        }
    }
}
