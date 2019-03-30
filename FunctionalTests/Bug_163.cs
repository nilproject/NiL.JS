using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace FunctionalTests
{
    [TestClass]
    public class Bug_163
    {
        [TestMethod]
        public void CheckBug163()
        {
            const string code = @"'use strict';

function cr(n, t, i, r, u) {
    var o;
    if (n && n.length > 0 && (o = n.length, o > 0)) {
        var f = r === undefined || r < 0 ? 0 : r,
            s = u === undefined || f + u > o - 1 ? o - 1 : f + u,
            e = void 0;
        for (arguments.length <= 2 ? (e = n[f], f++) : e = i; f <= s;) e = t(e, n[f], f), f++;
        return e
    }
    return i
}";
            var context = new Context(true);


            var result = context.Eval(code);
        }
    }
}
