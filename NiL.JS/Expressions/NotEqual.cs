﻿using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions;

#if !(PORTABLE || NETCORE)
[Serializable]
#endif
public sealed class NotEqual : Equal
{
    protected internal override PredictedType ResultType
    {
        get
        {
            return PredictedType.Bool;
        }
    }

    public NotEqual(Expression first, Expression second)
        : base(first, second)
    {

    }

    public override JSValue Evaluate(Context context)
    {
        return base.Evaluate(context)._iValue == 0;
    }

    public override T Visit<T>(Visitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public override string ToString()
    {
        return "(" + _left + " != " + _right + ")";
    }
}