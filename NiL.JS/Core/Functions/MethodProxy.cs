using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using NiL.JS.Backward;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core.Functions
{
    [Flags]
    internal enum ConvertArgsOptions
    {
        Default = 0,
        ThrowOnError = 1,
        StrictConversion = 2,
        AllowDefaultValues = 4
    }

    internal delegate object WrapperDelegate(object target, Context initiator, Expressions.Expression[] arguments, Arguments argumentsObject);

    [Prototype(typeof(Function), true)]
    internal sealed class MethodProxy : Function
    {
        private delegate object RestPrmsConverter(Context initiator, Expressions.Expression[] arguments, Arguments argumentsObject);

        private static readonly Dictionary<MethodBase, WrapperDelegate> WrapperCache = new Dictionary<MethodBase, WrapperDelegate>();
        private static readonly MethodInfo ArgumentsGetItemMethod = typeof(Arguments).GetMethod("get_Item", new[] { typeof(int) });

        private readonly bool _forceInstance;
        private readonly bool _strictConversion;
        private readonly RestPrmsConverter _restPrmsArrayCreator;
        private readonly ConvertValueAttribute[] _paramsConverters;
        private readonly string _name;

        internal readonly WrapperDelegate _fastWrapper;
        internal readonly ParameterInfo[] _parameters;
        internal readonly MethodBase _method;
        internal readonly object _hardTarget;
        internal readonly ConvertValueAttribute _returnConverter;

        public ParameterInfo[] Parameters => _parameters;

        public override string name => _name;

        public override JSValue prototype { get => null; set { } }

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
                        || (_parameters[0].ParameterType != typeof(JSValue)))
                        throw new ArgumentException("Force-instance method \"" + methodBase + "\" has invalid signature");

                    _parameters = _parameters.Skip(1).ToArray();
                    if (_paramsConverters != null)
                        _paramsConverters = _paramsConverters.Skip(1).ToArray();
                }

                if (_parameters.Length > 0 && _parameters.Last().GetCustomAttribute(typeof(ParamArrayAttribute), false) != null)
                {
                    _restPrmsArrayCreator = makeRestPrmsArrayCreator();
                }

                if (!WrapperCache.TryGetValue(methodBase, out _fastWrapper))
                    WrapperCache[methodBase] = _fastWrapper = makeFastWrapper(methodInfo);

                RequireNewKeywordLevel = RequireNewKeywordLevel.WithoutNewOnly;
            }
            else if (methodBase is ConstructorInfo)
            {
                if (!WrapperCache.TryGetValue(methodBase, out _fastWrapper))
                    WrapperCache[methodBase] = _fastWrapper = makeFastWrapper(methodBase as ConstructorInfo);

                if (_parameters.Length > 0 && _parameters.Last().GetCustomAttribute(typeof(ParamArrayAttribute), false) != null)
                {
                    _restPrmsArrayCreator = makeRestPrmsArrayCreator();
                }
            }
            else
                throw new NotImplementedException();
        }

        private MethodProxy(Context context, object hardTarget, MethodBase method, ParameterInfo[] parameters, WrapperDelegate fastWrapper, bool forceInstance)
            : base(context)
        {
            _hardTarget = hardTarget;
            _method = method;
            _parameters = parameters;
            _fastWrapper = fastWrapper;
            _forceInstance = forceInstance;
            RequireNewKeywordLevel = RequireNewKeywordLevel.WithoutNewOnly;
        }

        private RestPrmsConverter makeRestPrmsArrayCreator()
        {
            var convertArg = ((Func<int, JSValue, object>)convertArgument).GetMethodInfo();
            var processArg = ((Func<Expressions.Expression[], Context, int, object>)processArgument).GetMethodInfo();

            var context = Expression.Parameter(typeof(Context), "context");
            var arguments = Expression.Parameter(typeof(Expressions.Expression[]), "arguments");
            var argumentsObjectPrm = Expression.Parameter(typeof(Arguments), "argumentsObject");
            var restItemType = _parameters.Last().ParameterType.GetElementType();
            var returnLabel = Expression.Label("return");

            var argumentIndex = Expression.Variable(typeof(int), "argumentIndex");
            var resultArray = Expression.Variable(_parameters.Last().ParameterType, "resultArray");
            var resultArrayIndex = Expression.Variable(typeof(int), "resultArrayIndex");
            var tempValue = Expression.Variable(typeof(object), "temp");

            var resultArrayCtor = resultArray.Type.GetConstructor(new[] { typeof(int) });

            var convertedValueArgObj = Expression.Call(Expression.Constant(this), convertArg, argumentIndex, Expression.Call(argumentsObjectPrm, ArgumentsGetItemMethod, Expression.PostIncrementAssign(argumentIndex)));
            var conditionArgObj = Expression.GreaterThanOrEqual(argumentIndex, Expression.PropertyOrField(argumentsObjectPrm, nameof(Arguments.Length)));
            var arrayAssignArgObj = Expression.Assign(Expression.ArrayAccess(resultArray, Expression.PostIncrementAssign(resultArrayIndex)), Expression.Convert(convertedValueArgObj, restItemType));

            var conditionExp = Expression.GreaterThanOrEqual(argumentIndex, Expression.ArrayLength(arguments));
            var getValueExp = Expression.Call(
                                Expression.Constant(this),
                                processArg,
                                arguments,
                                context,
                                argumentIndex);
            var arrayAssignExp = Expression.Assign(Expression.ArrayAccess(resultArray, Expression.PostIncrementAssign(resultArrayIndex)), Expression.Convert(getValueExp, restItemType));

            var tree = new Expression[]
            {
                Expression.Assign(argumentIndex, Expression.Constant(_parameters.Length - 1)),
                Expression.Assign(resultArrayIndex, Expression.Constant(0)),
                Expression.IfThenElse(
                    Expression.NotEqual(argumentsObjectPrm, Expression.Constant(null)),
                    Expression.Block(
                        Expression.IfThen(
                            Expression.Equal(Expression.PropertyOrField(argumentsObjectPrm, nameof(Arguments.Length)),
                                Expression.Constant(_parameters.Length)),
                            Expression.Block(
                                Expression.Assign(tempValue,
                                    Expression.Call(Expression.Constant(this), convertArg, argumentIndex, Expression.Call(argumentsObjectPrm, ArgumentsGetItemMethod, argumentIndex))),
                                Expression.IfThen(Expression.NotEqual(tempValue, Expression.Constant(null)),
                                    Expression.Return(returnLabel, tempValue)))),
                        Expression.Assign(resultArray,
                            Expression.New(resultArrayCtor, Expression.Subtract(Expression.PropertyOrField(argumentsObjectPrm, nameof(Arguments.Length)),argumentIndex))),
                        Expression.Loop(
                            Expression.IfThenElse(conditionArgObj,
                                Expression.Return(returnLabel, Expression.Assign(tempValue, resultArray)),
                                arrayAssignArgObj))),
                    Expression.Block(
                        Expression.IfThen(
                            Expression.Equal(Expression.ArrayLength(arguments),
                                Expression.Constant(_parameters.Length)),
                            Expression.Block(
                                Expression.Assign(tempValue, getValueExp),
                                Expression.IfThen(
                                    Expression.TypeIs(tempValue, _parameters.Last().ParameterType),
                                    Expression.Return(returnLabel, tempValue)),
                                Expression.Assign(tempValue, Expression.NewArrayInit(restItemType, Expression.Convert(tempValue, restItemType))),
                                Expression.Return(returnLabel, tempValue))),
                        Expression.Assign(resultArray,
                            Expression.New(resultArrayCtor,
                                Expression.Subtract(Expression.ArrayLength(arguments), argumentIndex))),
                        Expression.Loop(
                            Expression.IfThenElse(conditionExp,
                                Expression.Return(returnLabel, Expression.Assign(tempValue, resultArray)),
                                Expression.Block(arrayAssignExp, Expression.PostIncrementAssign(argumentIndex)))))),
                Expression.Label(returnLabel),
                tempValue
            };

            var lambda = Expression.Lambda<RestPrmsConverter>(
                Expression.Block(new[] { argumentIndex, resultArray, resultArrayIndex, tempValue }, tree),
                context, arguments, argumentsObjectPrm);

            return lambda.Compile();
        }

        private WrapperDelegate makeFastWrapper(MethodInfo methodInfo)
        {
            Expression tree = null;
            var target = Expression.Parameter(typeof(object), "target");
            var context = Expression.Parameter(typeof(Context), "context");
            var arguments = Expression.Parameter(typeof(Expressions.Expression[]), "arguments");
            var argumentsObjectPrm = Expression.Parameter(typeof(Arguments), "argumentsObject");

            if (_parameters.Length == 0)
            {
                if (_forceInstance)
                {
                    tree = Expression.Call(methodInfo, Expression.Convert(target, typeof(JSValue)));
                }
                else
                {
                    if (methodInfo.IsStatic)
                        tree = Expression.Call(methodInfo);
                    else
                        tree = Expression.Call(Expression.Convert(target, methodInfo.DeclaringType), methodInfo);
                }
            }
            else
            {
                if ((_parameters.Length == 1 || (_parameters.Length == 2 && _forceInstance))
                    && _parameters[_parameters.Length - 1].ParameterType == typeof(Arguments))
                {
                    var argumentsObject = Expression.Condition(
                        Expression.NotEqual(argumentsObjectPrm, Expression.Constant(null)),
                        argumentsObjectPrm,
                        Expression.Assign(
                            argumentsObjectPrm,
                            Expression.Call(
                                ((Func<Expressions.Expression[], Context, Arguments>)Tools.CreateArguments).GetMethodInfo(),
                                arguments,
                                context)));

                    if (_forceInstance)
                    {
                        tree = Expression.Call(methodInfo, Expression.Convert(target, typeof(JSValue)), argumentsObject);
                    }
                    else
                    {
                        if (methodInfo.IsStatic)
                            tree = Expression.Call(methodInfo, argumentsObject);
                        else
                            tree = Expression.Call(Expression.Convert(target, methodInfo.DeclaringType), methodInfo, argumentsObject);
                    }
                }
                else
                {
                    var processArg = ((Func<Expressions.Expression[], Context, int, object>)processArgument).GetMethodInfo();
                    var processArgTail = ((Func<Expressions.Expression[], Context, int, object>)processArgumentsTail).GetMethodInfo();
                    var convertArg = ((Func<int, JSValue, object>)convertArgument).GetMethodInfo();

                    var prms = new Expression[_parameters.Length + (_forceInstance ? 1 : 0)];

                    if (_restPrmsArrayCreator != null)
                    {
                        prms[prms.Length - 1] =
                            Expression.Convert(
                                Expression.Call(
                                    Expression.Constant(this),
                                    ((Func<Context, Expressions.Expression[], Arguments, object>)callRestPrmsConverter).GetMethodInfo(),
                                    context,
                                    arguments,
                                    argumentsObjectPrm),
                            _parameters[_parameters.Length - 1].ParameterType);
                    }

                    var targetPrmIndex = 0;
                    if (_forceInstance)
                        prms[targetPrmIndex++] = Expression.Convert(target, typeof(JSValue));

                    for (var i = 0; targetPrmIndex < prms.Length; i++, targetPrmIndex++)
                    {
                        if (targetPrmIndex == prms.Length - 1 && _restPrmsArrayCreator != null)
                            continue;

                        prms[targetPrmIndex] = Expression.Convert(
                            Expression.Call(
                                Expression.Constant(this),
                                targetPrmIndex + 1 < prms.Length ? processArg : processArgTail,
                                arguments,
                                context,
                                Expression.Constant(i)),
                            _parameters[i].ParameterType);
                    }

                    if (methodInfo.IsStatic)
                        tree = Expression.Call(methodInfo, prms);
                    else
                        tree = Expression.Call(Expression.Convert(target, methodInfo.DeclaringType), methodInfo, prms);

                    targetPrmIndex = 0;
                    if (_forceInstance)
                        targetPrmIndex++;
                    for (var i = 0; targetPrmIndex < prms.Length; i++, targetPrmIndex++)
                    {
                        if (targetPrmIndex == prms.Length - 1 && _restPrmsArrayCreator != null)
                            continue;

                        prms[targetPrmIndex] = Expression.Convert(
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

        private WrapperDelegate makeFastWrapper(ConstructorInfo constructorInfo)
        {
            Expression tree = null;
            var target = Expression.Parameter(typeof(object), "target");
            var context = Expression.Parameter(typeof(Context), "context");
            var arguments = Expression.Parameter(typeof(Expressions.Expression[]), "arguments");
            var argumentsObjectPrm = Expression.Parameter(typeof(Arguments), "argumentsObject");

            if (_parameters.Length == 0)
            {
                tree = Expression.New(constructorInfo);
            }
            else
            {
                if (_parameters.Length == 1 && _parameters[0].ParameterType == typeof(Arguments))
                {
                    Expression argumentsObject = Expression.Condition(
                        Expression.NotEqual(argumentsObjectPrm, Expression.Constant(null)),
                        argumentsObjectPrm,
                        Expression.Assign(
                            argumentsObjectPrm,
                            Expression.Call(
                                ((Func<Expressions.Expression[], Context, Arguments>)Tools.CreateArguments).GetMethodInfo(),
                                arguments,
                                context)));

                    tree = Expression.New(constructorInfo, argumentsObject);
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

                    tree = Expression.New(constructorInfo, prms);

                    var argumentsObject = argumentsObjectPrm;

                    for (var i = 0; i < prms.Length; i++)
                    {
                        prms[i] = Expression.Convert(
                            Expression.Call(
                                Expression.Constant(this),
                                convertArg,
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

        private object callRestPrmsConverter(Context initiator, Expressions.Expression[] arguments, Arguments argumentsObject)
        {
            return _restPrmsArrayCreator(initiator, arguments, argumentsObject);
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

            if (value is not null
                && targetValue is not null
                && (value is not JSValue jsval || jsval._valueType == targetValue._valueType)
                && value == targetValue.Value)
                return targetValue;

            return Context.GlobalContext.ProxyValue(value);
        }

        private object invokeMethod(JSValue targetValue, Expressions.Expression[] argumentsSource, Arguments argumentsObject, Context initiator)
        {
            object value;
            var target = GetTargetObject(targetValue, _hardTarget);
            if (_parameters.Length == 0 && argumentsSource != null)
            {
                for (var i = 0; i < argumentsSource.Length; i++)
                    argumentsSource[i].Evaluate(initiator);
            }

            value = _fastWrapper(target, initiator, argumentsSource, argumentsObject);

            if (_returnConverter != null)
                value = _returnConverter.From(value);

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
            var res = Tools.ConvertJStoObj(target, targetType, false);
            return res;
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private object processArgument(Expressions.Expression[] arguments, Context initiator, int index)
        {
            var value = arguments.Length > index
                ? Tools.EvalExpressionSafe(initiator, arguments[index]) 
                : notExists;

            return convertArgument(index, value);
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private object convertArgument(int index, JSValue value)
        {
            var cvtArgs = ConvertArgsOptions.ThrowOnError | ConvertArgsOptions.AllowDefaultValues;
            if (_strictConversion)
                cvtArgs |= ConvertArgsOptions.StrictConversion;

            return convertArgument(
                index,
                value,
                cvtArgs);
        }

        private object convertArgument(int index, JSValue value, ConvertArgsOptions options)
        {
            if (_paramsConverters != null && _paramsConverters[index] != null)
                return _paramsConverters[index].To(value);

            var strictConversion = (options & ConvertArgsOptions.StrictConversion) == ConvertArgsOptions.StrictConversion;
            var processRest = _restPrmsArrayCreator != null && index >= _parameters.Length - 1 && (index >= _parameters.Length || value.ValueType != JSValueType.Object || !(value.Value is BaseLibrary.Array));
            var parameterInfo = processRest ? _parameters[_parameters.Length - 1] : _parameters[index];
            var parameterType = processRest ? parameterInfo.ParameterType.GetElementType() : parameterInfo.ParameterType;
            object result = null;

            if (value._valueType >= JSValueType.Object && value._oValue == null && parameterType.GetTypeInfo().IsClass)
            {
                return null;
            }
            else if (value._valueType > JSValueType.Undefined)
            {
                result = Tools.ConvertJStoObj(value, parameterType, !strictConversion);
                if (strictConversion && result == null)
                {
                    if ((options & ConvertArgsOptions.ThrowOnError) != 0)
                        ExceptionHelper.ThrowTypeError("Unable to convert " + value + " to type " + parameterType);

                    if ((options & ConvertArgsOptions.AllowDefaultValues) == 0)
                        return null;
                }
            }
            else
            {
                if (parameterType.IsAssignableFrom(value.GetType()))
                    return value;
            }

            if (result == null
                && _restPrmsArrayCreator == null
                && (options & ConvertArgsOptions.AllowDefaultValues) != 0
                && ((parameterInfo.Attributes & ParameterAttributes.HasDefault) != 0
                    || parameterInfo.ParameterType.IsValueType))
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
                        ExceptionHelper.ThrowTypeError("Unable to convert " + value + " to type " + parameterType);

                    if (parameterType.GetTypeInfo().IsValueType)
                        result = Activator.CreateInstance(parameterType);
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
                Context,
                convertTargetObject(args[0], _method.DeclaringType) ?? args[0].Value as JSObject ?? args[0],
                _method,
                _parameters,
                _fastWrapper,
                _forceInstance);
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
                return base.MakeDelegate(delegateType);
            }
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
