using System;
using System.Collections.Generic;
using System.Security;
using NiL.JS.Core;
using NiL.JS.Core.JIT;
using System.Reflection;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class GetArgumentsExpression : GetVariableExpression
    {
        internal GetArgumentsExpression(int functionDepth)
            : base("arguments", functionDepth)
        {
        }

        internal override JSObject EvaluateForAssing(Context context)
        {
            var res = context.caller._arguments;
            if (res is Arguments)
                context.caller._arguments = res = res.CloneImpl();
            if (context.fields != null && context.fields.ContainsKey(Name))
                context.fields[Name] = res;
            return res;
        }

        internal sealed override JSObject Evaluate(Context context)
        {
            var res = context.caller._arguments;
            return res;
        }
    }

    [Serializable]
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

        internal override JSObject EvaluateForAssing(Context context)
        {
            if (context.strict || forceThrow)
            {
                var res = Descriptor.Get(context, false, functionDepth);
                if (res.valueType < JSObjectType.Undefined && (!suspendThrow || forceThrow))
                    throw new JSException((new NiL.JS.Core.BaseTypes.ReferenceError("Variable \"" + variableName + "\" is not defined.")));
                return res;
            }
            return descriptor.Get(context, true, functionDepth);
        }

        internal override JSObject Evaluate(Context context)
        {
            var res = descriptor.Get(context, false, functionDepth);
            if (res.valueType == JSObjectType.NotExists && !suspendThrow)
                throw new JSException(new NiL.JS.Core.BaseTypes.ReferenceError("Variable \"" + variableName + "\" is not defined."));
            if (res.valueType == JSObjectType.Property)
            {
                var getter = (res.oValue as PropertyPair).get;
                if (getter == null)
                    return JSObject.notExists;
                return getter.Invoke(context.objectSource, null);
            }
            return res;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        public override string ToString()
        {
            return variableName;
        }

        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            dynamicValues.Add(this);
            var res = System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.ArrayIndex(JITHelpers.DynamicValuesParameter, JITHelpers.cnst(dynamicValues.Count - 1)),
                forAssign ? EvaluateForAssignMethod : EvaluateMethod,
                JITHelpers.ContextParameter
                );
            if (expectedType == typeof(int))
                res = System.Linq.Expressions.Expression.Call(JITHelpers.JSObjectToInt32Method, res);
            return res;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict, CompilerMessageCallback message)
        {
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
            if (depth >= 0 && depth < 2 && desc.IsDefined)
            {
                _this = null;
                if (message != null)
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position), "Unused get of defined variable was removed. Maybe, something missing.");
            }
            else if (variableName == "arguments"
                && functionDepth > 0)
                _this = new GetArgumentsExpression(functionDepth) { descriptor = descriptor };
            return false;
        }
    }
}