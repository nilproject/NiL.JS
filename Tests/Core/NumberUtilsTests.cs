using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;
using NiL.JS.Backward;
using NiL.JS;
using NiL.JS.Extensions;
using System;

namespace Tests.Core;

[TestClass]
public sealed class NumberUtilsTests
{
    [TestMethod]
    public void LongNumberShouldParsedCorrectly()
    {
        var numbers = new (double Number, string String)[]
        {
            (1.42119667662395410000, "1.42119667662395410000"),
            (26394813313751084.00000000000000000000, "26394813313751084.00000000000000000000"),
            (16.00000000000000000000,"16.00000000000000000000"),
            (10.41269841269841300000, "10.41269841269841300000"),
            (0.76190476190476186000, "0.76190476190476186000"),
            (0.00002260186576944384, "0.00002260186576944384"),
            (15.49206349206349200000, "15.49206349206349200000"),
            (14.22222222222222100000, "14.22222222222222100000"),
            (13.96825396825396800000, "13.96825396825396800000"),
            (13.20634920634920600000, "13.20634920634920600000"),
            (8.98846567431158e+307, "8.98846567431158e+307"),
            (0.35826121851261677000, "0.35826121851261677000"),
            (3.29119230376073580000, "3.29119230376073580000"),
            (1664158979.1109629, "1664158979.11096290000000000000"),
            (0.00021140449751288852, "0.00021140449751288852"),
            (34.970703125, "34.970703125"),
            (1.7158203125, "1.7158203125"),
            (0.6, "0.6"),
            (123.456, "1_23.45_6"),
            (123, "1_23._456"),
        };

        foreach (var number in numbers)
        {
            var result = NumberUtils.TryParse(number.String, 0, false, out var parsedNumber, out _);

            Assert.AreNotEqual(0, result);
            Assert.AreEqual(number.Number, parsedNumber);
        }
    }

    [TestMethod]
    public void DoubleToString()
    {
        var numbers = new (double, string)[]
        {
            (69.85, "69.85"),
            (10.43, "10.43"),
            (10.67, "10.67"),
            (10.2, "10.2"),
            (12.34, "12.34"),
            (1.3, "1.3"),
            (20.20, "20.2"),
            (0.00021140449751288852, "0.0002114044975128885"),
            (34.970703125, "34.970703125"),
            (1.7158203125, "1.7158203125"),
            (20.99, "20.99"),
            (10.19, "10.19"),
            (8.06, "8.06"),
            (10.36, "10.36"),
            (0.56, "0.56"),
            (0.68, "0.68"),
            (9.12, "9.12")
        };

        foreach (var (number, expected) in numbers)
        {
            var parsedNumber = NumberUtils.DoubleToString(number);

            Assert.AreEqual(expected, parsedNumber);
        }
    }

    [TestMethod]
    public void TestValidCodeWithSeparators()
    {
        var script = Script.Parse("1_234.5_6");
        var value = script.Root.Children[0].Evaluate(null);

        Assert.AreEqual(1234.56, value.As<double>());
    }

    [TestMethod]
    public void TestInvalidCodeWithSeparators0()
    {
        Assert.ThrowsException<JSException>(() => Script.Parse("1_234._6"));
    }

    [TestMethod]
    public void TestInvalidCodeWithSeparators1()
    {
        Assert.ThrowsException<JSException>(() => Script.Parse("1_234.5_"));
    }

    [TestMethod]
    public void TestInvalidCodeWithSeparators2()
    {
        Assert.ThrowsException<JSException>(() => Script.Parse("1_234_.56"));
    }

    [TestMethod]
    public void TestInvalidCodeWithSeparators3()
    {
        Assert.ThrowsException<JSException>(() => Script.Parse("_1234.56"));
    }
}
