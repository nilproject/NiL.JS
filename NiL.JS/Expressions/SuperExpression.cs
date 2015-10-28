using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Expressions
{
    public sealed class SuperExpression : GetVariableExpression
    {
        internal SuperExpression(int functionDepth)
            : base("super", functionDepth)
        {

        }
    }
}
