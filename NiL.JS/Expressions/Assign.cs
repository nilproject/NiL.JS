using System;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Assign : Expression
    {
        private Arguments setterArgs;
        private bool saveResult;

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        public Assign(Expression first, Expression second)
            : base(first, second, false)
        {
        }

        internal override JSObject Evaluate(Context context)
        {
            JSObject temp;
            JSObject field = first.EvaluateForAssing(context);
            if (field.valueType == JSObjectType.Property)
            {
                lock (this)
                {
                    if (setterArgs == null)
                        setterArgs = new Arguments();
                    var fieldSource = context.objectSource;
                    temp = second.Evaluate(context);
                    if (saveResult)
                    {
                        if (tempContainer == null)
                            tempContainer = new JSObject();
                        tempContainer.Assign(temp);
                        temp = tempContainer;
                        tempContainer = null;
                    }
                    setterArgs.Reset();
                    setterArgs.length = 1;
                    setterArgs[0] = temp;
                    var setter = (field.oValue as PropertyPair).set;
                    if (setter != null)
                        setter.Invoke(fieldSource, setterArgs);
                    else if (context.strict)
                        throw new JSException(new TypeError("Can not assign to readonly property \"" + first + "\""));
                    if (saveResult)
                        tempContainer = temp;
                    return temp;
                }
            }
            else
            {
                if ((field.attributes & JSObjectAttributesInternal.ReadOnly) != 0 && context.strict)
                    throw new JSException(new TypeError("Can not assign to readonly property \"" + first + "\""));
            }
            temp = second.Evaluate(context);
            field.Assign(temp);
            return temp;
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> vars, bool strict, CompilerMessageCallback message, FunctionStatistic statistic, OptimizationOptions opts)
        {
#if GIVENAMEFUNCTION
            if (first is VariableReference && second is FunctionExpression)
            {
                var fs = second as FunctionExpression;
                if (fs.name == null)
                    fs.name = (first as VariableReference).Name;
            }
#endif
            var r = base.Build(ref _this, depth, vars, strict, message, statistic, opts);

            var f = first as VariableReference ?? ((first is OpAssignCache) ? (first as OpAssignCache).Source as VariableReference : null);
            if (f != null)
            {
                (f.Descriptor.assignations ??
                    (f.Descriptor.assignations = new System.Collections.Generic.List<CodeNode>())).Add(this);
            }
#if DEBUG
            if (r)
                System.Diagnostics.Debugger.Break();
#endif
            var gme = first as GetMemberExpression;
            if (gme != null)
                _this = new SetMemberExpression(gme.first, gme.second, second);
            if (depth > 1)
                saveResult = true;
            return r;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message)
        {
            baseOptimize(owner, message);
            var vr = first as VariableReference;
            if (vr != null)
            {
                if (vr.descriptor.isDefined)
                {
                    var stype = second.ResultType;
                    if (vr.descriptor.lastPredictedType != stype && vr.descriptor.lastPredictedType != PredictedType.Unknown)
                    {
                        if (Tools.IsEqual(vr.descriptor.lastPredictedType, stype, PredictedType.Group))
                            vr.descriptor.lastPredictedType = stype & PredictedType.Group;
                        else
                        {
                            if (message != null)
                                message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Variable \"" + vr.Name + "\" has ambiguous type. It can be make impossible some optimizations and cause errors.");
                            vr.descriptor.lastPredictedType = PredictedType.Ambiguous;
                        }
                    }
                }
                else if (message != null)
                    message(MessageLevel.CriticalWarning, new CodeCoordinates(0, Position, Length), "Assign to undefined variable \"" + vr.Name + "\". It will declare a global variable.");
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            string f = first.ToString();
            if (f[0] == '(')
                f = f.Substring(1, f.Length - 2);
            string t = second.ToString();
            if (t[0] == '(')
                t = t.Substring(1, t.Length - 2);
            return "(" + f + " = " + t + ")";
        }
    }
}