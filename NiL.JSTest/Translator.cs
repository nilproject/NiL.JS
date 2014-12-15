using NiL.JS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JSTest
{
    public static class Translator
    {
        private sealed class Visitor : Visitor<Visitor>
        {
            private StringBuilder result;

            public Visitor()
            {
                result = new StringBuilder();
            }

            protected override Visitor Visit(CodeNode node)
            {
                result.Append(node);
                return this;
            }

            protected override Visitor Visit(JS.Statements.CodeBlock node)
            {
                if (node.LocalVariables.Length > 0)
                {
                    for (var i = 0; i < node.LocalVariables.Length; i++)
                    {
                        if (i == 0)
                            result.Append("object ").Append(node.LocalVariables[i].Name);
                        else
                            result.Append(", ").Append(node.LocalVariables[i].Name);
                        if (node.LocalVariables[i].Inititalizator != null)
                        {
                            result.Append(" = ");
                            node.LocalVariables[i].Inititalizator.Visit(this);
                        }
                    }
                    result.Append(";").Append(Environment.NewLine);
                }
                for (var i = node.Body.Length; i-- > 0; )
                {
                    node.Body[i].Visit(this);
                    result.Append(';').Append(Environment.NewLine);
                }
                return this;
            }

            protected override Visitor Visit(JS.Statements.VariableDefineStatement node)
            {
                for (var i = 0; i < node.Initializators.Length; i++)
                    node.Initializators[i].Visit(this);
                return this;
            }

            protected override Visitor Visit(JS.Expressions.FunctionExpression node)
            {
                result.Append("(Func<");
                var prms = node.Parameters;
                for (var i = 0; i < prms.Count; i++)
                    result.Append("object,");
                result.Append("object>)");
                result.Append("(");
                for (var i = 0; i < prms.Count; i++)
                {
                    if (i > 0)
                        result.Append(",");
                    result.Append(prms[i].Name);
                }
                result.Append(") => {");
                node.Body.Visit(this);
                result.Append("return null").Append(Environment.NewLine).Append('}');
                return this;
            }

            protected override Visitor Visit(JS.Statements.ReturnStatement node)
            {
                result.Append("return");
                if (node.Body != node)
                {
                    result.Append(" ");
                    node.Body.Visit(this);
                }
                return this;
            }

            public override string ToString()
            {
                return result.ToString();
            }
        }

        public static string Translate(CodeNode node)
        {
            object test = (Func<int, int, int>)delegate(int a, int b) { return a + b; };
            return node.Visit(new Visitor()).ToString();
        }
    }
}
