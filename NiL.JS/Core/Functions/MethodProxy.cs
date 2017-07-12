using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

#if NET40
using NiL.JS.Backward;
#endif

namespace NiL.JS.Core.Functions
{
    [Flags]
    internal enum ConvertArgsOptions
    {
        Default = 0,
        ThrowOnError = 1,
        StrictConversion = 2,
        DummyValues = 4
    }

    [Prototype(typeof(Function), true)]
    internal sealed class MethodProxy : Function
    {
        private delegate object WrapperDelegate(object target, Context initiator, Expressions.Expression[] arguments, Arguments argumentsObject);

        private static readonly Dictionary<MethodBase, WrapperDelegate> WrapperCache = new Dictionary<MethodBase, WrapperDelegate>();
        private static readonly MethodInfo ArgumentsGetItemMethod = typeof(Arguments).GetMethod("get_Item", new[] { typeof(int) });

        private readonly WrapperDelegate _fastWrapper;
        private readonly bool _forceInstance;
        private readonly bool _strictConversion;
        private readonly ConvertValueAttribute[] _paramsConverters;
        private readonly string _name;

        internal readonly ParameterInfo[] _parameters;
        internal readonly bool _raw;
        internal readonly MethodBase _method;
        internal readonly object _hardTarget;
        internal readonly ConvertValueAttribute _returnConverter;

        public ParameterInfo[] Parameters
        {
            get
            {
                return _parameters;
            }
        }

        public override string name
        {
            get
            {
                return _name;
            }
        }

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

        public MethodProxy(Context context, MethodBase methodBase)
            : this(context, methodBase, null)
        {
        }

        public MethodProxy(Context context, MethodBase methodBase, object hardTarget)
            : base(context)
        {
            _method = methodBase;
            _hardTarget = hardTarget;
            _parameters = methodBase.GetParameters();
            _strictConversion = methodBase.IsDefined(typeof(StrictConversionAttribute), true);
            _name = methodBase.Name;

            if (methodBase.IsDefined(typeof(JavaScriptNameAttribute), false))
            {
                _name = (methodBase.GetCustomAttributes(typeof(JavaScriptNameAttribute), false).First() as JavaScriptNameAttribute).Name;
                if (_name.StartsWith("@@"))
                    _name = _name.Substring(2);
            }

            if (_length == null)
                _length = new Number(0) { _attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.SystemObject };

            if (methodBase.IsDefined(typeof(ArgumentsCountAttribute), false))
            {
                var argsCountAttribute = methodBase.GetCustomAttributes(typeof(ArgumentsCountAttribute), false).First() as ArgumentsCountAttribute;
                _length._iValue = argsCountAttribute.Count;
            }
            else
            {
                _length._iValue = _parameters.Length;
            }

            for (int i = 0; i < _parameters.Length; i++)
            {
                if (_parameters[i].IsDefined(typeof(ConvertValueAttribute), false))
                {
                    var t = _parameters[i].GetCustomAttributes(typeof(ConvertValueAttribute), false).First();
                    if (_paramsConverters == null)
                        _paramsConverters = new ConvertValueAttribute[_parameters.Length];
                    _paramsConverters[i] = t as ConvertValueAttribute;
                }
            }

            var methodInfo = methodBase as MethodInfo;
            if (methodInfo != null)
            {
                _returnConverter = methodInfo.ReturnParameter.GetCustomAttribute(typeof(ConvertValueAttribute), false) as ConvertValueAttribute;

                _forceInstance = methodBase.IsDefined(typeof(InstanceMemberAttribute), false);

                if (_forceInstance)
                {
                    if (!methodInfo.IsStatic
                        || (_parameters.Length == 0)
                        || (_parameters.Length > 2)
                        || (_parameters[0].ParameterType != typeof(JSValue))
                        || (_parameters.Length > 1 && _parameters[1].ParameterType != typeof(Arguments)))
                        throw new ArgumentException("Force-instance method \"" + methodBase + "\" has invalid signature");

                    _raw = true;
                }

                if (!WrapperCache.TryGetValue(methodBase, out _fastWrapper))
                    WrapperCache[methodBase] = _fastWrapper = makeFastWrapper(methodInfo);

                _raw |= _parameters.Length == 0
                    || (_parameters.Length == 1 && _parameters[0].ParameterType == typeof(Arguments));

                RequireNewKeywordLevel = RequireNewKeywordLevel.WithoutNewOnly;
            }
            else if (methodBase is ConstructorInfo)
            {
                if (!WrapperCache.TryGetValue(methodBase, out _fastWrapper))
                    WrapperCache[methodBase] = _fastWrapper = makeFastWrapper(methodBase as ConstructorInfo);

                _raw |= _parameters.Length == 0
                    || (_parameters.Length == 1 && _parameters[0].ParameterType == typeof(Arguments));
            }
            else
                throw new NotImplementedException();
        }

