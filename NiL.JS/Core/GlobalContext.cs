﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Functions;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;
using System.Dynamic;
using System.Threading.Tasks;
using NiL.JS.Backward;
using System.Runtime.ExceptionServices;

namespace NiL.JS.Core
{
    public enum IndexersSupport
    {
        WithAttributeOnly = 0,
        ForceEnable,
        ForceDisable
    }

    public enum GlobalObjectsAssignMode
    {
        Allow = 0,
        ScriptLocal,
        Deny,
        DenyThrowException
    }

    [Flags]
    public enum MarshalinOptions
    {
        None = 0,
        DictionaryAsObject = 1 << 0,
    }

#if !NETCORE
    [Serializable]
#endif
    public sealed class GlobalContext : Context
    {
        internal JSObject _globalPrototype;
        private readonly Dictionary<Type, JSObject> _proxies;

        internal JSObject _numberPrototype;
        internal JSObject _booleanPrototype;
        internal JSObject _symbolPrototype;
        internal JSObject _stringPrototype;

        public string Name { get; private set; }
        public IndexersSupport IndexersSupport { get; set; }
        public GlobalObjectsAssignMode GlobalObjectsAssignMode { get; set; }
        public JsonSerializersRegistry JsonSerializersRegistry { get; set; }
        public TimeZoneInfo CurrentTimeZone { get; set; }
        public MarshalinOptions MarshalingOptions { get; set; }

        public GlobalContext()
            : this("")
        {
            Name = null;
        }

        public GlobalContext(string name)
            : base(null)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Name = name;

            _proxies = new Dictionary<Type, JSObject>();
            JsonSerializersRegistry = new JsonSerializersRegistry();

            ResetContext();
        }

        internal void ResetContext()
        {
            if (_parent != null)
                throw new InvalidOperationException("Try to reset non-global context");

            ActivateInCurrentThread();
            try
            {
                if (_variables != null)
                    _variables.Clear();
                else
                    _variables = JSObject.getFieldsContainer();

                _proxies.Clear();
                _globalPrototype = null;

                var objectConstructor = GetConstructor(typeof(JSObject)) as Function;
                _variables.Add("Object", objectConstructor);
                objectConstructor._attributes |= JSValueAttributesInternal.DoNotDelete;

                _globalPrototype = objectConstructor.prototype as JSObject;
                _globalPrototype._objectPrototype = JSValue.@null;

                _stringPrototype = GetPrototype(typeof(BaseLibrary.String));
                _numberPrototype = GetPrototype(typeof(Number));
                _booleanPrototype = GetPrototype(typeof(BaseLibrary.Boolean));
                _symbolPrototype = GetPrototype(typeof(Symbol));

                DefineConstructor(typeof(BaseLibrary.Math));
                DefineConstructor(typeof(BaseLibrary.Array));
                DefineConstructor(typeof(JSON));
                DefineConstructor(typeof(BaseLibrary.String));
                DefineConstructor(typeof(Function));
                DefineConstructor(typeof(Date));
                DefineConstructor(typeof(Number));
                DefineConstructor(typeof(Symbol));
                DefineConstructor(typeof(BaseLibrary.Boolean));
                DefineConstructor(typeof(Error));
                DefineConstructor(typeof(TypeError));
                DefineConstructor(typeof(ReferenceError));
                DefineConstructor(typeof(EvalError));
                DefineConstructor(typeof(RangeError));
                DefineConstructor(typeof(URIError));
                DefineConstructor(typeof(SyntaxError));
                DefineConstructor(typeof(RegExp));
                DefineConstructor(typeof(ArrayBuffer));
                DefineConstructor(typeof(Int8Array));
                DefineConstructor(typeof(Uint8Array));
                DefineConstructor(typeof(Uint8ClampedArray));
                DefineConstructor(typeof(Int16Array));
                DefineConstructor(typeof(Uint16Array));
                DefineConstructor(typeof(Int32Array));
                DefineConstructor(typeof(Uint32Array));
                DefineConstructor(typeof(Float32Array));
                DefineConstructor(typeof(Float64Array));
                DefineConstructor(typeof(Promise));
                DefineConstructor(typeof(Map));
                DefineConstructor(typeof(Set));

                DefineConstructor(typeof(Debug));
#if !PORTABLE
                DefineVariable("console").Assign(ProxyValue(new JSConsole()));
#endif

                #region Base Functions
                DefineVariable("eval").Assign(new EvalFunction());
                _variables["eval"]._attributes |= JSValueAttributesInternal.Eval;
                DefineVariable("isNaN").Assign(new ExternalFunction(GlobalFunctions.isNaN));
                DefineVariable("unescape").Assign(new ExternalFunction(GlobalFunctions.unescape));
                DefineVariable("escape").Assign(new ExternalFunction(GlobalFunctions.escape));
                DefineVariable("encodeURI").Assign(new ExternalFunction(GlobalFunctions.encodeURI));
                DefineVariable("encodeURIComponent").Assign(new ExternalFunction(GlobalFunctions.encodeURIComponent));
                DefineVariable("decodeURI").Assign(new ExternalFunction(GlobalFunctions.decodeURI));
                DefineVariable("decodeURIComponent").Assign(new ExternalFunction(GlobalFunctions.decodeURIComponent));
                DefineVariable("isFinite").Assign(new ExternalFunction(GlobalFunctions.isFinite));
                DefineVariable("parseFloat").Assign(new ExternalFunction(GlobalFunctions.parseFloat));
                DefineVariable("parseInt").Assign(new ExternalFunction(GlobalFunctions.parseInt));
                #endregion
#if DEV
                DefineVariable("__pinvoke").Assign(new ExternalFunction(GlobalFunctions.__pinvoke));
#endif
                #region Consts
                _variables["undefined"] = JSValue.undefined;
                _variables["Infinity"] = Number.POSITIVE_INFINITY;
                _variables["NaN"] = Number.NaN;
                _variables["null"] = JSValue.@null;
                #endregion

                foreach (var v in _variables.Values)
                    v._attributes |= JSValueAttributesInternal.DoNotEnumerate;
            }
            finally
            {
                Deactivate();
            }

            CurrentTimeZone = TimeZoneInfo.Local;
        }

