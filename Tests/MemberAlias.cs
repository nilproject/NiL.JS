using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using System.Diagnostics;
using NiL.JS.Core.Interop;

namespace Tests
{
    [TestClass]
    public class MemberAlias
    {
        [TestMethod]
        public void CreateInstanceOfGenericType()
        {
            var context = new Context();
            context.DefineConstructor(typeof(List<>));

            var result = context.Eval("new (List(Number))()").Value;

            Assert.AreSame(typeof(List<Number>), result.GetType());
        }

        private static class ClassWithAliasedMember
        {
            [JavaScriptName("NameForJsSide")]
            public static string NameForDotNetSide()
            {
                return new StackFrame().GetMethod().Name;
            }

            [JavaScriptName("@@MySymbol")]
            public static string MethodBySymbol()
            {
                return new StackFrame().GetMethod().Name;
            }
        }

        [TestMethod]
        public void AliasForMember_ShouldChangePropertyName()
        {
            var context = new Context();
            context.DefineConstructor(typeof(ClassWithAliasedMember));

            var names = context.Eval("Object.getOwnPropertyNames(ClassWithAliasedMember)").Value as NiL.JS.BaseLibrary.Array;

            Assert.IsTrue((bool)NiL.JS.BaseLibrary.Array.includes(names, new Arguments { "NameForJsSide" }));
            Assert.IsFalse((bool)NiL.JS.BaseLibrary.Array.includes(names, new Arguments { "NameForDotNetSide" }));
        }

        [TestMethod]
        public void AliasForMember_ShouldChangeNameOfFunction()
        {
            var context = new Context();
            context.DefineConstructor(typeof(ClassWithAliasedMember));

            var name = context.Eval("ClassWithAliasedMember.NameForJsSide.name").Value.ToString();

            Assert.AreEqual("NameForJsSide", name);
        }

        [TestMethod]
        public void AliasForMember_ShouldProcessSymbolName()
        {
            var context = new Context();
            context.DefineConstructor(typeof(ClassWithAliasedMember));

            var name = context.Eval("ClassWithAliasedMember[Symbol.for('MySymbol')].name").Value.ToString();

            Assert.AreEqual("MySymbol", name);
        }
    }
}
