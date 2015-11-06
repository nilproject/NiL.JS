using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ForOfStatement : CodeNode
    {
        private CodeNode variable;
        private CodeNode source;
        private CodeNode body;
        private string[] labels;

        public CodeNode Variable { get { return variable; } }
        public CodeNode Source { get { return source; } }
        public CodeNode Body { get { return body; } }
        public ReadOnlyCollection<string> Labels
        {
            get
            {
#if PORTABLE
                return new ReadOnlyCollection<string>(labels);
#else
                return System.Array.AsReadOnly<string>(labels);
#endif
            }
        }

        private ForOfStatement()
        {

        }

        internal static CodeNode Parse(ParsingState state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            if (!Parser.Validate(state.Code, "for(", ref i) && (!Parser.Validate(state.Code, "for (", ref i)))
                return null;
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            var res = new ForOfStatement()
            {
                labels = state.Labels.GetRange(state.Labels.Count - state.LabelCount, state.LabelCount).ToArray()
            };
            var vStart = i;
            if (Parser.Validate(state.Code, "var", ref i))
            {
                while (char.IsWhiteSpace(state.Code[i]))
                    i++;
                int start = i;
                string varName;
                if (!Parser.ValidateName(state.Code, ref i, state.strict))
                    ExceptionsHelper.Throw(new SyntaxError("Invalid variable name at " + CodeCoordinates.FromTextPosition(state.Code, start, 0)));
                varName = Tools.Unescape(state.Code.Substring(start, i - start), state.strict);
                if (state.strict)
                {
                    if (varName == "arguments" || varName == "eval")
                        ExceptionsHelper.Throw((new SyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(state.Code, start, i - start))));
                }
                res.variable = new VariableDefineStatement(varName, new GetVariableExpression(varName, state.functionsDepth) { Position = start, Length = i - start, defineFunctionDepth = state.functionsDepth }, false, state.functionsDepth) { Position = vStart, Length = i - vStart };
            }
            else
            {
                if (state.Code[i] == ';')
                    return null;
                while (char.IsWhiteSpace(state.Code[i]))
                    i++;
                int start = i;
                string varName;
                if (!Parser.ValidateName(state.Code, ref i, state.strict))
                    return null;
                varName = Tools.Unescape(state.Code.Substring(start, i - start), state.strict);
                if (state.strict)
                {
                    if (varName == "arguments" || varName == "eval")
                        ExceptionsHelper.Throw((new SyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(state.Code, start, i - start))));
                }
                res.variable = new GetVariableExpression(varName, state.functionsDepth) { Position = start, Length = i - start, defineFunctionDepth = state.functionsDepth };
            }
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            if (state.Code[i] == '=')
            {
                do
                    i++;
                while (char.IsWhiteSpace(state.Code[i]));
                var defVal = ExpressionTree.Parse(state, ref i, false, false, false, true, false, true);
                if (defVal == null)
                    return defVal;
                NiL.JS.Expressions.Expression exp = new GetValueForAssignmentOperator(res.variable as GetVariableExpression ?? (res.variable as VariableDefineStatement).initializators[0] as GetVariableExpression);
                exp = new AssignmentOperator(
                    exp,
                    (NiL.JS.Expressions.Expression)defVal)
                    {
                        Position = res.variable.Position,
                        Length = defVal.EndPosition - res.variable.Position
                    };
                if (res.variable == exp.first.first)
                    res.variable = exp;
                else
                    (res.variable as VariableDefineStatement).initializators[0] = exp;
                while (char.IsWhiteSpace(state.Code[i]))
                    i++;
            }
            if (!Parser.Validate(state.Code, "of", ref i))
                return null;
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            res.source = Parser.Parse(state, ref i, CodeFragmentType.Expression);
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            if (state.Code[i] != ')')
                ExceptionsHelper.Throw((new SyntaxError("Expected \")\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            i++;
            state.AllowBreak.Push(true);
            state.AllowContinue.Push(true);
            res.body = Parser.Parse(state, ref i, 0);
            if (res.body is FunctionDefinition)
            {
                if (state.strict)
                    ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                if (state.message != null)
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, res.body.Position, res.body.Length), "Do not declare function in nested blocks.");
                res.body = new CodeBlock(new[] { res.body }, state.strict); // для того, чтобы не дублировать код по декларации функции, 
                // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
            }
            state.AllowBreak.Pop();
            state.AllowContinue.Pop();
            res.Position = index;
            res.Length = i - index;
            index = i;
            return res;
        }

        public override JSValue Evaluate(Context context)
        {
            var s = source.Evaluate(context);
            JSValue v = null;
            if (variable is AssignmentOperator)
            {
                variable.Evaluate(context);
                v = (variable as AssignmentOperator).first.Evaluate(context);
            }
            else
                v = variable.EvaluateForWrite(context);
            if (!s.IsDefined
                || (s.valueType >= JSValueType.Object && s.oValue == null)
                || body == null)
                return JSValue.undefined;
            int index = 0;
            HashSet<string> processedKeys = new HashSet<string>(StringComparer.Ordinal);
            while (s != null)
            {
                if (s.oValue is NiL.JS.BaseLibrary.Array)
                {
                    var src = s.oValue as NiL.JS.BaseLibrary.Array;
                    foreach (var item in src.data)
                    {
                        if (item == null
                            || !item.IsExists
                            || (item.attributes & JSValueAttributesInternal.DoNotEnumerate) != 0)
                            continue;
                        v.Assign(item);
#if DEV
                        if (context.debugging && !(body is CodeBlock))
                            context.raiseDebugger(body);
#endif
                        context.lastResult = body.Evaluate(context) ?? context.lastResult;
                        if (context.abortType != AbortType.None)
                        {

                            var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                            var _break = (context.abortType > AbortType.Continue) || !me;
                            if (context.abortType < AbortType.Return && me)
                            {
                                context.abortType = AbortType.None;
                                context.abortInfo = JSValue.notExists;
                            }
                            if (_break)
                                return null;
                        }
                    }
                    if (src.fields != null)
                        foreach (var item in src.fields)
                        {
                            if (item.Value == null
                                || !item.Value.IsExists
                                || (item.Value.attributes & JSValueAttributesInternal.DoNotEnumerate) != 0)
                                continue;
                            if (processedKeys.Contains(item.Key))
                                continue;
                            processedKeys.Add(item.Key);
                            v.Assign(item.Value);
#if DEV
                            if (context.debugging && !(body is CodeBlock))
                                context.raiseDebugger(body);
#endif
                            context.lastResult = body.Evaluate(context) ?? context.lastResult;
                            if (context.abortType != AbortType.None)
                            {

                                var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                                var _break = (context.abortType > AbortType.Continue) || !me;
                                if (context.abortType < AbortType.Return && me)
                                {
                                    context.abortType = AbortType.None;
                                    context.abortInfo = JSValue.notExists;
                                }
                                if (_break)
                                    return null;
                            }
                        }
                }
                else
                {
                    var keys = s.GetEnumerator(false, EnumerationMode.NeedValues);
                    for (; ; )
                    {
                        try
                        {
                            if (!keys.MoveNext())
                                break;
                        }
                        catch (InvalidOperationException)
                        {
                            keys = s.GetEnumerator();
                            for (int i = 0; i < index && keys.MoveNext(); i++)
                                ;
                        }
                        if (processedKeys.Contains(keys.Current.Key))
                            continue;
                        processedKeys.Add(keys.Current.Key);
                        if ((keys.Current.Value.attributes & JSValueAttributesInternal.DoNotEnumerate) != 0)
                            continue;
                        v.Assign(keys.Current.Value);
#if DEV
                        if (context.debugging && !(body is CodeBlock))
                            context.raiseDebugger(body);
#endif
                        context.lastResult = body.Evaluate(context) ?? context.lastResult;
                        if (context.abortType != AbortType.None)
                        {
                            var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                            var _break = (context.abortType > AbortType.Continue) || !me;
                            if (context.abortType < AbortType.Return && me)
                            {
                                context.abortType = AbortType.None;
                                context.abortInfo = JSValue.notExists;
                            }
                            if (_break)
                                return null;
                        }
                        index++;
                    }
                }
                s = s.__proto__;
                if (s == JSValue.Null || !s.IsDefined || (s.valueType >= JSValueType.Object && s.oValue == null))
                    break;
            }
            return null;
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                body,
                variable,
                source
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            Parser.Build(ref variable, 2, variables, codeContext | CodeContext.InExpression, message, statistic, opts);
            var tvar = variable as VariableDefineStatement;
            if (tvar != null)
                variable = tvar.initializators[0];
            if (variable is AssignmentOperator)
                ((variable as AssignmentOperator).first.first as GetVariableExpression).forceThrow = false;
            Parser.Build(ref source, 2, variables, codeContext | CodeContext.InExpression, message, statistic, opts);
            Parser.Build(ref body, System.Math.Max(1, depth), variables, codeContext | CodeContext.Conditional | CodeContext.InLoop, message, statistic, opts);
            if (variable is Expressions.CommaOperator)
            {
                if ((variable as Expressions.CommaOperator).SecondOperand != null)
                    throw new InvalidOperationException("Invalid left-hand side in for-in");
                variable = (variable as Expressions.CommaOperator).FirstOperand;
            }
            if (message != null
                && (source is ObjectNotation
                || source is ArrayDefinition
                || source is ConstantDefinition))
                message(MessageLevel.Recomendation, new CodeCoordinates(0, Position, Length), "for..in with constant source. This reduce performance. Rewrite without using for..in.");
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            source.Optimize(ref source, owner, message, opts, statistic);
            if (body != null)
                body.Optimize(ref body, owner, message, opts, statistic);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "for (" + variable + " in " + source + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}