using System;
using System.Collections.Generic;
using System.Reflection;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core.Functions
{
#if !PORTABLE
    [Serializable]
#endif
    [Prototype(typeof(Function))]
    internal class ProxyConstructor : Function
    {
        // На втором проходе будет выбираться первый метод, 
        // для которого получится сгенерировать параметры по-умолчанию.
        // Если нужен более строгий подбор, то количество проходов нужно
        // уменьшить до одного
        private const int passesCount = 2;

        private static readonly object[] _objectA = new object[0];
        internal readonly TypeProxy proxy;
        private MethodProxy[] constructors;

        [Hidden]
        public override string name
        {
            [Hidden]
            get
            {
                return proxy.hostedType.Name;
            }
        }

        [Field]
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public override JSValue prototype
        {
            [Hidden]
            get
            {
                return _prototype ?? (_prototype = TypeProxy.GetPrototype(proxy.hostedType));
            }
            [Hidden]
            set
            {
                _prototype = value;
            }
        }

        [Hidden]
        public ProxyConstructor(TypeProxy typeProxy)
        {
            fields = typeProxy.fields;
            proxy = typeProxy;

#if PORTABLE
            if (proxy.hostedType.GetTypeInfo().ContainsGenericParameters)
                ExceptionsHelper.Throw((new TypeError(proxy.hostedType.Name + " can't be created because it's generic type.")));
#else
            if (proxy.hostedType.ContainsGenericParameters)
                ExceptionsHelper.Throw((new TypeError(proxy.hostedType.Name + " can't be created because it's generic type.")));
#endif
            var ownew = typeProxy.hostedType.IsDefined(typeof(RequireNewKeywordAttribute), true);
            var owonew = typeProxy.hostedType.IsDefined(typeof(DisallowNewKeywordAttribute), true);

            if (ownew && owonew)
                throw new InvalidOperationException("Unacceptably use of " + typeof(RequireNewKeywordAttribute).Name + " and " + typeof(DisallowNewKeywordAttribute).Name + " for same type.");

            if (ownew)
                RequireNewKeywordLevel = RequireNewKeywordLevel.WithNewOnly;
            if (owonew)
                RequireNewKeywordLevel = RequireNewKeywordLevel.WithoutNewOnly;

            if (_length == null)
                _length = new Number(0) { attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate };

#if PORTABLE
            var ctors = System.Linq.Enumerable.ToArray(typeProxy.hostedType.GetTypeInfo().DeclaredConstructors);
            List<MethodProxy> ctorsL = new List<MethodProxy>(ctors.Length + (typeProxy.hostedType.GetTypeInfo().IsValueType ? 1 : 0));
#else
            var ctors = typeProxy.hostedType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            List<MethodProxy> ctorsL = new List<MethodProxy>(ctors.Length + (typeProxy.hostedType.IsValueType ? 1 : 0));
#endif
            for (int i = 0; i < ctors.Length; i++)
            {
                if (ctors[i].IsStatic)
                    continue;

                if (!ctors[i].IsDefined(typeof(HiddenAttribute), false) || ctors[i].IsDefined(typeof(ForceUseAttribute), true))
                {
                    ctorsL.Add(new MethodProxy(ctors[i]));
                    length.iValue = System.Math.Max(ctorsL[ctorsL.Count - 1]._length.iValue, _length.iValue);
                }
            }
            ctorsL.Sort((x, y) => x.Parameters.Length == 1 && x.Parameters[0].ParameterType == typeof(Arguments) ? 1 :
                y.Parameters.Length == 1 && y.Parameters[0].ParameterType == typeof(Arguments) ? -1 :
                x.Parameters.Length - y.Parameters.Length);
            constructors = ctorsL.ToArray();
        }

        [Hidden]
        internal protected override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            if (memberScope < PropertyScope.Super && key.valueType != JSValueType.Symbol)
            {
                if (key.ToString() == "prototype") // Все прокси-прототипы read-only и non-configurable. Это и оптимизация, и устранение необходимости навешивания атрибутов
                    return prototype;
                var res = proxy.GetProperty(key, forWrite && memberScope == PropertyScope.Own, memberScope);
                if (res.Exists || (memberScope == PropertyScope.Own && forWrite))
                {
                    if (forWrite && res.NeedClone)
                        res = proxy.GetProperty(key, true, memberScope);
                    return res;
                }
                res = __proto__.GetProperty(key, forWrite, memberScope);
                if (memberScope == PropertyScope.Own && (res.valueType != JSValueType.Property || (res.attributes & JSValueAttributesInternal.Field) == 0))
                    return notExists; // если для записи, то первая ветка всё разрулит и сюда выполнение не придёт
                return res;
            }
            return base.GetProperty(key, forWrite, memberScope);
        }

        protected internal override bool DeleteProperty(JSValue name)
        {
            return proxy.DeleteProperty(name) && __proto__.DeleteProperty(name);
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments, Function newTarget)
        {
            var objc = targetObject as ObjectWrapper;
            if (construct) // new
            {

            }
            else
            {
                if (proxy.hostedType == typeof(Date))
                    return new Date().ToString();
            }

            try
            {
                object obj;
                if (proxy.hostedType == typeof(NiL.JS.BaseLibrary.Array))
                {
                    if (arguments == null)
                        obj = new NiL.JS.BaseLibrary.Array();
                    else
                        switch (arguments.length)
                        {
                            case 0:
                                obj = new NiL.JS.BaseLibrary.Array();
                                break;
                            case 1:
                                {
                                    switch (arguments.a0.valueType)
                                    {
                                        case JSValueType.Integer:
                                            obj = new NiL.JS.BaseLibrary.Array(arguments.a0.iValue);
                                            break;
                                        case JSValueType.Double:
                                            obj = new NiL.JS.BaseLibrary.Array(arguments.a0.dValue);
                                            break;
                                        default:
                                            obj = new NiL.JS.BaseLibrary.Array(arguments);
                                            break;
                                    }
                                    break;
                                }
                            default:
                                obj = new NiL.JS.BaseLibrary.Array(arguments);
                                break;
                        }
                }
                else
                {
                    if ((arguments == null || arguments.length == 0)
#if PORTABLE
 && proxy.hostedType.GetTypeInfo().IsValueType)
#else
 && proxy.hostedType.IsValueType)
#endif
                        obj = Activator.CreateInstance(proxy.hostedType);
                    else
                    {
                        object[] args = null;
                        MethodProxy constructor = findConstructor(arguments, ref args);
                        if (constructor == null)
                            ExceptionsHelper.Throw((new TypeError(proxy.hostedType.Name + " can't be created.")));
                        obj = constructor.InvokeImpl(
                            null,
                            args,
                            arguments == null ? constructor.parameters.Length != 0 ? new Arguments()
                                                                                   : null
                                              : arguments);
                    }
                }

                JSValue res = obj as JSValue;

                if (construct)
                {
                    if (res != null)
                    {
                        // Для Number, Boolean и String
                        if (res.valueType < JSValueType.Object)
                        {
                            objc.instance = obj;
                            if (objc.__prototype == null)
                                objc.__prototype = res.__proto__;
                            res = objc;
                        }
                        else if (res.oValue is JSValue)
                        {
                            res.oValue = res;
                            // На той стороне понять, по new или нет вызван конструктор не удастся,
                            // поэтому по соглашению такие типы себя настраивают так, как будто они по new,
                            // а в oValue пишут экземпляр аргумента на тот случай, если вызван конструктор типа как функция
                            // с передачей в качестве аргумента существующего экземпляра
                        }
                    }
                    else
                    {
                        objc.instance = obj;

                        objc.attributes |= proxy.hostedType.IsDefined(typeof(ImmutableAttribute), false) ? JSValueAttributesInternal.Immutable : JSValueAttributesInternal.None;
                        if (obj.GetType() == typeof(Date))
                            objc.valueType = JSValueType.Date;
                        else if (res != null)
                            objc.valueType = (JSValueType)System.Math.Max((int)objc.valueType, (int)res.valueType);

                        res = objc;
                    }
                }
                else
                {
                    if (proxy.hostedType == typeof(JSValue))
                    {
                        if ((res.oValue is JSValue) && (res.oValue as JSValue).valueType >= JSValueType.Object)
                            return res.oValue as JSValue;
                    }

                    res = res ?? new ObjectWrapper(obj)
                    {
                        attributes = JSValueAttributesInternal.SystemObject | (proxy.hostedType.IsDefined(typeof(ImmutableAttribute), false) ? JSValueAttributesInternal.Immutable : JSValueAttributesInternal.None)
                    };
                }
                return res;
            }
            catch (TargetInvocationException e)
            {
#if !PORTABLE
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Log(10, "Exception", e.Message);
#endif
                throw e.InnerException;
            }
        }

        protected internal override JSValue ConstructObject()
        {
            return new ObjectWrapper(null) { __prototype = TypeProxy.GetPrototype(proxy.hostedType) };
        }

        [Hidden]
        private MethodProxy findConstructor(Arguments argObj, ref object[] args)
        {
            args = null;
            var len = argObj == null ? 0 : argObj.length;
            for (var pass = 0; pass < passesCount; pass++)
            {
                for (int i = 0; i < constructors.Length; i++)
                {
                    if (constructors[i].parameters.Length == 1 && (constructors[i].parameters[0].ParameterType == typeof(Arguments)))
                        return constructors[i];

                    if (pass == 1 || constructors[i].parameters.Length == len)
                    {
                        if (len == 0)
                            args = _objectA;
                        else
                        {
                            args = constructors[i].ConvertArgs(argObj, pass == 1);
                            if (args == null)
                                continue;

                            for (var j = args.Length; j-- > 0;)
                            {
                                if (args[j] != null ?
                                    !constructors[i].parameters[j].ParameterType.IsAssignableFrom(args[j].GetType())
                                    :
                                    constructors[i].parameters[j].ParameterType.IsValueType)
                                {
                                    j = 0;
                                    args = null;
                                }
                            }

                            if (args == null)
                                continue;
                        }

                        return constructors[i];
                    }
                }
            }
            return null;
        }

        internal override JSObject GetDefaultPrototype()
        {
            return TypeProxy.GetPrototype(typeof(Function));
        }

        [Hidden]
        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnumerable, EnumerationMode enumerationMode)
        {
            var e = __proto__.GetEnumerator(hideNonEnumerable, enumerationMode);
            while (e.MoveNext())
                yield return e.Current;
            e = proxy.GetEnumerator(hideNonEnumerable, enumerationMode);
            while (e.MoveNext())
                yield return e.Current;
        }

        [Hidden]
        public override string ToString(bool headerOnly)
        {
            return "function " + proxy.hostedType.Name + "() { [native code] }";
        }
    }
}
