using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace Tests.Core
{
    [TestClass]
    public sealed class NumberUtilsTests
    {
        [TestMethod]
        public void LongNumberShouldParsedCorrectly()
        {
            var numbers = new KeyValuePair<double, string>[]
            {
                new KeyValuePair<double, string>(1.42119667662395410000, "1.42119667662395410000"),
                new KeyValuePair<double, string>(26394813313751084.00000000000000000000, "26394813313751084.00000000000000000000"),
                new KeyValuePair<double, string>(16.00000000000000000000,"16.00000000000000000000"),
                new KeyValuePair<double, string>(10.41269841269841300000, "10.41269841269841300000"),
                new KeyValuePair<double, string>(0.76190476190476186000, "0.76190476190476186000"),
                new KeyValuePair<double, string>(0.00002260186576944384, "0.00002260186576944384"),
                new KeyValuePair<double, string>(15.49206349206349200000, "15.49206349206349200000"),
                new KeyValuePair<double, string>(14.22222222222222100000, "14.22222222222222100000"),
                new KeyValuePair<double, string>(13.96825396825396800000, "13.96825396825396800000"),
                new KeyValuePair<double, string>(13.20634920634920600000, "13.20634920634920600000"),
                new KeyValuePair<double, string>(8.98846567431158e+307, "8.98846567431158e+307"),
                new KeyValuePair<double, string>(0.35826121851261677000, "0.35826121851261677000"),
                new KeyValuePair<double, string>(3.29119230376073580000, "3.29119230376073580000"),
                new KeyValuePair<double, string>(1664158979.1109629, "1664158979.11096290000000000000"),
                new KeyValuePair<double, string>(0.00021140449751288852, "0.00021140449751288852"),
                new KeyValuePair<double, string>(34.970703125, "34.970703125"),
                new KeyValuePair<double, string>(1.7158203125, "1.7158203125"),
                new KeyValuePair<double, string>(0.6, "0.6")
            };

            foreach (var number in numbers)
            {
                var parsedNumber = 0.0;
                var result = NumberUtils.TryParse(number.Value, 0, out parsedNumber);

                Assert.AreNotEqual(0, result);
                Assert.AreEqual(number.Key, parsedNumber);
            }
        }

        [TestMethod]
        public void DoubleToString()
        {
            var numbers = new KeyValuePair<double, string>[]
            {
                new KeyValuePair<double, string>(69.85, "69.85"),
                new KeyValuePair<double, string>(10.2, "10.2"),
                new KeyValuePair<double, string>(12.34, "12.34"),
                new KeyValuePair<double, string>(1.3, "1.3"),
                new KeyValuePair<double, string>(20.20, "20.2"),
                new KeyValuePair<double, string>(0.00021140449751288852, "0.0002114044975128885"),
                new KeyValuePair<double, string>(34.970703125, "34.970703125"),
                new KeyValuePair<double, string>(1.7158203125, "1.7158203125")
            };

            foreach (var (number, expected) in numbers)
            {
                var parsedNumber = NumberUtils.DoubleToString(number);

                Assert.AreEqual(expected, parsedNumber);
            }
        }
    }
}
