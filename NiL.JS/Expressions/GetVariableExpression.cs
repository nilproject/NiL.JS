using System;
using System.Collections.Generic;
using NiL.JS.Core;
#if !PORTABLE
using NiL.JS.Core.JIT;
#endif

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class GetArgumentsExpression : GetVariableExpression
    {
        internal GetArgumentsExpression(int functionDepth)
            : base("arguments", functionDepth)
        {
        }

        internal protected override JSValue EvaluateForWrite(Context context)
        {
            if (context.owner.creator.type == BaseLibrary.FunctionKind.Arrow)
                context = context.parent;
            if (context.arguments == null)
                context.owner.BuildArgumentsObject();
            var res = context.arguments;
            if (res is Arguments)
                context.arguments = res = res.CloneImpl(false);
            if (context.fields != null && context.fields.ContainsKey(Name))
                context.fields[Name] = res;
            return res;
        }

        public override JSValue Evaluate(Context context)
        {
            if (context.owner.creator.type == BaseLibrary.FunctionKind.Arrow)
                context = context.parent;
            if (context.arguments == null)
                context.owner.BuildArgumentsObject();
            return context.arguments;
        }
    }

#if !PORTABLE
    [Serializable]
#endif
    public class GetVariableExpression : VariableReference
    {
        private string variableName;
        internal bool suspendThrow;
        internal bool forceThrow;

        public override string Name { get { return variableName; } }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal GetVariableExpression(string name, int scopeDepth)
        {
            this.ScopeLevel = scopeDepth;
            int i = 0;
            if ((name != "this") && (name != "super") && !Parser.ValidateName(name, i, true, true, false))
                throw new ArgumentException("Invalid variable name");
            this.variableName = name;
        }

        internal protected override JSValue EvaluateForWrite(Context context)
        {
            if (context.strict || forceThrow)
            {
                var res = Descriptor.Get(context, false, _scopeLevel);
                if (res.valueType < JSValueType.Undefined && (!suspendThrow || forceThrow))
                    ExceptionsHelper.ThrowVariableNotDefined(variableName);
                if (context.strict)
                {
                    if ((res.attributes & JSValueAttributesInternal.Argument) != 0)
                        context.owner.BuildArgumentsObject();
                }
                return res;
            }
            return _descriptor.Get(context, true, _scopeLevel);
        }

        public override JSValue Evaluate(Context context)
        {
            var res = _descriptor.Get(context, false, _scopeLevel);
            switch (res.valueType)
            {
                case JSValueType.NotExists:
                    {
                        if (!suspendThrow)
                            ExceptionsHelper.ThrowVariableNotDefined(variableName);
                        break;
                    }
                case JSValueType.Property:
                    {
                        return Tools.InvokeGetter(res, context.objectSource);
                    }
            }
            return res;
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            return null;
        }

        public override string ToString()
        {
            return variableName;
        }

#if !NET35 && !PORTABLE
        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            dynamicValues.Add(this);
            var res = System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.ArrayAccess(JITHelpers.DynamicValuesParameter, JITHelpers.cnst(dynamicValues.Count - 1)),
                forAssign ? JITHelpers.EvaluateForWriteMethod : JITHelpers.EvaluateMethod,
                JITHelpers.ContextParameter
                );
            if (expectedType == typeof(int))
                res = System.Linq.Expressions.Expression.Call(JITHelpers.JSObjectToInt32Method, res);
            return res;
        }
#endif
        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            _codeContext = codeContext;

            VariableDescriptor desc = null;
            if (!variables.TryGetValue(variableName, out desc) || desc == null)
            {
                desc = new VariableDescriptor(this, 1) { isDefined = false };
                variables[variableName] = this.Descriptor;
            }
            else
            {
                if (!desc.references.Contains(this))
                    desc.references.Add(this);
                _descriptor = desc;
            }

            if (variableName == "this")
            {
                stats.ContainsThis = true;
                desc.definitionScopeLevel = -1;
            }
            else if (((codeContext & CodeContext.InWith) != 0) || (stats.ContainsEval && !desc.isDefined))
            {
                ScopeLevel = -Math.Abs(ScopeLevel);
                desc.definitionScopeLevel = -Math.Abs(desc.definitionScopeLevel);
            }

            forceThrow |= desc.lexicalScope; // часть TDZ

            if (expressionDepth >= 0 && expressionDepth < 2 && desc.IsDefined && !desc.lexicalScope && (opts & Options.SuppressUselessExpressionsElimination) == 0)
            {
                _this = null;
                Eliminated = true;
                if (message != null)
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Unused getting of defined variable was removed. Maybe something missing.");
            }
            else if (variableName == "arguments" && (codeContext & CodeContext.InFunction) != 0)
            {
                if (stats != null)
                    stats.ContainsArguments = true;
                _this = new GetArgumentsExpression(ScopeLevel) { _descriptor = _descriptor };
            }

            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            if ((opts & Options.SuppressConstantPropogation) == 0
                && !_descriptor.captured
                && _descriptor.isDefined
                && !stats.ContainsWith
                && !stats.ContainsEval
                && (_descriptor.owner != owner || !owner._functionInfo.ContainsArguments))
            {
                var assigns = _descriptor.assignments;
                if (assigns != null && assigns.Count > 0)
                {
                    /*
                     * Применение оптимизации зависит от порядка добавления присваиваний.
                     * Этот порядок в свою очередь зависит от порядка следования операций в CodeBlock.
                     * Раньше этот порядок был обратным, сейчас прямой, поэтому здесь присваивания нужно перебирать
                     * в обратном порядке. Оптимизация не применится если найдется изменение в котором first указывает на
                     * это использование. Это говорит о том, что в данном месте этой переменной
                     * присваивается значение
                     */
                    CodeNode lastAssign = null;
                    for (var i = assigns.Count; i-- > 0;)
                    {
                        if (assigns[i].first == this
                            || ((assigns[i].first is AssignmentOperatorCache) && assigns[i].first.first == this))
                        {
                            // оптимизация не применяется
                            lastAssign = null;
                            break;
                        }

                        if (assigns[i].Position > Position)
                        {
                            if ((_codeContext & CodeContext.InLoop) != 0 && ((assigns[i] as Expression)._codeContext & CodeContext.InLoop) != 0)
                            // присваивание может быть после этого использования, но если всё это в цикле, то выполнение вернётся сюда.
                            {
                                // оптимизация не применяется
                                lastAssign = null;
                                break;
                            }
                            continue; // пропускаем ноду
                        }

                        if (_descriptor.isReadOnly)
                        {
                            if (assigns[i] is ForceAssignmentOperator)
                            {
                                lastAssign = assigns[i];
                                break;
                            }
                        }
                        else if (lastAssign == null || assigns[i].Position > lastAssign.Position)
                        {
                            lastAssign = assigns[i];
                        }
                    }
                    var assign = lastAssign as AssignmentOperator;
                    if (assign != null && (assign._codeContext & CodeContext.Conditional) == 0 && assign.second is ConstantDefinition)
                    {
                        _this = assign.second;
                    }
                }
            }
        }
    }
}