        public void ActivateInCurrentThread()
        {
            if (CurrentContext != null)
            {
                if (CurrentContext is GlobalContext)
                    CurrentContext.Deactivate();
                else
                    throw new InvalidOperationException();
            }

            if (!Activate())
                throw new Exception("Unable to activate base context");
        }

        public new void Deactivate(bool dropStackFrame = true)
        {
            if (CurrentContext != this)
                throw new InvalidOperationException();

            if (base.Deactivate(dropStackFrame) != null)
                throw new InvalidOperationException("Invalid state");
        }

        internal JSObject GetPrototype(Type type)
        {
            return (GetConstructor(type) as Function)?.prototype as JSObject;
        }

        public JSObject GetConstructor(Type type)
        {
            if (!_proxies.TryGetValue(type, out JSObject constructor))
            {
                lock (_proxies)
                {
                    JSObject dynamicProxy = null;

                    if (type.GetTypeInfo().ContainsGenericParameters)
                    {
                        constructor = GetGenericTypeSelector(new[] { type });
                    }
                    else
                    {
                        var indexerSupport = IndexersSupport == IndexersSupport.ForceEnable
                            || (IndexersSupport == IndexersSupport.WithAttributeOnly && type.GetTypeInfo().IsDefined(typeof(UseIndexersAttribute), false));

                        var staticProxy = new StaticProxy(this, type, indexerSupport);
                        if (type.GetTypeInfo().IsAbstract)
                        {
                            _proxies[type] = staticProxy;
                            return staticProxy;
                        }

                        JSObject parentPrototype = null;
                        var pa = type.GetTypeInfo().GetCustomAttributes(typeof(PrototypeAttribute), true).ToArray();
                        if (pa.Length != 0 && (pa[0] as PrototypeAttribute).PrototypeType != type)
                        {
                            var parentType = (pa[0] as PrototypeAttribute).PrototypeType;
                            parentPrototype = (GetConstructor(parentType) as Function).prototype as JSObject;

                            if ((pa[0] as PrototypeAttribute).Replace && parentType.IsAssignableFrom(type))
                            {
                                dynamicProxy = parentPrototype;
                            }
                            else
                            {
                                dynamicProxy = new PrototypeProxy(this, type, indexerSupport)
                                {
                                    _objectPrototype = parentPrototype
                                };
                            }
                        }
                        else
                        {
                            dynamicProxy = new PrototypeProxy(this, type, indexerSupport);
                        }

                        if (type == typeof(JSObject))
                            constructor = new ObjectConstructor(this, staticProxy, dynamicProxy);
                        else
                            constructor = new ConstructorProxy(this, staticProxy, dynamicProxy);

                        if (type.GetTypeInfo().IsDefined(typeof(ImmutableAttribute), false))
                            dynamicProxy._attributes |= JSValueAttributesInternal.Immutable;
                        constructor._attributes = dynamicProxy._attributes;
                        dynamicProxy._attributes |= JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.NonConfigurable | JSValueAttributesInternal.ReadOnly;

                        if (dynamicProxy != parentPrototype && type != typeof(ConstructorProxy))
                        {
                            dynamicProxy._fields["constructor"] = constructor;
                        }
                    }

                    _proxies[type] = constructor;

                    if (dynamicProxy != null && typeof(JSValue).IsAssignableFrom(type))
                    {
                        if (dynamicProxy._objectPrototype == null)
                            dynamicProxy._objectPrototype = _globalPrototype ?? JSValue.@null;
                        _ = (dynamicProxy as PrototypeProxy).PrototypeInstance;
                    }
                }
            }

            return constructor;
        }

