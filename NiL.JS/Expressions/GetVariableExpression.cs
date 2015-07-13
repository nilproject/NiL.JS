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

        internal override JSValue EvaluateForAssing(Context context)
        {
            var res = context.caller._arguments;
            if (res is Arguments)
                context.caller._arguments = res = res.CloneImpl();
            if (context.fields != null && context.fields.ContainsKey(Name))
                context.fields[Name] = res;
            return res;
        }

        internal sealed override JSValue Evaluate(Context context)
        {
            var res = context.caller._arguments;
            return res;
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

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal GetVariableExpression(string name, int functionDepth)
        {
            this.functionDepth = functionDepth;
            int i = 0;
            if ((name != "this") && !Parser.ValidateName(name, i, true, true, false))
                throw new ArgumentException("Invalid variable name");
            this.variableName = name;
        }

        internal override JSValue EvaluateForAssing(Context context)
        {
            if (context.strict || forceThrow)
            {
                var res = Descriptor.Get(context, false, functionDepth);
                if (res.valueType < JSValueType.Undefined && (!suspendThrow || forceThrow))
                    throwRefError();
                if ((res.attributes & JSObjectAttributesInternal.Argument) != 0)
                    context.caller.buildArgumentsObject();
                return res;
            }
            return descriptor.Get(context, true, functionDepth);
        }

        internal override JSValue Evaluate(Context context)
        {
            var res = descriptor.Get(context, false, functionDepth);
            if (res.valueType == JSValueType.NotExists)
            {
                if (!suspendThrow)
                    throwRefError();
            }
            else if (res.valueType == JSValueType.Property)
            {
                return processProp(context, res);
            }
            return res;
        }

        private static JSValue processProp(Context context, JSValue res)
        {
            var getter = (res.oValue as PropertyPair).get;
            if (getter == null)
                return JSValue.notExists;
            return getter.Invoke(context.objectSource, null);
        }

        private void throwRefError()
        {
            throw new JSException(new NiL.JS.BaseLibrary.ReferenceError("Variable \"" + variableName + "\" is not defined."));
        }

        protected override CodeNode[] getChildsImpl()
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
                forAssign ? EvaluateForAssignMethod : EvaluateMethod,
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

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            codeContext = state;

            if (statistic != null && variableName == "this")
                statistic.UseThis = true;
            VariableDescriptor desc = null;
            if (!variables.TryGetValue(variableName, out desc) || desc == null)
            {
                desc = new VariableDescriptor(this, false, functionDepth);
                descriptor = desc;
                variables[variableName] = this.Descriptor;
            }
            else
            {
                desc.references.Add(this);
                descriptor = desc;
            }
            if (depth >= 0 && depth < 2 && desc.IsDefined && (opts & Options.SuppressUselessExpressionsElimination) == 0)
            {
                _this = null;
                Eliminated = true;
                if (message != null)
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Unused get of defined variable was removed. Maybe, something missing.");
            }
            else if (variableName == "arguments"
                && functionDepth > 0)
            {
                if (statistic != null)
                    statistic.ContainsArguments = true;
                _this = new GetArgumentsExpression(functionDepth) { descriptor = descriptor };
            }
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionNotation owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            base.Optimize(ref _this, owner, message, opts, statistic);
            if ((opts & Options.SuppressConstantPropogation) == 0
                && !descriptor.captured
                && descriptor.isDefined
                && !statistic.ContainsWith
                && !statistic.ContainsEval
                && (descriptor.owner != owner || !owner.statistic.ContainsArguments))
            {
                var assigns = descriptor.assignations;
                if (assigns != null && assigns.Count > 0)
                {
                    CodeNode lastAssign = null;
                    for (var i = 0; i < assigns.Count; i++)
                    {
                        if (assigns[i].Position == Position)
                        {
                            // оптимизация не применяется
                            lastAssign = null;
                            break;
                        }

                        if (assigns[i].Position > Position)
                        {
                            if ((codeContext & _BuildState.InLoop) != 0 && ((assigns[i] as Expression).codeContext & _BuildState.InLoop) != 0)
                            // присваивание может быть после этого использования, но если всё это в цикле, то выполнение вернётся сюда.
                            {
                                // оптимизация не применяется
                                lastAssign = null;
                                break;
                            }
                            continue; // пропускаем ноду
                        }

                        if (descriptor.isReadOnly)
                        {
                            if ((assigns[i] is AssignmentOperator)
                                && (assigns[i] as AssignmentOperator).first is Statements.VariableDefineStatement.AllowWriteCN)
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
                    if (assign != null && (assign.codeContext & _BuildState.Conditional) == 0 && assign.second is ConstantNotation)
                    {
                        _this = assign.second;
                    }
                }
            }
        }
    }
}