        private MethodProxy(Context context, bool raw, object hardTarget, MethodBase method, ParameterInfo[] Parameters, WrapperDelegate fastWrapper, bool forceInstance)
            : base(context)
        {
            _raw = raw;
            _hardTarget = hardTarget;
            _method = method;
            _parameters = Parameters;
            _fastWrapper = fastWrapper;
            _forceInstance = forceInstance;
            RequireNewKeywordLevel = RequireNewKeywordLevel.WithoutNewOnly;
        }

        private WrapperDelegate makeFastWrapper(MethodInfo methodInfo)
        {
            Expression tree = null;
            var target = Expression.Parameter(typeof(object), "target");
            var context = Expression.Parameter(typeof(Context), "context");
            var arguments = Expression.Parameter(typeof(Expressions.Expression[]), "arguments");
            var argumentsObjectPrm = Expression.Parameter(typeof(Arguments), "argumentsObject");
            var argumentsObject = Expression.Condition(
                Expression.NotEqual(argumentsObjectPrm, Expression.Constant(null)),
                argumentsObjectPrm,
                Expression.Assign(argumentsObjectPrm, Expression.Call(((Func<Expressions.Expression[], Context, Arguments>)Tools.CreateArguments).GetMethodInfo(), arguments, context)));

            if (_forceInstance)
            {
                for (;;)
                {
                    if (methodInfo.IsStatic && _parameters[0].ParameterType == typeof(JSValue))
                    {
                        if (_parameters.Length == 1)
                        {
                            tree = Expression.Call(methodInfo, Expression.Convert(target, typeof(JSValue)));
                            break;
                        }
                        else if (_parameters.Length == 2 && _parameters[1].ParameterType == typeof(Arguments))
                        {
                            tree = Expression.Call(
                                methodInfo,
                                Expression.Convert(target, typeof(JSValue)),
                                argumentsObject);
                            break;
                        }
                    }

                    throw new ArgumentException("Invalid method signature");
                }
            }
            else if (_parameters.Length == 0)
            {
                if (methodInfo.IsStatic)
                    tree = Expression.Call(methodInfo);
                else
                    tree = Expression.Call(Expression.Convert(target, methodInfo.DeclaringType), methodInfo);
            }
            else
            {
                if (_parameters.Length == 1 && _parameters[0].ParameterType == typeof(Arguments))
                {
                    if (methodInfo.IsStatic)
                        tree = Expression.Call(methodInfo, argumentsObject);
                    else
                        tree = Expression.Call(Expression.Convert(target, methodInfo.DeclaringType), methodInfo, argumentsObject);
                }
                else
                {
                    var processArg = ((Func<Expressions.Expression[], Context, int, object>)processArgument).GetMethodInfo();
                    var processArgTail = ((Func<Expressions.Expression[], Context, int, object>)processArgumentsTail).GetMethodInfo();
                    var convertArg = ((Func<int, JSValue, object>)convertArgument).GetMethodInfo();

                    var prms = new Expression[_parameters.Length];
                    for (var i = 0; i < prms.Length; i++)
                    {
                        prms[i] = Expression.Convert(
                            Expression.Call(
                                Expression.Constant(this),
                                i + 1 < prms.Length ? processArg : processArgTail,
                                arguments,
                                context,
                                Expression.Constant(i)),
                            _parameters[i].ParameterType);
                    }

                    if (methodInfo.IsStatic)
                        tree = Expression.Call(methodInfo, prms);
                    else
                        tree = Expression.Call(Expression.Convert(target, methodInfo.DeclaringType), methodInfo, prms);

                    for (var i = 0; i < prms.Length; i++)
                    {
                        prms[i] = Expression.Convert(
                            Expression.Call(
                                Expression.Constant(this),
                                convertArg,
                                Expression.Constant(i),
                                Expression.Call(argumentsObjectPrm, ArgumentsGetItemMethod, Expression.Constant(i))),
                            _parameters[i].ParameterType);
                    }

                    Expression treeWithObjectAsSource;
                    if (methodInfo.IsStatic)
                        treeWithObjectAsSource = Expression.Call(methodInfo, prms);
                    else
                        treeWithObjectAsSource = Expression.Call(Expression.Convert(target, methodInfo.DeclaringType), methodInfo, prms);

                    tree = Expression.Condition(Expression.Equal(argumentsObjectPrm, Expression.Constant(null)),
                                                 tree,
                                                 treeWithObjectAsSource);
                }
            }

            if (methodInfo.ReturnType == typeof(void))
                tree = Expression.Block(tree, Expression.Constant(null));

            try
            {
                return Expression
                    .Lambda<WrapperDelegate>(
                        Expression.Convert(tree, typeof(object)),
                        methodInfo.Name,
                        new[]
                        {
                            target,
                            context,
                            arguments,
                            argumentsObjectPrm
                        })
                    .Compile();
            }
            catch
            {
                throw;
            }
        }

