using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ForInStatement : Statement
    {
        private Statement variable;
        private Statement source;
        private Statement body;
        private List<string> labels;

        public Statement Variable { get { return variable; } }
        public Statement Source { get { return source; } }
        public Statement Body { get { return body; } }
        public string[] Labels { get { return labels.ToArray(); } }

        private ForInStatement()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            while (char.IsWhiteSpace(code[i])) i++;
            if (!Parser.Validate(code, "for(", ref i) && (!Parser.Validate(code, "for (", ref i)))
                return new ParseResult();
            while (char.IsWhiteSpace(code[i])) i++;
            var res = new ForInStatement()
            {
                labels = state.Labels.GetRange(state.Labels.Count - state.LabelCount, state.LabelCount)
            };
            var vStart = i;
            if (Parser.Validate(code, "var", ref i))
            {
                while (char.IsWhiteSpace(code[i])) i++;
                int start = i;
                string varName;
                if (!Parser.ValidateName(code, ref i, state.strict.Peek()))
                    throw new ArgumentException();
                varName = Tools.Unescape(code.Substring(start, i - start), state.strict.Peek());
                if (state.strict.Peek())
                {
                    if (varName == "arguments" || varName == "eval")
                        throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at " + Tools.PositionToTextcord(code, start))));
                }
                res.variable = new VariableDefineStatement(varName, new GetVariableStatement(varName) { Position = start, Length = i - start }) { Position = vStart, Length = i - vStart };
            }
            else
            {
                if (code[i] == ';')
                    return new ParseResult();
                res.variable = OperatorStatement.Parse(state, ref i, true, true).Statement;
            }
            while (char.IsWhiteSpace(code[i])) i++;
            if (!Parser.Validate(code, "in", ref i))
                return new ParseResult();
            while (char.IsWhiteSpace(code[i])) i++;
            res.source = Parser.Parse(state, ref i, 1);
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != ')')
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \")\" at + " + Tools.PositionToTextcord(code, i))));
            i++;
            state.AllowBreak++;
            state.AllowContinue++;
            res.body = Parser.Parse(state, ref i, 0);
            if (res.body is FunctionStatement && state.strict.Peek())
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
            state.AllowBreak--;
            state.AllowContinue--;
            res.Position = index;
            res.Length = i - index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = res
            };
        }

        internal override JSObject Invoke(Context context)
        {
            JSObject res = JSObject.undefined;
            var s = Tools.RaiseIfNotExist(source.Invoke(context));
            var v = variable.InvokeForAssing(context);
            int index = 0;
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
                    if (v.assignCallback != null)
                        v.assignCallback(v);
#if DEV
                    if (context.debugging && !(body is CodeBlock))
                        context.raiseDebugger(body);
#endif
                    res = body.Invoke(context) ?? res;
                    if (context.abort != AbortType.None)
                    {
                        bool _break = (context.abort > AbortType.Continue) || ((context.abortInfo != null) && (labels.IndexOf(context.abortInfo.oValue as string) == -1));
                        if (context.abort < AbortType.Return && ((context.abortInfo == null) || (labels.IndexOf(context.abortInfo.oValue as string) != -1)))
                        {
                            context.abort = AbortType.None;
                            context.abortInfo = null;
                        }
                        if (_break)
                            return null;
                    }
                    index++;
                }
                s = s.__proto__;
            }
            return res;
        }

        protected override Statement[] getChildsImpl()
        {
            var res = new List<Statement>()
            {
                body,
                variable,
                source
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            Parser.Optimize(ref variable, 1, variables, strict);
            if (variable is VariableDefineStatement)
                variable = (variable as VariableDefineStatement).initializators[0];
            Parser.Optimize(ref source, 1, variables, strict);
            Parser.Optimize(ref body, System.Math.Max(1, depth), variables, strict);
            if (variable is Operators.None)
            {
                if ((variable as Operators.None).SecondOperand != null)
                    throw new InvalidOperationException("Invalid left-hand side in for-in");
                variable = (variable as Operators.None).FirstOperand;
            }
            return false;
        }

        public override string ToString()
        {
            return "for (" + variable + " in " + source + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}