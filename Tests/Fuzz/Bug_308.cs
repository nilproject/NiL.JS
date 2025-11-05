using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using System.Linq;

namespace Tests.Fuzz;

[TestClass]
public class Bug_308
{
    [TestMethod]
    public void ShouldNotThrow0()
    {
        var module = new Module(
            """
            export default function(a,b){
                return a+b; 
            }
            """);

        module.Run();
    }

    [TestMethod]
    public void ShouldNotThrow1()
    {
        var module = new Module(
            """
            export function foo(a,b){
                return a+b; 
            }

            foo();
            """);

        module.Run();
    }

    [TestMethod]
    public void ShouldThrow0()
    {
        var test = () =>
        {
            var module = new Module(
                """
                export function(a,b){
                    return a+b; 
                }
                """);

            module.Run();
        };

        Assert.ThrowsException<JSException>(test);
    }
}