        private WrapperDelegate makeFastWrapper(ConstructorInfo constructorInfo)
        {
            Expression tree = null;
            var target = Expression.Parameter(typeof(object), "target");
            var context = Expression.Parameter(typeof(Context), "context");
            var arguments = Expression.Parameter(typeof(Expressions.Expression[]), "arguments");
            var argumentsObjectPrm = Expression.Parameter(typeof(Arguments), "argumentsObject");
            var argumentsObject = Expression.Condition(
                Expression.NotEqual(argumentsObjectPrm, Expression.Constant(null)),
                argumentsObjectPrm,
                Expression.Assign(argumentsObjectPrm, Expression.Call(((Func<Expressions.Expression[], Context, Arguments>)Tools.CreateArguments).GetMethodInfo(), arguments, context)));

            if (_parameters.Length == 0)
            {
                tree = Expression.New(constructorInfo);
            }
            else
            {
                if (_parameters.Length == 1 && _parameters[0].ParameterType == typeof(Arguments))
                {
                    tree = Expression.New(constructorInfo, argumentsObject);
                }
                else
                {
                    Func<Expressions.Expression[], Context, int, object> processArg = processArgument;
                    Func<Expressions.Expression[], Context, int, object> processArgTail = processArgumentsTail;
                    Func<int, JSValue, object> convertArg = convertArgument;

                    var prms = new Expression[_parameters.Length];
                    for (var i = 0; i < prms.Length; i++)
                    {
                        prms[i] = Expression.Convert(
                            Expression.Call(
                                Expression.Constant(this),
                                i + 1 < prms.Length ? processArg.GetMethodInfo() : processArgTail.GetMethodInfo(),
                                arguments,
                                context,
                                Expression.Constant(i)),
                            _parameters[i].ParameterType);
                    }

                    tree = Expression.New(constructorInfo, prms);

                    for (var i = 0; i < prms.Length; i++)
                    {
                        prms[i] = Expression.Convert(
                            Expression.Call(
                                Expression.Constant(this),
                                convertArg.GetMethodInfo(),
                                Expression.Constant(i),
                                Expression.Call(argumentsObject, ArgumentsGetItemMethod, Expression.Constant(i))),
                            _parameters[i].ParameterType);
                    }

                    Expression treeWithObjectAsSource;
                    treeWithObjectAsSource = Expression.New(constructorInfo, prms);

                    tree = Expression.Condition(Expression.Equal(argumentsObjectPrm, Expression.Constant(null)),
                                                 tree,
                                                 treeWithObjectAsSource);
                }
            }

            try
            {
                return Expression
                    .Lambda<WrapperDelegate>(
                        Expression.Convert(tree, typeof(object)),
                        constructorInfo.DeclaringType.Name,
                        new[]
                        {
                            target,
                            context,
                            arguments,
                            argumentsObjectPrm
                        })
                    .Compile();
            }
            catch
            {
                throw;
            }
        }

