using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NiL.JS.Core;
using NiL.JS.Core.JIT;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ForInStatement : CodeNode
    {
        internal sealed class _ForcedEnumerator<T> : IEnumerator<T>
        {
            private int index;
            private IEnumerable<T> owner;
            private IEnumerator<T> parent;

            private _ForcedEnumerator(IEnumerable<T> owner)
            {
                this.owner = owner;
                this.parent = owner.GetEnumerator();
            }

            public static _ForcedEnumerator<T> create(IEnumerable<T> owner)
            {
                return new _ForcedEnumerator<T>(owner);
            }

            #region Члены IEnumerator<T>

            public T Current
            {
                get { return parent.Current; }
            }

            #endregion

            #region Члены IDisposable

            public void Dispose()
            {
                parent.Dispose();
            }

            #endregion

            #region Члены IEnumerator

            object System.Collections.IEnumerator.Current
            {
                get { return parent.Current; }
            }

            public bool MoveNext()
            {
                try
                {
                    var res = parent.MoveNext();
                    if (res)
                        index++;
                    return res;
                }
                catch
                {
                    parent = owner.GetEnumerator();
                    for (int i = 0; i < index && parent.MoveNext(); i++) ;
                    return MoveNext();
                }
            }

            public void Reset()
            {
                parent.Reset();
            }

            #endregion
        }

        private CodeNode variable;
        private CodeNode source;
        private CodeNode body;
        private string[] labels;

        public CodeNode Variable { get { return variable; } }
        public CodeNode Source { get { return source; } }
        public CodeNode Body { get { return body; } }
        public IReadOnlyCollection<string> Labels { get { return Array.AsReadOnly<string>(labels); } }

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
            if (!Parser.Validate(state.Code, "in", ref i))
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
                var @enum = Expression.Parameter(typeof(_ForcedEnumerator<string>));
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
                                                Expression.Assign(@enum, Expression.Call(JITHelpers.methodof(new Func<JSObject, object>(_ForcedEnumerator<string>.create)), source))
                                                , Expression.Loop(
                                                    Expression.IfThenElse(Expression.Call(@enum, typeof(_ForcedEnumerator<string>).GetMethod("MoveNext")), Expression.Block(
                                                        Expression.Assign(Expression.Field(val, "valueType"), JITHelpers.wrap(JSObjectType.String))
                                                        , Expression.Assign(Expression.Field(val, "oValue"), Expression.Property(@enum, "Current"))
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
            if (!s.isDefinded || (s.valueType >= JSObjectType.Object && s.oValue == null))
                return JSObject.undefined;
            var v = variable.EvaluateForAssing(context);
            int index = 0;
            while (s != null)
            {
                if (s.oValue is Core.BaseTypes.Array)
                {
                    var src = s.oValue as Core.BaseTypes.Array;
                    foreach (var item in (src.data as IEnumerable<KeyValuePair<int, JSObject>>))
                    {
                        if (item.Value == null
                            || !item.Value.isExist
                            || (item.Value.attributes & JSObjectAttributesInternal.DoNotEnum) != 0)
                            continue;
                        if (item.Key >= 0)
                        {
                            v.attributes = (v.attributes & ~JSObjectAttributesInternal.ContainsParsedDouble) | JSObjectAttributesInternal.ContainsParsedInt;
                            v.iValue = item.Key;
                            v.oValue = item.Key.ToString();
                        }
                        else
                        {
                            v.attributes = (v.attributes & ~JSObjectAttributesInternal.ContainsParsedInt) | JSObjectAttributesInternal.ContainsParsedDouble;
                            v.dValue = (uint)item.Key;
                            v.oValue = ((uint)item.Key).ToString();
                        }
                        v.valueType = JSObjectType.String;
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
                            v.valueType = JSObjectType.String;
                            v.oValue = item.Key;
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
                    var keys = _ForcedEnumerator<string>.create(s);
                    for (; ; )
                    {
                        if (!keys.MoveNext())
                            break;
                        var o = keys.Current;
                        v.valueType = JSObjectType.String;
                        v.oValue = o;
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
            variable = (GetVariableStatement)variable;
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