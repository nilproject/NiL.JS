using System.Collections.Generic;
using System.Text;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    public enum TemplateStringMode
    {
        Regular,
        Tag
    }

    public sealed class TemplateString : Expression
    {
        private sealed class SuspendData
        {
            public int Index;
            public object result;
        }

        private Expression[] expressions;
        private string[] strings;

        public IEnumerable<string> Strings => strings;
        public IEnumerable<Expression> Expressions => expressions;
        public TemplateStringMode Mode { get; private set; }

        internal TemplateString(string[] strings, Expression[] expression)
        {
            this.strings = strings;
            this.expressions = expression;
        }

        public static CodeNode Parse(ParseInfo state, ref int index)
        {
            return Parse(state, ref index, TemplateStringMode.Regular);
        }

        public static Expression Parse(ParseInfo state, ref int index, TemplateStringMode mode)
        {
            if (state.Code[index] != '`')
                ExceptionHelper.ThrowSyntaxError(JS.Strings.UnexpectedToken, state.Code, index);

            var strings = new List<string>();
            var expressions = new List<Expression>();
            var pos = index + 1;

            for (;;)
            {
                var startPos = pos;
                var ignore = false;
                while (ignore || ((state.Code[pos] != '$' || state.Code[pos + 1] != '{') && state.Code[pos] != '`'))
                {
                    ignore = !ignore && state.Code[pos] == '\\';
                    pos++;
                }

                var tempStr = state.Code.Substring(startPos, pos - startPos);
                if (mode == TemplateStringMode.Regular)
                    tempStr = Tools.Unescape(tempStr, state.strict);
                strings.Add(tempStr);

                if (state.Code[pos] == '$')
                {
                    pos += 2;
                    expressions.Add((Expression)ExpressionTree.Parse(state, ref pos));

                    Tools.SkipSpaces(state.Code, ref pos);
                    if (state.Code[pos] != '}')
                        ExceptionHelper.ThrowSyntaxError(JS.Strings.UnexpectedToken, state.Code, pos);
                }

                pos++;

                if (state.Code[pos - 1] == '`')
                    break;
            }

            index = pos;
            return new TemplateString(strings.ToArray(), expressions.ToArray()) { Mode = mode };
        }

        public override JSValue Evaluate(Context context)
        {
            StringBuilder result = null;
            Array stringsArray = null;
            JSValue[] tagResult = null;
            int i = 0;

            if (context != null && context._executionMode >= ExecutionMode.Resume)
            {
                var suspendData = context.SuspendData[this] as SuspendData;

                if (Mode == TemplateStringMode.Regular)
                {
                    result = suspendData.result as StringBuilder;
                }
                else
                {
                    tagResult = suspendData.result as JSValue[];
                    stringsArray = tagResult[0]["raw"].Value as Array;
                }

                i = suspendData.Index;
            }
            else
            {
                if (Mode == TemplateStringMode.Regular)
                {
                    result = new StringBuilder();
                }
                else
                {
                    stringsArray = new Array(strings);
                    tagResult = new JSValue[1 + expressions.Length];
                    tagResult[0] = stringsArray;
                    stringsArray = (stringsArray["raw"] = new Array(strings.Length)) as Array;
                }
            }

            for (; i < strings.Length; i++)
            {
                if (i > 0)
                {
                    var temp = expressions[i - 1].Evaluate(context);
                    if (context != null && context._executionMode != ExecutionMode.None)
                    {
                        if (context._executionMode == ExecutionMode.Suspend)
                        {
                            var suspendData = new SuspendData();
                            suspendData.Index = i;
                            suspendData.result = result;
                            context.SuspendData[this] = suspendData;
                        }

                        return null;
                    }

                    if (Mode == TemplateStringMode.Regular)
                        result.Append(temp);
                    else
                        tagResult[i] = temp;
                }

                if (Mode == TemplateStringMode.Regular)
                    result.Append(strings[i]);
                else
                    stringsArray.Add(strings[i].Replace(@"\", @"\\"));
            }

            if (Mode == TemplateStringMode.Regular)
                return result.ToString();
            else
                return new JSValue { _oValue = tagResult, _valueType = JSValueType.SpreadOperatorResult };
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (expressions.Length == 0)
            {
                if (Mode == TemplateStringMode.Regular)
                    _this = new Constant(strings[0]);
                return false;
            }

            for (var i = 0; i < expressions.Length; i++)
                Parser.Build(ref expressions[i], expressionDepth, variables, codeContext, message, stats, opts);

            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            for (var i = 0; i < expressions.Length; i++)
                expressions[i].Optimize(ref expressions[i], owner, message, opts, stats);
        }

        public override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            var lastDecomposeIndex = -1;
            for (var i = 0; i < expressions.Length; i++)
            {
                expressions[i].Decompose(ref expressions[i], result);
                if (expressions[i].NeedDecompose)
                {
                    lastDecomposeIndex = i;
                }
            }

            for (var i = 0; i < lastDecomposeIndex; i++)
            {
                if (!(expressions[i] is ExtractStoredValue))
                {
                    result.Add(new StoreValue(expressions[i], false));
                    expressions[i] = new ExtractStoredValue(expressions[i]);
                }
            }
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            for (var i = 0; i < expressions.Length; i++)
                expressions[i].RebuildScope(functionInfo, transferedVariables, scopeBias);
        }

        public override string ToString()
        {
            var result = new StringBuilder()
                .Append('`');
            for (var i = 0; i < strings.Length; i++)
            {
                if (i > 0)
                {
                    result.Append("${ ")
                          .Append(expressions[i - 1])
                          .Append(" }");
                }

                for (var j = 0; j < strings[i].Length; j++)
                {
                    JSON.escapeIfNeed(result, strings[i][j]);
                }
            }

            return result.Append('`').ToString();
        }
    }
}
