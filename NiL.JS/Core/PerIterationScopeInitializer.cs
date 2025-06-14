namespace NiL.JS.Core;

internal sealed class PerIterationScopeInitializer : CodeNode
{
    private VariableDescriptor[] _variables;

    public PerIterationScopeInitializer(VariableDescriptor[] variables)
    {
        _variables = variables;
    }

    public override void Decompose(ref CodeNode self)
    {

    }

    public override JSValue Evaluate(Context context)
    {
        if (_variables != null)
        {
            for (var i = 0; i < _variables.Length; i++)
            {
                if (_variables[i].isCaptured)
                    context.DefineVariable(_variables[i].name).Assign(_variables[i].cacheValue);
            }
        }

        return null;
    }
}