        internal override JSValue InternalInvoke(JSValue targetValue, Expressions.Expression[] argumentsSource, Context initiator, bool withSpread, bool withNew)
        {
            if (withNew)
            {
                if (RequireNewKeywordLevel == RequireNewKeywordLevel.WithoutNewOnly)
                {
                    ExceptionHelper.ThrowTypeError(string.Format(Strings.InvalidTryToCreateWithNew, name));
                }
            }
            else
            {
                if (RequireNewKeywordLevel == RequireNewKeywordLevel.WithNewOnly)
                {
                    ExceptionHelper.ThrowTypeError(string.Format(Strings.InvalidTryToCreateWithoutNew, name));
                }
            }

            object value = invokeMethod(targetValue, argumentsSource, null, initiator);
            return Context.GlobalContext.ProxyValue(value);
        }

        private object invokeMethod(JSValue targetValue, Expressions.Expression[] argumentsSource, Arguments argumentsObject, Context initiator)
        {
            object value;
            var target = GetTargetObject(targetValue, _hardTarget);
            try
            {
                if (_parameters.Length == 0 && argumentsSource != null)
                {
                    for (var i = 0; i < argumentsSource.Length; i++)
                        argumentsSource[i].Evaluate(initiator);
                }

                value = _fastWrapper(target, initiator, argumentsSource, argumentsObject);

                if (_returnConverter != null)
                    value = _returnConverter.From(value);
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

            return value;
        }

        private object processArgumentsTail(Expressions.Expression[] arguments, Context context, int index)
        {
            var result = processArgument(arguments, context, index);

            while (++index < arguments.Length)
                arguments[index].Evaluate(context);

            return result;
        }

        internal object GetTargetObject(JSValue targetValue, object hardTarget)
        {
            var target = hardTarget;
            if (target == null)
            {
                if (_forceInstance)
                {
                    if (targetValue != null && targetValue._valueType >= JSValueType.Object)
                    {
                        // Объект нужно развернуть до основного значения. Даже если это обёртка над примитивным значением
                        target = targetValue.Value;

                        var proxy = target as Proxy;
                        if (proxy != null)
                            target = proxy.PrototypeInstance ?? targetValue.Value;

                        // ForceInstance работает только если первый аргумент типа JSValue
                        if (!(target is JSValue))
                            target = targetValue;
                    }
                    else
                        target = targetValue ?? undefined;
                }
                else if (!_method.IsStatic && !_method.IsConstructor)
                {
                    target = convertTargetObject(targetValue ?? undefined, _method.DeclaringType);
                    if (target == null)
                    {
                        // Исключительная ситуация. Я не знаю почему Function.length обобщённое свойство, а не константа. Array.length работает по-другому.
                        if (_method.Name == "get_length" && typeof(Function).IsAssignableFrom(_method.DeclaringType))
                            return Function.Empty;

                        ExceptionHelper.Throw(new TypeError("Can not call function \"" + name + "\" for object of another type."));
                    }
                }
            }

            return target;
        }

        private static object convertTargetObject(JSValue target, Type targetType)
        {
            if (target == null)
                return null;

            target = target._oValue as JSValue ?? target; // это может быть лишь ссылка на какой-то другой контейнер
            var res = Tools.convertJStoObj(target, targetType, false);
            return res;
        }

