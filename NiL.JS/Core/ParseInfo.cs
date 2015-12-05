using System.Collections.Generic;

namespace NiL.JS.Core
{
    public sealed class ParseInfo
    {
        public readonly string SourceCode;
        public readonly Stack<bool> AllowBreak;
        public readonly Stack<bool> AllowContinue;
        public readonly List<VariableDescriptor> Variables;
        public readonly Dictionary<string, JSValue> stringConstants;
        public readonly Dictionary<int, JSValue> intConstants;
        public readonly Dictionary<double, JSValue> doubleConstants;

        public List<string> Labels;
        public int LabelsCount;
        public int AllowReturn;
        public int lexicalScopeLevel;
        public int functionScopeLevel;
        public CodeContext CodeContext;
        public bool strict;
        public string Code;
        public bool AllowDirectives;
        public int breaksCount;
        public int continiesCount;

        public readonly CompilerMessageCallback message;

        public ParseInfo(string code, string sourceCode, CompilerMessageCallback message)
        {
            Code = code;
            SourceCode = sourceCode;
            Labels = new List<string>();
            AllowDirectives = true;
            AllowBreak = new Stack<bool>();
            AllowBreak.Push(false);
            AllowContinue = new Stack<bool>();
            AllowContinue.Push(false);
            this.message = message;
            stringConstants = new Dictionary<string, JSValue>();
            intConstants = new Dictionary<int, JSValue>();
            doubleConstants = new Dictionary<double, JSValue>();
            Variables = new List<VariableDescriptor>();
        }
    }

    public sealed class FunctionInfo
    {
        public bool UseGetMember;
        public bool UseCall;
        public bool WithLexicalEnvironment;
        public bool ContainsArguments;
        public bool ContainsRestParameters;
        public bool ContainsEval;
        public bool ContainsWith;
        public bool ContainsYield;
        public bool ContainsInnerEntities;
        public bool ContainsThis;
        public bool ContainsDebugger;
        public bool ContainsTry;
        public readonly List<Expressions.Expression> Returns = new List<Expressions.Expression>();
        public PredictedType ResultType;
    }
}