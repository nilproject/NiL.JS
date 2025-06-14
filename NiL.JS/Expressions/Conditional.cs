using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;

namespace NiL.JS.Expressions;

#if !(PORTABLE || NETCORE)
[Serializable]
#endif
public sealed class Conditional : Expression
{
    private Expression[] _branches;

    protected internal override bool ContextIndependent
    {
        get
        {
            return base.ContextIndependent
                && (_branches[0] == null || _branches[0].ContextIndependent)
                && (_branches[1] == null || _branches[1].ContextIndependent);
        }
    }

    protected internal override PredictedType ResultType
    {
        get
        {
            var ftt = _branches[0].ResultType;
            var stt = _branches[1].ResultType;
            if (ftt == stt)
                return ftt;
            if (Tools.IsEqual(ftt, stt, PredictedType.Group))
                return ftt & PredictedType.Group;
            return PredictedType.Ambiguous;
        }
    }

    internal override bool ResultInTempContainer => false;

    public IList<Expression> Branches => new ReadOnlyCollection<Expression>(_branches);

    public Conditional(Expression first, Expression[] threads)
        : base(first, null, false)
    {
        this._branches = threads;
    }

    public override JSValue Evaluate(Context context) => (bool)_left.Evaluate(context) ? _branches[0].Evaluate(context) : _branches[1].Evaluate(context);

    public override bool Build(ref CodeNode _this, int expressionDepth, int scopeLevel, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
    {
        Parser.Build(ref _left, expressionDepth + 1, scopeLevel, variables, codeContext | CodeContext.Conditional | CodeContext.InExpression, message, stats, opts);
        Parser.Build(ref _branches[0], expressionDepth, scopeLevel, variables, codeContext | CodeContext.Conditional | CodeContext.InExpression, message, stats, opts);
        Parser.Build(ref _branches[1], expressionDepth, scopeLevel, variables, codeContext | CodeContext.Conditional | CodeContext.InExpression, message, stats, opts);

        if ((opts & Options.SuppressUselessExpressionsElimination) == 0 && expressionDepth < 1)
        {
            if (_branches[0] == null && _branches[1] == null)
            {
                if (_left.ContextIndependent)
                {
                    _this = null;
                    return false;
                }
                else
                {
                    _this = new Comma(_left, new Constant(JSValue.undefined));
                }
            }
            else if (_branches[0] == null)
            {
                _this = new LogicalDisjunction(_left, _branches[1]) { Position = Position, Length = Length };
                return true;
            }
            else if (_branches[1] == null)
            {
                _this = new LogicalConjunction(_left, _branches[0]) { Position = Position, Length = Length };
                return true;
            }
            else if (_left.ContextIndependent)
            {
                _this = ((bool)_left.Evaluate(null) ? _branches[0] : _branches[1]);
                return false;
            }
        }

        base.Build(ref _this, expressionDepth + 1, scopeLevel, variables, codeContext, message, stats, opts);
        return false;
    }

    public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
    {
        base.Optimize(ref _this, owner, message, opts, stats);
        for (var i = _branches.Length; i-- > 0;)
        {
            var cn = _branches[i] as CodeNode;
            cn.Optimize(ref cn, owner, message, opts, stats);
            _branches[i] = cn as Expression;
        }
        if (message != null
            && (_branches[0] is Variable || _branches[0] is Constant)
            && (_branches[1] is Variable || _branches[1] is Constant)
            && ResultType == PredictedType.Ambiguous)
            message(MessageLevel.Warning, Position, Length, "Type of an expression is ambiguous");
    }

    public override T Visit<T>(Visitor<T> visitor) => visitor.Visit(this);

    public override string ToString() => "(" + _left + " ? " + _branches[0] + " : " + _branches[1] + ")";
}