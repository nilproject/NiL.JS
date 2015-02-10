using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ForInStatement : CodeNode
    {
        private CodeNode variable;
        private CodeNode source;
        private CodeNode body;
        private string[] labels;

        public CodeNode Variable { get { return variable; } }
        public CodeNode Source { get { return source; } }
        public CodeNode Body { get { return body; } }
        public ReadOnlyCollection<string> Labels { get { return System.Array.AsReadOnly<string>(labels); } }

        private ForInStatement()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (!Parser.Validate(state.Code, "for(", ref i) && (!Parser.Validate(state.Code, "for (", ref i)))
                return new ParseResult();
            while (char.IsWhiteSpace(state.Code[i])) i++;
            var res = new ForInStatement()
            {
                labels = state.Labels.GetRange(state.Labels.Count - state.LabelCount, state.LabelCount).ToArray()
            };
            var vStart = i;
            if (Parser.Validate(state.Code, "var", ref i))
            {
                while (char.IsWhiteSpace(state.Code[i])) i++;
                int start = i;
                string varName;
                if (!Parser.ValidateName(state.Code, ref i, state.strict.Peek()))
                    throw new ArgumentException();
                varName = Tools.Unescape(state.Code.Substring(start, i - start), state.strict.Peek());
                if (state.strict.Peek())
                {
                    if (varName == "arguments" || varName == "eval")
                        throw new JSException(new Core.BaseTypes.SyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(state.Code, start, i - start)));
                }
                res.variable = new VariableDefineStatement(varName, new GetVariableExpression(varName, state.functionsDepth) { Position = start, Length = i - start, functionDepth = state.functionsDepth }, false, state.functionsDepth) { Position = vStart, Length = i - vStart };
            }
            else
            {
                if (state.Code[i] == ';')
                    return new ParseResult();
                while (char.IsWhiteSpace(state.Code[i])) i++;
                int start = i;
                string varName;
                if (!Parser.ValidateName(state.Code, ref i, state.strict.Peek()))
                {
                    if (Parser.ValidateValue(state.Code, ref i))
                    {
                        while (char.IsWhiteSpace(state.Code[i])) i++;
                        if (Parser.Validate(state.Code, "in", ref i))
                            throw new JSException(new SyntaxError("Invalid accumulator name at " + CodeCoordinates.FromTextPosition(state.Code, start, i - start)));
                        else
                            return new ParseResult();
                    }
                    else
                        return new ParseResult();
                        //throw new JSException(new SyntaxError("Unexpected token at " + CodeCoordinates.FromTextPosition(state.Code, start)));
                }
                //if (!Parser.ValidateName(state.Code, ref i, state.strict.Peek()))
                //    return new ParseResult(); // for (1 in {};;); должен вызвать синтаксическую ошибку, но это проверка заставляет перейти в обычный for, для которого такое выражение допустимо
                varName = Tools.Unescape(state.Code.Substring(start, i - start), state.strict.Peek());
                if (state.strict.Peek())
                {
                    if (varName == "arguments" || varName == "eval")
                        throw new JSException((new Core.BaseTypes.SyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(state.Code, start, i - start))));
                }
                res.variable = new GetVariableExpression(varName, state.functionsDepth) { Position = start, Length = i - start, functionDepth = state.functionsDepth };
            }
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (state.Code[i] == '=')
            {
                do i++; while (char.IsWhiteSpace(state.Code[i]));
                var defVal = ExpressionTree.Parse(state, ref i, true, false, false, true, false, true);
                if (!defVal.IsParsed)
                    return defVal;
                NiL.JS.Expressions.Expression exp = new OpAssignCache(res.variable as GetVariableExpression ?? (res.variable as VariableDefineStatement).initializators[0] as GetVariableExpression);
                exp = new Assign(
                    exp,
                    (NiL.JS.Expressions.Expression)defVal.Statement)
                    {
                        Position = res.variable.Position,
                        Length = defVal.Statement.EndPosition - res.variable.Position
                    };
                if (res.variable == exp.first.first)
                    res.variable = exp;
                else
                    (res.variable as VariableDefineStatement).initializators[0] = exp;
                while (char.IsWhiteSpace(state.Code[i])) i++;
            }
            if (!Parser.Validate(state.Code, "in", ref i))
                return new ParseResult();
            while (char.IsWhiteSpace(state.Code[i])) i++;
            res.source = Parser.Parse(state, ref i, 1);
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (state.Code[i] != ')')
                throw new JSException((new Core.BaseTypes.SyntaxError("Expected \")\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            i++;
            state.AllowBreak.Push(true);
            state.AllowContinue.Push(true);
            res.body = Parser.Parse(state, ref i, 0);
            if (res.body is FunctionExpression)
            {
                if (state.strict.Peek())
                    throw new JSException((new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                if (state.message != null)
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, res.body.Position, 0), "Do not declare function in nested blocks.");
                res.body = new CodeBlock(new[] { res.body }, state.strict.Peek()); // для того, чтобы не дублировать код по декларации функции, 
                // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
            }
            state.AllowBreak.Pop();
            state.AllowContinue.Pop();
            res.Position = index;
            res.Length = i - index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = res
            };
        }

        internal override JSObject Evaluate(Context context)
        {
            var s = source.Evaluate(context);
            JSObject v = null;
            if (variable is Assign)
            {
                variable.Evaluate(context);
                v = (variable as Assign).first.Evaluate(context);
            }
            else
                v = variable.EvaluateForAssing(context);
            if (!s.IsDefinded 
                || (s.valueType >= JSObjectType.Object && s.oValue == null)
                || body == null)
                return JSObject.undefined;
            int index = 0;
            HashSet<string> processedKeys = new HashSet<string>(StringComparer.Ordinal);
            while (s != null)
            {
                if (s.oValue is Core.BaseTypes.Array)
                {
                    var src = s.oValue as Core.BaseTypes.Array;
                    foreach (var item in (src.data as IEnumerable<KeyValuePair<int, JSObject>>))
                    {
                        if (item.Value == null
                            || !item.Value.IsExist
                            || (item.Value.attributes & JSObjectAttributesInternal.DoNotEnum) != 0)
                            continue;
                        if (item.Key >= 0)
                        {
                            v.attributes = (v.attributes & ~JSObjectAttributesInternal.ContainsParsedDouble) | JSObjectAttributesInternal.ContainsParsedInt;
                            v.oValue = item.Key.ToString();
                        }
                        else
                        {
                            v.attributes = (v.attributes & ~JSObjectAttributesInternal.ContainsParsedInt) | JSObjectAttributesInternal.ContainsParsedDouble;
                            v.oValue = ((uint)item.Key).ToString();
                        }
                        if (processedKeys.Contains(v.oValue.ToString()))
                            continue;
                        processedKeys.Add(v.oValue.ToString());
                        v.valueType = JSObjectType.String;
#if DEV
                        if (context.debugging && !(body is CodeBlock))
                            context.raiseDebugger(body);
#endif
                        context.lastResult = body.Evaluate(context) ?? context.lastResult;
                        if (context.abort != AbortType.None)
                        {
                            var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                            var _break = (context.abort > AbortType.Continue) || !me;
                            if (context.abort < AbortType.Return && me)
                            {
                                context.abort = AbortType.None;
                                context.abortInfo = JSObject.notExists;
                            }
                            if (_break)
                                return null;
                        }
                    }
                    if (src.fields != null)
                        foreach (var item in src.fields)
                        {
                            if (item.Value == null
                                || !item.Value.IsExist
                                || (item.Value.attributes & JSObjectAttributesInternal.DoNotEnum) != 0)
                                continue;
                            v.valueType = JSObjectType.String;
                            v.oValue = item.Key;
                            if (processedKeys.Contains(v.oValue.ToString()))
                                continue;
                            processedKeys.Add(v.oValue.ToString());
#if DEV
                            if (context.debugging && !(body is CodeBlock))
                                context.raiseDebugger(body);
#endif
                            context.lastResult = body.Evaluate(context) ?? context.lastResult;
                            if (context.abort != AbortType.None)
                            {

                                var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                                var _break = (context.abort > AbortType.Continue) || !me;
                                if (context.abort < AbortType.Return && me)
                                {
                                    context.abort = AbortType.None;
                                    context.abortInfo = JSObject.notExists;
                                }
                                if (_break)
                                    return null;
                            }
                        }
                }
                else
                {
                    var keys = NiL.JS.Core.Tools._ForcedEnumerator<string>.create(s);
                    try
                    {
                        for (; ; )
                        {
                            if (!keys.MoveNext())
                                break;
                            var key = keys.Current;
                            v.valueType = JSObjectType.String;
                            v.oValue = key;
                            if (processedKeys.Contains(key))
                                continue;
                            processedKeys.Add(key);
#if DEV
                            if (context.debugging && !(body is CodeBlock))
                                context.raiseDebugger(body);
#endif
                            context.lastResult = body.Evaluate(context) ?? context.lastResult;
                            if (context.abort != AbortType.None)
                            {

                                var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                                var _break = (context.abort > AbortType.Continue) || !me;
                                if (context.abort < AbortType.Return && me)
                                {
                                    context.abort = AbortType.None;
                                    context.abortInfo = JSObject.notExists;
                                }
                                if (_break)
                                    return null;
                            }
                            index++;
                        }
                    }
                    finally
                    {
                        keys.Dispose();
                    }
                }
                s = s.__proto__;
                if (s == JSObject.Null || !s.IsDefinded || (s.valueType >= JSObjectType.Object && s.oValue == null))
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

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict, CompilerMessageCallback message, FunctionStatistic statistic, Options opts)
        {
            Parser.Build(ref variable, 2, variables, strict, message, statistic, opts);
            var tvar = variable as VariableDefineStatement;
            if (tvar != null)
                variable = tvar.initializators[0];
            if (variable is Assign)
                ((variable as Assign).first.first as GetVariableExpression).forceThrow = false;
            Parser.Build(ref source, 2, variables, strict, message, statistic, opts);
            Parser.Build(ref body, System.Math.Max(1, depth), variables, strict, message, statistic, opts);
            if (variable is Expressions.None)
            {
                if ((variable as Expressions.None).SecondOperand != null)
                    throw new InvalidOperationException("Invalid left-hand side in for-in");
                variable = (variable as Expressions.None).FirstOperand;
            }
            if (message != null
                && (source is Json
                || source is ArrayExpression
                || source is Constant))
                message(MessageLevel.Recomendation, new CodeCoordinates(0, Position, Length), "for..in with constant source. This reduce performance. Rewrite without using for..in.");
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message)
        {
            source.Optimize(ref source, owner, message);
            if (body != null)
                body.Optimize(ref body, owner, message);
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