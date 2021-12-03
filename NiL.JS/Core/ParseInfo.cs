using System;
using System.Collections.Generic;

namespace NiL.JS.Core
{
    public sealed class ParseInfo
    {
        public readonly string SourceCode;
        public readonly Stack<bool> AllowBreak;
        public readonly Stack<bool> AllowContinue;
        public readonly List<VariableDescriptor> Variables;
        public readonly Dictionary<string, JSValue> StringConstants;
        public readonly Dictionary<int, JSValue> IntConstants;
        public readonly Dictionary<double, JSValue> DoubleConstants;
        public readonly InternalCompilerMessageCallback Message;

        public List<string> Labels { get; private set; }

        public string Code;

        public int LabelsCount;
        public int AllowReturn;
        public int LexicalScopeLevel;
        public int FunctionScopeLevel;
        public CodeContext CodeContext;
        public int BreaksCount;
        public int ContiniesCount;

        public bool Strict => (CodeContext & CodeContext.Strict) != 0;
        public bool AllowDirectives => (CodeContext & CodeContext.AllowDirectives) != 0;

        private ParseInfo(
            string code,
            List<string> labels,
            Stack<bool> allowBreak,
            Stack<bool> allowContinue,
            Dictionary<string, JSValue> stringConstants,
            Dictionary<int, JSValue> intConstants,
            Dictionary<double, JSValue> doubleConstants,
            List<VariableDescriptor> variables)
        {
            Code = code;
            SourceCode = code;
            Labels = labels;
            AllowBreak = allowBreak;
            AllowContinue = allowContinue;
            StringConstants = stringConstants;
            IntConstants = intConstants;
            DoubleConstants = doubleConstants;
            Variables = variables;
        }

        public ParseInfo(string sourceCode, InternalCompilerMessageCallback message)
        {
            Code = Parser.RemoveComments(sourceCode, 0);
            SourceCode = sourceCode;
            Message = message;

            CodeContext |= CodeContext.AllowDirectives;

            Labels = new List<string>();
            AllowBreak = new Stack<bool>();
            AllowContinue = new Stack<bool>();

            StringConstants = new Dictionary<string, JSValue>();
            IntConstants = new Dictionary<int, JSValue>();
            DoubleConstants = new Dictionary<double, JSValue>();

            Variables = new List<VariableDescriptor>();

            AllowContinue.Push(false);
            AllowBreak.Push(false);
        }

        internal JSValue GetCachedValue(int value)
        {
            if (!IntConstants.ContainsKey(value))
            {
                JSValue jsvalue = value;
                IntConstants[value] = jsvalue;
                return jsvalue;
            }
            else
            {
                return IntConstants[value];
            }
        }

        public ParseInfo AlternateCode(string code)
        {
            var result = new ParseInfo(code, Labels, AllowBreak, AllowContinue, StringConstants, IntConstants, DoubleConstants, Variables);
            return result;
        }

        private class ContextReseter : IDisposable
        {
            private readonly ParseInfo _parseInfo;
            private readonly CodeContext _oldCodeContext;

            public ContextReseter(ParseInfo parseInfo, CodeContext oldCodeContext)
            {
                _parseInfo = parseInfo;
                _oldCodeContext = oldCodeContext;
            }

            public void Dispose()
            {
                _parseInfo.CodeContext = _oldCodeContext;
            }
        }

        public IDisposable WithCodeContext(CodeContext codeContext = default(CodeContext))
        {
            var result = new ContextReseter(this, CodeContext);
            CodeContext |= codeContext;
            return result;
        }

        private class LabelsReseter : IDisposable
        {
            private readonly ParseInfo _parseInfo;
            private readonly List<string> _oldLabels;

            public LabelsReseter(ParseInfo parseInfo, List<string> oldLabels)
            {
                _parseInfo = parseInfo;
                _oldLabels = oldLabels;
            }

            public void Dispose()
            {
                _parseInfo.Labels = _oldLabels;
            }
        }

        public IDisposable WithNewLabelsScope()
        {
            var result = new LabelsReseter(this, Labels);
            this.Labels = new List<string>();
            return result;
        }
    }

#if !NETCORE
    [Serializable]
#endif
    public sealed class FunctionInfo
    {
        public bool UseGetMember;
        public bool UseCall;
        public bool WithLexicalEnvironment;
        public bool ContainsArguments;
        public bool ContainsRestParameters;
        public bool ContainsEval;
        public bool ContainsWith;
        public bool NeedDecompose;
        public bool ContainsInnerEntities;
        public bool ContainsThis;
        public bool ContainsDebugger;
        public bool ContainsTry;
        public readonly List<Expressions.Expression> Returns = new List<Expressions.Expression>();
        public PredictedType ResultType;
    }
}