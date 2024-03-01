﻿using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.Statements;

#if !(PORTABLE || NETCORE)
[Serializable]
#endif
public sealed class IfElse : CodeNode
{
    private Expression condition;
    private CodeNode then;
    private CodeNode @else;

    public CodeNode Then { get { return then; } }
    public CodeNode Else { get { return @else; } }
    public Expression Condition { get { return condition; } }

    private IfElse()
    {

    }

    public IfElse(Expression condition, CodeNode body, CodeNode elseBody)
    {
        this.condition = condition;
        this.then = body;
        this.@else = elseBody;
    }

    internal static CodeNode Parse(ParseInfo state, ref int index)
    {
        int i = index;
        if (!Parser.Validate(state.Code, "if (", ref i) && !Parser.Validate(state.Code, "if(", ref i))
            return null;

        while (Tools.IsWhiteSpace(state.Code[i]))
            i++;

        var condition = (Expression)ExpressionTree.Parse(state, ref i);
        while (Tools.IsWhiteSpace(state.Code[i]))
            i++;

        if (state.Code[i] != ')')
            throw new ArgumentException("code (" + i + ")");

        do
            i++;
        while (Tools.IsWhiteSpace(state.Code[i]));

        var combinedBody = state.Code[i] == '{';

        CodeNode body = Parser.Parse(state, ref i, 0);
        if (body is FunctionDefinition)
        {
            if (state.Message != null)
                state.Message(MessageLevel.CriticalWarning, body.Position, body.Length, Strings.DoNotDeclareFunctionInNestedBlocks);

            body = new CodeBlock([body]); // для того, чтобы не дублировать код по декларации функции, 
            // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
        }

        CodeNode elseBody = null;

        var pos = i;
        while (i < state.Code.Length && Tools.IsWhiteSpace(state.Code[i]))
            i++;

        if (i < state.Code.Length && !combinedBody && state.Code[i] == ';')
        {
            do
                i++;
            while (i < state.Code.Length && Tools.IsWhiteSpace(state.Code[i]));
        }

        if (Parser.Validate(state.Code, "else", ref i))
        {
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;

            elseBody = Parser.Parse(state, ref i, 0);
            if (elseBody is FunctionDefinition)
            {
                if (state.Message != null)
                    state.Message(MessageLevel.CriticalWarning, elseBody.Position, elseBody.Length, Strings.DoNotDeclareFunctionInNestedBlocks);

                elseBody = new CodeBlock([elseBody]); // для того, чтобы не дублировать код по декларации функции, 
                // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
            }
        }
        else
        {
            i = pos;
        }

        pos = index;
        index = i;
        return new IfElse()
        {
            then = body,
            condition = condition,
            @else = elseBody,
            Position = pos,
            Length = index - pos
        };
    }

    public override JSValue Evaluate(Context context)
    {
        var frame = ExceptionHelper.GetStackFrame(context, false);

        bool conditionResult;
        if (context._executionMode != ExecutionMode.Resume || !context.SuspendData.ContainsKey(this))
        {
            if (context._debugging)
                context.raiseDebugger(condition);

            frame.CodeNode = condition;
            conditionResult = (bool)condition.Evaluate(context);
            if (context._executionMode == ExecutionMode.Suspend)
                return null;
        }
        else
        {
            conditionResult = (bool)context.SuspendData[this];
        }

        if (conditionResult)
        {
            if (then != null)
            {
                if (context._debugging && !(then is CodeBlock))
                    context.raiseDebugger(then);

                frame.CodeNode = then;

                var temp = then.Evaluate(context);
                if (temp != null)
                    context._lastResult = temp;
            }
        }
        else if (@else != null)
        {
            if (context._debugging && !(@else is CodeBlock))
                context.raiseDebugger(@else);

            frame.CodeNode = @else;

            var temp = @else.Evaluate(context);
            if (temp != null)
                context._lastResult = temp;
        }

        if (context._executionMode == ExecutionMode.Suspend)
            context.SuspendData[this] = conditionResult;

        return null;
    }

    protected internal override CodeNode[] GetChildrenImpl()
    {
        var res = new List<CodeNode>()
        {
            then,
            condition,
            @else
        };
        res.RemoveAll(x => x == null);
        return res.ToArray();
    }

    public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
    {
        Parser.Build(ref condition, 2, variables, codeContext | CodeContext.InExpression, message, stats, opts);
        Parser.Build(ref then, expressionDepth, variables, codeContext | CodeContext.Conditional, message, stats, opts);
        Parser.Build(ref @else, expressionDepth, variables, codeContext | CodeContext.Conditional, message, stats, opts);

        if ((opts & Options.SuppressUselessStatementsElimination) == 0 && condition is ConvertToBoolean)
        {
            if (message != null)
                message(MessageLevel.Warning, condition.Position, 2, "Useless conversion. Remove double negation in condition");
            condition = condition._left;
        }
        
        if ((opts & Options.SuppressUselessStatementsElimination) == 0
            && (condition is Constant || (condition.ContextIndependent)))
        {
            if ((bool)condition.Evaluate(null))
                _this = then;
            else
                _this = @else;
            condition.Eliminated = true;
        }
        
        return false;
    }

    public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
    {
        var cc = condition as CodeNode;
        condition.Optimize(ref cc, owner, message, opts, stats);
        condition = (Expression)cc;

        if (then != null)
            then.Optimize(ref then, owner, message, opts, stats);

        if (@else != null)
            @else.Optimize(ref @else, owner, message, opts, stats);

        if ((opts & Options.SuppressUselessStatementsElimination) == 0)
        {
            if (then == null && @else == null)
                _this = new Comma(condition, new Constant(JSValue.undefined));
        }
    }

    public override void Decompose(ref CodeNode self)
    {
        condition.Decompose(ref condition);
        if (then != null)
            then.Decompose(ref then);
        if (@else != null)
            @else.Decompose(ref @else);
    }

    public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
    {
        condition?.RebuildScope(functionInfo, transferedVariables, scopeBias);
        then?.RebuildScope(functionInfo, transferedVariables, scopeBias);
        @else?.RebuildScope(functionInfo, transferedVariables, scopeBias);
    }

    public override T Visit<T>(Visitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public override string ToString()
    {
        string rp = Environment.NewLine;
        string rs = Environment.NewLine + "  ";
        var sbody = then.ToString();
        var sebody = @else == null ? "" : @else.ToString();
        return "if (" + condition + ")" +
            (then is CodeBlock ? sbody : Environment.NewLine + "  " + sbody.Replace(rp, rs)) +
            (@else != null ? Environment.NewLine +
            "else" + Environment.NewLine +
            (@else is CodeBlock ? sebody.Replace(rp, rs) : "  " + sebody) : "");
    }
}