        public Function GetGenericTypeSelector(IList<Type> types)
        {
            for (var i = 0; i < types.Count; i++)
            {
                for (var j = i + 1; j < types.Count; j++)
                {
                    if (types[i].GetGenericArguments().Length == types[j].GetGenericArguments().Length)
                        ExceptionHelper.Throw(new InvalidOperationException("Types have the same arguments"));
                }
            }

            return new ExternalFunction((_this, args) =>
            {
                Type type = null;

                for (var i = 0; i < types.Count; i++)
                {
                    if (types[i].GetGenericArguments().Length == args.Length)
                    {
                        type = types[i];
                        break;
                    }
                }

                if (type == null)
                    ExceptionHelper.ThrowTypeError("Invalid arguments count for generic constructor");

                if (args._iValue == 0)
                    return GetConstructor(type);

                var parameters = new Type[args._iValue];
                for (var i = 0; i < args._iValue; i++)
                {
                    parameters[i] = args[i].As<Type>();
                    if (parameters[i] == null)
                        ExceptionHelper.ThrowTypeError("Invalid argument #" + i + " for generic constructor");
                }

                return GetConstructor(type.MakeGenericType(parameters));
            });
        }

        public JSValue WrapValue(object value)
        {
            if (value is null)
                return JSValue.Null;

            return new ObjectWrapper(value, GetPrototype(value.GetType()));
        }

