using System;
using System.Collections.Generic;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Core.Functions;
using NiL.JS.Core.Interop;
using NiL.JS.Core.JIT;
using NiL.JS.Expressions;
using NiL.JS.Statements;
using linqEx = System.Linq.Expressions;

namespace NiL.JS.BaseLibrary
{
    /// <summary>
    /// Возможные типы функции в контексте использования.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    public enum FunctionType
    {
        Function = 0,
        Getter,
        Setter,
        AnonymousFunction,
        Generator,
        Method,
        Arrow
    }

#if !PORTABLE
    [Serializable]
#endif
    public enum RequireNewKeywordLevel
    {
        Both = 0,
        WithNewOnly,
        WithoutNewOnly
    }

#if !PORTABLE
    [Serializable]
#endif
    public class Function : JSObject
    {
        private static readonly FunctionDefinition creatorDummy = new FunctionDefinition("anonymous");
        internal static readonly Function emptyFunction = new Function();
        private static readonly Function TTEProxy = new MethodProxy(typeof(Function)
#if PORTABLE
            .GetTypeInfo().GetDeclaredMethod("ThrowTypeError"))
#else
.GetMethod("ThrowTypeError", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
#endif
            {
                attributes = JSValueAttributesInternal.DoNotDelete
                | JSValueAttributesInternal.Immutable
                | JSValueAttributesInternal.DoNotEnumerate
                | JSValueAttributesInternal.ReadOnly
            };
        protected static void ThrowTypeError()
        {
            ExceptionsHelper.Throw(new TypeError("Properties caller, callee and arguments not allowed in strict mode."));
        }
        internal static readonly JSValue propertiesDummySM = new JSValue()
        {
            valueType = JSValueType.Property,
            oValue = new PropertyPair() { get = TTEProxy, set = TTEProxy },
            attributes = JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.Immutable | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.NonConfigurable
        };

        private Dictionary<Type, Delegate> delegateCache;

        internal readonly FunctionDefinition creator;
        [Hidden]
        internal readonly Context parentContext;
        [Hidden]
        public Context Context
        {
            [Hidden]
            get { return parentContext; }
        }
        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        public virtual string name
        {
            [Hidden]
            get { return creator.name; }
        }
        [Hidden]
        internal Number _length = null;
        [Field]
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public virtual JSValue length
        {
            [Hidden]
            get
            {
                if (_length == null)
                {
                    _length = new Number(0) { attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate };
                    _length.iValue = creator.parameters.Length;
                }
                return _length;
            }
        }
        [Hidden]
        public virtual FunctionType Type
        {
            [Hidden]
            get { return creator.type; }
        }
        [Hidden]
        public virtual bool Strict
        {
            [Hidden]
            get
            {
                return creator.body.strict;
            }
        }
        [Hidden]
        public virtual CodeBlock Body
        {
            [Hidden]
            get
            {
                return creator != null ? creator.body : null;
            }
        }

        [Hidden]
        public virtual RequireNewKeywordLevel RequireNewKeywordLevel
        {
            [Hidden]
            get;
            [Hidden]
            internal protected set;
        }

        #region Runtime
        [Hidden]
        internal JSValue _prototype;
        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public virtual JSValue prototype
        {
            [Hidden]
            get
            {
                if (_prototype == null)
                {
                    if ((attributes & JSValueAttributesInternal.ProxyPrototype) != 0)
                    {
                        // Вызывается в случае Function.prototype.prototype
                        _prototype = new JSValue(); // выдавать тут константу undefined нельзя, иначе будет падать на вызове defineProperty
                    }
                    else
                    {
                        var res = JSObject.CreateObject(true);
                        res.attributes = JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.NonConfigurable;
                        (res.fields["constructor"] = this.CloneImpl(false)).attributes = JSValueAttributesInternal.DoNotEnumerate;
                        _prototype = res;
                    }
                }
                return _prototype;
            }
            [Hidden]
            set
            {
                _prototype = value.oValue as JSObject ?? value;
            }
        }
        /// <summary>
        /// Объект, содержащий параметры вызова функции либо null если в данный момент функция не выполняется.
        /// </summary>
        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        public virtual JSValue arguments
        {
            [Hidden]
            get
            {
                var context = Context.GetRunnedContextFor(this);
                if (context == null)
                    return null;
                if (creator.body.strict)
                    ExceptionsHelper.Throw(new TypeError("Property arguments not allowed in strict mode."));
                if (context.arguments == null && creator.recursionDepth > 0)
                    BuildArgumentsObject();
                return context.arguments;
            }
            [Hidden]
            set
            {
                var context = Context.GetRunnedContextFor(this);
                if (context == null)
                    return;
                if (creator.body.strict)
                    ExceptionsHelper.Throw(new TypeError("Property arguments not allowed in strict mode."));
                context.arguments = value;
            }
        }

        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        public virtual JSValue caller
        {
            [Hidden]
            get
            {
                var context = Context.GetRunnedContextFor(this);
                if (context == null || context.oldContext == null)
                    return null;
                if (context.strict || (context.oldContext.strict && context.oldContext.owner != null))
                    ExceptionsHelper.Throw(new TypeError("Property caller not allowed in strict mode."));
                return context.oldContext.owner;
            }
            [Hidden]
            set
            {
                var context = Context.GetRunnedContextFor(this);
                if (context == null || context.oldContext == null)
                    return;
                if (context.strict || (context.oldContext.strict && context.oldContext.owner != null))
                    ExceptionsHelper.Throw(new TypeError("Property caller not allowed in strict mode."));
            }
        }
        #endregion

        [DoNotEnumerate]
        public Function()
        {
            attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.SystemObject;
            creator = creatorDummy;
            valueType = JSValueType.Function;
            this.oValue = this;
        }

        [DoNotEnumerate]
        public Function(Arguments args)
        {
            attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.SystemObject;
            parentContext = (Context.CurrentContext ?? Context.GlobalContext).Root;
            if (parentContext == Context.globalContext)
                throw new InvalidOperationException("Special Functions constructor can be called only in runtime.");
            var index = 0;
            int len = args.Length - 1;
            var argn = "";
            for (int i = 0; i < len; i++)
                argn += args[i] + (i + 1 < len ? "," : "");
            string code = "function (" + argn + "){" + Environment.NewLine + (len == -1 ? "undefined" : args[len]) + Environment.NewLine + "}";
            var func = FunctionDefinition.Parse(new ParsingState(Tools.RemoveComments(code, 0), code, null), ref index, FunctionType.Method);
            if (func != null && code.Length == index)
            {
                Parser.Build(ref func, 0, new Dictionary<string, VariableDescriptor>(), parentContext.strict ? CodeContext.Strict : CodeContext.None, null, null, Options.Default);
                creator = func as FunctionDefinition;
            }
            else
                ExceptionsHelper.Throw(new SyntaxError("Unknown syntax error"));
            valueType = JSValueType.Function;
            this.oValue = this;
        }

        [Hidden]
        internal Function(Context context, FunctionDefinition creator)
        {
            attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.SystemObject;
            this.parentContext = context;
            this.creator = creator;
            valueType = JSValueType.Function;
            this.oValue = this;
        }

        [Hidden]
        public JSValue Construct(Arguments arguments)
        {
            if (RequireNewKeywordLevel == BaseLibrary.RequireNewKeywordLevel.WithoutNewOnly)
            {
                ExceptionsHelper.ThrowTypeError(string.Format(Strings.InvalidTryToCreateWithNew, name));
            }

            JSValue targetObject = ConstructObject();
            return Construct(targetObject, arguments);
        }

        [Hidden]
        public JSValue Construct(JSValue targetObject, Arguments arguments)
        {
            if (RequireNewKeywordLevel == BaseLibrary.RequireNewKeywordLevel.WithoutNewOnly)
            {
                ExceptionsHelper.ThrowTypeError(string.Format(Strings.InvalidTryToCreateWithNew, name));
            }

            var res = Invoke(true, targetObject, arguments);
            if (res.valueType < JSValueType.Object || res.oValue == null)
                return targetObject;
            return res;
        }

        protected internal virtual JSValue ConstructObject()
        {
            JSValue targetObject = new JSObject() { valueType = JSValueType.Object };
            targetObject.oValue = targetObject;
            targetObject.__proto__ = prototype.valueType < JSValueType.Object ? GlobalPrototype : prototype.oValue as JSObject;

            return targetObject;
        }

        internal virtual JSValue InternalInvoke(JSValue targetObject, Expression[] arguments, Context initiator, bool withSpread, bool construct)
        {
            if (!construct && !withSpread && this.GetType() == typeof(Function))
            {
                var body = creator.body;
                var result = notExists;
                notExists.valueType = JSValueType.NotExists;
                for (; ; )
                {
                    if (body != null)
                    {
                        if (body.lines.Length == 1)
                        {
                            var ret = body.lines[0] as ReturnStatement;
                            if (ret != null)
                            {
                                if (ret.Body != null)
                                {
                                    if (ret.Body.ContextIndependent)
                                        result = ret.Body.Evaluate(null);
                                    else
                                        break;
                                }
                            }
                            else
                                break;
                        }
                        else if (body.lines.Length != 0)
                            break;
                    }
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (!arguments[i].Evaluate(initiator).IsDefined)
                        {
                            if (creator.parameters.Length > i && creator.parameters[i].initializer != null)
                                creator.parameters[i].initializer.Evaluate(parentContext);
                        }
                    }
                    return result;
                }

                // быстро выполнить не получилось. 
                // Попробуем чуточку медленее
                if (creator != null
                    && !creator.statistic.ContainsArguments
                    && !creator.statistic.ContainsRestParameters
                    && !creator.statistic.ContainsEval
                    && !creator.statistic.ContainsWith
                    && creator.parameters.Length == arguments.Length // из-за необходимости иметь возможность построить аргументы, если они потребуются
                    && arguments.Length < 9)
                {
                    return fastInvoke(targetObject, arguments, initiator);
                }
            }

            // Совсем медленно. Плохая функция попалась
            Arguments argumentsObject = new Core.Arguments(initiator);
            IList<JSValue> spreadSource = null;

            int targetIndex = 0;
            int sourceIndex = 0;
            int spreadIndex = 0;

            while (sourceIndex < arguments.Length)
            {
                if (spreadSource != null)
                {
                    argumentsObject[targetIndex] = spreadSource[spreadIndex];
                    spreadIndex++;
                    if (spreadIndex == spreadSource.Count)
                    {
                        spreadSource = null;
                        sourceIndex++;
                    }
                }
                else
                {
                    var value = Tools.PrepareArg(initiator, arguments[sourceIndex]);
                    if (value.valueType == JSValueType.SpreadOperatorResult)
                    {
                        spreadIndex = 0;
                        spreadSource = value.oValue as IList<JSValue>;
                        continue;
                    }
                    else
                    {
                        sourceIndex++;
                        argumentsObject[targetIndex] = value;
                    }
                }
                targetIndex++;
            }

            argumentsObject.length = targetIndex;

            initiator.objectSource = null;

            if (construct)
            {
                if (targetObject == null || targetObject.valueType < JSValueType.Object)
                    return Construct(argumentsObject);
                return Construct(targetObject, argumentsObject);
            }
            else
                return Call(targetObject, argumentsObject);
        }

