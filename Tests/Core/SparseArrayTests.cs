using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace Tests.Core
{
    [TestClass]
    public class SparseArrayTests
    {
        [TestMethod]
        public void DirectOrderShouldWorkCorrectlyInSparseMode()
        {
            var sparseArray = new SparseArray<int>(ArrayMode.Sparse);

            unchecked
            {
                sparseArray[0] = 0;
                sparseArray[(int)0xf0000000] = 7;
                sparseArray[(int)0x70000000] = 5;
            }

            var output = sparseArray.DirectOrder.ToArray();
            Assert.AreEqual(0, output[0].Value);
            Assert.AreEqual(5, output[1].Value);
            Assert.AreEqual(7, output[2].Value);
        }
    }
}
