using System;
using System.Collections.Generic;
using NiL.JS.Core;
#if !PORTABLE
using NiL.JS.Core.JIT;
#endif

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class GetArgumentsExpression : Variable
    {
        internal GetArgumentsExpression(int functionDepth)
            : base("arguments", functionDepth)
        {
        }

        internal protected override JSValue EvaluateForWrite(Context context)
        {
            if (context._owner._functionDefinition.kind == BaseLibrary.FunctionKind.Arrow)
                context = context._parent;
            if (context._arguments == null)
                context._owner.BuildArgumentsObject();
            var res = context._arguments;
            if (res is Arguments)
                context._arguments = res = res.CloneImpl(false);
            if (context._variables != null && context._variables.ContainsKey(Name))
                context._variables[Name] = res;
            return res;
        }

        public override JSValue Evaluate(Context context)
        {
            if (context._owner._functionDefinition.kind == BaseLibrary.FunctionKind.Arrow)
                context = context._parent;
            if (context._arguments == null)
                context._owner.BuildArgumentsObject();
            return context._arguments;
        }
    }

#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public class Variable : VariableReference
    {
        private string _variableName;
        internal bool _SuspendThrow;
        internal bool _ForceThrow;

        public override string Name { get { return _variableName; } }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal Variable(string name, int scopeLevel)
        {
            int fake = 0;
            if (!Parser.ValidateName(name, fake, true, true, false))
                throw new ArgumentException("Invalid variable name");

            ScopeLevel = scopeLevel;
            _variableName = name;
        }

        internal protected override JSValue EvaluateForWrite(Context context)
        {
            var result = _descriptor.Get(context, true, _scopeLevel);

            if (context._strict || _ForceThrow)
            {
                if (result._valueType < JSValueType.Undefined && (!_SuspendThrow || _ForceThrow))
                {
                    if ((_codeContext & CodeContext.InEval) != 0)
                    {
                        ExceptionHelper.ThrowVariableIsNotDefined(_variableName, this);
                    }
                    else
                    {
                        var code = context.RootContext._owner?._functionDefinition?._body?.Code;
                        if (code == null)
                            code = context._module?.Code;

                        ExceptionHelper.ThrowVariableIsNotDefined(_variableName, code ?? "", Position, Length, this);
                    }
                }

                if (context._strict)
                {
                    if ((result._attributes & JSValueAttributesInternal.Argument) != 0)
                        context._owner.BuildArgumentsObject();
                }
            }

            return result;
        }

        public override JSValue Evaluate(Context context)
        {
            var res = _descriptor.Get(context, false, _scopeLevel);
            switch (res._valueType)
            {
                case JSValueType.NotExists:
                    {
                        if (!_SuspendThrow)
                        {
                            if ((_codeContext & CodeContext.InEval) != 0)
                            {
                                ExceptionHelper.ThrowVariableIsNotDefined(_variableName, this);
                            }
                            else
                            {
                                var code = context.RootContext._owner?._functionDefinition?._body?.Code;
                                if (code == null)
                                    code = context._module?.Code;

                                ExceptionHelper.ThrowVariableIsNotDefined(_variableName, code, Position, Length, this);
                            }
                        }
                        break;
                    }
                case JSValueType.Property:
                    {
                        return Tools.InvokeGetter(res, context._objectSource);
                    }
            }
            return res;
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            return null;
        }

        public override string ToString()
        {
            return _variableName;
        }

#if !NET35 && !PORTABLE
        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            dynamicValues.Add(this);
            var res = System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.ArrayAccess(JITHelpers.DynamicValuesParameter, JITHelpers.cnst(dynamicValues.Count - 1)),
                forAssign ? JITHelpers.EvaluateForWriteMethod : JITHelpers.EvaluateMethod,
                JITHelpers.ContextParameter
                );
            if (expectedType == typeof(int))
                res = System.Linq.Expressions.Expression.Call(JITHelpers.JSObjectToInt32Method, res);
            return res;
        }
#endif
        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            _codeContext = codeContext;

            VariableDescriptor desc = null;
            if (!variables.TryGetValue(_variableName, out desc) || desc == null)
            {
                desc = new VariableDescriptor(this, 1) { isDefined = false };
                variables[_variableName] = this.Descriptor;
            }
            else
            {
                if (!desc.references.Contains(this))
                    desc.references.Add(this);
                _descriptor = desc;
            }

            if (_variableName == "this")
            {
                stats.ContainsThis = true;
                desc.definitionScopeLevel = -1;
            }
            else if (((codeContext & CodeContext.InWith) != 0) || (stats.ContainsEval && !desc.isDefined))
            {
                ScopeLevel = -Math.Abs(ScopeLevel);
                desc.definitionScopeLevel = -Math.Abs(desc.definitionScopeLevel);
            }

            _ForceThrow |= desc.lexicalScope; // ����� TDZ

            if (expressionDepth >= 0 && expressionDepth < 2 && desc.IsDefined && !desc.lexicalScope && (opts & Options.SuppressUselessExpressionsElimination) == 0)
            {
                _this = null;
                Eliminated = true;
                if (message != null)
                    message(MessageLevel.Warning, Position, Length, "Unused getting of defined variable was removed. Maybe something missing.");
            }
            else if (_variableName == "arguments" && (codeContext & CodeContext.InFunction) != 0)
            {
                if (stats != null)
                    stats.ContainsArguments = true;
                _this = new GetArgumentsExpression(ScopeLevel) { _descriptor = _descriptor };
            }

            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            if ((opts & Options.SuppressConstantPropogation) == 0
                && !_descriptor.captured
                && _descriptor.isDefined
                && !stats.ContainsWith
                && !stats.ContainsEval
                && (_descriptor.owner != owner || !owner._functionInfo.ContainsArguments))
            {
                var assigns = _descriptor.assignments;
                if (assigns != null && assigns.Count > 0)
                {
                    CodeNode lastAssign = null;
                    for (var i = assigns.Count; i-- > 0;)
                    {
                        if (assigns[i]._left == this
                            || ((assigns[i]._left is AssignmentOperatorCache) && assigns[i]._left._left == this))
                        {
                            lastAssign = null;
                            break;
                        }

                        if (assigns[i].Position > Position)
                        {
                            if ((_codeContext & CodeContext.InLoop) != 0 && ((assigns[i] as Expression)._codeContext & CodeContext.InLoop) != 0)
                            {
                                lastAssign = null;
                                break;
                            }
                            continue;
                        }

                        if (_descriptor.isReadOnly)
                        {
                            if (assigns[i] is ForceAssignmentOperator)
                            {
                                lastAssign = assigns[i];
                                break;
                            }
                        }
                        else if (lastAssign == null || assigns[i].Position > lastAssign.Position)
                        {
                            lastAssign = assigns[i];
                        }
                    }
                    var assign = lastAssign as Assignment;
                    if (assign != null && (assign._codeContext & CodeContext.Conditional) == 0 && assign._right is Constant)
                    {
                        _this = assign._right;
                    }
                }
            }
        }
    }
}