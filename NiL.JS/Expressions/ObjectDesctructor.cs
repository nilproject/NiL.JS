using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !NETCORE
    [Serializable]
#endif
    public sealed class ObjectDesctructor : Expression
    {
        private sealed class DestructuringAcceptor : JSValue
        {
            private readonly Context _context;
            private readonly bool _force;
            private readonly Expression _definition;

            public DestructuringAcceptor(Expression definition, Context context, bool force)
            {
                _definition = definition;
                _context = context;
                _force = force;
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
                        assign(target, values[i], targetMap.Elements[i], ref setterArgs);
                    }
                }
            }

            private void assignValues(JSValue source, ObjectDefinition targetMap)
            {
                if (targetMap == null)
                    return;

                var j = 0;
                var values = new JSValue[targetMap.Properties.Length];
                for (var i = 0; i < targetMap.Properties.Length; i++, j++)
                {
                    values[j] =
                        Tools.GetPropertyOrValue(
                            source.GetProperty(
                                targetMap.Properties[i].Key.Evaluate(_context),
                                false,
                                PropertyScope.Common),
                            source)
                        .CloneImpl(false);
                }

                Arguments setterArgs = null;
                j = 0;
                for (var i = 0; i < targetMap.Properties.Length; i++, j++)
                {
                    if (targetMap.Properties[i].Value is ObjectDefinition)
                    {
                        assignValues(values[j], targetMap.Properties[i].Value as ObjectDefinition);
                        assignValues(values[j], targetMap.Properties[i].Value as ArrayDefinition);
                    }
                    else
                    {
                        var target = targetMap.Properties[i].Value.EvaluateForWrite(_context);
                        assign(target, values[j], targetMap.Properties[i].Value, ref setterArgs);
                    }
                }
            }

            private void assign(JSValue target, JSValue value, object targetName, ref Arguments setterArgs)
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
                        ExceptionHelper.ThrowTypeError(string.Format(Strings.CannotAssignReadOnly, targetName));
                }
                else
                {
                    if ((target._attributes & JSValueAttributesInternal.ReadOnly) != 0)
                    {
                        if (_force)
                        {
                            target._attributes &= ~JSValueAttributesInternal.ReadOnly;
                            target.Assign(value);
                            target._attributes |= JSValueAttributesInternal.ReadOnly;
                        }
                        else if (_context._strict)
                            ExceptionHelper.ThrowTypeError(string.Format(Strings.CannotAssignReadOnly, targetName));
                    }
                    else
                        target.Assign(value);
                }
            }
        }

        private readonly Expression _definition;

        protected internal override bool ContextIndependent => false;

        protected internal override PredictedType ResultType => PredictedType.Object;

        public bool Force { get; internal set; }

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

            for (var i = 0; i < objectDefinition.Properties.Length; i++)
            {
                if (!ExpressionTree.canBeAssignee(objectDefinition.Properties[i].Value))
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
            return new DestructuringAcceptor(_definition, context, Force);
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

            for (var i = 0; i < objectDefinition.Properties.Length; i++)
            {
                if (objectDefinition.Properties[i].Value is Variable)
                {
                    result.Add((Variable)objectDefinition.Properties[i].Value);
                }
                else
                {
                    collectTargetVariables(objectDefinition.Properties[i].Value as ObjectDefinition, result);
                    collectTargetVariables(objectDefinition.Properties[i].Value as ArrayDefinition, result);
                }
            }
        }

        public override string ToString()
        {
            return _definition.ToString();
        }
    }
}
