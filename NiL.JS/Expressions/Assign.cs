using System;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public class Assign : Expression
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

        protected internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public Assign(Expression first, Expression second)
            : base(first, second, false)
        {
        }

        internal override JSValue Evaluate(Context context)
        {
            JSValue temp;
            JSValue field = first.EvaluateForAssing(context);
            if (field.valueType == JSValueType.Property)
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
                            tempContainer = new JSValue();
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

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
#if GIVENAMEFUNCTION
            if (first is VariableReference && second is FunctionExpression)
            {
                var fs = second as FunctionExpression;
                if (fs.name == null)
                    fs.name = (first as VariableReference).Name;
            }
#endif
            var r = base.Build(ref _this, depth, variables, state, message, statistic, opts);

            var f = first as VariableReference ?? ((first is OpAssignCache) ? (first as OpAssignCache).Source as VariableReference : null);
            if (f != null)
            {
                (f.Descriptor.assignations ??
                    (f.Descriptor.assignations = new System.Collections.Generic.List<CodeNode>())).Add(this);
                if (second is Constant)
                {
                    var pt = second.ResultType;
                    if (f.descriptor.lastPredictedType == PredictedType.Unknown)
                        f.descriptor.lastPredictedType = pt;
                }
            }
#if DEBUG
            if (r)
                System.Diagnostics.Debugger.Break();
#endif
            var gme = first as GetMemberExpression;
            if (gme != null)
                _this = new SetMemberExpression(gme.first, gme.second, second) { Position = Position, Length = Length };

            if (depth > 1)
                saveResult = true;

            return r;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            baseOptimize(ref _this, owner, message, opts, statistic);
            var vr = first as VariableReference;
            if (vr != null)
            {
                if (vr.descriptor.isDefined)
                {
                    var stype = second.ResultType;
                    if (vr.descriptor.lastPredictedType == PredictedType.Unknown)
                        vr.descriptor.lastPredictedType = stype;
                    else if (vr.descriptor.lastPredictedType != stype)
                    {
                        if (Tools.IsEqual(vr.descriptor.lastPredictedType, stype, PredictedType.Group))
                            vr.descriptor.lastPredictedType = stype & PredictedType.Group;
                        else
                        {
                            if (message != null && stype >= PredictedType.Undefined && vr.descriptor.lastPredictedType >= PredictedType.Undefined)
                                message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Variable \"" + vr.Name + "\" has ambiguous type. It can be make impossible some optimizations and cause errors.");
                            vr.descriptor.lastPredictedType = PredictedType.Ambiguous;
                        }
                    }
                }
                else if (message != null)
                    message(MessageLevel.CriticalWarning, new CodeCoordinates(0, Position, Length), "Assign to undefined variable \"" + vr.Name + "\". It will declare a global variable.");
            }

            var gve = first as GetVariableExpression;
            if (gve != null && gve.descriptor.isDefined && (codeContext & _BuildState.InWith) == 0)
            {
                if (owner != null // не будем это применять в корневом узле. Только в функциях. Иначе это может задумываться как настройка контекста для последующего использования в Eval
                    && !gve.descriptor.captured
                    && (opts & Options.SuppressUselessExpressionsElimination) == 0
                    && !statistic.ContainsEval
                    && !statistic.ContainsWith) // можем упустить присваивание
                {
                    if ((owner == null || owner.body.strict || gve.descriptor.owner != owner || !owner.statistic.ContainsArguments) // аргументы это одна сущность с двумя именами
                        && (codeContext & _BuildState.InLoop) == 0)
                    {
                        bool last = true;
                        for (var i = 0; last && i < gve.descriptor.references.Count; i++)
                        {
                            last &= gve.descriptor.references[i].Eliminated || gve.descriptor.references[i].Position <= Position;
                        }
                        if (last)
                        {
                            if (second.IsContextIndependent)
                            {
                                _this.Eliminated = true;
                                _this = EmptyStatement.Instance;
                            }
                            else
                            {
                                _this = second;
                                this.second = null;
                                this.Eliminated = true;
                                this.second = _this as Expression;
                            }
                        }
                    }
                }
                if (_this == this && second.ResultInTempContainer) // это присваивание, не последнее, без with
                {
                    _this = new AssignOverReplace(first, second)
                    {
                        Position = Position,
                        Length = Length,
                        codeContext = codeContext
                    };
                }
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