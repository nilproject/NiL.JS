﻿using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions;

public sealed class Super : Expression
{
    public bool IsSuperConstructorCall { get; internal set; }

    protected internal override bool ContextIndependent
    {
        get
        {
            return false;
        }
    }

    protected internal override bool NeedDecompose
    {
        get
        {
            return false;
        }
    }

    protected internal override bool LValueModifier
    {
        get
        {
            return false;
        }
    }

    internal Super()
    {

    }

    protected internal override JSValue EvaluateForWrite(Context context)
    {
        ExceptionHelper.ThrowReferenceError(Strings.InvalidLefthandSideInAssignment);
        return null;
    }

    public override JSValue Evaluate(Context context)
    {
        if (IsSuperConstructorCall)
        {
            context._objectSource = context._thisBind;
            return context._owner.__proto__;
        }
        else
        {
            return context._thisBind;
        }
    }

    public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
    {
        return false;
    }

    public override void Optimize(ref Core.CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
    {

    }

    public override T Visit<T>(Visitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public override string ToString()
    {
        return "super";
    }
}
