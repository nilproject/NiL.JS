using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NiL.JS.Core;
using NiL.JS.Core.JIT;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ForOfStatement : CodeNode
    {
        private CodeNode variable;
        private CodeNode source;
        private CodeNode body;
        private string[] labels;

        public CodeNode Variable { get { return variable; } }
        public CodeNode Source { get { return source; } }
        public CodeNode Body { get { return body; } }
        public IReadOnlyCollection<string> Labels { get { return Array.AsReadOnly<string>(labels); } }

        private ForOfStatement()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (!Parser.Validate(state.Code, "for(", ref i) && (!Parser.Validate(state.Code, "for (", ref i)))
                return new ParseResult();
            while (char.IsWhiteSpace(state.Code[i])) i++;
            var res = new ForOfStatement()
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
                        throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at " + Tools.PositionToTextcord(state.Code, start))));
                }
                res.variable = new VariableDefineStatement(varName, new GetVariableStatement(varName, state.functionsDepth) { Position = start, Length = i - start, functionDepth = state.functionsDepth }, false) { Position = vStart, Length = i - vStart };
            }
            else
            {
                if (state.Code[i] == ';')
                    return new ParseResult();
                res.variable = ExpressionStatement.Parse(state, ref i, true, true).Statement;
            }
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (!Parser.Validate(state.Code, "of", ref i))
                return new ParseResult();
            while (char.IsWhiteSpace(state.Code[i])) i++;
            res.source = Parser.Parse(state, ref i, 1);
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (state.Code[i] != ')')
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \")\" at + " + Tools.PositionToTextcord(state.Code, i))));
            i++;
            state.AllowBreak.Push(true);
            state.AllowContinue.Push(true);
            res.body = Parser.Parse(state, ref i, 0);
            if (res.body is FunctionStatement && state.strict.Peek())
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
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

#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(Core.JIT.TreeBuildingState state)
        {
            var continueTarget = System.Linq.Expressions.Expression.Label("continue" + (DateTime.Now.Ticks % 10000));
            var nextProto = Expression.Label();
            var breakTarget = System.Linq.Expressions.Expression.Label("break" + (DateTime.Now.Ticks % 10000));
            for (var i = 0; i < labels.Length; i++)
                state.NamedContinueLabels[labels[i]] = continueTarget;
            state.BreakLabels.Push(breakTarget);
            state.ContinueLabels.Push(continueTarget);
            try
            {
                var val = Expression.Parameter(typeof(JSObject));
                var @enum = Expression.Parameter(typeof(NiL.JS.Statements.ForInStatement._ForcedEnumerator<string>));
                var source = Expression.Parameter(typeof(JSObject));
                var res = Expression.Block(new[] { val, @enum, source },
                    Expression.Assign(source, this.source.CompileToIL(state)),
                    Expression.Assign(val, Expression.Call(JITHelpers.ContextParameter, typeof(Context).GetMethod("GetVariable", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(string), typeof(bool) }, null), JITHelpers.wrap(variable.ToString()), JITHelpers.wrap(true))),
                    Expression.Loop(
                            Expression.IfThenElse(
                                    Expression.AndAlso(Expression.ReferenceNotEqual(source, JITHelpers.wrap(null)),
                                        Expression.AndAlso(Expression.Property(source, "isDefinded"),
                                                        Expression.OrElse(Expression.LessThan(Expression.Convert(Expression.Field(source, "valueType"), typeof(int)), JITHelpers.wrap((int)JSObjectType.Object)),
                                                                       Expression.ReferenceNotEqual(Expression.Field(source, "oValue"), JITHelpers.wrap(null))))),
                                      Expression.Block(
                                                Expression.Assign(@enum, Expression.Call(JITHelpers.methodof(new Func<JSObject, object>(NiL.JS.Statements.ForInStatement._ForcedEnumerator<string>.create)), source))
                                                , Expression.Loop(
                                                    Expression.IfThenElse(Expression.Call(@enum, typeof(NiL.JS.Statements.ForInStatement._ForcedEnumerator<string>).GetMethod("MoveNext")), Expression.Block(
                                                        Expression.Assign(Expression.Field(val, "valueType"), JITHelpers.wrap(JSObjectType.String))
                                                        , Expression.Assign(Expression.Field(val, "oValue"), Expression.Property(@enum, "Current"))
                                                        , Expression.Call(val, typeof(JSObject).GetMethod("Assign"), Expression.Call(source, typeof(JSObject).GetMethod("GetMember", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new Type[] { typeof(JSObject), typeof(bool), typeof(bool) }, null), val, JITHelpers.wrap(false), JITHelpers.wrap(false)))
                                                        , this.body.CompileToIL(state)
                                                        , Expression.Label(continueTarget)
                                                    ), Expression.Goto(nextProto)))
                                                , Expression.Label(nextProto)
                                                , Expression.IfThenElse(Expression.ReferenceNotEqual(Expression.Field(source, "__proto__"), JITHelpers.wrap(null)),
                                                    Expression.Assign(source, Expression.Field(source, "__proto__"))
                                                , Expression.Block(
                                                    Expression.Assign(Expression.Field(val, "valueType"), JITHelpers.wrap(JSObjectType.String))
                                                    , Expression.Assign(Expression.Field(val, "oValue"), JITHelpers.wrap("__proto__"))
                                                    , Expression.Assign(source, Expression.Call(source, typeof(JSObject).GetMethod("GetMember", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new Type[] { typeof(JSObject), typeof(bool), typeof(bool) }, null), val, JITHelpers.wrap(false), JITHelpers.wrap(false)))
                                                ))
                                            )
                                      , Expression.Goto(breakTarget)))
                    , Expression.Label(breakTarget)
                    );
                return res;
            }
            finally
            {
                if (state.BreakLabels.Peek() != breakTarget)
                    throw new InvalidOperationException();
                state.BreakLabels.Pop();
                if (state.ContinueLabels.Peek() != continueTarget)
                    throw new InvalidOperationException();
                state.ContinueLabels.Pop();
                for (var i = 0; i < labels.Length; i++)
                    state.NamedContinueLabels.Remove(labels[i]);
            }
        }