        private object processArgument(Expressions.Expression[] arguments, Context initiator, int index)
        {
            var value = arguments.Length > index ? Tools.EvalExpressionSafe(initiator, arguments[index]) : notExists;

            return convertArgument(index, value);
        }

        private object convertArgument(int index, JSValue value)
        {
            var cvtArgs = ConvertArgsOptions.ThrowOnError;
            if (_strictConversion)
                cvtArgs |= ConvertArgsOptions.StrictConversion;

            return convertArgument(
                index,
                value,
                cvtArgs);
        }

        private object convertArgument(int index, JSValue value, ConvertArgsOptions options)
        {
            if (_paramsConverters?[index] != null)
                return _paramsConverters[index].To(value);

            var strictConversion = options.HasFlag(ConvertArgsOptions.StrictConversion);
            var parameterInfo = _parameters[index];
            object result = null;

            if (value.IsNull && parameterInfo.ParameterType.GetTypeInfo().IsClass)
            {
                return null;
            }
            else if (value.Defined)
            {
                result = Tools.convertJStoObj(value, parameterInfo.ParameterType, !strictConversion);
                if (strictConversion && result == null)
                {
                    if (options.HasFlag(ConvertArgsOptions.ThrowOnError))
                        ExceptionHelper.ThrowTypeError("Unable to convert " + value + " to type " + parameterInfo.ParameterType);

                    if (!options.HasFlag(ConvertArgsOptions.DummyValues))
                        return null;
                }
            }
            else
            {
                if (parameterInfo.ParameterType.IsAssignableFrom(value.GetType()))
                    return value;
            }

            if (result == null
                && (options.HasFlag(ConvertArgsOptions.DummyValues) || parameterInfo.Attributes.HasFlag(ParameterAttributes.HasDefault)))
            {
                result = parameterInfo.DefaultValue;

#if (PORTABLE || NETCORE)
                if (result != null && result.GetType().FullName == "System.DBNull")
                {
#else
                if (result is DBNull)
                {
#endif
                    if (strictConversion && options.HasFlag(ConvertArgsOptions.ThrowOnError))
                        ExceptionHelper.ThrowTypeError("Unable to convert " + value + " to type " + parameterInfo.ParameterType);

                    if (parameterInfo.ParameterType.GetTypeInfo().IsValueType)
                        result = Activator.CreateInstance(parameterInfo.ParameterType);
                    else
                        result = null;
                }
            }

            return result;
        }

        internal object[] ConvertArguments(Arguments arguments, ConvertArgsOptions options)
        {
            if (_parameters.Length == 0)
                return null;

            if (_forceInstance)
                ExceptionHelper.Throw(new InvalidOperationException());

            object[] res = null;
            int targetCount = _parameters.Length;
            for (int i = targetCount; i-- > 0;)
            {
                var jsValue = arguments?[i] ?? undefined;

                var value = convertArgument(i, jsValue, options);

                if (value == null && !jsValue.IsNull)
                    return null;

                if (res == null)
                    res = new object[targetCount];

                res[i] = value;
            }

            return res;
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
        {
            if (arguments == null)
                arguments = new Arguments();

            var result = invokeMethod(targetObject, null, arguments, Context);

            if (result == null)
                return undefined;

            return result as JSValue ?? Context.GlobalContext.ProxyValue(result);
        }

        public sealed override Function bind(Arguments args)
        {
            if (_hardTarget != null || args.Length == 0)
                return this;

            return new MethodProxy(
                context: Context,
                raw: _raw,
                hardTarget: convertTargetObject(args[0], _method.DeclaringType) ?? args[0].Value as JSObject ?? args[0],
                method: _method,
                Parameters: _parameters,
                fastWrapper: _fastWrapper,
                forceInstance: _forceInstance);
        }
#if !NET40
        public override Delegate MakeDelegate(Type delegateType)
        {
            try
            {
                var methodInfo = _method as MethodInfo;
                return methodInfo.CreateDelegate(delegateType, _hardTarget);
            }
            catch
            {
            }

            return base.MakeDelegate(delegateType);
        }
#endif

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
