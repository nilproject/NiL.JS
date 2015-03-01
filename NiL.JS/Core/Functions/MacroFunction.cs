using System;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;
using NiL.JS.Core.TypeProxing;

namespace NiL.JS.Core.Functions
{
    /// <summary>
    /// Ограничения для макрофункций:
    /// * нет инструкции debugger
    /// * нет eval, arguments и with
    /// * не используется this
    /// * все используемые переменные и константы либо объявлены внутри функции, либо являются её аргументами
    /// * нет создания других функций
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    public sealed class MacroFunction : Function
    {
        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        public override JSObject arguments
        {
            [Hidden]
            get
            {
                if (_arguments == null)
                {
                    var args = new Arguments()
                    {
                        caller = this._caller,
                        callee = this,
                        length = creator.arguments.Length
                    };
                    for (var i = 0; i < creator.arguments.Length; i++)
                        args[i] = creator.arguments[i].cacheRes.CloneImpl();
                    _arguments = args;
                }
                return base.arguments;
            }
            [Hidden]
            set
            {
                base.arguments = value;
            }
        }

        public MacroFunction(Context context, Expressions.FunctionExpression creator)
            : base(context, creator)
        {

        }

        private JSObject[] initScope(Expressions.Expression[] arguments, Context initiator, bool storeOnly)
        {
            JSObject[] storedData = null;
            JSObject temp;
            int i;
            int j;
            storedData = new JSObject[creator.body.localVariables.Length + creator.arguments.Length];
            if (!storeOnly)
                for (i = 0; i < creator.arguments.Length; i++)
                {
                    if (i < arguments.Length)
                    {
                        storedData[i] = arguments[i].Evaluate(initiator).CloneImpl();
                        storedData[i].attributes = JSObjectAttributesInternal.DoNotDelete;
                    }
                    else
                        storedData[i] = new JSObject()
                        {
                            attributes = JSObjectAttributesInternal.Argument,
                            valueType = JSObjectType.Undefined
                        };
                }
            for (i = 0; i < creator.arguments.Length; i++)
            {
                temp = storedData[i];
                storedData[i] = creator.arguments[i].cacheRes;
                creator.arguments[i].cacheRes = temp;
            }
            j = i;
            if (!storeOnly)
                for (; i < arguments.Length; i++)
                    arguments[i].Evaluate(initiator);
            for (i = 0; i < creator.body.localVariables.Length; i++)
            {
                storedData[j++] = creator.body.localVariables[i].cacheRes;
                if (storeOnly)
                    creator.body.localVariables[i].cacheRes = null;
                else
                    creator.body.localVariables[i].cacheRes = new JSObject()
                    {
                        attributes = JSObjectAttributesInternal.DoNotDelete | (creator.body.localVariables[i].readOnly ? JSObjectAttributesInternal.ReadOnly : 0),
                        valueType = JSObjectType.Undefined
                    };
            }
            return storedData;
        }

        protected internal override JSObject InternalInvoke(JSObject self, Expressions.Expression[] arguments, Context initiator)
        {
            JSObject[] storedData = null;
            int i;
            int j;
            var body = creator.body;
            var context = this.context;
            if (body == null || body.lines.Length == 0)
            {
                correctThisBind(self, body.strict, context ?? initiator);
                for (i = 0; i < arguments.Length; i++)
                    arguments[i].Evaluate(initiator);
                notExists.valueType = JSObjectType.NotExistsInObject;
                return notExists;
            }
            lock (creator)
            {
                self = correctThisBind(self, body.strict, context);
                if (creator.recursiveDepth == 0)
                {
                    for (i = 0; i < creator.arguments.Length; i++)
                    {
                        if (i < arguments.Length)
                            creator.arguments[i].cacheRes.Assign(arguments[i].Evaluate(initiator));
                        else
                        {
                            creator.arguments[i].cacheRes.valueType = JSObjectType.Undefined;
                            creator.arguments[i].cacheRes.oValue = null;
                        }
                        creator.arguments[i].cacheContext = context;
                    }
                    for (; i < arguments.Length; i++)
                        arguments[i].Evaluate(initiator);
                    for (i = 0; i < creator.body.localVariables.Length; i++)
                    {
                        creator.body.localVariables[i].cacheRes.valueType = JSObjectType.Undefined;
                        creator.body.localVariables[i].cacheRes.oValue = null;
                        creator.body.localVariables[i].cacheContext = context;
                    }
                }
                else
                    storedData = initScope(arguments, initiator, false);
                creator.recursiveDepth++;
                if (creator.reference.descriptor != null)
                {
                    creator.reference.descriptor.cacheContext = context.parent;
                    // тонкое место. Родителем контекста может оказаться базовый контекст (тот, что над глобальным скрипта)
                    // а там о этой функции ничего не знают. Спасёт срабатывание кэша. Но если родителем, вдруг, окажется, базовый контекст, то
                    // кэш не сработает, будет попытка получить эту функцию из контекста над базовым, что приведёт к фаталу
                    creator.reference.descriptor.cacheRes = this;
                }
                var oldAbort = context.abort;
                var oldAbortInfo = context.abortInfo;
                var oldLastResult = context.lastResult;
                var oldOldContext = context.oldContext;
                var oldCaller = context.caller;
                var oldArgs = this._arguments;
                this._arguments = null;
                var oldSCaller = this._caller;
                this._caller = initiator.strict && initiator.caller != null && initiator.caller.creator.body.strict ? Function.propertiesDummySM : initiator.caller;
                context.caller = this;
                context.oldContext = null;
                bool deactivate = context.Activate();
                JSObject res = notExists;
                try
                {
                    body.Evaluate(context);
                }
                finally
                {
                    if (context.abort == AbortType.Return)
                        res = context.abortInfo;
                    context.abort = oldAbort;
                    context.abortInfo = oldAbortInfo;
                    context.lastResult = oldLastResult;
                    context.caller = oldCaller;
                    if (deactivate)
                        context.Deactivate();
                    context.oldContext = oldOldContext;
                    this._arguments = oldArgs;
                    this._caller = oldSCaller;
                    creator.recursiveDepth--;
                    if (creator.recursiveDepth > 0)
                    {
                        for (i = 0; i < creator.arguments.Length; i++)
                            creator.arguments[i].cacheRes = storedData[i];
                        j = i;
                        for (i = 0; i < creator.body.localVariables.Length; i++)
                            creator.body.localVariables[i].cacheRes = storedData[j++];
                    }
                }
                return res;
            }
        }

        public override NiL.JS.Core.JSObject Invoke(NiL.JS.Core.JSObject thisBind, NiL.JS.Core.Arguments args)
        {
            JSObject[] storedData = null;
            int rd = creator.recursiveDepth;
            if (creator.recursiveDepth > 0)
            {
                storedData = initScope(null, null, true);
                creator.recursiveDepth = 0;
            }
            JSObject res;
            try
            {
                res = base.Invoke(thisBind, args);
            }
            finally
            {
                if (rd != 0)
                {
                    int i;
                    for (i = 0; i < creator.arguments.Length; i++)
                    {
                        creator.arguments[i].cacheRes = storedData[i];
                        creator.arguments[i].cacheContext = context;
                    }
                    int j = i;
                    for (i = 0; i < creator.body.localVariables.Length; i++)
                    {
                        creator.body.localVariables[i].cacheRes = storedData[j++];
                        creator.body.localVariables[i].cacheContext = context;
                    }
                }
            }
            return res;
        }

        protected override JSObject getDefaultPrototype()
        {
            return TypeProxy.GetPrototype(typeof(Function));
        }
    }
}