#endif

        internal override JSObject Evaluate(Context context)
        {
            JSObject res = JSObject.undefined;
            var s = source.Evaluate(context);
            var v = variable.EvaluateForAssing(context);
            int index = 0;
            if (!s.isDefinded || (s.valueType >= JSObjectType.Object && s.oValue == null))
                return JSObject.undefined;
            while (s != null)
            {
                if (s.oValue is Core.BaseTypes.Array)
                {
                    var src = s.oValue as Core.BaseTypes.Array;
                    foreach (var item in src.data)
                    {
                        if (item == null
                            || !item.isExist
                            || (item.attributes & JSObjectAttributesInternal.DoNotEnum) != 0)
                            continue;
                        v.Assign(item);
#if DEV
                        if (context.debugging && !(body is CodeBlock))
                            context.raiseDebugger(body);
#endif
                        res = body.Evaluate(context) ?? res;
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
                                || !item.Value.isExist
                                || (item.Value.attributes & JSObjectAttributesInternal.DoNotEnum) != 0)
                                continue;
                            v.Assign(item.Value);
#if DEV
                        if (context.debugging && !(body is CodeBlock))
                            context.raiseDebugger(body);
#endif
                            res = body.Evaluate(context) ?? res;
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
                    var keys = s.GetEnumerator();
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
                            for (int i = 0; i < index && keys.MoveNext(); i++) ;
                        }
                        var o = keys.Current;
                        v.valueType = JSObjectType.String;
                        v.oValue = o;
                        v.Assign(s.GetMember(v, false, true));
#if DEV
                    if (context.debugging && !(body is CodeBlock))
                        context.raiseDebugger(body);
#endif
                        res = body.Evaluate(context) ?? res;
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
                s = s.__proto__ ?? s["__proto__"];
                if (!s.isDefinded || (s.valueType >= JSObjectType.Object && s.oValue == null))
                    break;
            }
            return res;
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

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            var tvar = variable;
            if (tvar is VariableDefineStatement)
                variable = (tvar as VariableDefineStatement).initializators[0];
            Parser.Optimize(ref tvar, 1, variables, strict);
            Parser.Optimize(ref source, 2, variables, strict);
            Parser.Optimize(ref body, System.Math.Max(1, depth), variables, strict);
            if (variable is Expressions.None)
            {
                if ((variable as Expressions.None).SecondOperand != null)
                    throw new InvalidOperationException("Invalid left-hand side in for-in");
                variable = (variable as Expressions.None).FirstOperand;
            }
            return false;
        }

        public override string ToString()
        {
            return "for (" + variable + " in " + source + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}