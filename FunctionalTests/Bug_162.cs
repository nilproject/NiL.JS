using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace FunctionalTests
{
    [TestClass]
    public class Bug_162
    {
        [TestMethod]
        public void CascadeIfElseShouldProcessCorrectly()
        {
            const string code = @"var arr = ['a', 'b', 'c'],
    num = 0
    ;

if (arr.length > 1)
    for (var i = 0; i < arr.length; i++)
        if (arr[i] == 'a') num += 12;
        else if (arr[i] == 'b') num += 5;
        else break;else num = -1;";
            var context = new Context(true);


            var result = context.Eval(code);
        }
    }
}
