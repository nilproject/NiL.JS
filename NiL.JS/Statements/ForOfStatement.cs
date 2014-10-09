using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ForOfStatement : CodeNode
    {
        private CodeNode variable;
        private CodeNode source;
        private CodeNode body;
        private List<string> labels;

        public CodeNode Variable { get { return variable; } }
        public CodeNode Source { get { return source; } }
        public CodeNode Body { get { return body; } }
        public string[] Labels { get { return labels.ToArray(); } }

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
                labels = state.Labels.GetRange(state.Labels.Count - state.LabelCount, state.LabelCount)
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
                        bool _break = (context.abort > AbortType.Continue) || ((context.abortInfo != null) && (labels.IndexOf(context.abortInfo.oValue as string) == -1));
                        if (context.abort < AbortType.Return && ((context.abortInfo == null) || (labels.IndexOf(context.abortInfo.oValue as string) != -1)))
                        {
                            context.abort = AbortType.None;
                            context.abortInfo = JSObject.notExists;
                        }
                        if (_break)
                            return null;
                    }
                    index++;
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
            Parser.Optimize(ref variable, 1, variables, strict);
            if (variable is VariableDefineStatement)
                variable = (variable as VariableDefineStatement).initializators[0];
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