using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NiL.JS.Backward;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Core.Interop
{
    public class JsonSerializer
    {
        private PropertyInfo[] _properties;
        private FieldInfo[] _fields;

        public Type TargetType { get; }
        public int Weight { get; }

        public JsonSerializer(Type targetType)
        {
            TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
#if NETSTANDARD1_3
            _properties = targetType.GetRuntimeProperties().ToArray();
            _fields = targetType.GetRuntimeFields().ToArray();
#else
            _properties = targetType.GetProperties();
            _fields = targetType.GetFields();
#endif
            var weight = 0;
            var curType = targetType;
            while (curType != null && curType != typeof(object))
            {
                weight++;
                curType = curType.GetTypeInfo().BaseType;
            }
            Weight = weight;
        }

        public virtual bool CanSerialize(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return TargetType.GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo());
        }

        public virtual string Serialize(string key, object value, Function replacer, HashSet<string> keys, string space, HashSet<JSValue> processed)
        {
            if (!CanSerialize(value))
                throw new ArgumentException("Cannot serialize value with type \"" + value.GetType().FullName + "\" as \"" + TargetType.FullName + "\"");

            var result = new StringBuilder("{");
            var first = true;

            for (var i = 0; i < _properties.Length; i++)
            {
                if (!first)
                    result.Append(",");
                first = false;

                if (space != null)
                {
                    result.Append(Environment.NewLine)
                          .Append(space);
                }

                var propValue = _properties[i].GetValue(value, null);
                result.Append("\"").Append(_properties[i].Name).Append("\"").Append(":");
                WriteValue(result, _properties[i].Name, propValue, replacer, keys, space, processed);
            }

            for (var i = 0; i < _fields.Length; i++)
            {
                if (!first)
                    result.Append(",");
                first = false;

                if (space != null)
                {
                    result.Append(Environment.NewLine)
                          .Append(space);
                }

                var fieldValue = _fields[i].GetValue(value);
                result.Append("\"").Append(_fields[i].Name).Append("\"");
                WriteValue(result, _fields[i].Name, fieldValue, replacer, keys, space, processed);
            }

            if (space != null)
            {
                result.Append(Environment.NewLine)
                      .Append(space);
            }

            result.Append("}");
            return result.ToString();
        }

        public object Deserialize(JSValue deserializedJson, object resultContainer = null)
        {
            if (deserializedJson == null)
                throw new ArgumentNullException(nameof(deserializedJson));

            if (deserializedJson._valueType < JSValueType.Object)
                return deserializedJson.Value;
#if NETSTANDARD1_3
            var result = resultContainer ?? TargetType.GetTypeInfo().DeclaredConstructors.Where(x => x.IsPublic).First(x=>x.GetParameters().Length == 0).Invoke(new object[0]);
#else
            var result = resultContainer ?? TargetType.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
#endif
            var tempSrcObject = deserializedJson._oValue as JSObject;
            foreach (var property in tempSrcObject._fields)
            {
                var prop = getProperty(property.Key);
                if (prop != null)
                {
                    object value = property.Value;
                    var deserializer = GetSerializer(value, Context.CurrentGlobalContext);
                    if (deserializer != null)
                        value = deserializer.Deserialize(property.Value);
                    else
                        value = Convert.ChangeType(property.Value.Value, prop.PropertyType);

                    prop.SetValue(result, value, null);
                    continue;
                }

                var field = getField(property.Key);
                if (field != null)
                {
                    object value = property.Value;
                    var deserializer = GetSerializer(value, Context.CurrentGlobalContext);
                    if (deserializer != null)
                        value = deserializer.Deserialize(property.Value);
                    else
                        value = Convert.ChangeType(property.Value.Value, field.FieldType);

                    field.SetValue(result, Convert.ChangeType(property.Value, field.FieldType));
                    continue;
                }
            }

            return result;
        }

        protected internal virtual JsonSerializer GetSerializer(object value, GlobalContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.GlobalContext?.JsonSerializersRegistry.GetSuitableJsonSerializer(value);
        }

        protected virtual void WriteValue(StringBuilder result, string key, object value, Function replacer, HashSet<string> keys, string space, HashSet<JSValue> processed)
        {
            if (value == null)
            {
                result.Append("null");
                return;
            }

            var jsValue = value as JSValue;
            if (jsValue != null)
            {
                result.Append(JSON.stringify(jsValue, replacer, keys, space));
                return;
            }

            switch (value.GetType().GetTypeCode())
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    {
                        result.Append(value);
                        return;
                    }
                default:
                    {
                        if (!(value is string))
                        {
                            var serializer = GetSerializer(value, Context.CurrentGlobalContext);
                            if (serializer != null)
                            {
                                result.Append(serializer.Serialize(key, value, replacer, keys, space, processed));
                                return;
                            }
                        }

                        result.Append('"').Append(value).Append('"');
                        return;
                    }
            }
        }

        private PropertyInfo getProperty(string name)
        {
            for (var i = 0; i < _properties.Length; i++)
            {
                if (_properties[i].Name == name)
                    return _properties[i];
            }

            return null;
        }

        private FieldInfo getField(string name)
        {
            for (var i = 0; i < _fields.Length; i++)
            {
                if (_fields[i].Name == name)
                    return _fields[i];
            }

            return null;
        }
    }
}
