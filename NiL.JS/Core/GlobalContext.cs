using System;
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

#if NET40 || NETCORE
using NiL.JS.Backward;
#endif

namespace NiL.JS.Core
{
    public enum IndexersSupportMode
    {
        WithAttributeOnly = 0,
        ForceEnable,
        ForceDisable
    }

    public sealed class GlobalContext : Context
    {
        internal JSObject _GlobalPrototype;
        private readonly Dictionary<Type, JSObject> _proxies;

        public string Name { get; private set; }
        public IndexersSupportMode IndexersSupportMode { get; set; }

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
                _GlobalPrototype = null;

                var objectConstructor = GetConstructor(typeof(JSObject)) as Function;
                _variables.Add("Object", objectConstructor);
                objectConstructor._attributes |= JSValueAttributesInternal.DoNotDelete;

                _GlobalPrototype = objectConstructor.prototype as JSObject;
                _GlobalPrototype._objectPrototype = JSValue.@null;

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
#if !PORTABLE
                DefineVariable("console").Assign(JSValue.Marshal(new JSConsole()));
#endif
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
        }

        public void ActivateInCurrentThread()
        {
            if (CurrentContext != null)
                throw new InvalidOperationException();

            if (!Activate())
                throw new Exception("Unable to activate base context");
        }

        public new void Deactivate()
        {
            if (CurrentContext != this)
                throw new InvalidOperationException();

            if (base.Deactivate() != null)
                throw new InvalidOperationException("Invalid state");
        }

        internal JSObject GetPrototype(Type type)
        {
            return (GetConstructor(type) as Function).prototype as JSObject;
        }

        public JSObject GetConstructor(Type type)
        {
            JSObject constructor = null;
            if (!_proxies.TryGetValue(type, out constructor))
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
                        var indexerSupport = IndexersSupportMode == IndexersSupportMode.ForceEnable
                            || (IndexersSupportMode == IndexersSupportMode.WithAttributeOnly && type.GetTypeInfo().IsDefined(typeof(UseIndexersAttribute), false));

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
                            dynamicProxy._objectPrototype = _GlobalPrototype ?? JSValue.@null;
                        var fake = (dynamicProxy as PrototypeProxy).PrototypeInstance;
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
                    if (types[i].GetGenericArguments().Length == args.length)
                    {
                        type = types[i];
                        break;
                    }
                }

                if (type == null)
                    ExceptionHelper.ThrowTypeError("Invalid arguments count for generic constructor");

                if (args.length == 0)
                    return GetConstructor(type);

                var parameters = new Type[args.length];
                for (var i = 0; i < args.length; i++)
                {
                    parameters[i] = args[i].As<Type>();
                    if (types[i] == null)
                        ExceptionHelper.ThrowTypeError("Invalid argument #" + i + " for generic constructor");
                }

                return GetConstructor(type.MakeGenericType(parameters));
            });
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
                        var timeZoneOffset = dateTime.Kind == DateTimeKind.Local ? dateTime.ToLocalTime().Ticks - dateTime.ToUniversalTime().Ticks : 0;
                        return new ObjectWrapper(new Date(dateTime.ToUniversalTime().Ticks + timeZoneOffset, timeZoneOffset));
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
                case TypeCode.UInt64:
                    {
                        var v = (long)value;
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
                            return new MethodProxy(this, ((Delegate)value).GetMethodInfo(), ((Delegate)value).Target);
                        }
                        else if (value is IList)
                        {
                            return new NativeList(value as IList);
                        }
                        else if (value is ExpandoObject)
                        {
                            return new ExpandoObjectWrapper(value as ExpandoObject);
                        }
                        else if (value is Task)
                        {
                            Task<JSValue> result;
                            if (value.GetType().GetTypeInfo().IsGenericType && typeof(Task<>).IsAssignableFrom(value.GetType().GetGenericTypeDefinition()))
                            {
                                result = new Task<JSValue>(() => ProxyValue(value.GetType().GetMethod("get_Result", new Type[0]).Invoke(value, null)));
                            }
                            else
                            {
                                result = new Task<JSValue>(() => JSValue.NotExists);
                            }

                            (value as Task).ContinueWith(task => result.Start());
                            return new ObjectWrapper(new Promise(result));
                        }
                        else
                        {
                            return new ObjectWrapper(value);
                        }
                    }
            }
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
