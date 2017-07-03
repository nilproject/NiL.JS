using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    public sealed class ObjectDesctructuringAcceptor : Expression
    {
        private sealed class DestructuringAcceptor : JSValue
        {
            private readonly Context _context;
            private readonly ObjectDefinition _objectDefinition;

            public DestructuringAcceptor(ObjectDefinition objectDefinition, Context context)
            {
                _objectDefinition = objectDefinition;
                _context = context;
            }

            public override void Assign(JSValue value)
            {
                assignValues(value, _objectDefinition);
            }

            private void assignValues(JSValue source, ObjectDefinition targetMap)
            {
                var values = new JSValue[targetMap.FieldNames.Length + targetMap.ComputedProperties.Length];

                var j = 0;
                for (var i = 0; i < targetMap.FieldNames.Length; i++, j++)
                    values[j] = source[targetMap.FieldNames[i]].CloneImpl(false);
                for (var i = 0; i < targetMap.ComputedProperties.Length; i++, j++)
                    values[j] = source.GetProperty(targetMap.ComputedProperties[i].Key.Evaluate(_context), false, PropertyScope.Сommon).CloneImpl(false);

                Arguments setterArgs = null;
                j = 0;
                for (var i = 0; i < targetMap.FieldNames.Length; i++, j++)
                {
                    if (targetMap.Values[i] is ObjectDefinition)
                    {
                        assignValues(values[j], targetMap.Values[i] as ObjectDefinition);
                    }
                    else
                    {
                        var target = targetMap.Values[i].EvaluateForWrite(_context);
                        setterArgs = assign(targetMap, target, values[j], j, i, setterArgs);
                    }
                }

                for (var i = 0; i < targetMap.ComputedProperties.Length; i++, j++)
                {
                    if (targetMap.ComputedProperties[i].Value is ObjectDefinition)
                    {
                        assignValues(values[j], targetMap.ComputedProperties[i].Value as ObjectDefinition);
                    }
                    else
                    {
                        var target = targetMap.ComputedProperties[i].Value.EvaluateForWrite(_context);
                        setterArgs = assign(targetMap, target, values[j], j, i, setterArgs);
                    }
                }
            }

            private Arguments assign(ObjectDefinition targetMap, JSValue target, JSValue value, int i, int j, Arguments setterArgs)
            {
                if (target._valueType == JSValueType.Property)
                {
                    if (setterArgs == null)
                        setterArgs = new Arguments();

                    var fieldSource = _context._objectSource;

                    setterArgs.Reset();
                    setterArgs.Add(value);

                    var setter = (target._oValue as Core.PropertyPair).setter;
                    if (setter != null)
                        setter.Call(fieldSource, setterArgs);
                    else if (_context._strict)
                        ExceptionHelper.ThrowTypeError(string.Format(Strings.CannotAssignReadOnly, targetMap.Values[i]));
                }
                else
                {
                    if ((target._attributes & JSValueAttributesInternal.ReadOnly) != 0 && _context._strict)
                        ExceptionHelper.ThrowTypeError(string.Format(Strings.CannotAssignReadOnly, targetMap.Values[i]));

                    target.Assign(value);
                }

                return setterArgs;
            }
        }

        private readonly ObjectDefinition _objectDefinition;

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Object;
            }
        }

        public ObjectDesctructuringAcceptor(ObjectDefinition objectDefinition)
        {
            if (!CheckObjectDefinition(objectDefinition, false))
                throw new ArgumentException(nameof(objectDefinition));

            _objectDefinition = objectDefinition;
        }

        public bool CheckObjectDefinition(ObjectDefinition objectDefinition, bool @throw)
        {
            return true;
        }

        public override JSValue Evaluate(Context context)
        {
            throw new InvalidOperationException();
        }

        protected internal override JSValue EvaluateForWrite(Context context)
        {
            return new DestructuringAcceptor(_objectDefinition, context);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            return _objectDefinition.Build(ref _this, expressionDepth, variables, codeContext, message, stats, opts);
        }
    }
}