        private JSValue fastInvoke(JSValue targetObject, Expression[] arguments, Context initiator)
        {
#if DEBUG && !PORTABLE
            if (creator.trace)
                System.Console.WriteLine("DEBUG: Run \"" + creator.Reference.Name + "\"");
#endif
            var body = creator.body;
            targetObject = correctTargetObject(targetObject, body.strict);
            if (creator.recursionDepth > creator.parametersStored) // рекурсивный вызов.
            {
                storeParameters();
                creator.parametersStored++;
            }

            JSValue res = null;
            Arguments args = null;
            bool tailCall = true;
            for (; ; )
            {
                var internalContext = new Context(parentContext, false, this);
                if (creator.type == FunctionType.Arrow)
                    internalContext.thisBind = parentContext.thisBind;
                else
                    internalContext.thisBind = targetObject;
                if (tailCall)
                    initParametersFast(arguments, initiator, internalContext);
                else
                    initParameters(args, internalContext);

                // Эта строка обязательно должна находиться после инициализации параметров
                creator.recursionDepth++;

                if (this.creator.reference.descriptor != null && creator.reference.descriptor.cacheRes == null)
                {
                    creator.reference.descriptor.cacheContext = internalContext.parent;
                    creator.reference.descriptor.cacheRes = this;
                }
                internalContext.strict |= body.strict;
                internalContext.variables = body.variables;
                internalContext.Activate();
                try
                {
                    res = evaluate(internalContext);
                    if (internalContext.abortType == AbortType.TailRecursion)
                    {
                        tailCall = false;
                        args = internalContext.abortInfo as Arguments;
                    }
                    else
                        tailCall = true;
                }
                finally
                {
#if DEBUG && !PORTABLE
                    if (creator.trace)
                        System.Console.WriteLine("DEBUG: Exit \"" + creator.Reference.Name + "\"");
#endif
                    creator.recursionDepth--;
                    if (creator.parametersStored > creator.recursionDepth)
                        creator.parametersStored--;
                    exit(internalContext);
                }
                if (tailCall)
                    break;
                targetObject = correctTargetObject(internalContext.objectSource, body.strict);
            }
            return res;
        }

