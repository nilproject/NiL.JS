using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    public sealed class ObjectDesctructor : Expression
    {
        private sealed class DestructuringAcceptor : JSValue
        {
            private readonly Context _context;
            private readonly Expression _definition;

            public DestructuringAcceptor(Expression definition, Context context)
            {
                _definition = definition;
                _context = context;
            }

            public override void Assign(JSValue value)
            {
                assignValues(value, _definition as ObjectDefinition);
                assignValues(value, _definition as ArrayDefinition);
            }

            private void assignValues(JSValue source, ArrayDefinition targetMap)
            {
                if (targetMap == null)
                    return;

                var values = new JSValue[targetMap.Elements.Length];

                for (var i = 0; i < targetMap.Elements.Length; i++)
                    values[i] = source[Tools.Int32ToString(i)].CloneImpl(false);

                Arguments setterArgs = null;
                for (var i = 0; i < targetMap.Elements.Length; i++)
                {
                    if (targetMap.Elements[i] is ObjectDefinition)
                    {
                        assignValues(values[i], targetMap.Elements[i] as ObjectDefinition);
                        assignValues(values[i], targetMap.Elements[i] as ArrayDefinition);
                    }
                    else
                    {
                        var target = targetMap.Elements[i].EvaluateForWrite(_context);
                        setterArgs = assign(target, values[i], setterArgs);
                    }
                }
            }

            private void assignValues(JSValue source, ObjectDefinition targetMap)
            {
                if (targetMap == null)
                    return;

                var j = 0;
                var values = new JSValue[targetMap.FieldNames.Length + targetMap.ComputedProperties.Length];
                for (var i = 0; i < targetMap.FieldNames.Length; i++, j++)
                    values[j] = source[targetMap.FieldNames[i]].CloneImpl(false);
                for (var i = 0; i < targetMap.ComputedProperties.Length; i++, j++)
                    values[j] = source.GetProperty(targetMap.ComputedProperties[i].Key.Evaluate(_context), false, PropertyScope.Common).CloneImpl(false);

                Arguments setterArgs = null;
                j = 0;
                for (var i = 0; i < targetMap.FieldNames.Length; i++, j++)
                {
                    if (targetMap.Values[i] is ObjectDefinition)
                    {
                        assignValues(values[j], targetMap.Values[i] as ObjectDefinition);
                        assignValues(values[j], targetMap.Values[i] as ArrayDefinition);
                    }
                    else
                    {
                        var target = targetMap.Values[i].EvaluateForWrite(_context);
                        setterArgs = assign(target, values[j], setterArgs);
                    }
                }

                for (var i = 0; i < targetMap.ComputedProperties.Length; i++, j++)
                {
                    if (targetMap.ComputedProperties[i].Value is ObjectDefinition)
                    {
                        assignValues(values[j], targetMap.ComputedProperties[i].Value as ObjectDefinition);
                        assignValues(values[j], targetMap.ComputedProperties[i].Value as ArrayDefinition);
                    }
                    else
                    {
                        var target = targetMap.ComputedProperties[i].Value.EvaluateForWrite(_context);
                        setterArgs = assign(target, values[j], setterArgs);
                    }
                }
            }

            private Arguments assign(JSValue target, JSValue value, Arguments setterArgs)
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
                        ExceptionHelper.ThrowTypeError(string.Format(Strings.CannotAssignReadOnly, value));
                }
                else
                {
                    if ((target._attributes & JSValueAttributesInternal.ReadOnly) != 0 && _context._strict)
                        ExceptionHelper.ThrowTypeError(string.Format(Strings.CannotAssignReadOnly, value));

                    target.Assign(value);
                }

                return setterArgs;
            }
        }

        private readonly Expression _definition;

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

        public ObjectDesctructor(Expression definition)
        {
            if (!(definition is ObjectDefinition) && !(definition is ArrayDefinition))
                throw new ArgumentException(nameof(definition));

            if (!CheckObjectDefinition(definition as ObjectDefinition, false) && !CheckObjectDefinition(definition as ArrayDefinition, false))
                throw new ArgumentException(nameof(definition));

            _definition = definition;
        }

        public static bool CheckObjectDefinition(ArrayDefinition arrayDefinition, bool @throw)
        {
            if (arrayDefinition == null)
                return true;

            for (var i = 0; i < arrayDefinition.Elements.Length; i++)
            {
                if (!ExpressionTree.canBeAssignee(arrayDefinition.Elements[i]))
                {
                    if (@throw)
                        ExceptionHelper.ThrowReferenceError(Strings.InvalidLefthandSideInAssignment);
                    return false;
                }
            }

            return true;
        }

        public static bool CheckObjectDefinition(ObjectDefinition objectDefinition, bool @throw)
        {
            if (objectDefinition == null)
                return true;

            for (var i = 0; i < objectDefinition.Values.Length; i++)
            {
                if (!ExpressionTree.canBeAssignee(objectDefinition.Values[i]))
                {
                    if (@throw)
                        ExceptionHelper.ThrowReferenceError(Strings.InvalidLefthandSideInAssignment);
                    return false;
                }
            }

            for (var i = 0; i < objectDefinition.ComputedProperties.Length; i++)
            {
                if (!ExpressionTree.canBeAssignee(objectDefinition.ComputedProperties[i].Value))
                {
                    if (@throw)
                        ExceptionHelper.ThrowReferenceError(Strings.InvalidLefthandSideInAssignment);
                    return false;
                }
            }

            return true;
        }

        public override JSValue Evaluate(Context context)
        {
            throw new InvalidOperationException();
        }

        protected internal override JSValue EvaluateForWrite(Context context)
        {
            return new DestructuringAcceptor(_definition, context);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            return _definition.Build(ref _this, expressionDepth, variables, codeContext, message, stats, opts);
        }

        public IList<Variable> GetTargetVariables()
        {
            var result = new List<Variable>();
            collectTargetVariables(_definition as ObjectDefinition, result);
            collectTargetVariables(_definition as ArrayDefinition, result);
            return result;
        }

        private static void collectTargetVariables(ArrayDefinition arrayDefinition, List<Variable> result)
        {
            if (arrayDefinition == null)
                return;

            for (var i = 0; i < arrayDefinition.Elements.Length; i++)
            {
                if (arrayDefinition.Elements[i] is Variable)
                {
                    result.Add((Variable)arrayDefinition.Elements[i]);
                }
                else
                {
                    collectTargetVariables(arrayDefinition.Elements[i] as ObjectDefinition, result);
                    collectTargetVariables(arrayDefinition.Elements[i] as ArrayDefinition, result);
                }
            }
        }

        private static void collectTargetVariables(ObjectDefinition objectDefinition, List<Variable> result)
        {
            if (objectDefinition == null)
                return;

            for (var i = 0; i < objectDefinition.Values.Length; i++)
            {
                if (objectDefinition.Values[i] is Variable)
                {
                    result.Add((Variable)objectDefinition.Values[i]);
                }
                else
                {
                    collectTargetVariables(objectDefinition.Values[i] as ObjectDefinition, result);
                    collectTargetVariables(objectDefinition.Values[i] as ArrayDefinition, result);
                }
            }

            for (var i = 0; i < objectDefinition.ComputedProperties.Length; i++)
            {
                if (objectDefinition.ComputedProperties[i].Value is Variable)
                {
                    result.Add((Variable)objectDefinition.ComputedProperties[i].Value);
                }
                else
                {
                    collectTargetVariables(objectDefinition.ComputedProperties[i].Value as ObjectDefinition, result);
                    collectTargetVariables(objectDefinition.ComputedProperties[i].Value as ArrayDefinition, result);
                }
            }
        }

        public override string ToString()
        {
            return _definition.ToString();
        }
    }
}