        public JSValue ProxyValue(object value)
        {
            if (value == null)
            {
                return JSValue.NotExists;
            }
            else
            {
                var jsvalue = value as JSValue;
                if (jsvalue != null)
                    return jsvalue;
            }
#if PORTABLE || NETCORE
            switch (value.GetType().GetTypeCode())
#else
            switch (Type.GetTypeCode(value.GetType()))
#endif
            {
                case TypeCode.Boolean:
                {
                    return new JSValue
                    {
                        _iValue = (bool)value ? 1 : 0,
                        _valueType = JSValueType.Boolean
                    };
                }
                case TypeCode.Byte:
                {
                    return new JSValue
                    {
                        _iValue = (byte)value,
                        _valueType = JSValueType.Integer
                    };
                }
                case TypeCode.Char:
                {
                    return new JSValue
                    {
                        _oValue = ((char)value).ToString(),
                        _valueType = JSValueType.String
                    };
                }
                case TypeCode.DateTime:
                {
                    var dateTime = (DateTime)value;
                    return new ObjectWrapper(new Date(dateTime), GetPrototype(typeof(Date)));
                }
                case TypeCode.Decimal:
                {
                    return new JSValue
                    {
                        _dValue = (double)(decimal)value,
                        _valueType = JSValueType.Double
                    };
                }
                case TypeCode.Double:
                {
                    return new JSValue
                    {
                        _dValue = (double)value,
                        _valueType = JSValueType.Double
                    };
                }
                case TypeCode.Int16:
                {
                    return new JSValue
                    {
                        _iValue = (short)value,
                        _valueType = JSValueType.Integer
                    };
                }
                case TypeCode.Int32:
                {
                    return new JSValue
                    {
                        _iValue = (int)value,
                        _valueType = JSValueType.Integer
                    };
                }
                case TypeCode.Int64:
                {
                    return new JSValue
                    {
                        _dValue = (long)value,
                        _valueType = JSValueType.Double
                    };
                }
                case TypeCode.SByte:
                {
                    return new JSValue
                    {
                        _iValue = (sbyte)value,
                        _valueType = JSValueType.Integer
                    };
                }
                case TypeCode.Single:
                {
                    return new JSValue
                    {
                        _dValue = (float)value,
                        _valueType = JSValueType.Double
                    };
                }
                case TypeCode.String:
                {
                    return new JSValue
                    {
                        _oValue = value,
                        _valueType = JSValueType.String
                    };
                }
                case TypeCode.UInt16:
                {
                    return new JSValue
                    {
                        _iValue = (ushort)value,
                        _valueType = JSValueType.Integer
                    };
                }
                case TypeCode.UInt32:
                {
                    var v = (uint)value;
                    if ((int)v != v)
                    {
                        return new JSValue
                        {
                            _dValue = v,
                            _valueType = JSValueType.Double
                        };
                    }
                    else
                    {
                        return new JSValue
                        {
                            _iValue = (int)v,
                            _valueType = JSValueType.Integer
                        };
                    }
                }
                case TypeCode.UInt64:
                {
                    var v = (ulong)value;
                    if (v > int.MaxValue)
                    {
                        return new JSValue
                        {
                            _dValue = v,
                            _valueType = JSValueType.Double
                        };
                    }
                    else
                    {
                        return new JSValue
                        {
                            _iValue = (int)v,
                            _valueType = JSValueType.Integer
                        };
                    }
                }
                default:
                {
                    if (value is Delegate)
                    {
                        if (value is ExternalFunctionDelegate)
                            return new ExternalFunction(value as ExternalFunctionDelegate);
                        return new MethodProxy(
                            this,
                            ((Delegate)value).GetMethodInfo(),
                            ((Delegate)value).Target)
                        {
                            _objectPrototype = GetPrototype(typeof(MethodProxy))
                        };
                    }
                    else if (value is IList)
                    {
                        return new NativeList(value as IList) { _objectPrototype = GetPrototype(typeof(NativeList)) };
                    }
                    else if (value is ExpandoObject)
                    {
                        return new DictionaryWrapper<string, object>(value as ExpandoObject)
                        {
                            _objectPrototype = GetPrototype(typeof(DictionaryWrapper<string, object>))
                        };
                    }
                    else if ((MarshalingOptions & MarshalinOptions.DictionaryAsObject) != 0
                        && value is IEnumerable
                        && (value is IDictionary || value.GetType().GetInterfaces().Any(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>))))
                    {
                        var dictionaryInterface = value.GetType().GetInterfaces().FirstOrDefault(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>));
                        if (dictionaryInterface != null)
                        {
                            var types = dictionaryInterface.GetGenericArguments();
                            return DictionaryWrapper.Of(types[0], types[1], value);
                        }
                        else
                        {
                            return new DictionaryWrapper<object, object>(new UntypedDictionaryWrapper(value as IDictionary));
                        }
                    }
                    else if (value is Task)
                    {
                        Task<JSValue> result;
                        if (Tools.IsTaskOfT(value.GetType()))
                        {
                            result = new Task<JSValue>(() =>
                            {
                                try
                                {
                                    return ProxyValue(value.GetType().GetMethod("get_Result", Type.EmptyTypes).Invoke(value, null));
                                }
                                catch (TargetInvocationException e)
                                {
                                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                                    throw;
                                }
                            });
                        }
                        else
                        {
                            result = new Task<JSValue>(() => JSValue.NotExists);
                        }

                        (value as Task).ContinueWith(task => result.Start());
                        return new ObjectWrapper(new Promise(result), GetPrototype(typeof(Promise)));
                    }
#if !NET40
                    else if (value is IEnumerable && NativeReadOnlyListCtors.IsReadOnlyList(value))
                    {
                        var result = NativeReadOnlyListCtors.Create(value);
                        result._objectPrototype = GetPrototype(result.GetType());
                        return result;
                    }
#endif
                    else
                    {
                        return new ObjectWrapper(value, value != null ? GetPrototype(value.GetType()) : null);
                    }
                }
            }
        }

        protected internal override JSValue GetVariable(string name, bool forWrite)
        {
            if (forWrite && GlobalObjectsAssignMode != GlobalObjectsAssignMode.Allow)
            {
                switch (GlobalObjectsAssignMode)
                {
                    case GlobalObjectsAssignMode.ScriptLocal:
                        return null;

                    case GlobalObjectsAssignMode.Deny:
                        return new JSValue { _attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.SystemObject };

                    case GlobalObjectsAssignMode.DenyThrowException:
                        ExceptionHelper.Throw(new ReferenceError("Invalid attempt to assign a value to a variable in the GlobalContext"));
                        break;
                }
            }

            return base.GetVariable(name, forWrite);
        }

        public override string ToString()
        {
            var result = "Global Context";
            if (string.IsNullOrEmpty(Name))
                return result;

            return result + " \"" + Name + "\"";
        }
    }
}