        [Hidden]
        public JSValue Call(Arguments args)
        {
            return Call(undefined, args);
        }

        [Hidden]
        public JSValue Call(JSValue targetObject, Arguments arguments)
        {
            if (RequireNewKeywordLevel == BaseLibrary.RequireNewKeywordLevel.WithNewOnly)
            {
                ExceptionsHelper.ThrowTypeError(string.Format(Strings.InvalidTryToCreateWithoutNew, name));
            }

            targetObject = correctTargetObject(targetObject, creator.body.strict);
            return Invoke(false, targetObject, arguments);
        }

        protected virtual JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
        {

#if DEBUG && !PORTABLE
            if (creator.trace)
                System.Console.WriteLine("DEBUG: Run \"" + creator.Reference.Name + "\"");
#endif
            JSValue res = null;
            var body = creator.body;
            if (body.lines.Length == 0)
            {
                notExists.valueType = JSValueType.NotExists;
                return notExists;
            }
            var ceocw = creator.statistic.ContainsEval || creator.statistic.ContainsWith;
            if (creator.recursionDepth > creator.parametersStored) // рекурсивный вызов.
            {
                if (!ceocw)
                    storeParameters();
                creator.parametersStored = creator.recursionDepth;
            }
            if (arguments == null)
            {
                var cc = Context.CurrentContext;
                arguments = new Arguments(cc);
            }
            for (; ; ) // tail recursion catcher
            {
                creator.recursionDepth++;
                var internalContext = new Context(parentContext, ceocw, this);
                internalContext.Activate();
                try
                {
                    if (this.creator.reference.descriptor != null && creator.reference.descriptor.cacheRes == null)
                    {
                        creator.reference.descriptor.cacheContext = internalContext.parent;
                        creator.reference.descriptor.cacheRes = this;
                    }
                    internalContext.thisBind = targetObject;
                    internalContext.strict |= body.strict;
                    internalContext.variables = body.variables;
                    if (creator.type == FunctionType.Arrow)
                    {
                        internalContext.arguments = internalContext.parent.arguments;
                        internalContext.thisBind = internalContext.parent.thisBind;
                    }
                    else
                    {
                        internalContext.arguments = arguments;
                        if (ceocw)
                            internalContext.fields["arguments"] = internalContext.arguments;
                        if (body.strict)
                        {
                            arguments.attributes |= JSValueAttributesInternal.ReadOnly;
                            arguments.callee = propertiesDummySM;
                            arguments.caller = propertiesDummySM;
                        }
                        else
                        {
                            arguments.callee = this;
                        }
                    }
                    initParameters(arguments, internalContext);
                    res = evaluate(internalContext);
                }
                finally
                {
#if DEBUG && !PORTABLE
                    if (creator.trace)
                        System.Console.WriteLine("DEBUG: Exit \"" + creator.Reference.Name + "\"");
#endif
                    creator.recursionDepth--;
                    if (creator.parametersStored > creator.recursionDepth)
                        creator.parametersStored = creator.recursionDepth;
                    exit(internalContext);
                }
                if (res != null) // tail recursion
                    break;
                arguments = internalContext.abortInfo as Arguments;
                targetObject = correctTargetObject(internalContext.objectSource, body.strict);
            }
            return res;
        }

