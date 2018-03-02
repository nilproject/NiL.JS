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
            JSValue field = _left.EvaluateForWrite(context);
            if (field._valueType == JSValueType.Property)
            {
                return setProperty(context, field);
            }
            else
            {
                if ((field._attributes & JSValueAttributesInternal.ReadOnly) != 0 && context._strict)
                    throwReadOnlyError();
            }
            temp = _right.Evaluate(context);
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
        private Arguments _setterArgs;
        private bool _saveResult;

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

        public Assignment(Expression left, Expression right)
            : base(left, right, false)
        {
        }

        public override JSValue Evaluate(Context context)
        {
            JSValue temp;

            JSValue field = _left.EvaluateForWrite(context);
            if (field._valueType == JSValueType.Property)
            {
                return setProperty(context, field);
            }
            else
            {
                if ((field._attributes & JSValueAttributesInternal.ReadOnly) != 0 && context._strict)
                    throwReadOnlyError();
            }

            temp = _right.Evaluate(context);
            field.Assign(temp);

            return temp;
        }

        protected void throwReadOnlyError()
        {
            ExceptionHelper.ThrowTypeError(string.Format(Strings.CannotAssignReadOnly, _left));
        }

        protected JSValue setProperty(Context context, JSValue field)
        {
            JSValue temp;
            lock (this)
            {
                var setterArgs = _setterArgs;
                _setterArgs = null;
                if (setterArgs == null)
                    setterArgs = new Arguments();

                var fieldSource = context._objectSource;
                temp = _right.Evaluate(context);

                if (_saveResult)
                {
                    if (_tempContainer == null)
                        _tempContainer = new JSValue();

                    _tempContainer.Assign(temp);
                    temp = _tempContainer;
                    _tempContainer = null;
                }

                setterArgs.Reset();
                setterArgs.Add(temp);

                var setter = (field._oValue as Core.PropertyPair).setter;
                if (setter != null)
                    setter.Call(fieldSource, setterArgs);
                else if (context._strict)
                    throwReadOnlyError();

                if (_saveResult)
                    _tempContainer = temp;

                _setterArgs = setterArgs;

                return temp;
            }
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
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

            var f = _left as VariableReference ?? ((_left is AssignmentOperatorCache) ? (_left as AssignmentOperatorCache).Source as VariableReference : null);
            if (f != null)
            {
                var assigns = (f.Descriptor.assignments ?? (f.Descriptor.assignments = new List<Expression>()));
                if (assigns.IndexOf(this) == -1)
                    assigns.Add(this);
                if (_right is Constant)
                {
                    var pt = _right.ResultType;
                    if (f._descriptor.lastPredictedType == PredictedType.Unknown)
                        f._descriptor.lastPredictedType = pt;
                }
            }

            var gme = _left as Property;
            if (gme != null)
                _this = new SetProperty(gme._left, gme._right, _right) { Position = Position, Length = Length };

            if ((codeContext & (CodeContext.InExpression | CodeContext.InEval)) != 0)
                _saveResult = true;

            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            baseOptimize(ref _this, owner, message, opts, stats);
            var vr = _left as VariableReference;
            if (vr != null)
            {
                if (vr._descriptor.IsDefined)
                {
                    var stype = _right.ResultType;
                    if (vr._descriptor.lastPredictedType == PredictedType.Unknown)
                        vr._descriptor.lastPredictedType = stype;
                    else if (vr._descriptor.lastPredictedType != stype)
                    {
                        if (Tools.IsEqual(vr._descriptor.lastPredictedType, stype, PredictedType.Group))
                            vr._descriptor.lastPredictedType = stype & PredictedType.Group;
                        else
                        {
                            if (message != null && stype >= PredictedType.Undefined && vr._descriptor.lastPredictedType >= PredictedType.Undefined)
                                message(MessageLevel.Warning, Position, Length, "Variable \"" + vr.Name + "\" has ambiguous type. It can be make impossible some optimizations and cause errors.");
                            vr._descriptor.lastPredictedType = PredictedType.Ambiguous;
                        }
                    }
                }
                else if (message != null)
                    message(MessageLevel.CriticalWarning, Position, Length, "Assign to undefined variable \"" + vr.Name + "\". It will declare a global variable.");
            }

            var variable = _left as Variable;
            if (variable != null && variable._descriptor.IsDefined && (_codeContext & CodeContext.InWith) == 0 && !variable._descriptor.captured)
            {
                if (!stats.ContainsEval && !stats.ContainsWith)
                {
                    if (owner != null // не будем это применять в корневом узле. Только в функциях.
                                      // Иначе это может задумываться как настройка контекста для последующего использования в Eval
                        && (opts & Options.SuppressUselessExpressionsElimination) == 0
                        && (_codeContext & CodeContext.InLoop) == 0)
                    {
                        if ((owner._body._strict || variable._descriptor.owner != owner || !owner._functionInfo.ContainsArguments)) // аргументы это одна сущность с двумя именами
                        {
                            bool last = true;
                            for (var i = 0; last && i < variable._descriptor.references.Count; i++)
                            {
                                last &= variable._descriptor.references[i].Eliminated || variable._descriptor.references[i].Position <= Position;
                            }

                            if (last)
                            {
                                if (_right.ContextIndependent)
                                {
                                    _this.Eliminated = true;
                                    _this = Empty.Instance;
                                }
                                else
                                {
                                    _this = _right;
                                    this._right = null;
                                    this.Eliminated = true;
                                    this._right = _this as Expression;
                                }
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
            string f = _left.ToString();
            if (f[0] == '(')
                f = f.Substring(1, f.Length - 2);
            string t = _right.ToString();
            if (t[0] == '(')
                t = t.Substring(1, t.Length - 2);
            return "(" + f + " = " + t + ")";
        }
    }
}
