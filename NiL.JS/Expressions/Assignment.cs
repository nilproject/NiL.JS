using System;
using NiL.JS.Core;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public class Assignment : Expression
    {
        private Arguments _setterArgs;
        private bool _saveResult;

        protected internal override bool ContextIndependent => false;

        internal override bool ResultInTempContainer => false;

        protected internal override bool LValueModifier => true;

        public bool Force { get; internal set; }

        public Assignment(Expression left, Expression right)
            : base(left, right, false)
        {
        }

        public override JSValue Evaluate(Context context)
        {
            var field = _left.EvaluateForWrite(context);
            var source = context._objectSource;
            var temp = _right.Evaluate(context);

            if (field._valueType == JSValueType.Property)
            {
                return setProperty(context, source, field, temp);
            }

            if ((field._attributes & JSValueAttributesInternal.ReadOnly) != 0)
            {
                if (Force)
                {
                    var oldAttrs = field._attributes;
                    field._attributes = oldAttrs & ~JSValueAttributesInternal.ReadOnly;
                    field.Assign(temp);
                    field._attributes = oldAttrs;
                }
                else if (context._strict)
                    throwReadOnlyError(context);
            }
            else
            {
                field.Assign(temp);
            }

            return temp;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected void throwReadOnlyError(Context context)
        {
            ExceptionHelper.ThrowTypeError(string.Format(Strings.CannotAssignReadOnly, _left), this, context);
        }

        protected JSValue setProperty(Context context, JSValue fieldSource, JSValue field, JSValue value)
        {
            lock (this)
            {
                var setterArgs = _setterArgs;
                _setterArgs = null;
                if (setterArgs == null)
                    setterArgs = new Arguments();

                var temp = value;

                if (_saveResult)
                {
                    if (_tempContainer == null)
                        _tempContainer = new JSValue();

                    _tempContainer.Assign(value);
                    temp = _tempContainer;
                    _tempContainer = null;
                }

                setterArgs.Reset();
                setterArgs.Add(temp);

                var setter = (field._oValue as Core.PropertyPair).setter;
                if (setter != null)
                    setter.Call(fieldSource, setterArgs);
                else if (context._strict)
                    throwReadOnlyError(context);

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
            if (owner != null // не будем это применять в корневом узле. Только в функциях.
                                 // Иначе это может использоваться как настройка контекста для последующего использования в Eval
                && variable != null
                && variable._descriptor.IsDefined
                && (!variable._descriptor.IsParameter || !stats.ContainsArguments)
                && (_codeContext & CodeContext.InWith) == 0
                && !variable._descriptor.captured
                && !stats.ContainsEval 
                && !stats.ContainsWith
                && (opts & Options.SuppressUselessExpressionsElimination) == 0
                && (_codeContext & CodeContext.InLoop) == 0)
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