        private JSValue evaluate(Context internalContext)
        {
            initVariables(creator.body, internalContext);
            creator.body.Evaluate(internalContext);
            if (internalContext.abortType == AbortType.TailRecursion)
                return null;
            var ai = internalContext.abortInfo;
            if (ai == null || ai.valueType < JSValueType.Undefined)
            {
                notExists.valueType = JSValueType.NotExists;
                return notExists;
            }
            else if (ai.valueType == JSValueType.Undefined)
                return undefined;
            else
            {
                // константы и новосозданные объекты копировать нет смысла
                if ((ai.attributes & JSValueAttributesInternal.SystemObject) == 0)
                    return ai.CloneImpl(false);
                return ai;
            }
        }

        private void exit(Context internalContext)
        {
            if (internalContext.abortType != AbortType.TailRecursion && creator.recursionDepth == 0)
            {
                var i = creator.body.localVariables.Length;
                for (; i-- > 0; )
                {
                    creator.body.localVariables[i].cacheContext = null;
                    creator.body.localVariables[i].cacheRes = null;
                }
                for (i = creator.parameters.Length; i-- > 0; )
                {
                    creator.parameters[i].cacheContext = null;
                    creator.parameters[i].cacheRes = null;
                }
            }
            internalContext.abortType = AbortType.Return;
            internalContext.Deactivate();
        }

