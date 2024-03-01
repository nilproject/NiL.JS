using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace Tests.Fuzz;

[TestClass]
public sealed class Bug228
{
    public enum Flags
    {
        Flag1 = 1,
        Flag2 = 2
    }

    public class JSQuestion : CustomType
    {
        public Flags flags { get; set; }
        public string outputColumnTemplateOVN { get; set; }
    }

    [TestMethod]
    public void Check()
    {
        var context = new Context();

        var q = new JSQuestion();
        context
           .DefineVariable("Q")
           .Assign(Context.CurrentGlobalContext.ProxyValue(q));

        context.DefineConstructor(typeof(Flags));

        context.Eval(@"
var t = Q;
t.outputColumnTemplateOVN = '1234';
t.flags = Flags.Flag1;
t.flags |= Flags.Flag2;
");

        Assert.AreEqual(Flags.Flag1 | Flags.Flag2, q.flags);
        Assert.AreEqual("1234", q.outputColumnTemplateOVN);
    }
}
