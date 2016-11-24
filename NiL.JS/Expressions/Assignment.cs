using System;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Statements;
using System.Collections.Generic;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    internal sealed class ForceAssignmentOperator : Assignment
    {
        public ForceAssignmentOperator(Expression first, Expression second)
            : base(first, second)
        {
        }

        public override JSValue Evaluate(Context context)
        {
            JSValue temp;
            JSValue field = first.EvaluateForWrite(context);
            if (field._valueType == JSValueType.Property)
            {
                return setProperty(context, field);
            }
            else
            {
                if ((field._attributes & JSValueAttributesInternal.ReadOnly) != 0 && context._strict)
                    throwRoError();
            }
            temp = second.Evaluate(context);
            var oldAttributes = field._attributes;
            field._attributes &= ~JSValueAttributesInternal.ReadOnly;
            field.Assign(temp);
            field._attributes = oldAttributes;
            return temp;
        }
    }

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public class Assignment : Expression
    {
        private Arguments setterArgs;
        private bool saveResult;

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        protected internal override bool LValueModifier
        {
            get
            {
                return true;
            }
        }

        public Assignment(Expression first, Expression second)
            : base(first, second, false)
        {
        }

        public override JSValue Evaluate(Context context)
        {
            JSValue temp;

            JSValue field = first.EvaluateForWrite(context);
            if (field._valueType == JSValueType.Property)
            {
                return setProperty(context, field);
            }
            else
            {
                if ((field._attributes & JSValueAttributesInternal.ReadOnly) != 0 && context._strict)
                    throwRoError();
            }

            temp = second.Evaluate(context);
            field.Assign(temp);

            return temp;
        }

        protected void throwRoError()
        {
            ExceptionHelper.Throw(new TypeError("Can not assign to readonly property \"" + first + "\""));
        }

        protected JSValue setProperty(Context context, JSValue field)
        {
            JSValue temp;
            lock (this)
            {
                if (setterArgs == null)
                    setterArgs = new Arguments();
                var fieldSource = context._objectSource;
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
                var setter = (field._oValue as Core.GsPropertyPair).setter;
                if (setter != null)
                    setter.Call(fieldSource, setterArgs);
                else if (context._strict)
                    ExceptionHelper.Throw(new TypeError("Can not assign to readonly property \"" + first + "\""));
                if (saveResult)
                    tempContainer = temp;
                return temp;
            }
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
#if GIVENAMEFUNCTION
            if (first is VariableReference && second is FunctionExpression)
            {
                var fs = second as FunctionExpression;
                if (fs.name == null)
                    fs.name = (first as VariableReference).Name;
            }
#endif
            base.Build(ref _this, expressionDepth, variables, codeContext, message, stats, opts);

            var f = first as VariableReference ?? ((first is AssignmentOperatorCache) ? (first as AssignmentOperatorCache).Source as VariableReference : null);
            if (f != null)
            {
                var assigns = (f.Descriptor.assignments ?? (f.Descriptor.assignments = new List<Expression>()));
                if (assigns.IndexOf(this) == -1)
                    assigns.Add(this);
                if (second is Constant)
                {
                    var pt = second.ResultType;
                    if (f._descriptor.lastPredictedType == PredictedType.Unknown)
                        f._descriptor.lastPredictedType = pt;
                }
            }

            var gme = first as Property;
            if (gme != null)
                _this = new SetProperty(gme.first, gme.second, second) { Position = Position, Length = Length };

            if ((codeContext & (CodeContext.InExpression | CodeContext.InEval)) != 0)
                saveResult = true;

            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            baseOptimize(ref _this, owner, message, opts, stats);
            var vr = first as VariableReference;
            if (vr != null)
            {
                if (vr._descriptor.IsDefined)
                {
                    var stype = second.ResultType;
                    if (vr._descriptor.lastPredictedType == PredictedType.Unknown)
                        vr._descriptor.lastPredictedType = stype;
                    else if (vr._descriptor.lastPredictedType != stype)
                    {
                        if (Tools.IsEqual(vr._descriptor.lastPredictedType, stype, PredictedType.Group))
                            vr._descriptor.lastPredictedType = stype & PredictedType.Group;
                        else
                        {
                            if (message != null && stype >= PredictedType.Undefined && vr._descriptor.lastPredictedType >= PredictedType.Undefined)
                                message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Variable \"" + vr.Name + "\" has ambiguous type. It can be make impossible some optimizations and cause errors.");
                            vr._descriptor.lastPredictedType = PredictedType.Ambiguous;
                        }
                    }
                }
                else if (message != null)
                    message(MessageLevel.CriticalWarning, new CodeCoordinates(0, Position, Length), "Assign to undefined variable \"" + vr.Name + "\". It will declare a global variable.");
            }

            var gve = first as GetVariable;
            if (gve != null && gve._descriptor.IsDefined && (_codeContext & CodeContext.InWith) == 0)
            {
                if (owner != null // не будем это применять в корневом узле. Только в функциях. Иначе это может задумываться как настройка контекста для последующего использования в Eval
                    && !gve._descriptor.captured
                    && (opts & Options.SuppressUselessExpressionsElimination) == 0
                    && !stats.ContainsEval
                    && !stats.ContainsWith) // можем упустить присваивание
                {
                    if ((owner._body._strict || gve._descriptor.owner != owner || !owner._functionInfo.ContainsArguments) // аргументы это одна сущность с двумя именами
                        && (_codeContext & CodeContext.InLoop) == 0)
                    {
                        bool last = true;
                        for (var i = 0; last && i < gve._descriptor.references.Count; i++)
                        {
                            last &= gve._descriptor.references[i].Eliminated || gve._descriptor.references[i].Position <= Position;
                        }
                        if (last)
                        {
                            if (second.ContextIndependent)
                            {
                                _this.Eliminated = true;
                                _this = Empty.Instance;
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

                /*if (_this == this && second.ResultInTempContainer) // это присваивание, не последнее, без with
                {
                    _this = new AssignmentOverReplace(first, second)
                    {
                        Position = Position,
                        Length = Length,
                        _codeContext = _codeContext
                    };
                }*/
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