        private void initParametersFast(Expression[] arguments, Core.Context initiator, Context internalContext)
        {
            JSValue a0 = null,
                    a1 = null,
                    a2 = null,
                    a3 = null,
                    a4 = null,
                    a5 = null,
                    a6 = null,
                    a7 = null; // Вместо кучи, выделяем память на стеке

            var argumentsCount = arguments.Length;
            if (creator.parameters.Length != argumentsCount)
                throw new ArgumentException("Invalid arguments count");
            if (argumentsCount > 8)
                throw new ArgumentException("To many arguments");
            if (argumentsCount == 0)
                return;

            /*
             * Да, от этого кода можно вздрогнуть, но по ряду причин лучше сделать не получится.
             * Такая она цена оптимизации
             */

            /*
             * Эти два блока нельзя смешивать. Текущие значения параметров могут быть использованы для расчёта новых. 
             * Поэтому заменять значения можно только после полного расчёта новых значений
             */

            a0 = Tools.PrepareArg(initiator, arguments[0]);
            if (argumentsCount > 1)
            {
                a1 = Tools.PrepareArg(initiator, arguments[1]);
                if (argumentsCount > 2)
                {
                    a2 = Tools.PrepareArg(initiator, arguments[2]);
                    if (argumentsCount > 3)
                    {
                        a3 = Tools.PrepareArg(initiator, arguments[3]);
                        if (argumentsCount > 4)
                        {
                            a4 = Tools.PrepareArg(initiator, arguments[4]);
                            if (argumentsCount > 5)
                            {
                                a5 = Tools.PrepareArg(initiator, arguments[5]);
                                if (argumentsCount > 6)
                                {
                                    a6 = Tools.PrepareArg(initiator, arguments[6]);
                                    if (argumentsCount > 7)
                                    {
                                        a7 = Tools.PrepareArg(initiator, arguments[7]);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            setParamValue(0, a0, internalContext);
            if (argumentsCount > 1)
            {
                setParamValue(1, a1, internalContext);
                if (argumentsCount > 2)
                {
                    setParamValue(2, a2, internalContext);
                    if (argumentsCount > 3)
                    {
                        setParamValue(3, a3, internalContext);
                        if (argumentsCount > 4)
                        {
                            setParamValue(4, a4, internalContext);
                            if (argumentsCount > 5)
                            {
                                setParamValue(5, a5, internalContext);
                                if (argumentsCount > 6)
                                {
                                    setParamValue(6, a6, internalContext);
                                    if (argumentsCount > 7)
                                    {
                                        setParamValue(7, a7, internalContext);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void setParamValue(int index, JSValue value, Context context)
        {
            if (creator.parameters[index].assignations != null)
            {
                value = value.CloneImpl(false);
                value.attributes |= JSValueAttributesInternal.Argument;
            }
            else
                value.attributes &= ~JSValueAttributesInternal.Cloned;
            if (!value.IsDefined && creator.parameters.Length > index && creator.parameters[index].initializer != null)
                value.Assign(creator.parameters[index].initializer.Evaluate(context));
            creator.parameters[index].cacheRes = value;
            creator.parameters[index].cacheContext = context;
            if (creator.parameters[index].captured)
            {
                if (context.fields == null)
                    context.fields = getFieldsContainer();
                context.fields[creator.parameters[index].name] = value;
            }
        }

        internal void BuildArgumentsObject()
        {
            var context = Context.GetRunnedContextFor(this);
            if (context != null && context.arguments == null)
            {
                var args = new Arguments()
                {
                    caller = context.oldContext != null ? context.oldContext.owner : null,
                    callee = this,
                    length = creator.parameters.Length
                };
                for (var i = 0; i < creator.parameters.Length; i++)
                {
                    if (creator.body.strict)
                        args[i] = creator.parameters[i].cacheRes.CloneImpl(false);
                    else
                        args[i] = creator.parameters[i].cacheRes;
                }
                context.arguments = args;
            }
        }

        private void initParameters(Arguments args, Context internalContext)
        {
            var ceaw = creator.statistic.ContainsEval || creator.statistic.ContainsArguments || creator.statistic.ContainsWith;
            int min = System.Math.Min(args.length, creator.parameters.Length - (creator.statistic.ContainsRestParameters ? 1 : 0));

            Array restArray = null;
            if (creator.statistic.ContainsRestParameters)
            {
                restArray = new Array();
            }

            for (var i = 0; i < min; i++)
            {
                JSValue t = args[i];
                var prm = creator.parameters[i];
                if (!t.IsDefined && prm.initializer != null)
                    t = prm.initializer.Evaluate(internalContext);
                if (creator.body.strict)
                {
                    if (ceaw)
                    {
                        args[i] = t.CloneImpl(false);
                        t = t.CloneImpl(false);
                    }
                    else if (prm.assignations != null)
                    {
                        t = t.CloneImpl(false);
                        args[i] = t;
                    }
                }
                else
                {
                    if (prm.assignations != null
                        || ceaw
                        || (t.attributes & JSValueAttributesInternal.Temporary) != 0)
                    {
                        t = t.CloneImpl(false);
                        args[i] = t;
                        t.attributes |= JSValueAttributesInternal.Argument;
                    }
                }
                t.attributes &= ~JSValueAttributesInternal.Cloned;
                if (prm.captured || ceaw)
                    (internalContext.fields ?? (internalContext.fields = getFieldsContainer()))[prm.Name] = t;
                prm.cacheContext = internalContext;
                prm.cacheRes = t;
                if (string.CompareOrdinal(prm.name, "arguments") == 0)
                    internalContext.arguments = t;
            }

            for (var i = min; i < args.length; i++)
            {
                JSValue t = args[i];
                if (ceaw)
                    args[i] = t = t.CloneImpl(false);
                t.attributes |= JSValueAttributesInternal.Argument;

                if (restArray != null)
                {
                    restArray.data.Add(t);
                }
            }

            for (var i = min; i < creator.parameters.Length; i++)
            {
                var arg = creator.parameters[i];
                if (arg.initializer != null)
                {
                    if (ceaw || arg.assignations != null)
                    {
                        arg.cacheRes = arg.initializer.Evaluate(internalContext).CloneImpl(false);
                    }
                    else
                    {
                        arg.cacheRes = arg.initializer.Evaluate(internalContext);
                        if (!arg.cacheRes.IsDefined)
                            arg.cacheRes = undefined;
                    }
                }
                else
                {
                    if (ceaw || arg.assignations != null)
                    {
                        if (i == min && restArray != null)
                            arg.cacheRes = restArray.CloneImpl(false);
                        else
                            arg.cacheRes = new JSValue() { valueType = JSValueType.Undefined };
                        arg.cacheRes.attributes = JSValueAttributesInternal.Argument;
                    }
                    else
                    {
                        if (i == min && restArray != null)
                            arg.cacheRes = restArray;
                        else
                            arg.cacheRes = JSValue.undefined;
                    }
                }
                arg.cacheContext = internalContext;
                if (arg.captured || ceaw)
                {
                    if (internalContext.fields == null)
                        internalContext.fields = getFieldsContainer();
                    internalContext.fields[arg.Name] = arg.cacheRes;
                }
                if (string.CompareOrdinal(arg.name, "arguments") == 0)
                    internalContext.arguments = arg.cacheRes;
            }
        }

        private void initVariables(CodeBlock body, Context internalContext)
        {
            if (body.localVariables != null)
            {
                var cew = creator.statistic.ContainsEval || creator.statistic.ContainsWith;
                for (var i = body.localVariables.Length; i-- > 0; )
                {
                    var v = body.localVariables[i];
                    bool isArg = string.CompareOrdinal(v.name, "arguments") == 0;
                    if (isArg && v.initializer == null)
                        continue;
                    JSValue f = new JSValue() { valueType = JSValueType.Undefined, attributes = JSValueAttributesInternal.DoNotDelete };
                    if (v.captured || cew)
                        (internalContext.fields ?? (internalContext.fields = getFieldsContainer()))[v.name] = f;
                    if (v.initializer != null)
                        f.Assign(v.initializer.Evaluate(internalContext));
                    if (v.isReadOnly)
                        f.attributes |= JSValueAttributesInternal.ReadOnly;
                    v.cacheRes = f;
                    v.cacheContext = internalContext;
                    if (isArg)
                        internalContext.arguments = f;
                }
            }
        }

        internal JSValue correctTargetObject(JSValue thisBind, bool strict)
        {
            if (thisBind == null)
                return strict ? undefined : parentContext != null ? parentContext.Root.thisBind : null;
            else if (parentContext != null)
            {
                if (!strict) // Поправляем this
                {
                    if (thisBind.valueType > JSValueType.Undefined && thisBind.valueType < JSValueType.Object)
                        return thisBind.ToObject();
                    else if (thisBind.valueType <= JSValueType.Undefined || thisBind.oValue == null)
                        return parentContext.Root.thisBind;
                }
                else if (thisBind.valueType <= JSValueType.Undefined)
                    return undefined;
            }
            return thisBind;
        }

        private void storeParameters()
        {
            if (creator.parameters.Length != 0)
            {
                var context = creator.parameters[0].cacheContext;
                if (context.fields == null)
                    context.fields = getFieldsContainer();
                for (var i = 0; i < creator.parameters.Length; i++)
                    context.fields[creator.parameters[i].Name] = creator.parameters[i].cacheRes;
            }
            if (creator.body.localVariables != null && creator.body.localVariables.Length > 0)
            {
                var context = creator.body.localVariables[0].cacheContext;
                if (context.fields == null)
                    context.fields = getFieldsContainer();
                for (var i = 0; i < creator.body.localVariables.Length; i++)
                    context.fields[creator.body.localVariables[i].Name] = creator.body.localVariables[i].cacheRes;
            }
        }

        [Hidden]
        internal protected override JSValue GetMember(JSValue nameObj, bool forWrite, MemberScope memberScope)
        {
            if (memberScope < MemberScope.Super && nameObj.valueType != JSValueType.Symbol)
            {
                string name = nameObj.ToString();
                if (creator.body.strict && (name == "caller" || name == "arguments"))
                    return propertiesDummySM;
                if ((attributes & JSValueAttributesInternal.ProxyPrototype) != 0 && name == "prototype")
                    return prototype;
            }
            return base.GetMember(nameObj, forWrite, memberScope);
        }

        [CLSCompliant(false)]
        [DoNotEnumerate]
        [ArgumentsLength(0)]
        public new virtual JSValue toString(Arguments args)
        {
            return ToString();
        }

        [Hidden]
        public override sealed string ToString()
        {
            return ToString(false);
        }

        [Hidden]
        public virtual string ToString(bool headerOnly)
        {
            StringBuilder res = new StringBuilder();
            switch (creator.type)
            {
                case FunctionType.Generator:
                    res.Append("function*");
                    break;
                case FunctionType.Getter:
                    res.Append("get");
                    break;
                case FunctionType.Setter:
                    res.Append("set");
                    break;
                default:
                    res.Append("function");
                    break;
            }
            res.Append(" ").Append(name).Append("(");
            if (creator != null && creator.parameters != null)
                for (int i = 0; i < creator.parameters.Length; )
                    res.Append(creator.parameters[i].Name).Append(++i < creator.parameters.Length ? "," : "");
            res.Append(")");
            if (!headerOnly)
                res.Append(' ').Append(creator != creatorDummy ? creator.body as object : "{ [native code] }");
            return res.ToString();
        }

        [Hidden]
        public override JSValue valueOf()
        {
            return base.valueOf();
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public JSValue call(Arguments args)
        {
            var newThis = args[0];
            var prmlen = args.length - 1;
            if (prmlen >= 0)
            {
                for (var i = 0; i <= prmlen; i++)
                    args[i] = args[i + 1];
                args[prmlen] = null;
                args.length = prmlen;
            }
            else
                args[0] = null;
            return Call(newThis, args);
        }

        [DoNotEnumerate]
        [ArgumentsLength(2)]
        [AllowNullArguments]
        public JSValue apply(Arguments args)
        {
            var nargs = args ?? new Arguments();
            var argsSource = nargs[1];
            var self = nargs[0];
            if (args != null)
                nargs.Reset();
            if (argsSource.IsDefined)
            {
                if (argsSource.valueType < JSValueType.Object)
                    ExceptionsHelper.Throw(new TypeError("Argument list has wrong type."));
                var len = argsSource["length"];
                if (len.valueType == JSValueType.Property)
                    len = (len.oValue as PropertyPair).get.Call(argsSource, null);
                nargs.length = Tools.JSObjectToInt32(len);
                if (nargs.length >= 50000)
                    ExceptionsHelper.Throw(new RangeError("Too many arguments."));
                for (var i = nargs.length; i-- > 0; )
                    nargs[i] = argsSource[Tools.Int32ToString(i)];
            }
            return Call(self, nargs);
        }

        [DoNotEnumerate]
        public JSValue bind(Arguments args)
        {
            var newThis = args.Length > 0 ? args[0] : null;
            var strict = (creator.body != null && creator.body.strict) || Context.CurrentContext.strict;
            if ((newThis != null && newThis.valueType > JSValueType.Undefined) || strict)
                return new BindedFunction(this, args);
            return this;
        }

        [Hidden]
        public virtual Delegate MakeDelegate(Type delegateType)
        {
            if (delegateCache != null)
            {
                Delegate cachedDelegate;
                if (delegateCache.TryGetValue(delegateType, out cachedDelegate))
                    return cachedDelegate;
            }

            var invokeMethod = delegateType.GetMethod("Invoke");
            var prms = invokeMethod.GetParameters();

            var handlerArgumentsParameters = new linqEx.ParameterExpression[prms.Length];
            for (var i = 0; i < prms.Length; i++)
            {
                handlerArgumentsParameters[i] = linqEx.Expression.Parameter(prms[i].ParameterType, prms[i].Name);
            }

            var argumentsParameter = linqEx.Expression.Parameter(typeof(Arguments), "arguments");
            var expressions = new List<linqEx.Expression>();

            if (prms.Length != 0)
            {
                expressions.Add(linqEx.Expression.Assign(argumentsParameter, linqEx.Expression.New(typeof(Arguments))));
                for (var i = 0; i < handlerArgumentsParameters.Length; i++)
                {
                    expressions.Add(linqEx.Expression.Call(argumentsParameter, typeof(Arguments).GetMethod("Add"),
                        linqEx.Expression.Call(JITHelpers.methodof(TypeProxy.Proxy), handlerArgumentsParameters[i])));
                }
            }

            var callTree = linqEx.Expression.Call(linqEx.Expression.Constant(this), JITHelpers.methodof(Call), argumentsParameter);

            if (invokeMethod.ReturnParameter.ParameterType != typeof(void))
            {
                var asMethod = typeof(JSValue).GetMethod("As").MakeGenericMethod(invokeMethod.ReturnParameter.ParameterType);
                callTree = linqEx.Expression.Call(callTree, asMethod);
            }

            expressions.Add(callTree);
            var result = linqEx.Expression.Block(new[] { argumentsParameter }, expressions);

            var @delegate = linqEx.Expression.Lambda(delegateType, result, "<delegateWrapper>" + name, handlerArgumentsParameters).Compile();

            if (delegateCache == null)
                delegateCache = new Dictionary<Type, Delegate>();
            delegateCache.Add(delegateType, @delegate);

            return @delegate;
        }
    }
}
