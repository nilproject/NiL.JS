using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using System.Linq;

namespace FunctionalTests.Fuzz
{
    [TestClass]
    public class Bug_106
    {
        [TestMethod]
        public void SetField91()
        {
            var oo = JSObject.CreateObject();
            oo["-1"] = -1;
            oo["test"] = false;
            var ro = SetField91_p(oo, "+1", 1);

            Assert.AreEqual(1, (int)ro["+1"]);  // pass

            bool pass = false;
            foreach (var kv in ro)
            {
                if (kv.Key == "+1")
                {
                    pass = true;
                }
            }
            Assert.IsTrue(pass);  // failed

            string json = JSON.stringify(ro, null, null, null);  // json = {"-1":-1,"test":false}
        }

        /// <summary>change field in immutable object</summary>
        private JSObject SetField91_p(JSObject inp, string field, JSValue val)
        {
            var jo = JSObject.CreateObject();
            foreach (var kv in inp.OrderByDescending(z => z.Key))
            {
                if (kv.Key == field)
                {
                    if (val != null)
                    {
                        jo[kv.Key] = val;
                    }
                    else
                    {
                        jo.DeleteProperty(kv.Key);
                    }
                }
                else
                {
                    jo[kv.Key] = kv.Value;
                }
            }
            if (val != null && !jo.GetProperty(field, PropertyScope.Own).Defined)
            {
                jo[field] = val;
            }
            return jo;
        }
    }
}
